Shader "VrCapture/BiltShader" {
	Properties {
		_MainTex ("Screen Blended", 2D) = "" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
	
	sampler2D _MainTex;
	float4 _BlitParams;
		
	v2f vert(appdata_img v) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);	
		o.uv = v.texcoord.xy;
		return o;
	} 
	half4 frag(v2f i) : COLOR {
		float2 uv = i.uv;
		float steps = _BlitParams.z;
		float pi = 3.1415926;

		float scale_u = uv.x * steps;
		float slice = floor(scale_u);		// int [0-360)
		float slice_u = scale_u - slice; 	// percent in slice [0-1)
		float scale = cos((uv.y - 0.5) * pi);
		uv.x = (slice + 0.5 + (slice_u - 0.5) * scale) / steps;

#if 1 // super sampling...
		float2 invTargetSize = float2(_BlitParams.x, _BlitParams.y);
		half3 color0 = tex2D(_MainTex, uv + float2(-0.375,  0.125) * invTargetSize).rgb;
		half3 color1 = tex2D(_MainTex, uv + float2(-0.125, -0.375) * invTargetSize).rgb;
		half3 color2 = tex2D(_MainTex, uv + float2(0.375, -0.125) * invTargetSize).rgb;
		half3 color3 = tex2D(_MainTex, uv + float2(0.125,  0.375) * invTargetSize).rgb;
		half3 color = (color0 + color1 + color2 + color3) * 0.25;
		return half4(color, 1);
#else
		return half4(tex2D(_MainTex, uv).rgb, 1);
#endif
	}
	ENDCG 
	
	Subshader {
	 Pass {
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }      

		  CGPROGRAM
		  //#pragma fragmentoption ARB_precision_hint_fastest 
		  #pragma vertex vert
		  #pragma fragment frag
		  ENDCG
	  }
	}
	Fallback off
}
