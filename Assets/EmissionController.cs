using UnityEngine;

public class EmissionController : MonoBehaviour
{
    #region Variables
    [HideInInspector] public Material material;
    [HideInInspector] public Color initialEmissionColor;

    public Color emissionColor = Color.white;
    public float pulseSpeed = 1f;

    private bool isPulsing = false;
    #endregion

    void Start()
    {
    }

    void Update()
    {
        if (isPulsing)
        {
            float emission = Mathf.PingPong(Time.time * pulseSpeed, 1.0f);
            Color finalColor = emissionColor * Mathf.LinearToGammaSpace(emission);
            material.SetColor("_EmissionColor", finalColor);
        }
    }

    public void ActivateEmission()
    {
        material.EnableKeyword("_EMISSION");
        isPulsing = true;
    }

    public void DeactivateEmission()
    {
        isPulsing = false;
        material.SetColor("_EmissionColor", initialEmissionColor);
        material.DisableKeyword("_EMISSION");
    }
}