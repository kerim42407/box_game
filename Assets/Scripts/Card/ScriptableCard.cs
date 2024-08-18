using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardData")]
public class ScriptableCard : ScriptableObject
{
    [field: SerializeField] public string CardName { get; private set; }
    [field: SerializeField, TextArea] public string CardEffectDescription { get; private set; }
    [field: SerializeField, TextArea] public string CardDescription { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public CardCategory Category { get; private set; }
    [field: SerializeField] public CardType Type { get; private set; }
    [field: SerializeField] public CardPlayStyle PlayStyle { get; private set; }
}

public enum CardCategory
{
    Luck,
    Market,
    Sabotage
}

public enum CardType
{
    Holdable,
    NotHoldable
}

public enum CardPlayStyle
{
    Random,
    Selectable
}