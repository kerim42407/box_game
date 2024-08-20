using System.Collections;
using TMPro;
using UnityEngine;

public class CardAnimation : MonoBehaviour
{
    public float speed = 5.0f;
    public RectTransform rectTransform;

    public IEnumerator MoveToTarget(Vector2 targetPosition)
    {
        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector2.Lerp(transform.position, targetPosition, Time.deltaTime * speed);
            yield return null;
        }

        transform.position = targetPosition;
        gameObject.SetActive(false);
    }
}