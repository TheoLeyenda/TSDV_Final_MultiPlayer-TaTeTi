using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ScreenManager : MonoBehaviour
{
    // Start is called before the first frame update
    public void LoadScene(string nameScene)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(nameScene);
    }
    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
