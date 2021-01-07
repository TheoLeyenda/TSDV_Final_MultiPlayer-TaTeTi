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
    void OnDisable()
    {
        InputFiled_Event.OnSettingInputField -= SetInputField;
    }

    void OnEnable()
    {
        InputFiled_Event.OnSettingInputField += SetInputField;
    }
    public void SetInputField(InputFiled_Event inputFiled_Event)
    {
        inputFieldAlias = inputFiled_Event.myInputField;
    }

    public void SetAliasPlayer(string aliasText) => aliasPlayer = inputFieldAlias.text;

    public string GetAliasPlayer() { return aliasPlayer; }

}
