Shader "Fresnel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ObjectColor("Object Color", Color) = (1, 1, 1, 1)
        _FI("Fresnel Intensity", Range(5, 10)) = 1
        _FS("Flashing Speed", Range(0, 10)) = 1
    }
    SubShader
    {
        Tags
        { 
            "RenderType"="Opaque"
            "LightMode" = "ForwardBase"
        }
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct meshdata
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
                float3 wPos: TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ObjectColor;
            float _FI;
            float _FS;

            Interpolators vert (meshdata v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.wPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag(Interpolators i) : SV_Target
            {
                //Normals and Camera location relative to object
                float3 normals = normalize(i.normal);
                float3 camera = normalize(_WorldSpaceCameraPos - i.wPos);

                //Temp base color
                float4 finalColor = float4(0, 0, 0, 1);

                //Fresnel effect with flashing
                float fresnel = saturate(1 - dot(camera, normals) * _FI);

                //*Zero value removes flashing*
                float flashing = (cos(_Time.y * _FS + radians(180)/2) * 0.5 + 1);
                return float4(_ObjectColor.xyz + fresnel.xxx * flashing, 1);
            }
            ENDCG
        }
    }
}
