using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(EdgeDetectionSobelRenderer), PostProcessEvent.AfterStack, "Custom/EdgeDetectionSobel")]
public sealed class EdgeDetectionSobel : PostProcessEffectSettings
{
    [Range(0.05f, 5.0f)]
    public FloatParameter edgeWidth = new FloatParameter { value = 0.3f };

    [ColorUsage(true, true)]
    public ColorParameter edgeColor = new ColorParameter { value = new Color(0.0f, 0.0f, 0.0f, 1) };
}

public sealed class EdgeDetectionSobelRenderer : PostProcessEffectRenderer<EdgeDetectionSobel>
{
    private Shader shader;
    public override void Init()
    {
        shader = Shader.Find("Hidden/Custom/EdgeDetectionSobel");
    }

    static class ShaderIDs
    {
        internal static readonly int Params = Shader.PropertyToID("_Params");
        internal static readonly int EdgeColor = Shader.PropertyToID("_EdgeColor");
    }

    public override void Render(PostProcessRenderContext context)
    {
        PropertySheet sheet = context.propertySheets.Get(shader);
        sheet.properties.SetVector(ShaderIDs.Params, new Vector2(settings.edgeWidth, 0));
        sheet.properties.SetColor(ShaderIDs.EdgeColor, settings.edgeColor);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}