using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public struct PortalPlacementData
{
    public bool isPlaceable;
    public Vector2 point;

}


public class PortalController : MonoBehaviour
{
    public LayerMask PortalableLayers;
    public LayerMask BlockingLayers;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {   
        if (Input.GetKeyDown("r"))
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void OnPortalHit(ArrowImpactData data)
    {
        Transform portal = transform.GetChild(data.isGreen ? 0 : 1);
        Collider2D portalCollider = portal.gameObject.GetComponent<Collider2D>();
        if (portalCollider != null) portalCollider.enabled = false;
        PortalPlacementData correctedData = PortalReposition(data);
        if (correctedData.isPlaceable)
        {
            portal.position = correctedData.point;
            portal.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, data.normal));
            portal.gameObject.SetActive(true);
        }
        if (portalCollider != null) portalCollider.enabled = true;
    }

    private PortalPlacementData PortalReposition(ArrowImpactData data)
    {
        PortalPlacementData correctedData = new PortalPlacementData { };
        correctedData.isPlaceable = false;
        correctedData.point = data.point;
        Vector2 exteriorPoint = data.point + data.normal * 0.1f;
        Vector2 interiorPoint = data.point + data.normal * -0.1f;
        //Assuming normal is 'up'
        Vector2 leftDirect = new Vector2(-data.normal.y, data.normal.x);
        Vector2 rightDirect = new Vector2(data.normal.y, -data.normal.x);
        Vector2 leftFurthest;
        Vector2 rightFurthest;
        Vector2 temp;
        //Left
        leftFurthest = FarthestValidPoint(exteriorPoint, leftDirect, BlockingLayers, false) + data.normal * -0.1f;
        temp = FarthestValidPoint(interiorPoint + leftDirect * 2, -leftDirect, PortalableLayers, true) + data.normal * 0.1f;
        if (Vector2.Dot(leftFurthest, leftDirect) > Vector2.Dot(temp, leftDirect)) leftFurthest = temp;
        //Right
        rightFurthest = FarthestValidPoint(exteriorPoint, rightDirect, BlockingLayers, false) + data.normal * -0.1f;
        temp = FarthestValidPoint(interiorPoint + rightDirect * 2, -rightDirect, PortalableLayers, true) + data.normal * 0.1f;
        if (Vector2.Dot(rightFurthest, rightDirect) > Vector2.Dot(temp, rightDirect)) rightFurthest = temp;

        if (Vector2.Distance(leftFurthest, rightFurthest) >= 2f)
        {
            correctedData.isPlaceable = true;
            if (Vector2.Distance(leftFurthest, data.point) < 1) correctedData.point = leftFurthest + rightDirect;
            else if (Vector2.Distance(rightFurthest, data.point) < 1) correctedData.point = rightFurthest + leftDirect;
        }
        return correctedData;
    }

    private Vector2 FarthestValidPoint(Vector2 start, Vector2 direct, LayerMask layers, bool layersWanted)
    {
        RaycastHit2D hit = Physics2D.Raycast(start, direct, 2f, layers, 0, 0);
        if (hit) return hit.point;
        else return (start + direct * 2);

    }

    void OnTriggerEnter2D(Collider2D other)
    {

    }

}
