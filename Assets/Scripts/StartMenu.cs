using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public string mainSceneName = "SampleScene";

    public void OnStart()
    {
        SceneManager.LoadScene(mainSceneName, LoadSceneMode.Single);
    }

    public void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
