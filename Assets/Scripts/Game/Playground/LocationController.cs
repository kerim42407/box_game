using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(EmissionController))]
public abstract class LocationController : NetworkBehaviour
{
    #region Fields and Properties

    [Header("Location Controller")]
    public string locationName;
    public int locationIndex;
    public LocationType locationType;
    public RegionType regionType;
    public PlaygroundController playgroundController;
    [HideInInspector] public Material locationOwnerMaterial;
    public Material[] indicateMaterials;
    private bool isSelectable;
    public UnityEvent<LocationController, Card> onClickEvent;
    public Card playedCard;

    [Header("Sync Variables")]
    [SyncVar(hook = nameof(SetOwnerPlayer))] public PlayerObjectController s_OwnerPlayer;
    public readonly SyncList<Card> s_ActiveCards = new();

    #endregion

    [Header("References")]
    [HideInInspector] public GameManager gameManager;
    [HideInInspector] public TextMeshPro locationNameText;
    [HideInInspector] public TextMeshPro rentRateText;
    [HideInInspector] public EmissionController emissionController;
    [HideInInspector] public SellLocationInfoPanelData sellLocationInfoPanelData;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
        playgroundController = PlaygroundController.Instance;
        emissionController = GetComponent<EmissionController>();
        playgroundController.allLocations.Add(this);
        SetupEmissionController();
        SetupLocation();
    }

    public abstract void SetupEmissionController();
    public virtual void IndicateLocation(bool shouldIndicate)
    {
        if (shouldIndicate)
        {
            foreach (Material material in indicateMaterials)
            {
                material.shader = Shader.Find("Universal Render Pipeline/Unlit");
            }
            if (rentRateText)
            {
                rentRateText.color = new Color(255, 255, 255, 1);
            }
            if (locationNameText)
            {
                locationNameText.color = new Color(255, 255, 255, 1);
            }
            isSelectable = true;
        }
        else
        {
            foreach (Material material in indicateMaterials)
            {
                material.shader = Shader.Find("Universal Render Pipeline/Lit");
            }
            if (rentRateText)
            {
                rentRateText.color = new Color(255, 255, 255, .25f);
            }
            if (locationNameText)
            {
                locationNameText.color = new Color(255, 255, 255, .25f);
            }
            isSelectable = false;
        }
        
    }
    public virtual void ResetIndicateLocation()
    {
        foreach (Material material in indicateMaterials)
        {
            material.shader = Shader.Find("Universal Render Pipeline/Lit");
        }
        if (rentRateText)
        {
            rentRateText.color = new Color(255, 255, 255, 1);
        }
        if (locationNameText)
        {
            locationNameText.color = new Color(255, 255, 255, 1);
        }
        isSelectable = false;
    }

    #region Update Functions
    public void SetOwnerPlayer(PlayerObjectController oldOwner, PlayerObjectController newOwner)
    {
        if (isServer)
        {
            s_OwnerPlayer = newOwner;
        }
        if (isClient && (oldOwner != newOwner))
        {
            UpdateOwnerPlayer(newOwner);
        }

    }

    public virtual void UpdateOwnerPlayer(PlayerObjectController newOwner)
    {
        if(newOwner == null)
        {
            locationOwnerMaterial.color = Color.red;
        }
        else
        {
            locationOwnerMaterial.color = newOwner.playerColor;
        }
        
    }

    #endregion

    #region Setup Functions

    public virtual void SetupLocation()
    {
        indicateMaterials = GetComponent<MeshRenderer>().materials;
    }


    /// <summary>
    /// Spawns location name text prefab according to the region type
    /// </summary>
    public void SpawnLocationNameTextPrefab()
    {
        locationNameText = Instantiate(playgroundController.locationNameTextPrefab, transform).GetComponent<TextMeshPro>();
        locationNameText.text = locationName;

        switch (regionType)
        {
            case RegionType.Clay:
                locationNameText.transform.localPosition = new Vector3(-0.625f, 0.0065f, 0);
                locationNameText.transform.localEulerAngles = new Vector3(75, 90, 0);
                break;
            case RegionType.Copper:
                locationNameText.transform.localPosition = new Vector3(-0.64f, 0.135f, 0);
                locationNameText.transform.localEulerAngles = new Vector3(50, -90, 0);
                break;
            case RegionType.Iron:
                locationNameText.transform.localPosition = new Vector3(-0.64f, 0.135f, 0);
                locationNameText.transform.localEulerAngles = new Vector3(50, -90, 0);
                break;
            case RegionType.Cotton:
                locationNameText.transform.localPosition = new Vector3(-0.64f, 0.135f, 0);
                locationNameText.transform.localEulerAngles = new Vector3(50, -90, 0);
                break;
            case RegionType.Coal:
                locationNameText.transform.localPosition = new Vector3(-0.625f, 0.0065f, 0);
                locationNameText.transform.localEulerAngles = new Vector3(75, 90, 0);
                break;
        }
    }

    /// <summary>
    /// Spawns rent rate text prefab according to the region type
    /// </summary>
    public void SpawnRentRateTextPrefab()
    {
        rentRateText = Instantiate(playgroundController.rentRateTextPrefab, transform).GetComponent<TextMeshPro>();
        switch (regionType)
        {
            case RegionType.Clay:
                rentRateText.transform.localEulerAngles = new Vector3(90, 90, 0);
                break;
            case RegionType.Copper:
                rentRateText.transform.localEulerAngles = new Vector3(90, -90, 0);
                break;
            case RegionType.Iron:
                rentRateText.transform.localEulerAngles = new Vector3(90, -90, 0);
                break;
            case RegionType.Cotton:
                rentRateText.transform.localEulerAngles = new Vector3(90, -90, 0);
                break;
            case RegionType.Coal:
                rentRateText.transform.localEulerAngles = new Vector3(90, 90, 0);
                break;
        }
        rentRateText.transform.localPosition = new Vector3(.675f, 0, 0);
    }

    /// <summary>
    /// Spawns resource prefab according to the region type
    /// </summary>
    public void SpawnResourcePrefab()
    {
        switch (regionType)
        {
            case RegionType.Clay:
                Instantiate(playgroundController.clayResourcePrefab, transform);
                break;
            case RegionType.Copper:
                Instantiate(playgroundController.copperResourcePrefab, transform);
                break;
            case RegionType.Iron:
                Instantiate(playgroundController.ironResourcePrefab, transform);
                break;
            case RegionType.Cotton:
                Instantiate(playgroundController.cottonResourcePrefab, transform);
                break;
            case RegionType.Coal:
                Instantiate(playgroundController.coalResourcePrefab, transform);
                break;
        }
    }

    #endregion

    #region Get Functions
    public virtual float GetProductivity()
    {
        return 100;
    }

    public virtual float GetCalculateSellToBankPrice()
    {
        return 0;
    }

    public virtual float GetRentRate()
    {
        return 0;
    }
    #endregion

    private void OnMouseDown()
    {
        if (isSelectable)
        {
            onClickEvent?.Invoke(this, playedCard);
        }
    }

}
