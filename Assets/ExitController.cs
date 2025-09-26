using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExitController : MonoBehaviour
{
    public Sprite[] sprites;
    private SpriteRenderer spriteRenderer;
    private int currentSprite = 0;
    public UnityEvent EndLevel;
    [SerializeField] private GameObject keyRequiredUI;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[currentSprite];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.GetComponent<PlayerController>().hasKey == true)
        {
            currentSprite = 1;
            spriteRenderer.sprite = sprites[currentSprite];
            EndLevel.Invoke();
        }
        else
        {
            if (keyRequiredUI != null) keyRequiredUI.SetActive(true);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.GetComponent<PlayerController>().hasKey == false)
        {
            if (keyRequiredUI != null) keyRequiredUI.SetActive(false);
        }
    }
}
