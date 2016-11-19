using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Scene_Manager : MonoBehaviour {

    void LoadScene(string Destination)
    {
        SceneManager.LoadScene(Destination);
    }

    void LoadCreatePassword()
    {
        LoadScene("Set_Shake");
    }

    void LoadTestPassword()
    {
        LoadScene("Test_Shake");
    }

    void LoadMainMenu()
    {
        LoadScene("MainMenu");
    }

    void LoadSuccess()
    {
        LoadScene("Successful_Login");
    }
}
