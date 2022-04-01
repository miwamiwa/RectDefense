using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemiesManager : MonoBehaviour
{
    // references
    MainManager MM;
    LevelManager LM;

    // Tile names used for lookup
    public const string StartTileName = "StartSquare";
    public const string EndTileName = "EndSquare";

    // stats for debug reports
    int successfulEnemies = 0;
    int deadEnemies = 0;

    // enemy we are spawning
    public GameObject BasicEnemy;
    
    // all enemies
    public List<EnemyController> enemies = new List<EnemyController>();

    // path start and end points 
    public List<Vector2> StartTiles = new List<Vector2>();
    public List<Vector2> FinishTiles = new List<Vector2>();

    // which way do enemies go from start tiles
    public Vector2 StartDirection = new Vector2(1, 0);

    // check to repeat level
    public bool ContinuousSpawning = false;

    // initial wait 
    public float FirstEnemyTime = 1f;
    // interval between groups
    public float Interval = 1f;
    // interval between enemies of same group
    public float shortInterval = 0.2f;

    // enemy speed (time it takes to travel 1 square)
    public float EnemyTickLength = 1f;
 
    // enemy UID
    int enemyUIDCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        MM = GetComponent<MainManager>();
        LM = GetComponent<LevelManager>();

        // read data from the Grid object
        GetStartAndFinishTiles();

        // Load this level, with its phases and enemy numbers
        LM.LoadLevelData();
        LevelManager.CurrentPhase = 0;

        // Prepare a wave of enemies and start spawning
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
        // get phase data from level manager
        List<List<List<int>>> data = LM.GetLevelData();
        List<int> Phase = data[LevelManager.CurrentLevel][LevelManager.CurrentPhase];

        // expand phase data into individual groups
        List<int> AllGroups = new List<int>();
        for(int groupSize=0; groupSize < Phase.Count; groupSize++)
        for(int i = 0; i < Phase[groupSize]; i++) AllGroups.Add(groupSize + 1);

        // randomize order
        HashSet<int> indices = new HashSet<int>();
        List<int> orderedGroups = new List<int>();
        while (orderedGroups.Count < AllGroups.Count)
        {
            int i = Mathf.FloorToInt(Random.Range(0f, AllGroups.Count - 0.01f));
            if (indices.Add(i)) orderedGroups.Add(AllGroups[i]);
        }

        // let's start generating
        StartCoroutine(SpawnEnemiesAtInterval(orderedGroups));
    }


    // SpawnEnemiesAtInterval()
    //
    // spawn enemies at a given interval
    IEnumerator SpawnEnemiesAtInterval(List<int> Groups)
    {
        // initial wait time
        yield return new WaitForSeconds(FirstEnemyTime);

        // reset counters
        int t = 0; // position along start tiles
        int groupsSpawned = 0;

        while (groupsSpawned < Groups.Count)
        {
            Debug.Log("Spawning group of " + Groups[groupsSpawned] +" ("+groupsSpawned+"/"+Groups.Count+")");

            for(int i=0; i<Groups[groupsSpawned]; i++)
            {

                // spawn enemy
                GameObject newEnemy = Instantiate(BasicEnemy);
                EnemyController enemy = newEnemy.GetComponent<EnemyController>();

                enemy.SetupEnemy(enemy.V2ToV3Int(StartTiles[t]), StartDirection, EnemyTickLength, MM, this, enemyUIDCounter);
                enemies.Add(enemy);
                enemyUIDCounter++;

                // alternate start tiles
                t = (t + 1) % StartTiles.Count;

                // chance to wait a little between enemies of same group
                yield return new WaitForSeconds(Mathf.RoundToInt(Random.Range(0f, 3f)) * shortInterval);
            }
            

            // wait for next spawn
            yield return new WaitForSeconds(Interval);
            groupsSpawned++;
            
        }

        Debug.Log("Phase " + LevelManager.CurrentPhase + " is over (Level " + LevelManager.CurrentLevel + ")");

        // next phase?
        LevelManager.CurrentPhase++;
        if (LevelManager.CurrentPhase == LM.GetLevelData()[LevelManager.CurrentLevel].Count)
        {
            // this level's final phase is over
            LevelManager.CurrentPhase = 0;

            // if continuous mode is active, reset
            if (ContinuousSpawning) GenerateEnemies();
        }

        // if there is another phase to this level, continue
        else GenerateEnemies();

        yield break;
    }


    // GetStartAndFinishTiles()
    //
    // Search Tilemap for end points

    void GetStartAndFinishTiles()
    {
        // get tiles from tilemap
        Grid grid = MM.PathGrid;
        Tilemap tilemap = MM.PathTilemap;
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        // create a list of start tiles and finish tiles
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

    // CheckCollision()
    //
    // check if there is already an enemy on the tile enemy is moving to

    public bool CheckCollision(Vector2 tile)
    {
        bool collided = false;

        foreach(EnemyController enemy in enemies)
        {
            if (enemy.ready && enemy.coords == enemy.V2ToV3Int(tile)) collided = true;
        }

        return collided;
    }


    // stat reporting: 

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
