using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ENet;
using Server;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SceneManager : MonoBehaviour
{
    public class Client
    {
        public uint ID;
        public int ID_RoomConnect;
        public uint ID_InRoom;
        public string alias;
        public int input;
        public bool isMyTurn;
        public bool inputOK;

        public Client(uint iD, int iD_RoomConnect, uint iD_InRoom, string alias, int input, bool isMyTurn, bool inputOK)
        {
            ID = iD;
            ID_RoomConnect = iD_RoomConnect;
            ID_InRoom = iD_InRoom;
            this.alias = alias;
            this.input = input;
            this.isMyTurn = isMyTurn;
            this.inputOK = inputOK;
        }
    }
    public class RoomsData
    {
        public string nameRoom;
        public int currentPlayers;
        public int maxPlayers;
        public int ID_Room;

        public RoomsData(string nameRoom, int currentPlayers, int maxPlayers)
        {
            this.nameRoom = nameRoom;
            this.currentPlayers = currentPlayers;
            this.maxPlayers = maxPlayers;
        }
    }
    public GameObject myPlayerFactory;
    public GameObject otherPlayerFactory;

    private GameObject _myPlayer;
    private uint _myPlayerId;
    private Client _myClient;
    private string alias;
    private float delayRequestUpdateRooms = 5;
    private float auxDelayRequestUpdateRooms;

    private Host _client;
    private Peer _peer;
    private int _skipFrame = 0;
    private Dictionary<uint, GameObject> _players = new Dictionary<uint, GameObject>();
    private Dictionary<uint, Client> _clients = new Dictionary<uint, Client>();
   
    private List<RoomsData> _roomsData = new List<RoomsData>();
    [SerializeField] private List<Text> _roomsNames;
    const int channelID = 0;
    public static SceneManager instanceSceneManager;

    private bool solisitarCantidadDeRooms = true;

    private int countRooms = 0;
    /*void Awake()
    {
        if (instanceSceneManager == null)
        {
            instanceSceneManager = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }*/

    void Start()
    {
        Application.runInBackground = true;
        InitENet();
        _myPlayer = Instantiate(myPlayerFactory);
        alias = GameData.instanceGameData.aliasPlayer;
        Debug.Log(alias);
        auxDelayRequestUpdateRooms = delayRequestUpdateRooms;
        delayRequestUpdateRooms = 0;
    }

    void Update()
    {
        for (int i = 0; i < countRooms; i++)
            UpdateENet();

        if (++_skipFrame < 3)
            return;

        //SendPositionUpdate();
        if (solisitarCantidadDeRooms)
        {
            SendCountRoomsUpdate();
        }

        if (delayRequestUpdateRooms <= 0 && countRooms > 0)
        {
            SendDataRoomsUpdate();
            delayRequestUpdateRooms = auxDelayRequestUpdateRooms;
            Debug.Log("Solisitud de Update Rooms enviada.");
        }
        else
        {
            delayRequestUpdateRooms = delayRequestUpdateRooms - Time.deltaTime;
        }

        _skipFrame = 0;
    }
    void FixedUpdate()
    {

    }

    void OnDestroy()
    {
        _client.Dispose();
        ENet.Library.Deinitialize();
    }

    private void InitENet()
    {
        const string ip = "127.0.0.1";
        const ushort port = 6005;
        ENet.Library.Initialize();
        _client = new Host();
        Address address = new Address();

        address.SetHost(ip);
        address.Port = port;
        _client.Create();
        Debug.Log("Connecting");
        _peer = _client.Connect(address);
    }

    private void UpdateENet()
    {
        ENet.Event netEvent;

        if (_client.CheckEvents(out netEvent) <= 0)
        {
            if (_client.Service(15, out netEvent) <= 0)
                return;
        }

        switch (netEvent.Type)
        {
            case ENet.EventType.None:
                break;

            case ENet.EventType.Connect:
                Debug.Log("Client connected to server - ID: " + _peer.ID);
                SendLogin();
                break;

            case ENet.EventType.Disconnect:
                Debug.Log("Client disconnected from server");
                break;

            case ENet.EventType.Timeout:
                Debug.Log("Client connection timeout");
                break;

            case ENet.EventType.Receive:
                Debug.Log("Packet received from server - Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);
                ParsePacket(ref netEvent);
                netEvent.Packet.Dispose();
                break;
        }
    }

    enum PacketId : byte
    {
        LoginRequest = 1,
        LoginResponse = 2,
        LoginEvent = 3,
        InputUpdateRequest = 4,
        InputUpdateEvent = 5,
        PositionUpdateRequest = 6,
        PositionUpdateEvent = 7,
        LogoutEvent = 8,
        DataRoomsUpdateRequest = 9,
        DataRoomsUpdateEvent = 10,
        CountRoomsUpdateRequest = 11,
        CountRoomsUpdateEvent = 12,
    }

    //EJEMPLOS PARA MANDAR DATOS AL SERVER
    private void SendPositionUpdate()
    {
        var x = _myPlayer.GetComponent<Rigidbody2D>().position.x;
        var y = _myPlayer.GetComponent<Rigidbody2D>().position.y;

        var protocol = new Protocol();
        var buffer = protocol.Serialize((byte)PacketId.PositionUpdateRequest, _myPlayerId, x, y);
        var packet = default(Packet);
        packet.Create(buffer);
        _peer.Send(channelID, ref packet);
    }

    public void ConnectToRoom(int id_room)
    {
        _myClient.ID_RoomConnect = id_room;
    }
    public void SetAlias(string alias)
    {
        _myClient.alias = alias;
    }

    public void SendInputUpdate(int newInput)
    {
        var protocol = new Protocol();
        var buffer = protocol.Serialize((byte)PacketId.InputUpdateRequest, _myPlayerId, _myClient.ID, _myClient.ID_RoomConnect,
                                  _myClient.ID_InRoom, _myClient.alias, _myClient.input, _myClient.isMyTurn, _myClient.inputOK);
        var packet = default(Packet);
        packet.Create(buffer);
        _peer.Send(channelID, ref packet);
    }

    public void SendCountRoomsUpdate()
    {
        var protocol = new Protocol();
        var buffer = protocol.Serialize((byte)PacketId.CountRoomsUpdateRequest, _myPlayerId);
        var packet = default(Packet);
        packet.Create(buffer);
        _peer.Send(channelID, ref packet);
    }
    public void SendDataRoomsUpdate()
    {
        var protocol = new Protocol();
        var buffer = protocol.Serialize((byte)PacketId.DataRoomsUpdateRequest, _myPlayerId);
        var packet = default(Packet);
        packet.Create(buffer);
        _peer.Send(channelID, ref packet);
    }

    

    private void SendLogin()
    {
        Debug.Log("SendLogin");
        var protocol = new Protocol();
        var buffer = protocol.Serialize((byte)PacketId.LoginRequest, 0);
        var packet = default(Packet);
        packet.Create(buffer);
        _peer.Send(channelID, ref packet);
    }

    private void ParsePacket(ref ENet.Event netEvent)
    {
        var readBuffer = new byte[1024];
        var readStream = new MemoryStream(readBuffer);
        var reader = new BinaryReader(readStream);

        readStream.Position = 0;
        netEvent.Packet.CopyTo(readBuffer);
        var packetId = (PacketId)reader.ReadByte();

        Debug.Log("ParsePacket received: " + packetId);

        if (packetId == PacketId.LoginResponse)
        {
            _myPlayerId = reader.ReadUInt32();
            _myClient = new Client(_myPlayerId, -1, 0, alias, -1, false, false);

            Debug.Log("MyPlayerId: " + _myPlayerId);
        }
        else if (packetId == PacketId.LoginEvent)
        {
            var playerId = reader.ReadUInt32();
            Debug.Log("OtherPlayerId: " + playerId);
            SpawnOtherPlayer(playerId);
        }
        else if (packetId == PacketId.PositionUpdateEvent)
        {
            var playerId = reader.ReadUInt32();
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            UpdatePosition(playerId, x, y);
        }
        else if (packetId == PacketId.LogoutEvent)
        {
            var playerId = reader.ReadUInt32();
            if (_players.ContainsKey(playerId))
            {
                Destroy(_players[playerId]);
                _players.Remove(playerId);
            }
            if (_clients.ContainsKey(playerId))
            {
                _clients.Remove(playerId);
            }
        }
        else if (packetId == PacketId.InputUpdateEvent)
        {
            // ACA RESIVO LA RESPUESTA DE INPUT DEL JUGADOR.
            //INPUTS EN GENERAL:
            //-1 = NADA NUEVO.
            //0 = EL CLIENTE INTENTA JUGAR EN LA POSICION 0 DEL TABLERO.
            //1 = EL CLIENTE INTENTA JUGAR EN LA POSICION 1 DEL TABLERO.
            //2 = EL CLIENTE INTENTA JUGAR EN LA POSICION 2 DEL TABLERO.
            //3 = EL CLIENTE INTENTA JUGAR EN LA POSICION 3 DEL TABLERO.
            //4 = EL CLIENTE INTENTA JUGAR EN LA POSICION 4 DEL TABLERO.
            //5 = EL CLIENTE INTENTA JUGAR EN LA POSICION 5 DEL TABLERO.
            //6 = EL CLIENTE INTENTA JUGAR EN LA POSICION 6 DEL TABLERO.
            //7 = EL CLIENTE INTENTA JUGAR EN LA POSICION 7 DEL TABLERO.
            //8 = EL CLIENTE INTENTA JUGAR EN LA POSICION 8 DEL TABLERO.
            //9 = LISTO PARA JUGAR. (Si los dos estan listos empieza la partida)
            //10 = SALIR DE LA ROOM (LLAMA A LA FUNCION DisconnectClient(int ID_InRoom) Y SACA AL CLIENTE ESPECIFICADO DE LA ROOM)
            //11 = INPUT QUE INFORMA AL JUGADOR QUE GANO LA PARTIDA.
            //12 = INPUT QUE INFORMA AL JUGADOR QUE PERDIO LA PARTIDA.
            //13 = INPUT QUE INFORMA AL JUGADOR QUE EMPATO LA PARTIDA.
            //14 = JUGADA VALIDA.
            //15 = JUGADA INVALIDA.
            //16 = CONECTAR A LA ROOM

            var playerId = reader.ReadUInt32();
            var ID = reader.ReadUInt32(); // public uint ID;
            var ID_RoomConnect = reader.ReadInt32(); // public int ID_RoomConnect;
            var ID_InRoom = reader.ReadUInt32(); // public uint ID_InRoom;
            var alias = reader.ReadString(); // public string alias;
            var input = reader.ReadInt32(); // public int input;
            var isMyTurn = reader.ReadBoolean(); // public bool isMyTurn;
            var inputOK = reader.ReadBoolean(); // public bool inputOK;

            // ACA ACTUO EN BASE A LO QUE ME DEVUELVE EL SERVIDOR.

            //FALTA HACER LA INTERFAZ PARA QUE EL JUGADOR SE PUEDA UNIR A LAS ROOMS, MOSTRAR SI GANO O PERDIO EN UNA ROOM 
            //Y VINCULAR LOS INPUTS DE LOS CLIENTES CON LOS INPUTS DEL SERVIDOR PARA QUE ESTOS PUEDAN
            //JUGAR, SALIR, HACER TODAS LAS COSAS QUE ESTAN PERMITIDAS EN EL JUEGO Y MOSTRAR SUS DISPLAY 
            //CORRESPONDIENTES DEPENDIENDO DEL INPUT DEVUELTO.

            //FALTA TERMINAR ESTA PARTE HACIENDO QUE EL CLIENTE ACTUE EN CONSECUENCIA DEL INPUT DEVUELTO POR EL SERVIDOR.
        }
        else if (packetId == PacketId.CountRoomsUpdateEvent)
        {
            var playerId = reader.ReadUInt32();
            var _countRooms = reader.ReadInt32();
            countRooms = _countRooms;

            Debug.Log(countRooms);

            if (countRooms > 0)
                solisitarCantidadDeRooms = false;
            else
                solisitarCantidadDeRooms = true;
            
            _roomsData.Clear();
            for (int i = 0; i < countRooms; i++)
            {
                _roomsData.Add(new RoomsData("Sin nombre", 0, 0));
            }

        }
        else if (packetId == PacketId.DataRoomsUpdateEvent)
        {
            var playerId = reader.ReadUInt32();
            var nameRoom = reader.ReadString();
            var countPlayersInThisRoom = reader.ReadInt32();
            var maxPlayersInThisRoom = reader.ReadInt32();
            var ID_Room = reader.ReadInt32();


            _roomsData[ID_Room].nameRoom = nameRoom;
            _roomsData[ID_Room].currentPlayers = countPlayersInThisRoom;
            _roomsData[ID_Room].maxPlayers = maxPlayersInThisRoom;

            _roomsNames[ID_Room].text = "Sala " + (ID_Room + 1) + "\n" + "Jugadores conectados " + _roomsData[ID_Room].currentPlayers + "/" + _roomsData[ID_Room].maxPlayers;
        }
    }

    private void SpawnOtherPlayer(uint playerId)
    {
        if (playerId == _myPlayerId)
            return;
        var newPlayer = Instantiate(otherPlayerFactory);
        newPlayer.transform.position = newPlayer.GetComponent<Rigidbody2D>().position + new Vector2(Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f));
        Debug.Log("Spawn other object " + playerId);
        _players[playerId] = newPlayer;
        _clients[playerId] = new Client(playerId, - 1, 0, "sin alias", -1, false, false);
    }

    private void UpdatePosition(uint playerId, float x, float y)
    {
        if (playerId == _myPlayerId)
            return;

        Debug.Log("UpdatePosition " + playerId);
        _players[playerId].transform.position = new Vector2(x, y);
    }
}
