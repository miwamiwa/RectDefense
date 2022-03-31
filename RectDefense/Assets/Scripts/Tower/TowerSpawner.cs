using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerSpawner : MonoBehaviour
{
    public bool TowerSpawned = false;
    public GameObject BasicTower;
    public Vector3 SpawnOffset = new Vector3(0f, 0.2f, -0.01f);
    public TowerBehaviour Tower;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseOver()
    {
        // mouse press
        if (Input.GetMouseButtonDown(0))
        {

            // spawn tower
            if (!TowerSpawned)
            {
                TowerSpawned = true;
                GameObject newTower = Instantiate(BasicTower);
                newTower.transform.position = transform.position + SpawnOffset;
                Tower = newTower.GetComponent<TowerBehaviour>();
            }
        }
    }
}
