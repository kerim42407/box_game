using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceController : LocationController
{
    #region Fields and Properties

    [Header("Resource Controller")]
    [Header("Sync Variables")]
    [SyncVar] public ProductionType s_ProductionType;
    [SyncVar(hook = nameof(SetRentRate))] public float s_RentRate;
    [SyncVar] public List<FactoryController> s_AffectedFactories;

    #endregion


    #region Methods

    public override void UpdateOwnerPlayer(PlayerObjectController newOwner)
    {
        base.UpdateOwnerPlayer(newOwner);
    }

    #region Setup Functions

    public override void SetupEmissionController()
    {
        emissionController.material = GetComponent<MeshRenderer>().materials[2];
        emissionController.initialEmissionColor = emissionController.material.GetColor("_EmissionColor");
    }

    public override void SetupLocation()
    {
        base.SetupLocation();
        SpawnRentRateTextPrefab();
        SpawnResourcePrefab();
        SetupResourceVariables();
        SpawnLocationNameTextPrefab();
    }

    /// <summary>
    /// Set resource variables
    /// </summary>
    private void SetupResourceVariables()
    {
        locationOwnerMaterial = GetComponent<MeshRenderer>().materials[1];
        playgroundController.resources.Add(this);
    }

    #endregion

    public override float GetProductivity()
    {
        return base.GetProductivity();
    }

    public override float GetCalculateSellToBankPrice()
    {
        return gameManager.CalculateResourceSellToBankPrice();
    }

    public override float GetRentRate()
    {
        return s_RentRate;
    }

    public void SetRentRate(float oldValue, float newValue)
    {
        if (isServer)
        {
            s_RentRate = newValue;
        }
        if (isClient)
        {
            UpdateRentRate(newValue);
        }
    }

    public void UpdateRentRate(float rentRate)
    {
        if (rentRate != 0)
        {
            rentRateText.text = $"{rentRate / 1000}K";
        }
        else
        {
            rentRateText.text = "";
        }
    }

    #endregion
}
