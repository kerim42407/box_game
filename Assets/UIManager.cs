using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : NetworkBehaviour
{
    public static UIManager instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public event Action<int> OnPlayerDiceResultChanged;

    [ClientRpc]
    public void ChangeDiceResult(int result)
    {
        OnPlayerDiceResultChanged?.Invoke(result);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
