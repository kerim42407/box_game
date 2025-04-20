using System.Collections;
using UnityEngine;

public class EmissionController : MonoBehaviour
{
    #region Variables
    public Material material;
    [HideInInspector] public Color initialEmissionColor;

    public Color emissionColor = Color.white;
    public float pulseSpeed = 1f;

    private bool isPulsing = false;
    private bool isPositive = false;
    private bool isNegative = false;
    #endregion

    void Update()
    {
        if (isPulsing)
        {
            float emission = Mathf.PingPong(Time.time * pulseSpeed, 1.0f);
            Color finalColor = emissionColor * Mathf.LinearToGammaSpace(emission);
            material.SetColor("_EmissionColor", finalColor);
        }

        if (isPositive)
        {
            float emission = Mathf.PingPong(Time.time * pulseSpeed, 1.0f);
            Color finalColor = Color.green * Mathf.LinearToGammaSpace(emission);
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

    public IEnumerator ChangeEmission(bool isPositive)
    {
        Color targetColor;
        if (isPositive)
        {
            targetColor = Color.green;
        }
        else
        {
            targetColor = Color.red;
        }
        // Yavaş yavaş yeşil yap
        yield return LerpEmission(initialEmissionColor, targetColor, 1);

        // 3 saniye bekle
        yield return new WaitForSeconds(1);

        // Yavaş yavaş eski rengine dön
        yield return LerpEmission(targetColor, Color.black, 1);
    }

    private IEnumerator LerpEmission(Color fromColor, Color toColor, float duration)
    {
        Debug.Log("zz");
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Zamanı ilerlet
            elapsedTime += Time.deltaTime;

            // Emission rengini ara değer ile ayarla
            Color currentColor = Color.Lerp(fromColor, toColor, elapsedTime / duration);
            SetEmissionColor(currentColor);

            // Bir frame bekle
            yield return null;
        }

        // Son değeri ayarla
        SetEmissionColor(toColor);
    }

    private void SetEmissionColor(Color color)
    {
        // Emission rengini ayarla
        material.SetColor("_EmissionColor", color * 10);

        // Emission'ı aktif et
        if (color != Color.black)
        {
            material.EnableKeyword("_EMISSION");
        }
        else
        {
            material.DisableKeyword("_EMISSION");
        }
    }
}