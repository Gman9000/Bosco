using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSource : MonoBehaviour
{
    [HideInInspector]public bool lit = false;
    public float strength = 3.0F;

    void Start()
    {
        DarkHandler.Instance.AddLightSource(this);
    }

    void Update()
    {
        
    }

    public void Light(bool _lit)
    {
        this.lit = _lit;
    }
}
