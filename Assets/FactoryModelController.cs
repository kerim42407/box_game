using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryModelController : MonoBehaviour
{
    public List<MeshRenderer> qwe;
    public List<Material> asd;

    // Start is called before the first frame update
    void Start()
    {
        asd.Add(qwe[0].materials[1]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
