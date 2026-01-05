Shader "Pixelise/Water"
{
    Properties
    {
        _Color ("Color", Color) = (0.2, 0.5, 0.8, 0.6)
        _WaveStrength ("Wave Strength", Float) = 0.05
        _WaveSpeed ("Wave Speed", Float) = 1.2
        _WaveScale ("Wave Scale", Float) = 0.8
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _Color;
            float _WaveStrength;
            float _WaveSpeed;
            float _WaveScale;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                // 🌊 VAGUES EN WORLD SPACE
                float wave =
                    sin(worldPos.x * _WaveScale + _Time.y * _WaveSpeed) +
                    cos(worldPos.z * _WaveScale + _Time.y * _WaveSpeed);

                worldPos.y += wave * _WaveStrength;

                o.pos = UnityWorldToClipPos(worldPos);
                o.worldPos = worldPos;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
