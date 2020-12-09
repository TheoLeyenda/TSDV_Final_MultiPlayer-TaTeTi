using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;
public class GestorDePhoton : MonoBehaviourPunCallbacks
{
    public string NamePlayer;
    private LogicaJugador myPlayer;
    public GameObject Camvas;
    public Button CicruloButton;
    public Button EquisButton;
    //public bool isConnectedRoom { set; get; }


    public static GestorDePhoton instanceGestorDePhoton;

    void Awake()
    {
        if (instanceGestorDePhoton == null)
        {
            instanceGestorDePhoton = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        //Conecta el cliente con los settings actuales de photon
        PhotonNetwork.ConnectUsingSettings();
        Camvas.SetActive(false);
        //isConnectedRoom = false;
    }
    //se ejecuta siempre que estes conectado al master server (no se si se ejecuta una vez o siempre que estes conectado al master)

    public override void OnConnectedToMaster()
    {
        //entra al lobby
        PhotonNetwork.JoinLobby();
    }
    //se ejecuta cuando estas entrando al lobby (solo una vez)
    public override void OnJoinedLobby()
    {
        //crea una room
        PhotonNetwork.JoinOrCreateRoom("Cuarto", new RoomOptions { MaxPlayers = 2 }, TypedLobby.Default);
    }

    //se ejecuta cuando entras a la room (solo una vez)
    public override void OnJoinedRoom()
    {
        //instancia al jugador buscando el archivo "jugador" en la carpeta de resource
        GameObject go = PhotonNetwork.Instantiate(NamePlayer, transform.position, Quaternion.identity);
        LogicaJugador logicaJugador = go.GetComponent<LogicaJugador>();
        myPlayer = logicaJugador;
        CicruloButton.onClick.AddListener(logicaJugador.SeleccionarFichaO);
        EquisButton.onClick.AddListener(logicaJugador.SeleccionarFichaX);
        Camvas.SetActive(true);
        //isConnectedRoom = true;
    }
}
