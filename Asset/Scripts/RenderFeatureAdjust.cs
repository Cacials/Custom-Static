using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

public class RenderFeatureAdjust : MonoBehaviour
{
    public UniversalRendererData data;
    public Shader snn;
    public Shader kuwahara;
    public Material mat;

    [Button]
    public void SetMat()
    {
        var renderObjectSettings = (data.rendererFeatures[4] as RenderObjects).settings;
        renderObjectSettings.overrideMaterial = mat;
    }

    [Button]
    public void SetShader()
    {
        var postShader = (data.rendererFeatures[0] as PostProcessRenderPassFeature).shader;
        (data.rendererFeatures[0] as PostProcessRenderPassFeature).shader = kuwahara;
    }
}
