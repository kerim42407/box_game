using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card Collection")]
public class CardCollection : ScriptableObject
{
    [field: SerializeField] public List<ScriptableCard> CardsInCollection { get; private set; }
}