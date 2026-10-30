Shader "Custom/ButtonFace"
{
    Properties
    {
        _MainTex ("Icon", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Lighting Off
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float front : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.front = step(0.5, v.normal.y);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 icon = tex2D(_MainTex, i.uv);
                fixed3 white = fixed3(1, 1, 1);
                fixed3 face = lerp(white, icon.rgb, icon.a * i.front);
                return fixed4(face * _Color.rgb, 1);
            }
            ENDCG
        }
    }
}
