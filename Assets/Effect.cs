using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    public enum EffectType
    {
        Positive,
        Neutral,
        Negative
    }

    public EffectType type;
    public float productivityCoef;
}
//public enum EffectType
//{
//    Positive,
//    Neutral,
//    Negative
//}

//public class Effect
//{
//    public string description { get; set; }
//    public EffectType type { get; set; }
//    public Action applyEffect { get; set; }

//    public Effect(string description, EffectType type, Action applyEffect)
//    {
//        this.description = description;
//        this.type = type;
//        this.applyEffect = applyEffect;
//    }
//}
