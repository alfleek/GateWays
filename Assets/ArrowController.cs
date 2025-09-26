using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct ArrowImpactData
{
    public Vector2 point;
    public Vector2 normal;
    public GameObject hitObject;
    public bool isGreen;
    public ArrowController arrow;

    public override string ToString()
    {
        return $"Point (X: {point.x}, Y: {point.y}),\nNormal (X: {normal.x}, Y: {normal.y}),\nIsGreen: {isGreen}";
    }
}
public class ArrowController : MonoBehaviour
{
    private Vector3 mousePos;
    private Camera mainCam;
    private Rigidbody2D rb;
    public float force;
    private bool isGreen;
    public event Action<ArrowImpactData> Impact;


    // Start is called before the first frame update
    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        rb = GetComponent<Rigidbody2D>();
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - transform.position;
        rb.velocity = new Vector2(direction.x, direction.y).normalized * force;
        float rot = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rot);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetGreen(bool green)
    {
        isGreen = green;
    }

    private bool impacted;
    void OnCollisionEnter2D(Collision2D c)
    {
        if (impacted) return;

        if (c.collider.CompareTag("Portal Surface"))
        {
            var contact = c.GetContact(0);
            Vector2 hitPoint = contact.point;
            Vector2 surfaceNormal = contact.normal; 

            Impact?.Invoke(new ArrowImpactData
            {
                point = hitPoint,
                normal = surfaceNormal,
                hitObject = c.collider.gameObject,
                isGreen = isGreen,
                arrow = this
            });
        }
        impacted = true;
        // prevent extra hits before destruction
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;
        Destroy(gameObject, 0.05f);

    }

}
