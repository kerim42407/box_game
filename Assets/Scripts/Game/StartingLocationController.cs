using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingLocationController : LocationController
{

    #region Setup Functions

    public override void SetupEmissionController()
    {
        emissionController.material = GetComponent<MeshRenderer>().materials[1];
        emissionController.initialEmissionColor = emissionController.material.GetColor("_EmissionColor");
    }

    public override void SetupLocation()
    {
        base.SetupLocation();
        SpawnLocationNameTextPrefab();
    }

    #endregion
}
