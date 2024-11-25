Shader "PostPro/SSAO_URP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
            TEXTURE2D (_MainTex);       SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            TEXTURE2D(_CameraDepthTexture);       SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_CameraNormalsTexture);       SAMPLER(sampler_CameraNormalsTexture);
            TEXTURE2D(_CameraDepthNormalTexture);       SAMPLER(sampler_CameraDepthNormalTexture);
            CBUFFER_END
            

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = v.uv;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(v.vertex.xyz);
                o.positionCS = positionInputs.positionCS;
                // o.positionWS = positionInputs.positionWS;
                
                // o.screenPosition = ComputeScreenPos(o.positionCS);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_CameraNormalsTexture,sampler_CameraNormalsTexture, i.uv);
                
                return col;
            }
            ENDHLSL
        }
    }
}
