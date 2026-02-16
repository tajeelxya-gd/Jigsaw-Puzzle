Shader "Custom/GPU_Animated_Toon"
{
    Properties
    {
        _MainTex ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        [Header(Toon Light Steps)]
        _LightSteps ("Light Steps", Range(1,10)) = 10
        _StepSmooth ("Step Smoothness", Range(0,1)) = 1
        _MinLight ("Minimum Light", Range(0,0.5)) = 0.15
        _LightIntensity ("Light Intensity", Range(0,3)) = 1

        [Header(Toon Specular)]
        _SpecularColor ("Specular Color", Color) = (1,1,1,1)
        _SpecSize ("Specular Size", Range(0.01,1)) = 0.15
        _SpecSharpness ("Specular Sharpness", Range(8,64)) = 32

        [Header(Emission)]
        _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _EmissionStrength ("Emission Strength", Range(0,5)) = 0

        [Header(GPU Animation)]
        _PosTexArray ("Position Tex Array", 2DArray) = "" {}
        _NmlTexArray ("Normal Tex Array", 2DArray) = "" {}
        _AnimLength ("Anim Length", Float) = 1
        _DT ("Time Offset", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 400

        CGINCLUDE
        #include "UnityCG.cginc"
        #include "Lighting.cginc"

        UNITY_DECLARE_TEX2DARRAY(_PosTexArray);
        UNITY_DECLARE_TEX2DARRAY(_NmlTexArray);

        float4 _PosTexArray_TexelSize;
        float _AnimLength, _DT;

        UNITY_INSTANCING_BUFFER_START(Anim)
            UNITY_DEFINE_INSTANCED_PROP(float, _AnimSpeed)
            UNITY_DEFINE_INSTANCED_PROP(float, _AnimOffset)
            UNITY_DEFINE_INSTANCED_PROP(float, _AnimIndex)
           /// UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
        UNITY_INSTANCING_BUFFER_END(Anim)

        float3 AnimPos(uint vid)
        {
            float speed  = UNITY_ACCESS_INSTANCED_PROP(Anim, _AnimSpeed);
            float offset = UNITY_ACCESS_INSTANCED_PROP(Anim, _AnimOffset);
            float index  = UNITY_ACCESS_INSTANCED_PROP(Anim, _AnimIndex);

            speed = (speed == 0) ? 1.0 : speed;

            float t = frac((_Time.y * speed + offset - _DT) / _AnimLength);
            float x = (vid + 0.5) * _PosTexArray_TexelSize.x;

            return UNITY_SAMPLE_TEX2DARRAY_LOD(
                _PosTexArray,
                float3(x, t, index),
                0
            ).xyz;
        }

        float3 AnimNormal(uint vid)
        {
            float index = UNITY_ACCESS_INSTANCED_PROP(Anim, _AnimIndex);
            float x = (vid + 0.5) * _PosTexArray_TexelSize.x;

            return normalize(
                UNITY_SAMPLE_TEX2DARRAY_LOD(
                    _NmlTexArray,
                    float3(x, 0, index),
                    0
                ).xyz
            );
        }
        ENDCG

        // ---------- FORWARD ----------
        Pass
        {
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_instancing

            sampler2D _MainTex;
            float4 _BaseColor;

            float _LightSteps, _StepSmooth, _MinLight, _LightIntensity;

            float4 _SpecularColor;
            float _SpecSize, _SpecSharpness;

            float4 _EmissionColor;
            float _EmissionStrength;

            struct appdata
            {
                float2 uv : TEXCOORD0;
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewWS : TEXCOORD2;
                float4 pos : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);

                float3 posOS = AnimPos(v.vertexID);
                float3 nrmOS = AnimNormal(v.vertexID);

                float3 worldPos = mul(unity_ObjectToWorld, float4(posOS,1)).xyz;

                v2f o;
                o.pos = UnityObjectToClipPos(float4(posOS,1));
                o.normalWS = UnityObjectToWorldNormal(nrmOS);
                o.viewWS = normalize(_WorldSpaceCameraPos - worldPos);
                o.uv = v.uv;

                UNITY_TRANSFER_INSTANCE_ID(v,o);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
              //  float4 baseCo = UNITY_ACCESS_INSTANCED_PROP(Anim, _BaseColor);
                fixed4 albedo = tex2D(_MainTex, i.uv) * _BaseColor;

            fixed3 NA = normalize(i.normalWS);
            fixed3 LA = normalize(_WorldSpaceLightPos0.xyz);
            fixed3 VA = normalize(i.viewWS);
            fixed3 HA = normalize(LA + VA);

            float ndl = saturate(dot(NA, LA));

            float steps = _LightSteps; 
            float smooth = _StepSmooth;
            float stepped = ndl * steps;
            float lowerStep = floor(stepped);
            float upperStep = lowerStep + 1.0;

            stepped = lerp(lowerStep / steps, upperStep / steps, smooth * (stepped - lowerStep));
            stepped = lerp(_MinLight, 1.0, stepped);
            stepped *= _LightIntensity;

            fixed3 color = albedo.rgb * stepped;

            float nh = saturate(dot(NA, HA));
            float spec = pow(nh, _SpecSharpness);
            spec = step(1.0 - _SpecSize, spec);
            color += spec * _SpecularColor.rgb;

            color += _EmissionColor.rgb * _EmissionStrength;

            return fixed4(color, albedo.a);
            }
            ENDCG
        }

        // ---------- SHADOW ----------
        // Pass
        // {
        //     Tags { "LightMode"="ShadowCaster" }
        //     ZWrite On

        //     CGPROGRAM
        //     #pragma target 4.5
        //     #pragma vertex vertShadow
        //     #pragma fragment fragShadow
        //     #pragma multi_compile_instancing
        //     #pragma multi_compile_shadowcaster

        //     struct appdata { uint vertexID : SV_VertexID; UNITY_VERTEX_INPUT_INSTANCE_ID };
        //     struct v2f { float4 pos : SV_POSITION; };

        //     v2f vertShadow(appdata v)
        //     {
        //         UNITY_SETUP_INSTANCE_ID(v);
        //         v2f o;
        //         o.pos = UnityObjectToClipPos(float4(AnimPos(v.vertexID),1));
        //         return o;
        //     }

        //     float4 fragShadow(v2f i) : SV_Target { return 0; }
        //     ENDCG
        // }
    }

    FallBack Off
}
