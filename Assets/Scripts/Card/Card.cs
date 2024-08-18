using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardUI))]
public class Card : MonoBehaviour
{
    #region Fields and Properties

    [field: SerializeField] public ScriptableCard CardData { get; private set; }

    #endregion

    #region Methods

    public void SetUp(ScriptableCard data)
    {
        CardData = data;
        GetComponent<CardUI>().SetCardUI();
    }

    #endregion
}
