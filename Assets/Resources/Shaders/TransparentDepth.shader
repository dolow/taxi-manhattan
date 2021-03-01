Shader "User/TransparentDepth"
{
    Properties {
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
        _TransparentMinDistance("Transparent min distance", float) = 1
        _TransparentMaxDistance("Transparent max distance", float) = 2
    }

    SubShader {
        Tags{ "Queue" = "Transparent" "IgnoreProjector" = "False" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass {
            CGPROGRAM
        
            #pragma vertex vert
            #pragma fragment frag
        
            sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform fixed4 _Color;
            uniform float _TransparentMinDistance;
            uniform float _TransparentMaxDistance;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : WORLD_POS;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v) {
                v2f o;
                o.uv = float2(v.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            float4 frag(v2f i) : COLOR {
                float range = _TransparentMaxDistance - _TransparentMinDistance;
                float diff = length(_WorldSpaceCameraPos - i.worldPos) - _TransparentMinDistance;
                
                float4 mainTex  = tex2D(_MainTex, i.uv);
                mainTex.a = min(1.0f, diff / range);
                return mainTex;
            }

            ENDCG
        }
    }
 
    Fallback "Mobile/Diffuse"
}
