Shader "PostPro/Kawase_URP"
{
    Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BlurSize ("Blur Size", Float) = 1.0
	}
	SubShader {
		HLSLINCLUDE
		
		// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
		
		sampler2D _MainTex;
		// TEXTURE2D(_MainTex);       SAMPLER(sampler_MainTex);
		half4 _MainTex_TexelSize;
		float _BlurSize;

		struct a2v
		{
			float4 vertex:POSITION;
			float2 texcoord:TEXCOORD0;
		};
		  
		struct v2f {
			float4 pos : SV_POSITION;
			half2 uv: TEXCOORD0;
		};
		  
		v2f vert(a2v v) {
			v2f o;
			o.pos = TransformObjectToHClip(v.vertex);
			
			half2 uv = v.texcoord;
			
			o.uv = uv;
			//o.uv[1] = uv + float2(0.0, _MainTex_TexelSize.y * 1.0) * _BlurSize;
			//o.uv[2] = uv - float2(0.0, _MainTex_TexelSize.y * 1.0) * _BlurSize;
			//o.uv[3] = uv + float2(0.0, _MainTex_TexelSize.y * 2.0) * _BlurSize;
			//o.uv[4] = uv - float2(0.0, _MainTex_TexelSize.y * 2.0) * _BlurSize;
					 
			return o;
		}
		
		
		half4 KawaseBlur( sampler2D tex,float2 uv, float2 texelSize, half pixelOffset)
		{
			half4 o = 0;
			// o += SAMPLE_TEXTURE2D(tex, sampler_MainTex, uv + float2(pixelOffset +0.5, pixelOffset +0.5) * texelSize); 
			o += tex2D(tex, uv + float2(pixelOffset +0.5, pixelOffset +0.5) * texelSize); 
			o += tex2D(tex, uv + float2(-pixelOffset -0.5, pixelOffset +0.5) * texelSize); 
			o += tex2D(tex, uv + float2(-pixelOffset -0.5, -pixelOffset -0.5) * texelSize); 
			o += tex2D(tex, uv + float2(pixelOffset +0.5, -pixelOffset -0.5) * texelSize); 
			return o * 0.25;
		}
		

		float4 fragBlur(v2f i) : SV_Target {
			return KawaseBlur(_MainTex, i.uv, _MainTex_TexelSize.xy, _BlurSize);

			// half4 o = 0;
			// o += _MainTex.Load(float3(i.pos.xy+ float2(_BlurSize +0.5, _BlurSize +0.5),0));
			// o += _MainTex.Load(float3(i.pos.xy+ float2(-_BlurSize -0.5, _BlurSize +0.5),0));
			// o += _MainTex.Load(float3(i.pos.xy+ float2(-_BlurSize -0.5, -_BlurSize -0.5),0));
			// o += _MainTex.Load(float3(i.pos.xy+ float2(_BlurSize +0.5, -_BlurSize -0.5),0));
			// return o * 0.25;
		}
		    
		ENDHLSL
		
		ZTest Always Cull Off ZWrite Off
		
		Pass {
			NAME "KawaseBlur"
			
			HLSLPROGRAM
			  
			#pragma vertex vert  
			#pragma fragment fragBlur
			  
			ENDHLSL  
		}
		

	} 
	FallBack "Diffuse"
}
