Shader "FREE Food Pack/Food_URP"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _FresnelSize ("FresnelSize", Range(0.5, 5)) = 1
        _FresnelIntensity ("FresnelIntensity", Float) = 0.2
        _FresnelColor ("FresnelColor", Color) = (1,1,1,1)
        _Push ("Push", Range(0, 0.01)) = 0
        _Speed ("Speed", Float) = 1
        _Intensity ("Intensity", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _FresnelSize;
                float _FresnelIntensity;
                float4 _FresnelColor;
                float _Push;
                float _Speed;
                float _Intensity;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                float t = _Time.y * _Speed;
                float wobble = _Push * (sin(t) * 0.5 + 0.5);

                float3 posOS = IN.positionOS.xyz + IN.normalOS * wobble;

                VertexPositionInputs posInputs = GetVertexPositionInputs(float4(posOS, 1.0));
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS = normalize(normalInputs.normalWS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);

                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float3 normalWS = normalize(IN.normalWS);
                float3 viewDir = normalize(GetCameraPositionWS() - IN.positionWS);

                float3 baseCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).rgb;

                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDir)), _FresnelSize);
                float3 rim = _FresnelColor.rgb * fresnel * _FresnelIntensity;

                float3 finalColor = (baseCol + rim) * _Intensity;

                return half4(finalColor, 1);
            }

            ENDHLSL
        }
    }
}