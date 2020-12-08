using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
namespace FinalMultiPlayer
{
    public class AutoLobby : MonoBehaviourPunCallbacks
    {
        public Button ConnectButton;
        public Button JoinRandomButton;
        public TextMeshProUGUI Log;
        public TextMeshProUGUI playerCountText;
        public int playersCount;

        public byte maxPlayerPerRoom = 2;
        // Start is called before the first frame update

        public void Connect()
        {
            if (!PhotonNetwork.IsConnected)
            {
                if (PhotonNetwork.ConnectUsingSettings())
                {
                    Log.text += "\nConnected to server";
                }
                else
                {
                    Log.text += "\nFailing connecting to server";
                }
            }
        }

        public override void OnConnectedToMaster()
        {
            ConnectButton.interactable = false;
            JoinRandomButton.interactable = true;
        }

        public void JoinRandom()
        {
            if (PhotonNetwork.JoinRandomRoom())
            {
                Log.text += "\nJoinned room";
            }
            else
            {
                Log.text += "\nFail joining room";
            }
        }
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Log.text += "\nNo rooms to join, creating one...";

            if (PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions() { MaxPlayers = maxPlayerPerRoom }))
            {
                Log.text += "\nRoom created";
            }
            else
            {
                Log.text += "\nFail creting room";
            }
        }

        public override void OnJoinedRoom()
        {
            Log.text += "\nJoinned";
            JoinRandomButton.interactable = false;
        }

        private void FixedUpdate()
        {
            if (PhotonNetwork.CurrentRoom != null)
            {
                playersCount = PhotonNetwork.CurrentRoom.PlayerCount;

            }
            playerCountText.text = "Players connected: " + playersCount + "/" + maxPlayerPerRoom;
        }
    }
}
