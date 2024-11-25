Shader "PostPro/Kuwahara_URP"
{
    Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BlurSize ("Blur Size", Float) = 1.0
	}
	SubShader {
		CGINCLUDE
		
		#include "UnityCG.cginc"
		
		sampler2D _MainTex;  
		half4 _MainTex_TexelSize;
		float _BlurSize;
		int _Iterations;
		  
		struct v2f {
			float4 pos : SV_POSITION;
			half2 uv: TEXCOORD0;
		};
		  

		// float CalcDistance( half3 c0,  half3 c1) {
		// 	half3 sub = c0 - c1;
		// 	 return dot(sub, sub);
		// }

		// Kuwahara
		half3 CalcKuwahara(in half2 uv,int half_width) {
    	// float2 src_size = iResolution.xy;
    	// float2 inv_src_size = 1.0f / src_size;
    	// float2 uv = fragCoord * inv_src_size;
    	
    	float n = float((half_width + 1) * (half_width + 1));
    	float inv_n = 1.0f / n;
    	
    	float3 col = float3(0, 0, 0);
    	
    	float sigma2 = 0.0f;
    	float min_sigma = 100.0f;
    	
    	float3 m = float3(0, 0, 0);
    	float3 s = float3(0, 0, 0);
    	
    	
    	for (int j = -half_width; j <= 0; ++j) {
    	    for (int i = -half_width; i <= 0; ++i) {
    	        float3 c = tex2D(_MainTex, uv + float2(i, j) * _MainTex_TexelSize.xy).rgb;
    	        m += c;
    	        s += c * c;
    	    }
    	}
    	
    	m *= inv_n;
    	s = abs(s * inv_n - m * m);
    	
    	sigma2 = s.x + s.y + s.z;
    	if (sigma2 < min_sigma) {
    	    min_sigma = sigma2;
    	    col = m;
    	}
    	
    	m = float3(0, 0, 0);
    	s = float3(0, 0, 0);
    	
    	for (int j = -half_width; j <= 0; ++j) {
    	    for (int i = 0; i <= half_width; ++i) {
    	        float3 c = tex2D(_MainTex, uv + float2(i, j) * _MainTex_TexelSize.xy).rgb;
    	        m += c;
    	        s += c * c;
    	    }
    	}
    	
    	m *= inv_n;
    	s = abs(s * inv_n - m * m);
    	
    	sigma2 = s.x + s.y + s.z;
    	if (sigma2 < min_sigma) {
    	    min_sigma = sigma2;
    	    col = m;
    	}
    	
    	m = float3(0, 0, 0);
    	s = float3(0, 0, 0);
    	
    	for (int j = 0; j <= half_width; ++j) {
    	    for (int i = 0; i <= half_width; ++i) {
    	        float3 c = tex2D(_MainTex, uv + float2(i, j) * _MainTex_TexelSize.xy).rgb;
    	        m += c;
    	        s += c * c;
    	    }
    	}
    	
    	m *= inv_n;
    	s = abs(s * inv_n - m * m);
    	
    	sigma2 = s.x + s.y + s.z;
    	if (sigma2 < min_sigma) {
    	    min_sigma = sigma2;
    	    col = m;
    	}
    	
    	m = float3(0, 0, 0);
    	s = float3(0, 0, 0);
    	
    	for (int j = 0; j <= half_width; ++j) {
    	    for (int i = -half_width; i <= 0; ++i) {
    	        float3 c = tex2D(_MainTex, uv + float2(i, j) * _MainTex_TexelSize.xy).rgb;
    	        m += c;
    	        s += c * c;
    	    }
    	}
    	
    	m *= inv_n;
    	s = abs(s * inv_n - m * m);
    	
    	sigma2 = s.x + s.y + s.z;
    	if (sigma2 < min_sigma) {
    	    min_sigma = sigma2;
    	    col = m;
    	}
    	
    	return col;
}
		

		v2f vertBlurVertical(appdata_img v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			
			half2 uv = v.texcoord;
			
			o.uv = uv;
			
					 
			return o;
		}
		

		
		fixed4 fragBlur(v2f i) : SV_Target {
			
			return fixed4(CalcKuwahara(i.uv,_Iterations), 1.0);
		}
		    
		ENDCG
		
		ZTest Always 
		Cull Off 
		ZWrite Off
		
		Pass {
			NAME "Kuwahara"
			
			CGPROGRAM
			  
			#pragma vertex vertBlurVertical  
			#pragma fragment fragBlur
			  
			ENDCG  
		}
		

	} 
	FallBack "Diffuse"
}
