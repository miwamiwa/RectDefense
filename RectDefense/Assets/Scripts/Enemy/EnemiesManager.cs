using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemiesManager : MonoBehaviour
{
    public const string StartTileName = "StartSquare";
    public const string EndTileName = "EndSquare";

    int successfulEnemies = 0;
    int deadEnemies = 0;

    public GameObject BasicEnemy;
    MainManager MM;
    public List<EnemyController> enemies = new List<EnemyController>();

    // which way do enemies go from start tiles
    public Vector2 StartDirection = new Vector2(1, 0);

    // how many enemies do we spawn in this level
    public int EnemiesPerWave = 4;
    public bool ContinuousSpawning = false;
    public int enemiesSpawnedThisWave = 0;
    public int enemiesSpawnedTotal = 0;


    public int Cutoff = 100;

    // enemy spawn timing
    public float FirstEnemyTime = 1f;
    public float Interval = 1f;
    public float Spread = 0.2f;

    // time it takes to travel 1 square
    public float EnemyTickLength = 1f;

    // start and end points 
    public List<Vector2> StartTiles = new List<Vector2>();
    public List<Vector2> FinishTiles = new List<Vector2>();

    // enemy UID
    int enemyUIDCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        MM = GetComponent<MainManager>();
        GetStartAndFinishTiles();
        GenerateEnemies();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // GenerateEnemies()
    //
    // Generate list of spawn times, then start spawning enemies
    void GenerateEnemies()
    {
        // get a random start time for each enemy
        SortedList<float, int> StartTimes = new SortedList<float, int>();
        for (int i = 1; i < EnemiesPerWave+1; i++) 
            StartTimes.Add(Mathf.Max(0f, i * Interval + Random.Range(- Spread, Spread)), 0);

        // use that to calculate time interval between enemies
        List<float> intervals = new List<float>();
        float lastval = 0f;
        foreach(float key in StartTimes.Keys)
        {
            intervals.Add(key - lastval);
            lastval = key;
        }

        // reset
        enemiesSpawnedThisWave = 0;

        // let's start!
        StartCoroutine(SpawnEnemiesAtInterval(intervals));
    }


    // SpawnEnemiesAtInterval()
    //
    // spawn enemies at a given interval
    IEnumerator SpawnEnemiesAtInterval(List<float> intervals)
    {
        // initial wait time
        yield return new WaitForSeconds(FirstEnemyTime);
        int t = 0; // position along start tiles

        while (enemiesSpawnedThisWave < EnemiesPerWave && (Cutoff == 0 || enemiesSpawnedTotal < Cutoff))
        {
            // spawn enemy
            GameObject newEnemy = Instantiate(BasicEnemy);
            EnemyController enemy = newEnemy.GetComponent<EnemyController>();
            
            enemy.SetupEnemy(enemy.V2ToV3Int(StartTiles[t]), StartDirection, EnemyTickLength, MM, this, enemyUIDCounter);
            enemies.Add(enemy);
            enemyUIDCounter++;

            // wait for next spawn
            yield return new WaitForSeconds(intervals[enemiesSpawnedThisWave]);
            enemiesSpawnedThisWave++;
            enemiesSpawnedTotal++;
            t = (t + 1) % StartTiles.Count;
        }

        // repeat if continuous spawning is active and cutoff wasn't reached yet
        if (ContinuousSpawning&&(Cutoff==0||enemiesSpawnedTotal<Cutoff)) GenerateEnemies();
        yield break;
    }


    // GetStartAndFinishTiles()
    //
    // Search Tilemap for end points
    void GetStartAndFinishTiles()
    {
        Grid grid = MM.PathGrid;
        Tilemap tilemap = MM.PathTilemap;

        Debug.Log(tilemap.GetLayoutCellCenter());

        BoundsInt bounds = tilemap.cellBounds;
        //Debug.Log(bounds.center);
        //Debug.Log(bounds.xMin);
        //Debug.Log(bounds.yMin);

        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    if (tile.name == StartTileName) StartTiles.Add(new Vector2(bounds.xMin+x, bounds.yMin + y));
                    else if (tile.name == EndTileName) FinishTiles.Add(new Vector2(bounds.xMin + x, bounds.yMin + y));
                }
            }
        }
    }

    public bool CheckCollision(Vector2 tile)
    {
        bool collided = false;

        foreach(EnemyController enemy in enemies)
        {
            if (enemy.ready && enemy.coords == enemy.V2ToV3Int(tile)) collided = true;
        }

        return collided;
    }

    public void TallySuccessfulEnemy()
    {
        successfulEnemies++;
        ReportStats();
    }

    public void TallyDeadEnemy()
    {
        deadEnemies++;
        ReportStats();
    }

    void ReportStats()
    {
        Debug.Log("Dead enemies: "+deadEnemies+". Successful enemies: "+successfulEnemies);
    }
}
