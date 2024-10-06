using UnityEngine;

public class DiceSound : MonoBehaviour
{
    private AudioSource audioSource;

    public float minVolume = 0.2f; // Minimum ses şiddeti
    public float maxVolume = 1.0f; // Maksimum ses şiddeti
    public float maxCollisionForce = 10.0f; // Çarpma şiddetinin maksimum değeri

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // Çarpma şiddetini alıyoruz
            float collisionForce = collision.relativeVelocity.magnitude;

            // Eğer çarpma şiddeti belirli bir eşikten büyükse ses oynat
            if (collisionForce > 1.0f)
            {
                // Sesin şiddetini (volume) çarpma gücüne göre ayarla
                float volume = Mathf.Clamp(collisionForce / maxCollisionForce, minVolume, maxVolume);

                // AudioSource'un volume değerini ayarla
                audioSource.volume = volume;

                // Sesi oynat
                audioSource.Play();
            }
        }
    }
}
