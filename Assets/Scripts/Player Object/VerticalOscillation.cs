using UnityEngine;

public class VerticalOscillation : MonoBehaviour
{
    public float amplitude = 0.3f; // Salınımın genliği (Yukarı aşağı ne kadar hareket edecek)
    public float frequency = 2f; // Salınımın frekansı (Salınımın hızı)

    private Vector3 startPosition;

    void Start()
    {
        // Objenin başlangıç pozisyonunu kaydedin
        startPosition = transform.position;
    }

    void Update()
    {
        // Y ekseninde sinüs dalgası boyunca hareket et
        float newY = startPosition.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
