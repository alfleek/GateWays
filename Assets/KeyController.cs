using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyController : MonoBehaviour
{
    [SerializeField] private GameObject collectedKeyUI;
    [SerializeField] private string playerTag = "Player";
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        other.GetComponent<PlayerController>().hasKey = true;

        if (collectedKeyUI != null) collectedKeyUI.SetActive(true);

        gameObject.SetActive(false);
    }
}
