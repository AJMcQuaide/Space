Shader "Outline"
{
    Properties
    {
        _C ("Color", Color) = (0, 0, 0, 1)
        _O ("OutlineColor", Color) = (1, 1, 1, 1)
        _OT ("OutlineThickness", Range(0.001, 0.02)) = 0.01
    }
    SubShader
    {
        Tags
        {
            "RendereQueue"="Overlay"
        }
        LOD 100
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
            };

            float4 _C;
            float4 _O;
            float _OT;

            Interpolators vert (appdata v)
            {
                Interpolators o;

                v.vertex += float4(normalize(v.normal) * _OT, 1);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (Interpolators i) : SV_Target
            {
                return _O;
            }
            ENDCG
        }

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _C;
            float4 _O;
            float _OT;

            Interpolators vert (appdata v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (Interpolators i) : SV_Target
            {
                return _C;
            }
            ENDCG
        }
    }
}