Shader "PostPro/SNN_URP"
{
    Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
//		_BlurSize ("Blur Size", Float) = 1.0
	}
	SubShader {
		HLSLINCLUDE
		
		// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
		
		sampler2D _MainTex;  
		half4 _MainTex_TexelSize;
		float _BlurSize;
		int _Iterations;


		struct a2v
		{
			float4 vertex:POSITION;
			float2 texcoord:TEXCOORD0;
		};

		
		struct v2f {
			float4 pos : SV_POSITION;
			half2 uv: TEXCOORD0;
		};
		  

		float CalcDistance( half3 c0,  half3 c1) {
			half3 sub = c0 - c1;
			 return dot(sub, sub);
		}

		// Symmetric Nearest Neighbor
		half3 CalcSNN(in half2 uv,int half_width) {
			//half2 src_size = iResolution.xy;
		    //half2 inv_src_size = 1.0f / src_size;
		    //half2 uv = fragCoord * inv_src_size;
		    
		    half3 c0 = tex2D(_MainTex, uv).rgb;
		    
		    half4 sum = half4(0.0f, 0.0f, 0.0f, 0.0f);
		    
		    for (int i = 0; i <= half_width; ++i) {
		        half3 c1 = tex2D(_MainTex, uv + half2(+i, 0) * _MainTex_TexelSize.xy).rgb;
		        half3 c2 = tex2D(_MainTex, uv + half2(-i, 0) * _MainTex_TexelSize.xy).rgb;
		        
		        float d1 = CalcDistance(c1, c0);
		        float d2 = CalcDistance(c2, c0);
		        if (d1 < d2) {
		            sum.rgb += c1;
		        } else {
		            sum.rgb += c2;
		        }
		        sum.a += 1.0f;
		    }
		 	for (int j = 1; j <= half_width; ++j) {
		    	for (int i = -half_width; i <= half_width; ++i) {
		            half3 c1 = tex2D(_MainTex, uv + half2(+i, +j) * _MainTex_TexelSize.xy).rgb;
		            half3 c2 = tex2D(_MainTex, uv + half2(-i, -j) * _MainTex_TexelSize.xy).rgb;
		            
		            float d1 = CalcDistance(c1, c0);
		            float d2 = CalcDistance(c2, c0);
		            if (d1 < d2) {
		            	sum.rgb += c1;
		            } else {
		                sum.rgb += c2;
		            }
		            sum.a += 1.0f;
				}
		    }
		    return sum.rgb / sum.a;
		}

		
		

		v2f vertBlurVertical(a2v v) {
			v2f o;
			o.pos = TransformObjectToHClip(v.vertex);
			
			half2 uv = v.texcoord;
			
			o.uv = uv;
			
					 
			return o;
		}
		
		
		float4 fragBlur(v2f i) : SV_Target {
			
			return float4(CalcSNN(i.uv,_Iterations), 1.0);
		}
		    
		ENDHLSL
		
		ZTest Always 
		Cull Off 
		ZWrite Off
		
		Pass {
			NAME "SNN"
			
			HLSLPROGRAM
			  
			#pragma vertex vertBlurVertical  
			#pragma fragment fragBlur
			  
			ENDHLSL  
		}
		

	} 

}
