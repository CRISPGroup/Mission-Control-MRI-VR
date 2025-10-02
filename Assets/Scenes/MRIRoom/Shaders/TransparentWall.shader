Shader "Custom/TransparentWall" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1, 0, 0, 1)
        _StencilVal ("Stencil Value", Int) = 1
        _OutsideColor ("Outside Color", Color) = (1, 1, 1, 1)
    }
    SubShader {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 200

        Stencil {
            Ref [_StencilVal]
            Comp NotEqual
            Pass Keep
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On

        Pass {
            Tags{"LightMode" = "UniversalForward"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _OutsideColor;
            float4 _MainTex_ST;
            int _StencilVal;


            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 outputColor = tex * _Color;

                // If the stencil value is not equal, render the outside color
                if (_StencilVal == 0) {
                    outputColor = _OutsideColor;
                }

                return outputColor;
            }

            ENDHLSL
        }
    }
}
