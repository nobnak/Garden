Shader "Unlit/ColorTextureTime" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _AnimTex_T ("Time", Float) = 0
        _Lifetime ("Lifetime", Float) = 10
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Cull Off

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
            float4 _Color;
            float _AnimTex_T;
            float _Lifetime;
			
			v2f vert (appdata v) {
				v2f o;
                v.vertex.xyz *= smoothstep(0.0, 0.1 * _Lifetime, _AnimTex_T) 
                    * smoothstep(_Lifetime, 0.9 * _Lifetime, _AnimTex_T);
				o.vertex = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz, 1));
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				fixed4 csrc = tex2D(_MainTex, i.uv);
				return csrc * _Color;
			}
			ENDCG
		}
	}
}
