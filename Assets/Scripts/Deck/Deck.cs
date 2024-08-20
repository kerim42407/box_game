using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    #region Fields and Properties

    public CardCollection cardCollection;
    [SerializeField] private Card cardPrefab;

    public GameObject playerCardContainer;

    private List<Card> deckPile = new();

    public List<Card> HandCards { get; private set; } = new();



    #endregion

    #region Methods

    private void Start()
    {

    }

    public void DrawCard(PlayerObjectController player, int cardIndex)
    {
        Card card;
        
        if (player.isLocalPlayer)
        {
            card = Instantiate(cardPrefab, Camera.main.WorldToScreenPoint(transform.position), Quaternion.identity, playerCardContainer.transform);
            card.SetUp(cardCollection.CardsInCollection[cardIndex], false);
            card.transform.localScale = new Vector3(.25f, .25f, .25f);
        }
        else
        {
            card = Instantiate(cardPrefab, Camera.main.WorldToScreenPoint(transform.position), Quaternion.identity, playerCardContainer.transform.parent);
            card.SetUp(cardCollection.CardsInCollection[cardIndex], true);
            card.transform.localScale = new Vector3(.25f, .25f, .25f);
            StartCoroutine(card.cardAnimation.MoveToTarget(player.gamePlayerListItem.transform.position));
        }
        player.playerCards.Add(card);
        if (player.isLocalPlayer)
        {
            if (card.CardData.Category == CardCategory.Market)
            {
                PlaygroundController.Instance.CmdPlayCard(player, player.playerCards.IndexOf(card));
            }
        }
        

        //int i = Random.Range(0, playerDeck.CardsInCollection.Count);
        //Card card = Instantiate(cardPrefab, Camera.main.WorldToScreenPoint(transform.position), Quaternion.identity, playerCardContainer.transform);
        //card.transform.localScale = new Vector3(.25f, .25f, .25f);
        //card.SetUp(playerDeck.CardsInCollection[i]);
        //card.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(transform.position);


        //Debug.Log(card.GetComponent<RectTransform>().anchoredPosition);
        //Debug.Log(Camera.main.WorldToScreenPoint(transform.position));
        //card.transform.SetParent(playerCardContainer.transform, true);
        //StartCoroutine(card.cardAnimation.MoveToTarget(playerCardContainer.transform.position, playerCardContainer.transform));
        //Debug.Log(Camera.main.WorldToScreenPoint(transform.position));
        //Debug.Log(playerCardContainer.GetComponent<RectTransform>().anchoredPosition);
        //Debug.Log(card.cardAnimation);
        //card.cardAnimation.MoveCard(Camera.main.WorldToScreenPoint(transform.position), playerCardContainer.GetComponent<RectTransform>().anchoredPosition);
        //HandCards.Add(card);
    }

    //public void DrawHand()
    //{
    //    int i = Random.Range(0, playerDeck.CardsInCollection.Count);
    //    Card card = Instantiate(cardPrefab);
    //    HandCards.Add(deckPile[i]);
    //    deckPile[i].gameObject.SetActive(true);
    //}

    //public void DiscardCard(Card card)
    //{
    //    if (HandCards.Contains(card))
    //    {
    //        HandCards.Remove(card);
    //        card.gameObject.SetActive(false);
    //    }
    //}

    #endregion
}
