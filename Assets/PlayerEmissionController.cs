using System.Collections.Generic;
using UnityEngine;

public class PlayerEmissionController : MonoBehaviour
{
    #region Variables
    public List<Material> materials;
    [HideInInspector] public Color initialEmissionColor;

    public Color emissionColor = Color.white;
    public float pulseSpeed = 1f;

    private bool isPulsing = false;
    #endregion

    void Update()
    {
        if (isPulsing)
        {
            float emission = Mathf.PingPong(Time.time * pulseSpeed, .25f);
            Color finalColor = emissionColor * Mathf.LinearToGammaSpace(emission);
            foreach (Material mat in materials)
            {
                mat.SetColor("_EmissionColor", finalColor);
            }
        }
    }

    public void ActivateEmission()
    {
        foreach (Material mat in materials)
        {
            mat.EnableKeyword("_EMISSION");
        }
        
        isPulsing = true;
    }

    public void DeactivateEmission()
    {
        isPulsing = false;
        foreach (Material mat in materials)
        {
            mat.SetColor("_EmissionColor", initialEmissionColor);
            mat.DisableKeyword("_EMISSION");
        }
    }
}