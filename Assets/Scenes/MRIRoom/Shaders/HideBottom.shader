Shader "Unlit/HideBottom" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _CutoffHeight ("Cutoff Height", Float) = 0.0
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _Color;
            float _CutoffHeight;
            float4 _MainTex_ST; // Déclaration de la variable _MainTex_ST pour la transformation de texture

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            Varyings vert (Attributes v) {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = TransformObjectToWorld(v.positionOS).xyz;
                return o;
            }

            half4 frag (Varyings i) : SV_Target {
                // Masque les pixels en dessous de la hauteur de coupure
                if (i.worldPos.y < _CutoffHeight) {
                    discard;
                }

                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return texColor * _Color;
            }
            ENDHLSL
        }
    }
    FallBack "Unlit/Transparent"
}
