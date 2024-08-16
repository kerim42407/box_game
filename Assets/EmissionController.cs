using UnityEngine;

public class EmissionController : MonoBehaviour
{
    public Material material;         // Değiştirmek istediğin materyal
    public Color emissionColor = Color.white; // Emission rengi
    public float pulseSpeed = 1f;     // Yanıp sönme hızı
    public float duration = 5f;       // Yanıp sönme süresi

    private bool isPulsing = false;   // Yanıp sönme aktif mi?
    private float timer = 0f;         // Zamanlayıcı
    private Color initialEmissionColor;

    void Start()
    {
        // Materyalin başlangıç emisyon rengini kaydet
        material = GetComponent<MeshRenderer>().materials[2];
        initialEmissionColor = material.GetColor("_EmissionColor");

        // Emission'u aktif et ve yanıp sönmeye başla
        ActivateEmission();
    }

    void Update()
    {
        if (isPulsing)
        {
            // Yanıp sönme işlemi
            timer += Time.deltaTime;
            float emission = Mathf.PingPong(Time.time * pulseSpeed, 1.0f);
            Color finalColor = emissionColor * Mathf.LinearToGammaSpace(emission);
            material.SetColor("_EmissionColor", finalColor);

            // Süre dolduysa yanıp sönmeyi durdur ve emisyonu devre dışı bırak
            if (timer >= duration)
            {
                DeactivateEmission();
            }
        }
    }

    public void ActivateEmission()
    {
        // Emission'u aktif et
        material.EnableKeyword("_EMISSION");
        isPulsing = true;
        timer = 0f;
    }

    public void DeactivateEmission()
    {
        // Emission'u deaktif et ve eski haline döndür
        isPulsing = false;
        material.SetColor("_EmissionColor", initialEmissionColor);
        material.DisableKeyword("_EMISSION");
    }
}