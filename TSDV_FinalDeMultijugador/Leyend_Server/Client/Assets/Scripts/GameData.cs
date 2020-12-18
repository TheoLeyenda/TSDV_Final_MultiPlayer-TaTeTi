using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameData : MonoBehaviour
{
    // Start is called before the first frame update
    public string aliasPlayer;
    [SerializeField] InputField inputAliasPlayer;
    public static GameData instanceGameData;
    void Awake()
    {
        if (instanceGameData == null)
        {
            instanceGameData = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetAlias()
    {
        aliasPlayer = inputAliasPlayer.text;
    }
}
