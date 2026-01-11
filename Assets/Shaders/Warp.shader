Shader "Unlit/Warp"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Gravity ("Gravity", float) = 1
        _Radius ("Radius", float) = 20
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "RendereQueue"="Transparent"
        }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct meshdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };


            struct interpolators
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _ReferencePos;
            float _Gravity;
            float _Radius;

            //Vertex Shader
            interpolators vert (meshdata v)
            {
                interpolators o;

                //Find world pos of vertex
                float3 vertexWorldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                //Difference vector
                float3 difference = float3(vertexWorldPos - _ReferencePos.xyz);
                //Distance from object
                float dist = abs(length(difference));
                //Direction
                float3 direction = normalize(difference);

                //Factor to scale the offset
                //float reverseDistance = float(_Radius - distance);

                //Move to 0 to 1
                //dist /= _Radius;

                //dist = dist * dist * (3.0 - 2.0 * dist);

                //dist *= _Radius;

                //Clamp the factor
                //reverseDistance = clamp(reverseDistance, 0, dist);

                //Offset
                //float3 offset = float3(direction * reverseDistance);

                float3 offset = direction * _Gravity;

                //Relocate Vertex
                //v.vertex += float4(offset, 1);

                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            //Fragment Shader
            fixed4 frag (interpolators i) : SV_Target
            {
                //return float4(_ReferencePos, 1);

                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
