using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyController : MonoBehaviour
{
    public Vector2 Direction = new Vector2(0f, 0f);
    public GameObject Highlight;
    float TickLength = 1f;
    int PeekDistance = 9;
    MainManager MM;
    EnemiesManager EM;
    public Vector3Int coords;
    public bool ready = false;
    bool TurnedCorner = false;
    public int UID = -1;
    public int markCounter = 0;
    bool dead = false;

    public float MaxHealth = 24f;
    float Health = 0f;
    public float HitRange = 0.2f;

    public HealthBar healthBar;

    // Start is called before the first frame update
    void Start()
    {
        Health = MaxHealth;   
    }

    private void FixedUpdate()
    {
        markCounter--;
        if (markCounter <= 0) NoHighlight();
        else HighlightMe();
    }

    // SetupEnemy()
    //
    // Setup enemy and start moving
    public void SetupEnemy(Vector3Int position, Vector2 direction, float tickLength, MainManager mm, EnemiesManager em, int uid)
    {
        NoHighlight();
        Direction = direction;
        TickLength = tickLength;
        MM = mm;
        EM = em;
        coords = position;
        transform.position = MM.PathGrid.CellToWorld(position);
        ready = true;
        UID = uid;

        StartCoroutine(MoveAlong());
    }

    IEnumerator MoveAlong()
    {
        Vector3Int coordinate = MM.PathGrid.WorldToCell(transform.position);
        coords = coordinate;
        Vector2 coord2d = new Vector2(coordinate.x, coordinate.y);

        // stop moving if dead
        if (dead)
        {
            yield break;
        }

        //Debug.Log(coord2d);

        // 0. check for end reached
        if (EM.FinishTiles.Contains(coord2d))
        {
            ReachEnd();
            // stop coroutine
            yield break;
        }

        // 1. check if we can turn left or right
        Vector2 left = GetCurrentLeft();
        bool CanTurnLeft = isTurnPossible(left, coordinate);
        bool CanTurnRight = isTurnPossible(-left, coordinate);
        bool CanMoveForward = EM.CheckCollision(coord2d + Direction) == false && MM.PathTilemap.GetTile(coordinate + V2ToV3Int(Direction)) !=null;

        if (!CanTurnLeft && !CanTurnRight) TurnedCorner = false;

        // 2. if we can turn, check obstacles first
        if (CanTurnLeft && EM.CheckCollision(coord2d + left)) CanTurnLeft = false;
        if (CanTurnRight && EM.CheckCollision(coord2d - left)) CanTurnRight = false;

        // chance to hesitate 
        if (CanMoveForward && (CanTurnLeft || CanTurnRight))
        {
            if (Random.Range(0f, 1f) < 0.45f)
            {
                CanTurnLeft = false;
                CanTurnRight = false;
            }
        }

        // 3. Turning: if at an intersection, make a choice
        if (!TurnedCorner && CanTurnLeft && CanTurnRight)
        {
            if(Random.Range(0, 1) < 0.5f)
            {
                // turn left
                MoveTo(MM.PathGrid.CellToWorld(coordinate + V2ToV3Int(left)));
                Direction = left;
                TurnedCorner = true;
            }
            else
            {
                // turn right
                MoveTo(MM.PathGrid.CellToWorld(coordinate - V2ToV3Int(left)));
                Direction = -left;
                TurnedCorner = true;
            }
        }

        // otherwise, if we can only turn Left or Right
        else if (!TurnedCorner&& CanTurnLeft)
        {
            // turn left
            MoveTo(MM.PathGrid.CellToWorld(coordinate + V2ToV3Int(left)));
            Direction = left;
            TurnedCorner = true;
        }
        else if (!TurnedCorner&& CanTurnRight)
        {
            // turn right
            MoveTo(MM.PathGrid.CellToWorld(coordinate - V2ToV3Int(left)));
            Direction = -left;
            TurnedCorner = true;
        }


        // 4. Move forward: if we can't turn, check if we can move forward
        else if(CanMoveForward)
        {
            
                // move forward
                MoveTo(MM.PathGrid.CellToWorld(coordinate + V2ToV3Int(Direction)));
            
        }



        // if we got all the way here, rinse and repeat
        yield return new WaitForSeconds(TickLength);
        StartCoroutine(MoveAlong());

        yield break;
    }

    public Vector3Int V2ToV3Int (Vector2 direction)
    {
        return new Vector3Int(Mathf.RoundToInt(direction.x), Mathf.RoundToInt(direction.y), 0);
    }


    // isTurnPossible()
    //
    //
    bool isTurnPossible(Vector2 dir, Vector3Int coordinate)
    {
        bool result = true;

        // if there are any empty tiles in peek range, we can't turn
        for(int i=0; i<PeekDistance; i++)
        {
            TileBase tile = MM.PathTilemap.GetTile(coordinate + new Vector3Int(Mathf.RoundToInt(dir.x * PeekDistance), Mathf.RoundToInt(dir.y * PeekDistance), 0));
            if (tile == null) result = false;
        }

        return result;
    }

    Vector2 GetCurrentLeft()
    {
        if (Direction == Vector2.left) return Vector2.down;
        else if (Direction == Vector2.down) return Vector2.right;
        else if (Direction == Vector2.right) return Vector2.up;
        else return Vector2.left;
    }


    void MoveTo(Vector2 position)
    {
        if(dead) return;

        Vector2 p1 = transform.position;
        Vector2 p2 = position;

        float fineTick = 0.05f; // 40 per second
        int numFineTicks = Mathf.FloorToInt(TickLength / fineTick);
        float deltaPos = (p2 - p1).magnitude / numFineTicks;

        StartCoroutine(MoveBetweenPoints(p2, fineTick, deltaPos, numFineTicks-1));
    }

    IEnumerator MoveBetweenPoints(Vector2 FinalPos, float fineTick, float delta, int numFineTicks)
    {
        while (numFineTicks > 0)
        {
            if (dead) break;

            transform.Translate( delta * Direction);
            yield return new WaitForSeconds(fineTick);
            numFineTicks--;
        }
        
        if(!dead) transform.position = FinalPos;
        yield break;
    }


    public void SubtractHealth(float value)
    {
        Health = Mathf.Max(0f, Health - value);
        healthBar.SetHealthPercent(Health/MaxHealth);

        if (!healthBar.shown)
        {
            healthBar.gameObject.SetActive(true);
            healthBar.shown = true;
        }

        if (Health <= 0f)
        {
            // dead
            EM.enemies.Remove(this);
            EM.TallyDeadEnemy();
            dead = true;

            StartCoroutine(DeadAnimation());
        }
    }

    IEnumerator DeadAnimation()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    void ReachEnd()
    {
        //Debug.Log("Enemy "+UID+" reached the end");
        EM.enemies.Remove(this);
        EM.TallySuccessfulEnemy();
        Destroy(gameObject);
    }


    public void HighlightMe()
    {
        Highlight.SetActive(true);
    }

    public void NoHighlight()
    {
        Highlight.SetActive(false);
    }

}
