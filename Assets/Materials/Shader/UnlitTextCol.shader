Shader "Unlit/UnlitTextCol"
{
	Properties
	{
		_Color("Color", Color) = (0,0,0,0)
		_MainTex ("Texture", 2D) = "white" {}
		_BorderSize("Border Size", float) = 0
		_BorderColor("Border Color", Color) = (0,0,0,0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			float4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _BorderSize;
			float4 _BorderColor;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			float4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float4 col = tex2D(_MainTex, i.uv) * _Color;
				


				// compute the border color
				float2 uv_ST = i.uv / _MainTex_ST.xy;
				float2 normalizedBorderSize = float2(_BorderSize / _MainTex_ST.x, _BorderSize / _MainTex_ST.y);
				if (uv_ST.x < normalizedBorderSize.x || uv_ST.x > 1.0f - normalizedBorderSize.x
			     || uv_ST.y < normalizedBorderSize.y || uv_ST.y > 1.0f - normalizedBorderSize.y) {
					col = _BorderColor;
				}
				else {
					//Clip transparent pixels
					clip(col.a - 0.5f);
				}
					

				return col;
			}
			ENDCG
		}
	}
}
