using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeCircleHandler : MonoBehaviour
{
    public List<GameObject> EnemiesInRange = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject);
        if (collision.gameObject.tag == "Enemy") EnemiesInRange.Add(collision.gameObject);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy") EnemiesInRange.Remove(collision.gameObject);
    }
}
