using UnityEngine;

public class DiceSound : MonoBehaviour
{
    private AudioSource audioSource;

    public float minVolume = 0.2f;
    public float maxVolume = 0.5f;
    public float maxCollisionForce = 10.0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            float collisionForce = collision.relativeVelocity.magnitude;

            if (collisionForce > 1.0f)
            {
                float volume = Mathf.Clamp(collisionForce / maxCollisionForce, minVolume, maxVolume);

                audioSource.volume = volume;

                audioSource.Play();
            }
        }
    }
}
