Shader "Hbh Shader/View Outline Shading" 
{
    Properties 
    {
        _Outline ("Outline", Range(0, 1)) = 0.1
        _Color ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
    }
    SubShader 
    {
        Pass 
        {
            Cull Back

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float _Outline;
            float4  _Color;
            float4  _OutlineColor;

            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
            };

            v2f vert (appdata_base v)
            {       
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                float3 ObjViewDir = normalize(ObjSpaceViewDir(v.vertex));
                float3 normal = normalize(v.normal);
                float factor = step(_Outline, dot(normal, ObjViewDir));
                o.color = _Color * (1 - factor) + _OutlineColor * factor;
                return o;
            }

            float4 frag(v2f i) : SV_Target 
            { 
                return i.color;
            }

            ENDCG
        }
    }
}