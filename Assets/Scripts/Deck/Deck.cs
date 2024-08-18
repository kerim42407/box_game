using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    #region Fields and Properties

    public static Deck Instance { get; private set; }

    [SerializeField] private CardCollection playerDeck;
    [SerializeField] private Card cardPrefab;

    [SerializeField] private Canvas canvas;

    private List<Card> deckPile = new();

    public List<Card> HandCards { get; private set; } = new();



    #endregion

    #region Methods

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InstantiateDeck();
        DrawHand();
    }

    private void InstantiateDeck()
    {
        for(int i = 0; i < playerDeck.CardsInCollection.Count; i++)
        {
            Card card = Instantiate(cardPrefab, canvas.transform);
            card.SetUp(playerDeck.CardsInCollection[i]);
            deckPile.Add(card);
            card.gameObject.SetActive(false);
        }
        
    }

    public void DrawHand()
    {
        int i = Random.Range(0, deckPile.Count);
        HandCards.Add(deckPile[i]);
        deckPile[i].gameObject.SetActive(true);
    }

    public void DiscardCard(Card card)
    {
        if (HandCards.Contains(card))
        {
            HandCards.Remove(card);
            card.gameObject.SetActive(false);
        }
    }

    #endregion
}
