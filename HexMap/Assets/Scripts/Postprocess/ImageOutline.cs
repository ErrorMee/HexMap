using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[System.Serializable]
[PostProcess(typeof(ImageOutlineRenderer), PostProcessEvent.AfterStack, "Custom/ImageOutline")]
public class ImageOutline : PostProcessEffectSettings
{
    [Range(0f, 1f), Tooltip("ImageOutline effect Threshold.")]
    public FloatParameter Threshold = new FloatParameter { value = 0.5f };
    [Tooltip("ImageOutline effect EdgeColor.")]
    public ColorParameter EdgeColor = new ColorParameter { value = new Color(0.0f, 0.0f, 0.0f, 1) };
}

public sealed class ImageOutlineRenderer : PostProcessEffectRenderer<ImageOutline>
{
    private Shader shader;
    static readonly int ThresholdProper = Shader.PropertyToID("_Threshold");
    static readonly int EdgeColorProper = Shader.PropertyToID("_EdgeColor");
    public override void Init()
    {
        shader = Shader.Find("Hidden/Custom/ImageOutline");
    }

    public override void Render(PostProcessRenderContext context)
    {
        PropertySheet sheet = context.propertySheets.Get(shader);
        sheet.properties.SetFloat(ThresholdProper, settings.Threshold);
        sheet.properties.SetColor(EdgeColorProper, settings.EdgeColor);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}