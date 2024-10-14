using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class RendererPass : ScriptableRenderPass
{
    private RTHandle _renderTexture;

    private List<ShaderTagId> _shaderTags = new List<ShaderTagId>() { new ShaderTagId("UniversalForward") };
    private FilteringSettings _filteringSettings;


    private RenderTargetIdentifier source;
    private Material _material;


    public RendererPass(RTHandle renderTexture, int mask, Material material)
    {
        _renderTexture = renderTexture;

        this._material = material;

        _filteringSettings = new FilteringSettings(RenderQueueRange.opaque, mask);
        _material = material;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        cmd.GetTemporaryRT(0, cameraTextureDescriptor);
        ConfigureTarget(_renderTexture);
        ConfigureClear(ClearFlag.All, Color.clear);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {

        var cmd = CommandBufferPool.Get("pass");
        var criteria = renderingData.cameraData.defaultOpaqueSortFlags;
        var draw = CreateDrawingSettings(_shaderTags, ref renderingData, criteria);
        draw.overrideMaterial = _material;
        RendererListParams param = new RendererListParams(renderingData.cullResults, draw, _filteringSettings);
        RendererList list = context.CreateRendererList(ref param);
        cmd.DrawRendererList(list);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}
