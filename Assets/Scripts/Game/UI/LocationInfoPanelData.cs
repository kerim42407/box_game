using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocationInfoPanelData : MonoBehaviour
{
    [Header("References")]
    public GameObject eventContainer;
    public TextMeshProUGUI productivityText;
    public TextMeshProUGUI factoryLevelText;
    public TextMeshProUGUI productionTypeText;
    public TextMeshProUGUI locationNameText;

    [Header("Prefabs")]
    public GameObject positiveEventPrefab;
    public GameObject negativeEventPrefab;
}
