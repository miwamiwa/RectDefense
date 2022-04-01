using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Amno : MonoBehaviour
{
    // references 
    EnemyController Target;
    SpriteRenderer renderer;

    // settings
    public float HitChance = 1f;
    public float TravelSpeed = 0.1f;
    public float Damage = 8f;

    // ready to travel
    bool ready = false;
    bool missed = false;
    
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    // SetupBullet()
    //
    // Called by Tower when ready to go
    public void SetupBullet(Vector3 position, EnemyController target)
    {
        transform.position = position;
        ready = true;
        Target = target;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (ready)
        {
            // move towards target
            Vector3 hereToTarget = Target.gameObject.transform.position - transform.position;
            transform.Translate(hereToTarget.normalized * TravelSpeed);

            // check collision 
            if (!missed && hereToTarget.magnitude < Target.HitRange)
            {
                // chance to miss
                if(Random.Range(0f, 1f) < HitChance)
                {
                    Target.SubtractHealth(Damage);
                    Destroy(gameObject);
                    return;
                }
                // after a miss, we stop checking for collisions
                else
                {
                    missed = true;
                }
            }

            // check if bullet is off screen
            if (!renderer.isVisible)
            {
                // remove bullet
                Destroy(gameObject);
                return;
            }
        }
    }
}
