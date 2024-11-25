using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.TestTools;

public class PostProcessRenderPassFeature : ScriptableRendererFeature
{
    class PostProcessRenderPass : ScriptableRenderPass
    {
        //RT的Filter
        public FilterMode filterMode { get =>FilterMode.Bilinear; }

        //当前阶段渲染的颜色RT
        RenderTargetIdentifier m_Source;
        
        //辅助RT
        RenderTargetHandle m_TemporaryColorTexture;

        int renderTextureID;

        private SNN_Volume m_volumeComponent;
        private int m_DownSample;
        private int m_Iterations;

        private Material m_Mat;
        
        public PostProcessRenderPass(RenderPassEvent renderEvent,int dowmSample,int iterations,Shader shader)
        {
            //在哪个阶段插入渲染
            renderPassEvent = renderEvent;
            m_DownSample = dowmSample;
            m_Iterations = iterations;
            if (shader != null)
            {
                m_Mat=CoreUtils.CreateEngineMaterial(shader);
            }
            else
            {
                Debug.LogError("未指定shader");
                return;
            }
            
            
        }

        public void Setup(RenderTargetIdentifier source)
        {
            //版本更新后renderer.cameraColorTargetHandle相关的需要放到ScriptableRenderPass内部处理，否则会报错
            // m_Source = source;
        }
        
        
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var stack = VolumeManager.instance.stack;      
            
            m_volumeComponent=stack.GetComponent<SNN_Volume>();
            // if (m_volumeComponent.use_PostPro.value)
            // {
            //     m_DownSample = m_volumeComponent.downSamples.value;
            //     m_Iterations = m_volumeComponent.iterations.value;
            // }
            // Debug.LogError(m_volumeComponent.active);
            
            //从Setup中移过来的
            var renderer = renderingData.cameraData.renderer;
            m_Source = renderer.cameraColorTargetHandle;
            
            RTRequest(cmd, ref renderingData);
        }

        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_Mat == null)
            {
                Debug.LogError("Material未初始化");
                return;
            }

            if (!m_volumeComponent.use_PostPro.value)
            {
                return;
            }
            
            CommandBuffer cBuffer = CommandBufferPool.Get();
            cBuffer.BeginSample("PostPro");
            cBuffer.ClearRenderTarget(true, true, Color.cyan,1);
            
            Render(cBuffer, ref renderingData);
            
            cBuffer.EndSample("PostPro");
            
            context.ExecuteCommandBuffer(cBuffer);
            cBuffer.Clear();
            
            CommandBufferPool.Release(cBuffer);
        }
        
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
            cmd.ReleaseTemporaryRT(renderTextureID);
        }

        void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {

            m_Mat.SetInt("_Iterations", m_Iterations);
            //cmd.SetGlobalTexture("_RTex", m_Source);
            // Blit(cmd, m_Source, renderTextureID);
            // Blit(cmd, renderTextureID, m_Source,m_Mat);
            // cmd.SetGlobalTexture(renderTextureID, m_Source);  
            
            cmd.Blit(m_Source, renderTextureID);
            cmd.Blit(renderTextureID, m_Source,m_Mat);
        }
        void RTRequest(CommandBuffer cmd,ref RenderingData renderingData)
        {
            //获得一个RT
            // var cameraData = renderingData.cameraData;
            // var camera = cameraData.camera;
            // int rtW = camera.scaledPixelWidth / m_DownSample;
            // int rtH = camera.scaledPixelHeight/ m_DownSample;
            //cmd.GetTemporaryRT(renderTextureID, rtW,rtH, 0,FilterMode.Trilinear, RenderTextureFormat.Default);
            
            //获得一个RT
            RenderTextureDescriptor cameraDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            cameraDescriptor.width /= m_DownSample;
            cameraDescriptor.height /= m_DownSample;
            
            renderTextureID = Shader.PropertyToID("_DownSampleTex");
            cmd.GetTemporaryRT(renderTextureID, cameraDescriptor, filterMode);

            //设置这个pass的m_ColorAttachments和m_DepthAttachment
            ConfigureTarget(renderTextureID);
        }
        
    }

    public RenderPassEvent renderEvent;

    public Shader shader;
    
    [Range(1, 6)]
    public int iterations;

    [Range(1, 8)]
    public int downSample;

    PostProcessRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new PostProcessRenderPass(renderEvent,downSample,iterations,shader);
        
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // m_ScriptablePass.Setup(renderer.cameraColorTargetHandle);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


