using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firing : MonoBehaviour
{
    public GameObject PortalArrow;
    public float firingDelay;
    private float lastFired;
    [SerializeField] private PortalController listener;
    public Animator animator;
    public Camera mainCam;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        lastFired -= Time.deltaTime;
        if (Input.GetMouseButton(0) && lastFired < 0)
        {
            createArrow(true);
        }
        else if (Input.GetMouseButton(1) && lastFired < 0)
        {
            createArrow(false);
        }
    }

    private void createArrow(bool isGreen)
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        if (mousePos.x > 0 != gameObject.GetComponent<PlayerController>().facingRight) gameObject.GetComponent<PlayerController>().Flip();
        lastFired = firingDelay;
        animator.SetTrigger("Shoot");

        GameObject arrowGO = Instantiate(PortalArrow, transform.position, Quaternion.identity);
        var arrow = arrowGO.GetComponent<ArrowController>();
        if (arrow != null)
        {
            arrow.SetGreen(isGreen);

            arrow.Impact += listener.OnPortalHit;

            arrowGO.AddComponent<UnsubOnDestroy>()
                   .Init(() => arrow.Impact -= listener.OnPortalHit);
        }
    } 

    public class UnsubOnDestroy : MonoBehaviour {
        private System.Action _onDestroy;
        public UnsubOnDestroy Init(System.Action a) { _onDestroy = a; return this; }
        private void OnDestroy() => _onDestroy?.Invoke();
    }
}
