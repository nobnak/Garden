Shader "Unlit/ScreenNoiseMap" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader {
        Cull Off
        ColorMask RGB

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma multi_compile OUTPUT_NORMAL OUTPUT_HEIGHT
			
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
            float4 _MainTex_TexelSize;
			
			v2f vert (appdata v) {
                float2 uvFromBottom = v.uv;

				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = uvFromBottom;
				return o;
			}
			
			float4 frag (v2f i) : SV_Target {
				float4 csrc = tex2D(_MainTex, i.uv);
                float4 cdst = 0.0;
                #ifdef OUTPUT_HEIGHT
                cdst = csrc.a;
                #else
                cdst = float4(csrc.rgb, 1.0);
                #endif
				return cdst;
			}
			ENDCG
		}
	}
}
