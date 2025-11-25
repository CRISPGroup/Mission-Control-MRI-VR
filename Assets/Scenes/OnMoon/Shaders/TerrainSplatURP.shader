Shader "Custom/TerrainSplatURP"
{
    Properties
    {
        _Control("Splatmap (RGBA)", 2D) = "white" {}

        _Splat0("Texture 0 (R)", 2D) = "white" {}
        _Splat1("Texture 1 (G)", 2D) = "white" {}
        _Splat2("Texture 2 (B)", 2D) = "white" {}
        _Splat3("Texture 3 (A)", 2D) = "white" {}

        _Tiling0("Tiling Layer 0", Float) = 8.0
        _Tiling1("Tiling Layer 1", Float) = 8.0
        _Tiling2("Tiling Layer 2", Float) = 8.0
        _Tiling3("Tiling Layer 3", Float) = 8.0

        _Exposure("Global Brightness", Range(0.1, 2.0)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
                float fogFactor    : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_Control); SAMPLER(sampler_Control);
            TEXTURE2D(_Splat0); SAMPLER(sampler_Splat0);
            TEXTURE2D(_Splat1); SAMPLER(sampler_Splat1);
            TEXTURE2D(_Splat2); SAMPLER(sampler_Splat2);
            TEXTURE2D(_Splat3); SAMPLER(sampler_Splat3);

            float _Tiling0;
            float _Tiling1;
            float _Tiling2;
            float _Tiling3;
            float _Exposure;

            Varyings vert (Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = v.uv;

                // Fog URP
                o.fogFactor = ComputeFogFactor(o.positionHCS.z);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // Splatmap
                float4 control = SAMPLE_TEXTURE2D(_Control, sampler_Control, i.uv);

                // Appliquer un tiling indépendant à chaque texture
                float4 col0 = SAMPLE_TEXTURE2D(_Splat0, sampler_Splat0, i.uv * _Tiling0);
                float4 col1 = SAMPLE_TEXTURE2D(_Splat1, sampler_Splat1, i.uv * _Tiling1);
                float4 col2 = SAMPLE_TEXTURE2D(_Splat2, sampler_Splat2, i.uv * _Tiling2);
                float4 col3 = SAMPLE_TEXTURE2D(_Splat3, sampler_Splat3, i.uv * _Tiling3);

                // Mélange pondéré selon RGBA de la splatmap
                float4 albedo = col0 * control.r +
                                col1 * control.g +
                                col2 * control.b +
                                col3 * control.a;

                // Éclairage directionnel URP
                Light mainLight = GetMainLight();
                float3 normal = normalize(i.normalWS);
                float NdotL = saturate(dot(normal, mainLight.direction));
                float3 litColor = albedo.rgb * (mainLight.color * NdotL + 0.1); // +0.1 lumière ambiante

                // Contrôle global de l’exposition
                litColor *= _Exposure;

                half4 finalColor = half4(litColor, 1.0);

                // Fog URP
                finalColor.rgb = MixFog(finalColor.rgb, i.fogFactor);

                return finalColor;
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
