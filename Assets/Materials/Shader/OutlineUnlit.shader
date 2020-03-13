Shader "Unlit/OutlineUnlit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BorderSize("Border size", Range(0,0.5)) = 0.1
		_BorderColor("Border color", Color) = (0,0,0,0)
		_BackgroundColor("Background color", Color) = (0,0,0,0)
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
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float4 _BorderColor;
			float4 _BackgroundColor;
			float _BorderSize;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = _BorderColor;
				if(i.uv.x > _BorderSize && i.uv.x < 1-_BorderSize) {
					if(i.uv.y > _BorderSize && i.uv.y < 1-_BorderSize) {
						col = _BackgroundColor;
					}
				}
				
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
