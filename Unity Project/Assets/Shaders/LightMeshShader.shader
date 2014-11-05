Shader "2D Lighting/LightMeshShader" {
	Properties {
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		#pragma surface surf Lambert


		struct Input {
			float4 color : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = IN.color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
