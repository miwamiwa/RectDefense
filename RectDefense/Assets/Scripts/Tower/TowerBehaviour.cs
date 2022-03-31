using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBehaviour : MonoBehaviour
{
    // references 
    EnemiesManager EM;

    // range display circle
    GameObject RangeCircle;
    public GameObject RangeCirclePrefab;
    // marker to show enemies in range
    public GameObject TargetMarker;
    List<EnemyController> enemiesInRange = new List<EnemyController>();

    // settings 

    // tower range: change with UpdateRangeCircleSize(float) or in the inspector
    public float RangeCircleSize = 1f;
    // amno used
    public GameObject AmnoType;
    // shooting interval
    public float ShotInterval = 1f;

    // Start is called before the first frame update
    void Start()
    {
        EM = GameObject.FindGameObjectWithTag("MainManager").GetComponent<EnemiesManager>();
        RangeCircle = Instantiate(RangeCirclePrefab);
        HideRange();
        UpdateRangeCircleSize();
        RangeCircle.transform.position = transform.position;

        // start shooting
        StartCoroutine(ShootAtInterval());
    }

    IEnumerator ShootAtInterval()
    {
        EnemyController nearestEnemy = GetNearestEnemy();
        if (nearestEnemy != null)
        {
            Amno newbullet = Instantiate(AmnoType).GetComponent<Amno>();
            newbullet.SetupBullet(transform.position, nearestEnemy);
        }

        yield return new WaitForSeconds(ShotInterval);
        StartCoroutine(ShootAtInterval());

        yield break;
    }

    EnemyController GetNearestEnemy()
    {
        EnemyController target = null;
        float shortestDistance = 999f;
        foreach(EnemyController enemy in enemiesInRange)
        {
            float distance = (transform.position - enemy.transform.position).magnitude;
            if (distance < shortestDistance)
            {
                target = enemy;
                shortestDistance = distance;
            }
        }
        return target;
    }

    private void OnMouseOver()
    {
        ShowRange();
    }

    private void OnMouseExit()
    {
        HideRange();
    }

    private void OnMouseEnter()
    {
        ShowRange();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // check for enemies in range
        enemiesInRange = new List<EnemyController>();
        foreach(EnemyController enemy in EM.enemies)
        {
            if((enemy.transform.position - transform.position).magnitude <= RangeCircleSize /2f)
            {
                enemiesInRange.Add(enemy);
                enemy.markCounter = 2;
            }
        }
    }

    public void ShowRange()
    {
        UpdateRangeCircleSize();
        RangeCircle.SetActive(true);
    }

    public void HideRange()
    {
        RangeCircle.SetActive(false);
    }

    public void UpdateRangeCircleSize(float size)
    {
        RangeCircleSize = size;
        UpdateRangeCircleSize();
    }

    public void UpdateRangeCircleSize()
    {
        RangeCircle.transform.localScale = new Vector3(RangeCircleSize, RangeCircleSize, 0f);

    }
}
