using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Amno : MonoBehaviour
{
    bool ready = false;
    public float HitChance = 1f;
    public float TravelSpeed = 0.1f;
    public float Damage = 8f;
    bool missed = false;
    EnemyController Target;
    SpriteRenderer renderer;
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

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
            //transform.LookAt(Target.gameObject.transform);
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
                // flag a miss so that it doesn't get reevaluated
                else
                {
                    missed = true;
                }
            }

            // check if bullet is off screen
            if (!renderer.isVisible)
            {
                Destroy(gameObject);
                return;
            }
        }
    }


}
