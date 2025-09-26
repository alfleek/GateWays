using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMenu : MonoBehaviour
{
    public void Enterlevel(int level)
    {
        SceneManager.LoadScene(level);
    }
}
