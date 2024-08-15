using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocationInfoPanelData : MonoBehaviour
{
    [Header("References")]
    public GameObject eventContainer;
    public TextMeshProUGUI locationNameText;
    public TextMeshProUGUI productivityText;

    [Header("Prefabs")]
    public GameObject positiveEventPrefab;
    public GameObject negativeEventPrefab;
}
