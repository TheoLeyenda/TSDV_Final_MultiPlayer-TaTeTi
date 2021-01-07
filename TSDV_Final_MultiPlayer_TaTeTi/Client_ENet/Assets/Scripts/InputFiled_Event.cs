using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class InputFiled_Event : MonoBehaviour
{
    public InputField myInputField;
    private PlayerData pd;
    // Start is called before the first frame update
    public static event Action<InputFiled_Event> OnSettingInputField;
    
    void Start()
    {
        pd = PlayerData.instancePlayerData;
        SendEventInputField();
        myInputField.onValueChanged.AddListener(pd.SetAliasPlayer);
        myInputField.onEndEdit.AddListener(pd.SetAliasPlayer);
    }
    public void SendEventInputField()
    {
        if (OnSettingInputField != null)
            OnSettingInputField(this);
    }
}
