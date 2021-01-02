using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerData : MonoBehaviour
{
    // Start is called before the first frame update
    public static PlayerData instancePlayerData;
    private string aliasPlayer;
    [SerializeField]
    private InputField inputFieldAlias;

    void Awake()
    {
        if (instancePlayerData == null)
        {
            instancePlayerData = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetAliasPlayer() => aliasPlayer = inputFieldAlias.text;

    public string GetAliasPlayer() { return aliasPlayer; }

}
