// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/*
//  Copyright (c) 2015 José Guerreiro. All rights reserved.
//  MIT license, see http://www.opensource.org/licenses/mit-license.php
*/

Shader "Hidden/OutlineEffect" 
{
    Properties 
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _OutlineSource ("Outline Source", 2D) = "black" {}
        _LineColor1 ("Line Color 1", Color) = (1,1,1,1)
        _LineColor2 ("Line Color 2", Color) = (1,1,1,1)
        _LineColor3 ("Line Color 3", Color) = (1,1,1,1)
        _LineThicknessX ("Line Thickness X", Float) = 1
        _LineThicknessY ("Line Thickness Y", Float) = 1
        _LineIntensity ("Line Intensity", Float) = 1
        _Softness ("Outline Softness", Range(0.01, 1.0)) = 0.5
        _FillAmount ("Fill Amount", Float) = 0
        _FillColor ("Fill Color", Color) = (1,1,1,1)
        _UseFillColor ("Use Fill Color", Int) = 0
        _Dark ("Darken Background", Int) = 0
        _FlipY ("Flip Y", Int) = 0
        _CornerOutlines ("Corner Outlines", Int) = 0
    }
    SubShader 
    {
        // --- Pass 0: Color Collision Detection ---
        Pass
        {
            ZTest Always ZWrite Off Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _OutlineSource;
            float _LineThicknessX;
            float _LineThicknessY;

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
            float _FillAmount, _Softness;
            fixed4 _FillColor;

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
                
                // Unity UV handling
                if (_FlipY == 1) uv.y = 1 - uv.y;
                #if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y < 0) uv.y = 1 - uv.y;
                #endif

                half4 screen = tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(input.uv, _MainTex_ST));
                half4 mask = tex2D(_OutlineSource, UnityStereoScreenSpaceUVAdjust(uv, _MainTex_ST));

                // 1. Edge Sampling
                float2 offX = float2(_LineThicknessX, 0);
                float2 offY = float2(0, _LineThicknessY);
                
                half4 s1 = tex2D(_OutlineSource, uv + offX);
                half4 s2 = tex2D(_OutlineSource, uv - offX);
                half4 s3 = tex2D(_OutlineSource, uv + offY);
                half4 s4 = tex2D(_OutlineSource, uv - offY);

                half4 maxS = max(max(s1, s2), max(s3, s4));

                if (_CornerOutlines) {
                    maxS = max(maxS, tex2D(_OutlineSource, uv + offX + offY));
                    maxS = max(maxS, tex2D(_OutlineSource, uv - offX - offY));
                    maxS = max(maxS, tex2D(_OutlineSource, uv + offX - offY));
                    maxS = max(maxS, tex2D(_OutlineSource, uv - offX + offY));
                }

                // 2. Anti-Aliasing via Smoothstep
                // This creates a smooth curve between the background and the edge.
                float edge = smoothstep(0, _Softness, maxS.a - mask.a);
                
                // 3. Color Logic
                half4 outCol = _LineColor1;
                if (maxS.g > maxS.r && maxS.g > maxS.b) outCol = _LineColor2;
                else if (maxS.b > maxS.r && maxS.b > maxS.g) outCol = _LineColor3;

                half4 outline = outCol * _LineIntensity;

                // 4. Darkening Logic
                if (_Dark) {
                    screen.rgb *= lerp(1.0, (1.0 - outCol.a), edge);
                }

                // 5. Final Composition
                // Lerp creates the semi-transparent anti-aliased edge
                half4 combined = lerp(screen, outline, edge);
                
                // Inner Fill
                if (_FillAmount < 1.0) {
                    half4 fill = _UseFillColor ? _FillColor : (outline * _FillAmount);
                    combined = lerp(combined, combined + fill, mask.a * (1.0 - _FillAmount));
                }

                return combined;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}