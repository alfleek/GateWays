using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync("Level 1");
    }

    void Update()
    {
        transform.position += new Vector3(10 * Time.deltaTime, 0, 0);
    }
}
