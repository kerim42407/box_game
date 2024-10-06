using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeckController : LocationController    
{
    #region Fields and Properties

    public CardCollection cardCollection;
    public GameObject cardPrefab;
    //[SerializeField] private Card cardPrefab;

    public GameObject playerCardContainer;

    private List<Card> deckPile = new();

    public List<Card> HandCards { get; private set; } = new();



    #endregion

    #region Setup Functions

    public override void SetupEmissionController()
    {
        emissionController.material = GetComponent<MeshRenderer>().materials[2];
        emissionController.initialEmissionColor = emissionController.material.GetColor("_EmissionColor");
    }

    public override void SetupLocation()
    {
        base.SetupLocation();
        SpawnLocationNameTextPrefab();
        playerCardContainer = UIManager.Instance.playerCardContainer;
    }

    #endregion

}
