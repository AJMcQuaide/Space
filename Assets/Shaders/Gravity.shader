Shader "Gravity"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            
            static const float G = 0.0000000000667;
            static const float c = 299792458.0;
            static const float S = 10000000.0;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _GridMultiplier;
            int _CBCount;
            float4 _Position[100];
            float _Mass[100];
            float _MaxAcceleration[100];
            int useGPU;

            float SqrLength(float3 vec)
            {
                float length = (vec.x * vec.x) + (vec.y * vec.y) + (vec.z * vec.z);
                return abs(length);
            }

            float GetAcceleration(float differenceUnity, float mass)
            {
                float r = differenceUnity * S;
                float g = (G * mass) / (r * r);
                return g;
            }

            float3 Difference(float3 a, float3 b)
            {
                return (a-b);
            }

            float3 WarpGrid(float3 vertexWorldPos)
            {
                //Each Cb
                float3 offset = float3(0, 0, 0);
                //All Cb's
                float3 totalOffset = float3(0, 0, 0);
                //difference
                float3 difference = float3(0, 0, 0);

                //For each celestial body
                for (int i = 0; i < _CBCount; i++)
                {
                    //Distance Vector from the mesh vertex to the celestial body
                    float3 difference = Difference(_Position[i].xyz, vertexWorldPos);
                    //Warp the mesh using the acceleration due to gravity at the vertex of all celestial bodies
                    float accel = GetAcceleration(length(difference), _Mass[i]);
                    //Clamp at the radius of the cb
                    accel = clamp(accel, 0, _MaxAcceleration[i]);
                    offset = _GridMultiplier * accel * normalize(difference);
                    if (SqrLength(offset) > SqrLength(difference))
                    {
                        offset = difference;
                    }
                    //Combine
                    totalOffset += offset;
                }
                return totalOffset;
            }

            //Vertex Shader
            interpolators vert (meshdata v)
            {
                interpolators o;

                //Warp
                if (useGPU == 1)
                {
                    float3 vertexWorldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    v.vertex.xyz += WarpGrid(vertexWorldPos);
                }
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            //Fragment Shader
            fixed4 frag (interpolators i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col.xyz += 0.5;
                return col;
            }
            ENDCG
        }
    }
}
