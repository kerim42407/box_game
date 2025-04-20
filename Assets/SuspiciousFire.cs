using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspiciousFire : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(OnEnd), 10f);
    }

    [Server]
    private void OnEnd()
    {
        Destroy(transform.gameObject);
    }
}
