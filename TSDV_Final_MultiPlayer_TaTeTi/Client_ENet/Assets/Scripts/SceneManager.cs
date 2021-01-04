using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ENet;
using Server;
using UnityEngine.UI;
using Random = UnityEngine.Random;

//PROYECTO QUE ANDA.

public class SceneManager : MonoBehaviour
{
    public const int NADA_NUEVO = -1;
    public const int ID_ROOM_INVALIDO = -1;
    public const int JUGAR_POSICION_0_DEL_TABLERO = 0;
    public const int JUGAR_POSICION_1_DEL_TABLERO = 1;
    public const int JUGAR_POSICION_2_DEL_TABLERO = 2;
    public const int JUGAR_POSICION_3_DEL_TABLERO = 3;
    public const int JUGAR_POSICION_4_DEL_TABLERO = 4;
    public const int JUGAR_POSICION_5_DEL_TABLERO = 5;
    public const int JUGAR_POSICION_6_DEL_TABLERO = 6;
    public const int JUGAR_POSICION_7_DEL_TABLERO = 7;
    public const int JUGAR_POSICION_8_DEL_TABLERO = 8;
    public const int LISTO_PARA_JUGAR = 9;
    public const int SALIR_DE_LA_ROOM = 10;
    public const int INPUT_GANO_PARTIDA = 11;
    public const int INPUT_PERDIO_PARTIDA = 12;
    public const int INPUT_EMPATO_PARTIDA = 13;
    public const int INPUT_VALIDO = 14;
    public const int INPUT_INVALIDO = 15;
    public const int CONECTAR_A_LA_ROOM = 16;
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
        public string nameMyPlayer;
        public string nameOtherPlayer;
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

    private uint _myPlayerId;

    private const int TAM_BUFFER = 4080;

    private Host _client;
    private Peer _peer;
    private int _skipFrame = 3;
    private Dictionary<uint, GameObject> _players = new Dictionary<uint, GameObject>();
    private Client _myClient;
    private string myAlias = "Sin Nombre";
    private bool requestCountRooms = true;
    private int countRooms;
    const int channelID = 0;



    [Header("Objectos Canvas")]
    [SerializeField] private GameObject canvasLoby;
    [SerializeField] private GameObject canvasChargeRoom;
    [SerializeField] private GameObject canvasWaitReadyRoom;
    [SerializeField] private GameObject canvasSelectToken;
    [SerializeField] private GameObject canvasRoom;

    [Header("Data de canvasLoby")]
    [SerializeField] private List<Text> _roomsNames;
    private List<RoomsData> _roomsData = new List<RoomsData>();
    [SerializeField] private float delayRequestDataCanvasLobby = 2.5f;
    private float auxDelayRequestDataCanvasLobby;

    [Header("Data de canvasWaitReadyRoom")]
    [SerializeField] private Text aliasMyPlayer_text;
    [SerializeField] private Text aliasOtherPlayer_text;
    [SerializeField] private Color colorReady;
    [SerializeField] private Color colorNotReady;
    [SerializeField] private Image readyImage_MyPlayer;
    [SerializeField] private Image readyImage_OtherPlayer;
    [SerializeField] private Text playersConnected_text;
    [SerializeField] private Text delayStartParty_text;
    [SerializeField] private float delayStartParty;
    [SerializeField] private float delayRequestDataCanvasWaitReadyRoom = 0.5f;
    [SerializeField] private Text curretWaitingRoomName;
    [SerializeField] private Button buttonReady;
    private float auxDelayRequestDataCanvasWaitReadyRoom;

    private float auxDelayStartParty;
    private bool readyForPlaying_myPlayer; // RESETEAR ESTO UNA VEZ COMIENZA LA PARTIDA.
    private bool readyForPlaying_otherPlayer; // RESETEAR ESTO UNA VEZ COMIENZA LA PARTIDA.

    private int currentNumberRoom;
    private string currentOtherPlayerAlias;

    [Header("Data de canvasToken")]
    [SerializeField] private Text TokenX_text;
    [SerializeField] private Text TokenO_text;
    [SerializeField] private Color myToken_color;
    [SerializeField] private Color enemyToken_color;
    [SerializeField] private GameObject myToken;
    [SerializeField] private GameObject enemyToken;

    [Header("Data de canvasRoom")]
    [SerializeField] private Text nameTurn_text;
    [SerializeField] private Text nameMyPlayer;
    [SerializeField] private Text nameOtherPlayer;
    [SerializeField] private Text nameCurrentRoom;
    [SerializeField] private GameObject[] slots;
    [SerializeField] private Button buttonContinue;
    [SerializeField] private Text myScore_text;
    [SerializeField] private Text enemyScore_text;

    private int myScore;
    private int enemyScore;
    private bool cancelParty = false;

    private bool gameOver = false;
    private string result = "None";
    void Start ()
    {
        Application.runInBackground = true;
        InitENet();
        countRooms = -1;
        auxDelayRequestDataCanvasLobby = delayRequestDataCanvasLobby;
        auxDelayRequestDataCanvasWaitReadyRoom = delayRequestDataCanvasWaitReadyRoom;
        auxDelayStartParty = delayStartParty;
        readyForPlaying_myPlayer = false;
        readyForPlaying_otherPlayer = false;
        myAlias = PlayerData.instancePlayerData.GetAliasPlayer();
    }

	void Update ()
    {
        UpdateENet();
        UpdateGame();
        
    }
    private void UpdateGame()
    {
        if (canvasLoby.activeSelf)
        {
            delayStartParty_text.gameObject.SetActive(false);
            if (delayRequestDataCanvasLobby > 0)
            {
                delayRequestDataCanvasLobby = delayRequestDataCanvasLobby - Time.deltaTime;
            }
            else
            {
                delayRequestDataCanvasLobby = auxDelayRequestDataCanvasLobby;
                SendDataRoomRequest();
            }
        }
        else if (canvasWaitReadyRoom.activeSelf)
        {
            if (delayRequestDataCanvasWaitReadyRoom > 0)
            {
                delayRequestDataCanvasWaitReadyRoom = delayRequestDataCanvasWaitReadyRoom - Time.deltaTime;
            }
            else
            {
                delayRequestDataCanvasWaitReadyRoom = auxDelayRequestDataCanvasWaitReadyRoom;
                SendDataWaitReadyRoomRequest();
            }
            if (readyForPlaying_myPlayer && readyForPlaying_otherPlayer)
            {
                delayStartParty_text.gameObject.SetActive(true);
                delayStartParty_text.text = "La Partida Empieza en " + (int)delayStartParty;
                
                if (delayStartParty >= 1f)
                {
                    delayStartParty = delayStartParty - Time.deltaTime;
                }
                else
                {
                    delayStartParty = auxDelayStartParty;
                    NextCanvas(4);
                }

            }
        }
        else if (canvasSelectToken.activeSelf)
        {

        }
        else if (canvasRoom.activeSelf)
        {
            if (result != "None" && nameTurn_text.text != result)
            {
                nameTurn_text.text = result;
            }
        }
    }
    void FixedUpdate()
    {
        if (++_skipFrame < 3)
            return;

        if (requestCountRooms)
            SendCountRoomsRequest();

        _skipFrame = 0;
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
        PositionUpdateRequest = 4,
        PositionUpdateEvent = 5,
        LogoutEvent = 6,
        TestMensaggeRequest = 7,
        TestMensaggeEvent = 8,
        CountRoomsRequest = 9,
        CountRoomsEvent = 10,
        DataRoomsUpdateRequest = 11,
        DataRoomsUpdateEvent = 12,
        InputUpdateRequest = 13,
        InputUpdateEvent = 14,
        DataWaitReadyRoomRequest = 15,
        DataWaitReadyRoomEvent = 16,
        DataTurnRequest = 17,
        DataTurnEvent = 18,
        DisconnectRoomRequest = 19,
        DisconnectRoomEvent = 20,
    }
    public void SendTestMensagge()
    {
        var protocol = new Protocol();
        var buffer = protocol.Serialize((byte)PacketId.TestMensaggeRequest, _myPlayerId);
        var packet = default(Packet);
        packet.Create(buffer);
        _peer.Send(channelID, ref packet);
    }

    public void SendPlayInput(int input)
    {
        _myClient.input = input;
        SendInputUpdate();
        SendDataTurnRequest();
    }

    public void SelectToken(string token)
    {
        switch (token)
        {
            case "X":
                TokenX_text.color = myToken_color;
                myToken = TokenX_text.gameObject;

                TokenO_text.color = enemyToken_color;
                enemyToken = TokenO_text.gameObject;
                break;
            case "O":
                TokenO_text.color = myToken_color;
                myToken = TokenO_text.gameObject;

                TokenX_text.color = enemyToken_color;
                enemyToken = TokenX_text.gameObject;
                break;
        }
    }

    public void ConnectToRoom(int id_room)
    {
        _myClient.input = CONECTAR_A_LA_ROOM;
        _myClient.ID_RoomConnect = id_room;
        currentNumberRoom = id_room + 1;
        NextCanvas(2);
    }
    private void ResetScore()
    {
        myScore = 0;
        enemyScore = 0;
        myScore_text.text = "" + myScore;
        enemyScore_text.text = "" + enemyScore;
    }
    private void AddScore(ref int score, ref Text textScore)
    {
        score++;
        textScore.text = "" + score;
    }
    public void NextCanvas(int numberCanvas)
    {
        switch (numberCanvas)
        {
            case 1:
                delayRequestDataCanvasLobby = 0.05f;
                canvasLoby.SetActive(true);
                canvasChargeRoom.SetActive(false);
                canvasWaitReadyRoom.SetActive(false);
                canvasSelectToken.SetActive(false);
                canvasRoom.SetActive(false);
                ResetScore();
                break;
            case 2:
                delayRequestDataCanvasLobby = 0.05f;
                canvasLoby.SetActive(false);
                canvasChargeRoom.SetActive(true);
                canvasWaitReadyRoom.SetActive(false);
                canvasSelectToken.SetActive(false);
                canvasRoom.SetActive(false);
                ResetScore();
                break;
            case 3:
                buttonReady.interactable = true;
                buttonContinue.gameObject.SetActive(false);
                result = "None";
                gameOver = false;
                aliasMyPlayer_text.text = myAlias;
                readyImage_MyPlayer.color = colorNotReady;
                readyImage_OtherPlayer.color = colorNotReady;
                readyForPlaying_myPlayer = false;
                readyForPlaying_otherPlayer = false;
                curretWaitingRoomName.text = "Sala " + currentNumberRoom;
                SendDataWaitReadyRoomRequest();
                
                delayRequestDataCanvasLobby = 0.05f;
                canvasLoby.SetActive(false);
                canvasChargeRoom.SetActive(false);
                canvasWaitReadyRoom.SetActive(true);
                canvasSelectToken.SetActive(false);
                canvasRoom.SetActive(false);
                break;
            case 4:
                delayStartParty_text.gameObject.SetActive(false);
                nameTurn_text.gameObject.SetActive(false);
                SendDataTurnRequest();
                delayRequestDataCanvasLobby = 0.05f;
                canvasLoby.SetActive(false);
                canvasChargeRoom.SetActive(false);
                canvasWaitReadyRoom.SetActive(false);
                canvasSelectToken.SetActive(true);
                canvasRoom.SetActive(false);
                break;
            case 5:
                nameMyPlayer.text = _myClient.alias;
                nameOtherPlayer.text = currentOtherPlayerAlias;
                nameCurrentRoom.text = "Sala " + currentNumberRoom;
                delayRequestDataCanvasLobby = 0.05f;
                canvasLoby.SetActive(false);
                canvasChargeRoom.SetActive(false);
                canvasWaitReadyRoom.SetActive(false);
                canvasSelectToken.SetActive(false);
                canvasRoom.SetActive(true);
                break;
        }
    }
    public void SendDataWaitReadyRoomRequest()
    {
        var protocol = new Protocol();
        var buffer = protocol.Serialize((byte)PacketId.DataWaitReadyRoomRequest, _myPlayerId, _myClient.ID_RoomConnect, readyForPlaying_myPlayer, _myClient.alias);
        var packet = default(Packet);
        packet.Create(buffer);
        _peer.Send(channelID, ref packet);
    }
    public void SendDisconnectRoomRequest()
    {
        var protocol = new Protocol();
        var buffer = protocol.Serialize((byte)PacketId.DisconnectRoomRequest, _myPlayerId, _myClient.ID_RoomConnect, _myClient.ID_InRoom);
        var packet = default(Packet);
        packet.Create(buffer);
        _peer.Send(channelID, ref packet);
    }
    void SendDataTurnRequest()
    {
        var protocol = new Protocol();
        var buffer = protocol.Serialize((byte)PacketId.DataTurnRequest, _myPlayerId, _myClient.ID_RoomConnect, _myClient.ID_InRoom);
        var packet = default(Packet);
        packet.Create(buffer);
        _peer.Send(channelID, ref packet);
    }
    public void SendInputUpdate()
    {
        var protocol = new Protocol();
        var buffer = protocol.Serialize((byte)PacketId.InputUpdateRequest, _myPlayerId, _myClient.ID, _myClient.ID_RoomConnect,
                                  _myClient.ID_InRoom, _myClient.alias, _myClient.input, _myClient.isMyTurn, _myClient.inputOK);
        var packet = default(Packet);
        packet.Create(buffer);
        _peer.Send(channelID, ref packet);
    }
    public void SendCountRoomsRequest()
    {
        var protocol = new Protocol();
        var buffer = protocol.Serialize((byte)PacketId.CountRoomsRequest, _myPlayerId);
        var packet = default(Packet);
        packet.Create(buffer);
        _peer.Send(channelID, ref packet);
    }
    public void SendDataRoomRequest()
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

    public void ReadyForPlay()
    {
        readyForPlaying_myPlayer = true;
        readyImage_MyPlayer.color = colorReady;
        _myClient.input = LISTO_PARA_JUGAR;
        SendInputUpdate();
    }
    public void SwitchTurn(bool _isMyTurn)
    {
        if (_isMyTurn)
        {
            nameTurn_text.text = "Turno: \n" + _myClient.alias;
        }
        else
        {
            nameTurn_text.text = "Turno: \n" + currentOtherPlayerAlias;
        }
    }
    private void ParsePacket(ref ENet.Event netEvent)
    {
        var readBuffer = new byte[TAM_BUFFER];
        var readStream = new MemoryStream(readBuffer);
        var reader = new BinaryReader(readStream);

        readStream.Position = 0;
        netEvent.Packet.CopyTo(readBuffer);
        var packetId = (PacketId)reader.ReadByte();

        Debug.Log("ParsePacket received: " + packetId);

        if (packetId == PacketId.LoginResponse)
        {
            _myPlayerId = reader.ReadUInt32();
            _myClient = new Client(_myPlayerId, -1, 0, myAlias, -1, false, false);
            Debug.Log("MyPlayerId: " + _myPlayerId);
        }
        else if (packetId == PacketId.LoginEvent)
        {
            var playerId = reader.ReadUInt32();
            Debug.Log("OtherPlayerId: " + playerId);
            //SpawnOtherPlayer(playerId);
        }
        else if (packetId == PacketId.LogoutEvent)
        {
            var playerId = reader.ReadUInt32();
            if (_players.ContainsKey(playerId))
            {
                Destroy(_players[playerId]);
                _players.Remove(playerId);
            }
        }
        else if (packetId == PacketId.TestMensaggeEvent)
        {
            var playerId = reader.ReadUInt32();

            Debug.Log("RESPUESTA DEL SERVIDOR: Mensaje recibido desde Id " + playerId);
        }
        else if (packetId == PacketId.CountRoomsEvent)
        {
            var playerId = reader.ReadUInt32();
            countRooms = reader.ReadInt32();
            if (countRooms > 0 && requestCountRooms)
            {
                requestCountRooms = false;
                _roomsData.Clear();
                for (int i = 0; i < countRooms; i++)
                {
                    _roomsData.Add(new RoomsData("Sin nombre", 0, 0));
                    Debug.Log("Creada room " + (i + 1));
                }
                delayRequestDataCanvasLobby = 0;
            }
            Debug.Log("Cantidad de Rooms: " + countRooms);
        }
        else if (packetId == PacketId.DataRoomsUpdateEvent)
        {
            var playerId = reader.ReadUInt32();
            var nameRoom = reader.ReadString();
            var countPlayersInThisRoom = reader.ReadInt32();
            var maxPlayersInThisRoom = reader.ReadInt32();
            var ID_Room = reader.ReadInt32();

            if (ID_Room < _roomsData.Count && ID_Room >= 0)
            {
                _roomsData[ID_Room].nameRoom = nameRoom;
                _roomsData[ID_Room].currentPlayers = countPlayersInThisRoom;
                _roomsData[ID_Room].maxPlayers = maxPlayersInThisRoom;

                _roomsNames[ID_Room].text = "Sala " + (ID_Room + 1) + "\n" + "Jugadores conectados " + _roomsData[ID_Room].currentPlayers + "/" + _roomsData[ID_Room].maxPlayers;
            }
        }
        else if (packetId == PacketId.DataWaitReadyRoomEvent)
        {
            var playerID = reader.ReadUInt32();
            var otherPlayerReady = reader.ReadBoolean();
            var otherPlayerAlias = reader.ReadString();
            var countPlayersInRoom = reader.ReadInt32();
            var ID_RoomConecteed = reader.ReadInt32();
            if (_myPlayerId != playerID && ID_RoomConecteed == _myClient.ID_RoomConnect && ID_RoomConecteed != ID_ROOM_INVALIDO)
            {
                if (otherPlayerReady)
                    readyImage_OtherPlayer.color = colorReady;
                else
                    readyImage_OtherPlayer.color = colorNotReady;

                currentOtherPlayerAlias = otherPlayerAlias;
                aliasOtherPlayer_text.text = otherPlayerAlias;
                readyForPlaying_otherPlayer = otherPlayerReady;
                playersConnected_text.text = "Jugadores conectados: " + countPlayersInRoom + "/2";
            }
        }
        else if (packetId == PacketId.DisconnectRoomEvent)
        {
            var playerId = reader.ReadUInt32();
            var ID_RoomConnect = reader.ReadInt32();
            var ID_InRoom = reader.ReadUInt32();

            if (playerId == _myClient.ID)
            {
                //if (ID_RoomConnect == ID_ROOM_INVALIDO)
                //{
                _myClient.ID_RoomConnect = ID_RoomConnect;
                _myClient.ID_InRoom = ID_InRoom;
                currentOtherPlayerAlias = "Falta Jugador...";
                nameOtherPlayer.text = "Falta Jugador...";
                aliasOtherPlayer_text.text = "Falta Jugador...";
                delayStartParty = auxDelayStartParty;
                delayStartParty_text.gameObject.SetActive(false);
                readyForPlaying_myPlayer = false;
                readyForPlaying_otherPlayer = false;
                readyImage_MyPlayer.color = colorNotReady;
                readyImage_OtherPlayer.color = colorNotReady;
                playersConnected_text.text = "Jugadores conectados: 1/2";
                buttonReady.interactable = true;
                //}
                //else
                //{
                //    _myClient.ID_RoomConnect = ID_RoomConnect;
                //    _myClient.ID_InRoom = ID_InRoom;
                //    currentOtherPlayerAlias = "Falta Jugador...";
                //    nameOtherPlayer.text = "Falta Jugador...";
                //    aliasOtherPlayer_text.text = "Falta Jugador...";
                //}
            }
        }
        else if (packetId == PacketId.DataTurnEvent)
        {
            var playerId = reader.ReadUInt32();
            var _isMyTurn = reader.ReadBoolean();
            Debug.Log(playerId + " == " + _myClient.ID);
            if (playerId == _myClient.ID)
            {
                SwitchTurn(_isMyTurn);
                nameTurn_text.gameObject.SetActive(true);
            }
        }
        else if (packetId == PacketId.InputUpdateEvent)
        {
            var playerID = reader.ReadUInt32();
            var ID = reader.ReadUInt32();
            var ID_RoomConnect = reader.ReadInt32();
            var ID_InRoom = reader.ReadUInt32();
            var alias = reader.ReadString();
            var input = reader.ReadInt32();
            var isMyTurn = reader.ReadBoolean();
            var inputOK = reader.ReadBoolean();

            if (_myClient.input == CONECTAR_A_LA_ROOM && input == INPUT_VALIDO && playerID == _myClient.ID)
            {
                SendDataRoomRequest();

                NextCanvas(3);
                _myClient.input = NADA_NUEVO;
                _myClient.ID_InRoom = ID_InRoom;
                //Debug.Log(_myClient.ID_InRoom);
            }
            else if (_myClient.input == CONECTAR_A_LA_ROOM && input == INPUT_INVALIDO && playerID == _myClient.ID)
            {
                NextCanvas(1);
                _myClient.input = NADA_NUEVO;
            }
            else if (ID_RoomConnect == _myClient.ID_RoomConnect)
            {
                if ((input == INPUT_EMPATO_PARTIDA || input == INPUT_GANO_PARTIDA || input == INPUT_PERDIO_PARTIDA) && playerID == _myClient.ID)
                {
                    Debug.Log("TERMINO PARTIDA");
                    gameOver = true;
                    buttonContinue.gameObject.SetActive(true);
                    switch (input)
                    {
                        case INPUT_EMPATO_PARTIDA:
                            result = "EMPATE !";
                            break;
                        case INPUT_GANO_PARTIDA:
                            result = "GANASTE :D";
                            AddScore(ref myScore, ref myScore_text);
                            break;
                        case INPUT_PERDIO_PARTIDA:
                            result = "PERDISTE :'c";
                            AddScore(ref enemyScore, ref enemyScore_text);
                            break;
                    }
                }
                else
                {
                    if (ID_InRoom == _myClient.ID_InRoom)
                    {
                        if (_myClient.input == JUGAR_POSICION_0_DEL_TABLERO && input == INPUT_VALIDO)
                        {
                            //MI JUGADA
                            Instantiate(myToken, slots[0].transform);
                            SwitchTurn(isMyTurn);
                        }
                        else if (_myClient.input == JUGAR_POSICION_1_DEL_TABLERO && input == INPUT_VALIDO)
                        {
                            //MI JUGADA
                            Instantiate(myToken, slots[1].transform);
                            SwitchTurn(isMyTurn);
                        }
                        else if (_myClient.input == JUGAR_POSICION_2_DEL_TABLERO && input == INPUT_VALIDO)
                        {
                            //MI JUGADA
                            Instantiate(myToken, slots[2].transform);
                            SwitchTurn(isMyTurn);
                        }
                        else if (_myClient.input == JUGAR_POSICION_3_DEL_TABLERO && input == INPUT_VALIDO)
                        {
                            //MI JUGADA
                            Instantiate(myToken, slots[3].transform);
                            SwitchTurn(isMyTurn);
                        }
                        else if (_myClient.input == JUGAR_POSICION_4_DEL_TABLERO && input == INPUT_VALIDO)
                        {
                            //MI JUGADA
                            Instantiate(myToken, slots[4].transform);
                            SwitchTurn(isMyTurn);
                        }
                        else if (_myClient.input == JUGAR_POSICION_5_DEL_TABLERO && input == INPUT_VALIDO)
                        {
                            //MI JUGADA
                            Instantiate(myToken, slots[5].transform);
                            SwitchTurn(isMyTurn);
                        }
                        else if (_myClient.input == JUGAR_POSICION_6_DEL_TABLERO && input == INPUT_VALIDO)
                        {
                            //MI JUGADA
                            Instantiate(myToken, slots[6].transform);
                            SwitchTurn(isMyTurn);
                        }
                        else if (_myClient.input == JUGAR_POSICION_7_DEL_TABLERO && input == INPUT_VALIDO)
                        {
                            //MI JUGADA
                            Instantiate(myToken, slots[7].transform);
                            SwitchTurn(isMyTurn);
                        }
                        else if (_myClient.input == JUGAR_POSICION_8_DEL_TABLERO && input == INPUT_VALIDO)
                        {
                            //MI JUGADA
                            Instantiate(myToken, slots[8].transform);
                            SwitchTurn(isMyTurn);
                        }
                    }
                    else if (ID_InRoom != _myClient.ID_InRoom)
                    {
                        if (input == JUGAR_POSICION_0_DEL_TABLERO)
                        {
                            //JUGADA DEL CONTRINCANTE
                            Instantiate(enemyToken, slots[0].transform);
                            SwitchTurn(isMyTurn);
                        }
                        else if (input == JUGAR_POSICION_1_DEL_TABLERO)
                        {
                            //JUGADA DEL CONTRINCANTE
                            Instantiate(enemyToken, slots[1].transform);
                            SwitchTurn(isMyTurn);
                        }
                        else if (input == JUGAR_POSICION_2_DEL_TABLERO)
                        {
                            //JUGADA DEL CONTRINCANTE
                            Instantiate(enemyToken, slots[2].transform);
                            SwitchTurn(isMyTurn);
                        }
                        else if (input == JUGAR_POSICION_3_DEL_TABLERO)
                        {
                            //JUGADA DEL CONTRINCANTE
                            Instantiate(enemyToken, slots[3].transform);
                            SwitchTurn(isMyTurn);
                        }
                        else if (input == JUGAR_POSICION_4_DEL_TABLERO)
                        {
                            //JUGADA DEL CONTRINCANTE
                            Instantiate(enemyToken, slots[4].transform);
                            SwitchTurn(isMyTurn);
                        }
                        else if (input == JUGAR_POSICION_5_DEL_TABLERO)
                        {
                            //JUGADA DEL CONTRINCANTE
                            Instantiate(enemyToken, slots[5].transform);
                            SwitchTurn(isMyTurn);
                        }
                        else if (input == JUGAR_POSICION_6_DEL_TABLERO)
                        {
                            //JUGADA DEL CONTRINCANTE
                            Instantiate(enemyToken, slots[6].transform);
                            SwitchTurn(isMyTurn);
                        }
                        else if (input == JUGAR_POSICION_7_DEL_TABLERO)
                        {
                            //JUGADA DEL CONTRINCANTE
                            Instantiate(enemyToken, slots[7].transform);
                            SwitchTurn(isMyTurn);
                        }
                        else if (input == JUGAR_POSICION_8_DEL_TABLERO)
                        {
                            //JUGADA DEL CONTRINCANTE
                            Instantiate(enemyToken, slots[8].transform);
                            SwitchTurn(isMyTurn);
                        }
                    }
                }
                input = NADA_NUEVO;
            }

            //HACER QUE LA DATA DE LA UI DE LAS ROOMS SE ACTUALICE EN BASE A LO QUE LE CORRESPONDE:
            //- CANVAS PARA ELEGIR FICHA QUE VAYA ANTES DEL CANVAS DE CanvasWaitReadyRoom Y DESPUES DEL CanvasChargeRoom
            //- TERMINAR EL FLOW DE NIVELES (HACER QUE DEL CanvasLoby PASE a CanvasChargeRoom PASE a CanvasElegirFicha PASE a CanvasWaitReadyRoom 
            //  Y DESPUES pase al CanvasRoom donde empezara la partida)
            //- SALA CON SU NUMERO
            //- NOMBRES DE JUGADORES
            //- CANTIDAD DE JUGADORES CONECTADOS
            //- SI EL JUGADOR ESTA LISTO O NO.
            //- FUNCIONES DE LOS BOTONES
            //- FUCIONALIDAD DE LOS BOTONES PARA COLOCAR LA FICHA
            //- PUNTAJE DE CADA JUGADOR
            //- DE QUIEN ES EL TURNO.
            //- FUNCIONALIDAD DEL CANVAS PARA SELECCIONAR FICHA.


            //TAREAS DE CONEXION PENDIENTES:
            //- CUANDO ME RETIRO DE LA ROOM QUE SE DESCONECTE DE ESTA Y VUELVA AL LOBY
            //- CUANDO CIERRO EL JUEGO ME DESCONECTO COMPLETAMENTE (DE LA ROOM Y DEL LOBY).
            //- CUANDO SALGO AL MODO OFFLINE ME DESCONECTO COMPLETAMENTE (DE LA ROOM Y DEL LOBY).
        }
        
    }

    public void ClearTable()
    {
        GameObject[] tokens = GameObject.FindGameObjectsWithTag("Ficha");
        for (int i = 0; i < tokens.Length; i++)
        {
            tokens[i].transform.parent = null;
            Destroy(tokens[i]);
        }
    }
    
}
