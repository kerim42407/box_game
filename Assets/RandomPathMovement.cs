using System.Collections;
using UnityEngine;

public class RandomPathMovement : MonoBehaviour
{
    public Vector3 targetPosition;  // Hedef pozisyon
    public float moveDuration = 5f; // Hedefe ulaşmak için belirlenen süre (saniye)
    public float swayAmount = 0.5f; // Sağa sola sapma miktarı
    public float swaySpeed = 1f;    // Sapma hızı

    private Vector3 startPosition;  // Başlangıç pozisyonu

    void Start()
    {
        // Objenin başlangıç pozisyonunu (0,0,0) olarak ayarla
        startPosition = Vector3.zero;
        transform.position = startPosition;

        // Hareket etmeye başla
        StartCoroutine(MoveTowardsTargetWithDuration());
    }

    IEnumerator MoveTowardsTargetWithDuration()
    {
        float elapsedTime = 0f;  // Geçen süreyi takip eden değişken

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;  // Geçen zamanı güncelle

            // Hedefe olan ana yön
            Vector3 direction = (targetPosition - transform.position).normalized;

            // Hareketin ilerleme oranını hesapla
            float progress = elapsedTime / moveDuration;

            // Sapmayı hedefe yaklaştıkça azalt (örneğin, %90'a yaklaştıkça sapma sıfırlanacak)
            float currentSwayAmount = swayAmount * (1f - progress);

            // X ve Z eksenlerinde hafif bir rastgele sapma ekliyoruz
            float swayX = Mathf.Sin(Time.time * swaySpeed) * currentSwayAmount;
            float swayZ = Mathf.Cos(Time.time * swaySpeed) * currentSwayAmount;

            // Sapmayı yalnızca küçük bir ölçüde uygula
            Vector3 sway = new Vector3(swayX, 0, swayZ);

            // Hedefe olan ilerlemeyi Lerp ile hesapla
            Vector3 movePosition = Vector3.Lerp(startPosition, targetPosition, progress);

            // Sapmayı ekleyerek pozisyonu güncelle
            transform.position = movePosition + sway;

            yield return null;  // Her frame'de döngüye devam et
        }

        // Hedefe tam olarak ulaştıktan sonra pozisyonu hedefle eşitle
        transform.position = targetPosition;
    }
}
