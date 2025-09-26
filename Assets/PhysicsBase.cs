using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBase : MonoBehaviour
{
    public Vector2 velocity;
    public float gravityFactor;
    public float desiredx;
    public bool grounded;

    // Start is called before the first frame update
    void Start()
    {

    }

    public virtual void CollideHorizontal(Collider2D other)
    {

    }
    public virtual void CollideVertical(Collider2D other)
    {

    }

    public void Movement(Vector2 move, bool horizontal)
    {
        if (move.magnitude < 0.00001f) return;
        grounded = false;
        RaycastHit2D[] hits = new RaycastHit2D[16];
        int cnt = GetComponent<Rigidbody2D>().Cast(move, hits, move.magnitude + 0.01f);
        bool collision = false;
        for (int i = 0; i < cnt; i++)
        {
            if (Mathf.Abs(hits[i].normal.x) > 0.03f && horizontal)
            {
                collision = true;
                CollideHorizontal(hits[i].collider);
            }
            if (Mathf.Abs(hits[i].normal.y) > 0.03f && !horizontal)
            {
                if (hits[i].normal.y > 0.03f)
                {
                    grounded = true;
                    velocity.y = 0;
                }
                collision = true;
                CollideVertical(hits[i].collider);
            }
        }
        if (collision) return;
        transform.position += (Vector3)move;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 acceleration = 9.81f * Vector2.down * gravityFactor;
        velocity += acceleration * Time.fixedDeltaTime;
        velocity.x = desiredx;
        Vector2 move = velocity * Time.fixedDeltaTime;
        Movement(new Vector2(move.x, 0), true);
        Movement(new Vector2(0, move.y), false);

    }
}
