using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MaterialExtensions
{
public static void ToOpaqueMode(this Material material)
    {
        material.SetOverrideTag("RenderType", "Opaque");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.SetShaderPassEnabled("ShadowCaster", true);

        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        material.SetFloat("_Surface", 0);
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
    }

    public static void ToFadeMode(this Material material)
    {
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.SetShaderPassEnabled("ShadowCaster", false);

        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        material.SetFloat("_Surface", 1); 
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }
}