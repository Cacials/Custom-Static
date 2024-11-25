using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System;
[Serializable]
[PostProcess(typeof(ColorTintRenderer), PostProcessEvent.AfterStack, "Unity/ColorTint")]
public class ColorTint : PostProcessEffectSettings
{
    [Tooltip("ColorTint")]
    public ColorParameter color = new ColorParameter { value = new Color(1f, 1f, 1f, 1f) };

    [Range(0f, 1f), Tooltip("ColorTint intensity")]
    public FloatParameter blend = new FloatParameter { value = 0.5f }; 
}

public sealed class ColorTintRenderer : PostProcessEffectRenderer<ColorTint>
{
    public override void Render(PostProcessRenderContext context)
    {
        var cmd = context.command;
        cmd.BeginSample("ScreenColorTint");
        
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/PostProcessing/ColorTint"));
        sheet.properties.SetColor("_Color", settings.color);
        sheet.properties.SetFloat("_BlendMultiply", settings.blend);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        
        cmd.EndSample("ScreenColorTint");
    }
}