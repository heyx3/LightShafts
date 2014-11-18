Shader "2D Lighting/LightMeshShader"
{
	Properties
	{
		_ZPos("Z Position", Float) = -1.0
		_Intensity("Intensity", Range(0.0, 1.0)) = 1.0
	}
	SubShader
	{
		Pass
		{
			Blend One One

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag


			uniform float _ZPos;
			uniform float _Intensity;


			struct vertIn
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct vertOut
			{
				float4 pos : POSITION;
				float4 col: COLOR;
			};

			vertOut vert(vertIn v) : POSITION
			{
				vertOut o;
				o.pos = mul(UNITY_MATRIX_MVP, float4(v.vertex.x, v.vertex.y, _ZPos, v.vertex.w));
				o.col = float4(v.color.rgb * _Intensity, v.color.a);
				return o;
			}
			fixed4 frag(vertOut i) : COLOR
			{
				return i.col;
			}


			ENDCG
		}
	}
	FallBack "Diffuse"
}
