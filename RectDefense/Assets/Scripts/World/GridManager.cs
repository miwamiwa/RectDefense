using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public bool HideOnStart = true;
    // Start is called before the first frame update
    void Start()
    {
        if(HideOnStart) gameObject.SetActive(false);
    }
}
