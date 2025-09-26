using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length;
    private float startPos;
    private float startY;
    public GameObject Camera;
    public float parallaxEffect;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position.x;
        startY = transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void Update()
    {
        float temp = (Camera.transform.position.x * (1 - parallaxEffect));
        float distX = (Camera.transform.position.x * parallaxEffect);
        float distY = (Camera.transform.position.y * parallaxEffect);

        transform.position = new Vector3(startPos + distX, startY + distY, transform.position.z);

        if (temp > startPos + length) startPos += length;
        else if (temp < startPos - length) startPos -= length;
    }
}
