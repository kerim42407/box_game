using Mirror;
using TMPro;
using UnityEngine;

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
    public Material playerColorMaterial;

    [Header("Sync Variables")]
    [SyncVar(hook = nameof(SetOwnerPlayer))] public PlayerObjectController s_OwnerPlayer;
    public readonly SyncList<Card> s_ActiveCards = new();

    #endregion

    [Header("References")]
    [HideInInspector] public GameManager gameManager;
    [HideInInspector] public TextMeshPro rentRateText;
    [HideInInspector] public EmissionController emissionController;
    [HideInInspector] public SellLocationInfoPanelData sellLocationInfoPanelData;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
        playgroundController = PlaygroundController.Instance;
        emissionController = GetComponent<EmissionController>();
        SetupEmissionController();
        SetupLocation();
    }

    public abstract void SetupEmissionController();

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
            playerColorMaterial.color = Color.red;
        }
        else
        {
            playerColorMaterial.color = newOwner.playerColor;
        }
        
    }

    #endregion

    #region Setup Functions

    public virtual void SetupLocation()
    {

    }


    /// <summary>
    /// Spawns location name text prefab according to the region type
    /// </summary>
    public void SpawnLocationNameTextPrefab()
    {
        TextMeshPro locationNameText = Instantiate(playgroundController.locationNameTextPrefab, transform).GetComponent<TextMeshPro>();
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
    /// Spawns factory prefab
    /// </summary>
    public void SpawnFactoryPrefab()
    {
        Instantiate(playgroundController.level1FactoryPrefab, transform);
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
}
