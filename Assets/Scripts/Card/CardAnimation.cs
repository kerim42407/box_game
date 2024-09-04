using System.Collections;
using TMPro;
using UnityEngine;

public class CardAnimation : MonoBehaviour
{
    public float speed = 5.0f;
    public RectTransform rectTransform;
    public Card card;
    private int duration = 2;

    private void Start()
    {
        card = GetComponent<Card>();
    }


    //public IEnumerator MoveToTarget(Vector2 targetPosition)
    //{
    //    while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
    //    {
    //        transform.position = Vector2.Lerp(transform.position, targetPosition, Time.deltaTime * speed);
    //        yield return null;
    //    }

    //    transform.position = targetPosition;
    //    gameObject.SetActive(false);
    //}

    /// <summary>
    /// Moves card to the game player list item
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    public IEnumerator MoveToPlayerUI(Vector2 targetPosition)
    {
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            transform.position = Vector2.Lerp(transform.position, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            if(elapsedTime > duration * 0.75f)
            {
                gameObject.SetActive(false);
            }
            yield return null;
        }
        transform.position = targetPosition;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Moves card to the play card area
    /// </summary>
    /// <returns></returns>
    public IEnumerator MoveToPlayArea()
    {
        if(card == null)
        {
            card = GetComponent<Card>();
        }

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
        if (card.isOwned)
        {
            GameManager.Instance.CmdPlayCard(card.s_OwnerPlayer, card);
            //card.playCardEvent?.Invoke(card);
        }

    }
}