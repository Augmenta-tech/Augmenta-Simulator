Shader "Unlit/AugmentaUnlitOnTop"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_CenterColor("Center color", Color) = (0,0,0,0)
		_BorderColor("Border color", Color) = (0,0,0,0)
		_BorderThickness("Border Thickness", Range(0.0, 1.0)) = 0.1
		_Transparency("Transparency", Range(0.0, 1.0)) = 1.0
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
		
		//Cull Off
		//ZWrite Off
		//ZTest Always     
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

			float4 _CenterColor;
			float4 _BorderColor;
			float _BorderThickness;
			float _Transparency;

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
				fixed4 col = _CenterColor;

				
				if(i.uv.x < _BorderThickness  || i.uv.x > 1 - _BorderThickness || i.uv.y < _BorderThickness || i.uv.y > 1 - _BorderThickness) {
						col = _BorderColor;
					
				}
				
				col.a = _Transparency;

				return col;
			}
			ENDCG
		}
	}
}
