Shader "2D Lighting/LightMeshShader"
{
	Properties
	{
	}
	SubShader
	{
		Pass
		{
			Blend One One

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag


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
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.col = v.color;
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
