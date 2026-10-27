Shader "Custom/InteractionGlow"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.75, 0.35, 1)
        _Intensity ("Intensity", Range(0, 4)) = 1.5
        _Softness ("Soft Edge", Range(0.1, 1)) = 0.55
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }
        Cull Off
        ZWrite Off
        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Intensity;
            float _Softness;

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
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 centered = i.uv * 2.0 - 1.0;
                float dist = length(centered);
                float alpha = saturate(1.0 - dist / _Softness);
                alpha *= alpha;
                fixed4 col = _Color;
                col.rgb *= _Intensity;
                col.a *= alpha;
                return col;
            }
            ENDCG
        }
    }
}
