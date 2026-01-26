// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/*
//  Copyright (c) 2015 José Guerreiro. All rights reserved.
//
//  MIT license, see http://www.opensource.org/licenses/mit-license.php
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
*/

Shader "Hidden/OutlineEffect" 
{
    Properties 
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Softness ("Outline Softness", Range(1, 20)) = 10
    }
    SubShader 
    {
        // --- Pass 0: Handles color collisions (unchanged logic, optimized) ---
        Pass
        {
            ZTest Always ZWrite Off Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _OutlineSource;
            float4 _MainTex_ST;
            float _LineThicknessX;
            float _LineThicknessY;
            float4 _MainTex_TexelSize;

            struct v2f {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_img v) {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            half4 frag(v2f input) : COLOR {
                float2 uv = input.uv;
                half4 s1 = tex2D(_OutlineSource, uv + float2(_LineThicknessX, 0));
                half4 s2 = tex2D(_OutlineSource, uv + float2(-_LineThicknessX, 0));
                half4 s3 = tex2D(_OutlineSource, uv + float2(0, _LineThicknessY));
                half4 s4 = tex2D(_OutlineSource, uv + float2(0, -_LineThicknessY));

                const float h = .95f;
                bool red = s1.r > h || s2.r > h || s3.r > h || s4.r > h;
                bool green = s1.g > h || s2.g > h || s3.g > h || s4.g > h;
                bool blue = s1.b > h || s2.b > h || s3.b > h || s4.b > h;
                 
                if ((red && blue) || (green && blue) || (red && green))
                    return float4(0,0,0,0);
                
                return tex2D(_OutlineSource, uv);
            }
            ENDCG
        }

       // --- Pass 1: The Smooth Anti-Aliased Outline ---
        Pass
        {
            ZTest Always ZWrite Off Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _OutlineSource;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            
            float _LineThicknessX;
            float _LineThicknessY;
            float _LineIntensity;
            half4 _LineColor1, _LineColor2, _LineColor3;
            int _FlipY, _Dark, _UseFillColor, _CornerOutlines;
            float _FillAmount;
            fixed4 _FillColor;
            
            // ENSURE THIS IS DECLARED HERE
            float _Softness; 

            struct v2f {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_img v) {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            half4 frag (v2f input) : COLOR {
                float2 uv = input.uv;
                
                // Fix for Unity's coordinate system
                if (_FlipY == 1) uv.y = 1 - uv.y;
                #if UNITY_UV_STARTS_AT_TOP
                    if (_MainTex_TexelSize.y < 0) uv.y = 1 - uv.y;
                #endif

                half4 originalPixel = tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(input.uv, _MainTex_ST));
                half4 center = tex2D(_OutlineSource, UnityStereoScreenSpaceUVAdjust(uv, _MainTex_ST));

                // 1. Sample Neighbors
                float2 offsetX = float2(_LineThicknessX, 0);
                float2 offsetY = float2(0, _LineThicknessY);
                
                half4 s1 = tex2D(_OutlineSource, uv + offsetX);
                half4 s2 = tex2D(_OutlineSource, uv - offsetX);
                half4 s3 = tex2D(_OutlineSource, uv + offsetY);
                half4 s4 = tex2D(_OutlineSource, uv - offsetY);

                half4 maxS = max(max(s1, s2), max(s3, s4));

                if (_CornerOutlines) {
                    maxS = max(maxS, tex2D(_OutlineSource, uv + offsetX + offsetY));
                    maxS = max(maxS, tex2D(_OutlineSource, uv - offsetX - offsetY));
                    maxS = max(maxS, tex2D(_OutlineSource, uv + offsetX - offsetY));
                    maxS = max(maxS, tex2D(_OutlineSource, uv - offsetX + offsetY));
                }

                // 2. SMOOTHNESS CALCULATION
                // We use the difference between the neighbor max and center to find the edge
                // Higher _Softness makes the edge thinner/sharper.
                float edge = saturate((maxS.a - center.a) * _Softness);
                
                // 3. COLOR SELECTION
                half4 outColor = _LineColor1;
                if (maxS.g > maxS.r && maxS.g > maxS.b) outColor = _LineColor2;
                else if (maxS.b > maxS.r && maxS.b > maxS.g) outColor = _LineColor3;

                half4 outlineCol = outColor * _LineIntensity;

                // 4. DARKENING / ADDITIVE
                if (_Dark) {
                    originalPixel.rgb *= lerp(1.0, (1.0 - outColor.a), edge);
                }

                // 5. FINAL BLEND
                // This blends the original screen with the outline color using our smooth 'edge'
                half4 final = lerp(originalPixel, outlineCol, edge);

                // Apply Fill if needed
                if (_FillAmount < 1.0) {
                    half4 fill = _UseFillColor ? _FillColor : (outlineCol * _FillAmount);
                    // Only fill where the center object actually exists
                    final = lerp(final, final + fill, (center.a) * (1.0 - _FillAmount));
                }

                return final;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}