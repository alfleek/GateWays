using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairCursor : MonoBehaviour
{
    public Sprite[] sprites;
    private SpriteRenderer spriteRenderer;
    private int currentSprite = 0;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[currentSprite];
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mouseCursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mouseCursorPos;
        GameObject portals = GameObject.Find("Portals");
        bool greenActive = portals.transform.GetChild(0).gameObject.activeSelf;
        bool redActive = portals.transform.GetChild(1).gameObject.activeSelf;
        if (!greenActive && redActive) currentSprite = 1;
        else if (greenActive && !redActive) currentSprite = 2;
        else if (greenActive && redActive) currentSprite = 3;
        else currentSprite = 0;
        spriteRenderer.sprite = sprites[currentSprite];
    }

}
