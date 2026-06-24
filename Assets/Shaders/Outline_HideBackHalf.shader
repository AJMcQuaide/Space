//Show an outline on an object of solid color, and hide the half that is furthest from the camera from the center point of the object
Shader "Outline_HideBackSide"
{
    Properties
    {
        _C ("Color", Color) = (0, 0, 0, 1)
        _O ("OutlineColor", Color) = (1, 1, 1, 1)
        _OT ("OutlineThickness", Range(0, 0.02)) = 0.01
        _I ("Intersection", Range(-1, 1)) = 0
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
        //Cull Front
        // ZWrite On
        // ZTest LEqual

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
                //float3 worldPos : TEXCOORD2;
                float3 vertexLocal : TEXCOORD3;
            };

            float4 _C;
            float4 _O;
            float _OT;
            float _I;

            Interpolators vert (appdata v)
            {
                Interpolators o;

                v.vertex += float4(normalize(v.normal) * 0.05 * _OT, 1);

                o.vertexLocal = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 0.0)).xyz;

                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (Interpolators i) : SV_Target
            {
                float3 objectCenterToCamera = _WorldSpaceCameraPos - unity_ObjectToWorld._m03_m13_m23;
                float dotProd = dot(normalize(i.vertexLocal), normalize(objectCenterToCamera));
                dotProd += _I;
                clip(dotProd);
                return _C;
            }
            ENDCG
        }

        // Pass
        // {
        //     CGPROGRAM

        //     #pragma vertex vert
        //     #pragma fragment frag

        //     #include "UnityCG.cginc"

        //     struct appdata
        //     {
        //         float4 vertex : POSITION;
        //         float2 uv : TEXCOORD0;
        //     };

        //     struct Interpolators
        //     {
        //         float2 uv : TEXCOORD0;
        //         float4 vertex : SV_POSITION;
        //         //float3 worldPos : TEXCOORD1;
        //         float3 vertexLocal : TEXCOORD2;
        //     };

        //     float4 _C;
        //     float4 _O;
        //     float _OT;

        //     Interpolators vert (appdata v)
        //     {
        //         Interpolators o;
        //         o.vertexLocal = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 0.0)).xyz;
        //         o.vertex = UnityObjectToClipPos(v.vertex);
        //         //o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        //         o.uv = v.uv;
        //         return o;
        //     }

        //     fixed4 frag (Interpolators i) : SV_Target
        //     {
        //         float3 objectCenterToCamera = _WorldSpaceCameraPos - unity_ObjectToWorld._m03_m13_m23;
        //         float dotProd = dot(normalize(i.vertexLocal), normalize(objectCenterToCamera));
        //         //clip(dotProd);
        //         return _C;
        //     }
        //    ENDCG
        // }
    }
}