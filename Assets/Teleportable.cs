using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleportable : MonoBehaviour
{
    public Rigidbody2D rb;
    private bool isTeleporting;
    [SerializeField] private float teleportCooldown;
    public float lastTeleported { get; private set; }
    private Transform lastOutPortal;
    private Vector2 preSolveVelocity;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        isTeleporting = false;
    }

    // Update is called once per frame
    void Update()
    {
        lastTeleported -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        preSolveVelocity = rb.velocity;
    }

    public void Teleport(Transform inPortal, Transform outPortal, Rigidbody2D rb, Vector2 v0)
    {
        Vector2 newPos = outPortal.position + outPortal.up * 1f;
        Vector2 newVel = MapVelocity(v0, inPortal.up, outPortal.up);

        var prevMode = rb.collisionDetectionMode;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;

        rb.position = newPos;

        rb.velocity = newVel;

        StartCoroutine(RestoreCCDNextFixed(rb, prevMode));

        lastTeleported = teleportCooldown;
        lastOutPortal = outPortal;
        var nv = newVel;
        Debug.Log($"post map=({nv.x:F2},{nv.y:F2})");
        Debug.Log($"post map real=({rb.velocity.x:F2},{rb.velocity.y:F2})");
    }

    //Rotates vector 90 degrees counterclockwise
    static Vector2 Perp(Vector2 v) => new Vector2(-v.y, v.x);

    public static Vector2 MapVelocity(Vector2 vWorld, Vector2 normalIn, Vector2 normalOut)
    {
        normalIn = normalIn.normalized;
        normalOut = normalOut.normalized;
        Vector2 tangentIn = Perp(normalIn);
        Vector2 tangentOut = Perp(normalOut);

        float vTangent = Vector2.Dot(vWorld, tangentIn);
        float vNormal = Vector2.Dot(vWorld, normalIn);

        return vTangent * tangentOut + -vNormal * normalOut;
    }



    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Portal"))
        {
            Transform inPortal = other.gameObject.transform;
            Transform outPortal = inPortal.parent.GetChild(Mathf.Abs(inPortal.GetSiblingIndex() - 1));
            if ((lastTeleported < 0 || lastOutPortal != inPortal) && outPortal.gameObject.activeSelf) // || lastOutPortal != inPortal
            {
                var v = preSolveVelocity;
                float vT = Vector2.Dot(v, Perp(inPortal.up));
                float vN = Vector2.Dot(v, inPortal.up);
                Debug.Log($"pre v=({v.x:F2},{v.y:F2})  vT={vT:F2} vN={vN:F2}");
                Teleport(inPortal, outPortal, rb, preSolveVelocity);
            }
        }
    }

    private IEnumerator RestoreCCDNextFixed(Rigidbody2D rb, CollisionDetectionMode2D prevMode)
    {
        yield return new WaitForFixedUpdate();
        rb.collisionDetectionMode = prevMode;
    }
    
    /*
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Portal"))
        {
            Transform inPortal = other.gameObject.transform;
            Transform outPortal = inPortal.parent.GetChild(Mathf.Abs(inPortal.GetSiblingIndex() - 1));
            if (lastTeleported < 0 && outPortal.gameObject.activeSelf)
            {
                Teleport(inPortal, outPortal, rb);
            }
        }
    }
    */
}
