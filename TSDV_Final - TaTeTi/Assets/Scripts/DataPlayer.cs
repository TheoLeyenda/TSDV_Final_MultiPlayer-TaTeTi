using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System;
using TMPro;
public class DataPlayer : MonoBehaviourPunCallbacks, IPunObservable
{
    public string alias { set; get; }
    public StatePlayer statePlayer { set; get; }
    public int team { set; get; }
    public enum StatePlayer
    {
        None,
        Win,
        Lose,
    }

    private TMP_InputField InputField_AliasPlayer;
    private DisplayDataPlayers displayDataPlayers;
    private void Start()
    {
        GameObject go_InputFiled = GameObject.Find("InputField (TMP)");
        if (go_InputFiled != null)
        {
            InputField_AliasPlayer = go_InputFiled.GetComponent<TMP_InputField>();
            InputField_AliasPlayer.onEndEdit.AddListener(SetAlias);
        }
        displayDataPlayers = DisplayDataPlayers.instanceDisplayDataPlayers;
    }
    public void SetAlias(string algo)
    {
        alias = InputField_AliasPlayer.text;
        SetDisplayPlayer();
    }
    
    public void Update()
    {
        CheckDesconectedOtherUser();
    }

    #region DisplayDataUsers
    private bool settingInformation = false;
    public void SetDisplayPlayer()
    {
        //Debug.Log(settingInformation);
        if (!settingInformation)
        {
            //Debug.Log("SETEADA LA INFO");
            if(team == (int)Ficha.TeamFicha.O)
                displayDataPlayers.textNamePlayerOne.text = "Equipo O = " + alias;
            else if(team == (int)Ficha.TeamFicha.X)
                displayDataPlayers.textNamePlayerOne.text = "Equipo X = " + alias;

            displayDataPlayers.namePlayer1 = alias;
            //Debug.Log(team);
            if(team == (int)Ficha.TeamFicha.O || team == (int)Ficha.TeamFicha.X)
                displayDataPlayers.myTeam = team;

            displayDataPlayers.namePlayerOneAssigned = true;
            settingInformation = true;
        }
    }
    public void CheckDesconectedOtherUser()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers
            && displayDataPlayers.namePlayerOneAssigned && displayDataPlayers.namePlayerTwoAssigned)
        {
            displayDataPlayers.namePlayerTwoAssigned = false;
            displayDataPlayers.allAssigned = false;
            if (team == (int)Ficha.TeamFicha.O)
                displayDataPlayers.textNamePlayerTwo.text = "Equipo X = Esperando jugador...";
            else
                displayDataPlayers.textNamePlayerTwo.text = "Equipo O = Esperando jugador...";
        }
    }
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // SOY EL CLIENTE ?
        {
            //Debug.Log("ENTRE AL IsWriting");
            stream.SendNext(alias);
            stream.SendNext(settingInformation);

            if (PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                if (team == (int)Ficha.TeamFicha.O)
                    displayDataPlayers.textNamePlayerTwo.text = "Equipo X = Esperando jugador...";
                else
                    displayDataPlayers.textNamePlayerTwo.text = "Equipo O = Esperando jugador...";
            }
            //else if (settingInformation)
            //{
                //displayDataPlayers.namePlayerOneAssigned = true;
                //displayDataPlayers.namePlayerTwoAssigned = true;
            //}
            
        }
        else // O SOY UN AVATAR ?
        {
            //Debug.Log("ENTRE AL AVATAR...");
            string otherAlias = (string)stream.ReceiveNext();
            bool otherSettingInformation = (bool)stream.ReceiveNext();

            if (displayDataPlayers != null && otherSettingInformation)
            {
                displayDataPlayers.namePlayer2 = otherAlias;
                displayDataPlayers.namePlayerTwoAssigned = true;
            }

            if (displayDataPlayers != null && PhotonNetwork.CurrentRoom.PlayerCount >= PhotonNetwork.CurrentRoom.MaxPlayers && settingInformation)
            {
                displayDataPlayers.namePlayerOneAssigned = true;
                displayDataPlayers.namePlayerTwoAssigned = true;
            }
            
        }
    }
    #endregion
}
