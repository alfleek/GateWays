using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFinish : MonoBehaviour
{
    [SerializeField] GameObject levelFinish;
    [SerializeField] GameObject nextLevelButton;
    private int currentSceneIndex;
    private int nextSceneIndex;

    void Start()
    {

    }
    public void Finish()
    {
        levelFinish.SetActive(true);
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        nextSceneIndex = currentSceneIndex + 1;
        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings) nextLevelButton.SetActive(false);
        Time.timeScale = 0;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
        Time.timeScale = 1;
    }

    public void Settings()
    {

    }

    public void Restart()
    {
        SceneManager.LoadScene(currentSceneIndex);
        Time.timeScale = 1;
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(nextSceneIndex);
        Time.timeScale = 1;
    }
}
