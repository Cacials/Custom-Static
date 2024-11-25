using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GetRTex : ScriptableRendererFeature
{
    class GetRTexRenderPass : ScriptableRenderPass
    {

        //RT��Filter
        public FilterMode filterMode { get; set; }

        //��ǰ�׶���Ⱦ����ɫRT
        RenderTargetIdentifier m_Source;

        //����RT
        RenderTargetHandle m_TemporaryColorTexture;

        int renderTextureID;

        //Profiling����ʾ
        ProfilingSampler m_ProfilingSampler = new ProfilingSampler("GetDistortionTargetRT");
        public GetRTexRenderPass(RenderPassEvent renderEvent)
        {
            //���ĸ��׶β�����Ⱦ
            //renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
            //renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
            //renderPassEvent = RenderPassEvent.AfterRendering;
            renderPassEvent = renderEvent;

            //��ʼ������RT����
            m_TemporaryColorTexture.Init("TemRT");
        }


        public void Setup(RenderTargetIdentifier source)
        {
            //�汾���º�renderer.cameraColorTargetHandle��ص���Ҫ�ŵ�ScriptableRenderPass�ڲ���������ᱨ��
            // m_Source = source;
        }

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //��Setup���ƹ�����
            var renderer = renderingData.cameraData.renderer;
            m_Source = renderer.cameraColorTargetHandle;
            
            RTRequest(cmd, ref renderingData);
            
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            
            CommandBuffer cmd = CommandBufferPool.Get();
            //using���������ǿ�����FrameDebug�Ͽ��������������Ⱦ
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                //����һ��RT
                cmd.ClearRenderTarget(true, true, Color.blue,1);
                //RTPost(cmd, ref renderingData);
                RTPost1(cmd, ref renderingData);
            }
            //ִ��
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            //����
            CommandBufferPool.Release(cmd);

        }

        void RTPost(CommandBuffer cmd,ref RenderingData renderingData)
        {
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;
            cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, filterMode);

            //cmd.SetGlobalTexture("_RTex", m_Source);


            //����ǰ֡����ɫRT���Լ�����ɫ���ִ���Ȼ���������������ͼ��
            cmd.SetGlobalTexture("_RTex", m_TemporaryColorTexture.id);
            //Blit(cmd, m_Source, m_TemporaryColorTexture.Identifier(), m_Mat);
            Blit(cmd, m_Source, m_TemporaryColorTexture.Identifier());
            ConfigureTarget(renderTextureID);
            //��������RT������Ⱦ����ǰ֡����ɫRT��
            //Blit(cmd, m_TemporaryColorTexture.Identifier(), m_Source);
        }

        void RTPost1(CommandBuffer cmd, ref RenderingData renderingData)
        {

            //cmd.SetGlobalTexture("_RTex", m_Source);
            Blit(cmd, m_Source, renderTextureID);
        }
        void RTRequest(CommandBuffer cmd,ref RenderingData renderingData)
        {
            //RT��Ϣ
            RenderTextureDescriptor cameraDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            renderTextureID = Shader.PropertyToID("_RTex");
            cmd.GetTemporaryRT(renderTextureID, cameraDescriptor, filterMode);

            //�������pass��m_ColorAttachments��m_DepthAttachment
            ConfigureTarget(renderTextureID);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
            cmd.ReleaseTemporaryRT(renderTextureID);
        }
    }

    GetRTexRenderPass m_ScriptablePass;

    public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new GetRTexRenderPass(Event);

        // Configures where the render pass should be injected.
        //m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // m_ScriptablePass.Setup(renderer.cameraColorTargetHandle);
        renderer.EnqueuePass(m_ScriptablePass);
        m_ScriptablePass.ConfigureInput(ScriptableRenderPassInput.Color);
    }
}


