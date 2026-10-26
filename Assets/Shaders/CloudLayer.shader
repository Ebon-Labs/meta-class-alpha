Shader "Custom/CloudLayer"
{
    Properties
    {
        _MainTex ("Clouds", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Opacity ("Opacity", Range(0, 1)) = 0.8
        _ScrollSpeed ("Scroll Speed", Float) = 0.005
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }
        Cull Back
        ZWrite Off
        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Opacity;
            float _ScrollSpeed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.x += _Time.y * _ScrollSpeed;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 cloud = tex2D(_MainTex, i.uv);
                float luminance = dot(cloud.rgb, float3(0.299, 0.587, 0.114));
                fixed4 col;
                col.rgb = _Color.rgb;
                col.a = luminance * _Opacity * _Color.a;
                return col;
            }
            ENDCG
        }
    }
}
