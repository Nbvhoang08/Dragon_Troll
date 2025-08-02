Shader "UI/CircularHoleExpand"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Radius ("Hole Radius", Range(0,2)) = 0
        _Center ("Circle Center UV", Vector) = (0.5,0.5,0,0)
        _Aspect ("UV Aspect Scale", Vector) = (1,1,0,0)
        _Edge ("Edge Feather", Range(0,0.5)) = 0.05
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Stencil
        {
            Ref 0
            Comp Always
            Pass Keep
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                half2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Radius;
            float4 _Center; 
            float4 _Aspect; 
            float _Edge;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord;

                // Scale theo aspect để giữ vòng tròn
                float2 offset = uv - _Center.xy;
                offset *= _Aspect.xy; 
                float dist = length(offset);

                fixed4 col = tex2D(_MainTex, uv) * i.color;

                // Alpha = 0 trong vòng tròn, viền mờ
                float hole = smoothstep(_Radius - _Edge, _Radius, dist); 
                // dist < _Radius - _Edge => 0 (trong suốt)
                // dist > _Radius => 1 (giữ màu)
                col.a *= hole;

                return col;
            }
            ENDCG
        }
    }
}
