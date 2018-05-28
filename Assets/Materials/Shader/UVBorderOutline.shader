Shader "Custom/UVBorderOutline"
{
	Properties
	{
		[Toggle] _UseTex("Use texture", Range(0,1)) = 0
		_MainTex ("Texture", 2D) = "white" {}
		_PointColor("Point color", Color) = (0,0,0,0)
		_CenterColor("Center color", Color) = (0,0,0,0)
		_CenterSize ("CenterSize", Range(0, 0.5)) = 0.1
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
     
		ZWrite Off
		ZTest Always     
		Blend SrcAlpha OneMinusSrcAlpha 
		
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

			float4 _PointColor;
			float4 _CenterColor;
			float _SquareSize;
			float _UseTex;
			float _CenterSize;

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
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
				fixed4 col = _PointColor;

				if(i.uv.x > _CenterSize && i.uv.x < 1 - _CenterSize) {
					if(i.uv.y > _CenterSize && i.uv.y < 1- _CenterSize) {
						if(_UseTex)
							col = tex2D(_MainTex, i.uv) * _CenterColor;
						else
							col = _CenterColor;
					}
				}

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
