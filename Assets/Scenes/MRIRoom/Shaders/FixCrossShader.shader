Shader "Unlit/AlwaysOnTopShader"
{
    Properties
    {
        _Color ("Crosshair Color", Color) = (1,1,1,1) // Crosshair color
        _BackgroundColor ("Background Color", Color) = (0,0,0,0.2) // Transparent background color
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }

        Pass
        {
            ZWrite On
            ZTest Always
            Cull Off
            
            // Set up blending for the crosshair and background
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Color;
            float4 _BackgroundColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // Mix the crosshair color with the background color
                col.rgb = lerp(_BackgroundColor.rgb, col.rgb, col.a);
                col.a = max(_Color.a, _BackgroundColor.a); // Make the crosshair fully opaque and blend the background
                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
