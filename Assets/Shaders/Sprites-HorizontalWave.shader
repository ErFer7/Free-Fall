// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// Customizado

Shader "Sprites/HorizontalWave"
{
	Properties{
		_Color("Tint", Color) = (0, 0, 0, 1)
		_MainTex("Texture", 2D) = "white" {}
		_Frequency("Frequency", Float) = 1
		_Amplitude("Amplitude", Float) = 1
	}

	SubShader{
		Tags{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}

		Blend SrcAlpha OneMinusSrcAlpha

		ZWrite off
		Cull off

		Pass{

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float4 _MainTex_ST;

			fixed4 _Color;

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f {
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			float _Frequency;
			float _Amplitude;

			v2f vert(appdata v) {
				v2f o;
				v.vertex.x += sin(v.vertex.y + _Time.y * _Frequency) * _Amplitude;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_TARGET{
				fixed4 col = tex2D(_MainTex, i.uv);
				col *= _Color;
				col *= i.color;
				return col;
			}

			ENDCG
		}
	}
}
