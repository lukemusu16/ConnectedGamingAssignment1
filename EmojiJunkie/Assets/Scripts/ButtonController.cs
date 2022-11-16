using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    public void GoToMenu()
    {
        SceneManager.LoadScene("Welcome_Scene");
    }

    public void GoToGame()
    {
        SceneManager.LoadScene("Game_Scene");
    }

    public void GoToStore()
    {
        SceneManager.LoadScene("Store_Scene");
    }
}
