using System.Collections;
using TMPro;
using UnityEngine;

public class CardAnimation : MonoBehaviour
{
    public float speed = 5.0f;
    public RectTransform rectTransform;
    public Card card;

    private void Start()
    {
        card = GetComponent<Card>();
    }

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

    public IEnumerator MoveToPlayArea(PlayerObjectController player, float duration)
    {
        card.GetComponent<CardUI>().hiddenImage.gameObject.SetActive(false);
        float elapsedTime = 0;
        while(elapsedTime < duration)
        {
            transform.position = Vector2.Lerp(transform.position, UIManager.Instance.playCardArea.transform.position, elapsedTime / duration);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = UIManager.Instance.playCardArea.transform.position;
        transform.localScale = Vector3.one;
        card.playCardEvent?.Invoke(player);
    }
}