Shader "Custom/FresnelOutline"
{
    Properties
    {
        _Color ("Outline Color", Color) = (0,1,1,1)
        _FresnelPower ("Fresnel Power", Range(1, 8)) = 4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Blend One One // Pure additive blending
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float _FresnelPower;
            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldViewDir : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
                o.worldViewDir = normalize(_WorldSpaceCameraPos - worldPos);
                o.pos = UnityObjectToClipPos(v.vertex); // No inflation
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float fresnel = pow(1.0 - dot(normalize(i.worldNormal), i.worldViewDir), _FresnelPower);
                return _Color * fresnel;
            }
            ENDCG
        }
    }
}
