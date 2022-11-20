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
        DatabaseManager dm = new DatabaseManager();
        dm.TrackClicks("game");
    }

    public void GoToStore()
    {
        SceneManager.LoadScene("Store_Scene");
        DatabaseManager dm = new DatabaseManager();
        dm.TrackClicks("store");
    }

    public void ClosePrivacyPolicy()
    {
        GameObject pp = GameObject.Find("Canvas").transform.Find("Privacy").gameObject;
        pp.SetActive(false);
        Destroy(pp);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
