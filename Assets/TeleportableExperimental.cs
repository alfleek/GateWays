using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportableExperimental : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float teleportCooldown = 0.10f;
    [SerializeField] private float extraSkin = 0.01f;      // tiny epsilon after trigger/geometry
    [SerializeField] private bool requireForwardEntry = true;

    private CapsuleCollider2D capsule;                     // player collider (1x2 Vertical)
    public float lastTeleported;
    private bool suppressPortalsUntilFixed;                // prevent immediate re-trigger

    void Reset() => rb = GetComponent<Rigidbody2D>();

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        capsule = GetComponent<CapsuleCollider2D>();
        if (!capsule) Debug.LogWarning("Teleportable: CapsuleCollider2D recommended for precise clearance.");
    }

    void Update() => lastTeleported -= Time.deltaTime;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (suppressPortalsUntilFixed) return;
        if (!other.CompareTag("Portal")) return;

        Transform inPortal = other.transform;
        Transform outPortal = inPortal.parent.GetChild(Mathf.Abs(inPortal.GetSiblingIndex() - 1));
        if (lastTeleported > 0f || !outPortal.gameObject.activeInHierarchy) return;
        if (requireForwardEntry && Vector2.Dot(rb.velocity, (Vector2)inPortal.up) <= 0f) return;

        TeleportImmediate(inPortal, outPortal);
    }

    private void TeleportImmediate(Transform inPortal, Transform outPortal)
    {
        var v = rb.velocity;
        float vT = Vector2.Dot(v, Perp(inPortal.up));
        float vN = Vector2.Dot(v, inPortal.up);
        Debug.Log($"pre v=({v.x:F2},{v.y:F2})  vT={vT:F2} vN={vN:F2}");

        Vector2 n = outPortal.up;

        // --- 1) Compute robust spawn offset ---
        float portalForward = ComputePortalForwardExtent(outPortal);
        float playerClear   = ComputePlayerClearanceAlong(n);
        float offset        = portalForward + playerClear + extraSkin;

        Vector2 targetPos   = (Vector2)outPortal.position + n * offset;
        Vector2 newVel      = MapVelocity(rb.velocity, inPortal.up, outPortal.up);

        // --- 2) Atomic move (no physics between set position and next solve) ---
        // Disable *portal* re-triggers while we settle
        suppressPortalsUntilFixed = true;

        // Temporarily disable our colliders during the warp
        var cols = GetComponents<Collider2D>();
        foreach (var c in cols) c.enabled = false;

        rb.simulated = false;
        transform.position = targetPos;
        // If you rotate through portals, also set transform.rotation here.
        Physics2D.SyncTransforms();

        // Re-enable colliders so we can compute distances to solids
        foreach (var c in cols) c.enabled = true;
        rb.simulated = true;

        // --- 3) Post-placement clearance solve (no triggers) ---
        ResolveOverlapsOnce(cols);

        // --- 4) Restore velocity AFTER we’re guaranteed clear ---
        rb.velocity = newVel;
        lastTeleported = teleportCooldown;

        var nv = newVel;
        Debug.Log($"post map=({nv.x:F2},{nv.y:F2})");


        // lift suppression next physics tick
        StartCoroutine(ClearPortalSuppressionNextFixed());
    }

    private IEnumerator ClearPortalSuppressionNextFixed()
    {
        yield return new WaitForFixedUpdate();
        suppressPortalsUntilFixed = false;
    }

    // Push out of any solid colliders using minimum translation vectors
    private void ResolveOverlapsOnce(Collider2D[] myCols)
    {
        // Build a filter that ignores triggers
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.useLayerMask = false;

        // Small iteration cap to avoid edge cases; usually resolves in 1 step
        const int MAX_ITERS = 3;

        for (int iter = 0; iter < MAX_ITERS; iter++)
        {
            bool moved = false;

            foreach (var myCol in myCols)
            {
                if (!myCol || !myCol.enabled) continue;

                // Gather overlapping solids
                List<Collider2D> hits = new List<Collider2D>(8);
                Physics2D.OverlapCollider(myCol, filter, hits);

                foreach (var other in hits)
                {
                    if (!other || other.isTrigger) continue;

                    // Compute min translation vector (MTV) to separate colliders
                    ColliderDistance2D dist = myCol.Distance(other);
                    if (dist.isOverlapped && dist.distance < 0f)
                    {
                        // Move along the MTV (normal * distance)
                        Vector2 mtv = dist.normal * dist.distance; // distance is negative when overlapping
                        rb.position += mtv;     // positional correction
                        moved = true;
                    }
                }
            }

            if (!moved) break; // done
            Physics2D.SyncTransforms();
        }
    }

    // How far the exit portal trigger protrudes in +Y (its local forward).
    private float ComputePortalForwardExtent(Transform outPortal)
    {
        var box = outPortal.GetComponent<BoxCollider2D>();
        if (!box) return 0.05f; // fallback
        float localForwardExtent = box.offset.y + box.size.y * 0.5f; // plane at local y=0
        return Mathf.Abs(localForwardExtent * outPortal.lossyScale.y);
    }

    // Exact clearance along direction n for a vertical capsule aligned to transform.up
    private float ComputePlayerClearanceAlong(Vector2 n)
    {
        if (capsule)
        {
            float halfH = capsule.size.y * 0.5f * Mathf.Abs(transform.lossyScale.y);
            float radius = capsule.size.x * 0.5f * Mathf.Abs(transform.lossyScale.x); // vertical capsule: width = diameter
            float cos = Mathf.Abs(Vector2.Dot(transform.up, n)); // alignment with exit normal
            return cos * Mathf.Max(0f, halfH - radius) + radius; // 0.5..1.0 for your 1x2
        }

        // Fallback using bounds projection if capsule missing
        var any = GetComponent<Collider2D>();
        if (any)
        {
            var b = any.bounds.extents;
            return Mathf.Abs(n.x) * b.x + Mathf.Abs(n.y) * b.y;
        }
        return 0.5f;
    }

    // --- Your velocity mapping, unchanged ---
    static Vector2 Perp(Vector2 v) => new Vector2(-v.y, v.x);
    public static Vector2 MapVelocity(Vector2 vWorld, Vector2 normalIn, Vector2 normalOut)
    {
        normalIn = normalIn.normalized;
        normalOut = normalOut.normalized;
        Vector2 tangentIn  = Perp(normalIn);
        Vector2 tangentOut = Perp(normalOut);
        float vTangent = Vector2.Dot(vWorld, tangentIn);
        float vNormal  = Vector2.Dot(vWorld, normalIn);
        return vTangent * tangentOut + (-vNormal) * normalOut;
    }
}
