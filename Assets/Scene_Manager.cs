using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Scene_Manager : MonoBehaviour {

    public void LoadScene(string Destination)
    {
        SceneManager.UnloadScene(SceneManager.GetActiveScene());
        SceneManager.LoadScene(Destination);
    }

    public void LoadCreatePassword()
    {
        LoadScene("Set_Shake");
    }

    public void LoadTestPassword()
    {
        LoadScene("Test_Shake");
    }

    public void LoadMainMenu()
    {
        LoadScene("MainMenu");
    }

    public void LoadSuccess()
    {
        LoadScene("Successful_Login");
    }
    public void exitApplication(){
        Application.Quit();
    }
    public void LoadFail(){
        LoadScene("Unsuccessful_Login");
    }
}
