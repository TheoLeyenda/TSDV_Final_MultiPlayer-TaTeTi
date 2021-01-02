using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public void LoadScene(string name)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(name);
    }
}
