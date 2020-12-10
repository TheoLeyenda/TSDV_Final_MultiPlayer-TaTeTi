using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ENet;
using Server;
using Random = UnityEngine.Random;

struct PlayerInput
{
    public int seqNumber;
    public bool buttonLeft;
    public bool buttonRight;
    public bool buttonUp;
    public bool buttonDown;
}
struct EntityState
{
    public int id;
    public float x;
    public float y;
}
struct GameState
{
    public int lastProcessSeqNumber;
    public int entitiesCount;
    public EntityState[] entities;
}

enum PacketId : byte
{
    LoginRequest = 1,
    LoginResponse = 2,
    LoginEvent = 3,
    LogoutEvent = 4,
    PlayerInputEvent = 5,
    GameStateEvent = 6,
}
public class SceneManager : MonoBehaviour {

    int seqNumber;
    List<PlayerInput> inputHistory;
    GameState localState;

    public GameObject myPlayerFactory;
    public GameObject otherPlayerFactory;

    private GameObject _myPlayer;
    private uint _myPlayerId;

    private Host _client;
    private Peer _peer;
    private int _skipFrame = 0;
    private Dictionary<uint, GameObject> _players = new Dictionary<uint, GameObject>();

   

    const int channelID = 0;

    void Start ()
    {
        Application.runInBackground = true;
        InitENet();
        _myPlayer = Instantiate(myPlayerFactory);
    }

	void Update (){}

    void FixedUpdate()
    {
        //Procesar mensajes del server
        ProcessServerMessages();
        //Samplear el teclado y generar un PlayerInput.
        //Incrementar el numero de secuencia.
        var input = ProcessInput();
        ApplyPlayerInput(input);
        //Enviar el input al server.
        SendPlayerInput(input);
        //Agregar input al historial.
        inputHistory.Add(input);
    }
    private void ApplyPlayerInput(PlayerInput input)
    {

    }
    private void SendPlayerInput(PlayerInput input)
    {

    }
    private PlayerInput ProcessInput()
    {
        //Temporal
        return new PlayerInput();
    }
  
    private void ApplyGameState(GameState state)
    {
        //update all entities state (piso la prediccion del player tambien)
        //aca estamos parejos con el server (pero en el pasado para este cliente)

        //descartar playerInput del historial, que sean una secuencia anterior a state.lastProcessSeqNumber
        
        //replay de playerInput que quedaron en el historial

        //update playerState

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

    private void ProcessServerMessages()
    {
        ENet.Event netEvent;

        bool polled = false;

        while (!polled)
        {
            if (_client.CheckEvents(out netEvent) <= 0)
            {
                if (_client.Service(15, out netEvent) <= 0)
                    break;

                polled = true;
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
            Debug.Log("MyPlayerId: " + _myPlayerId);
        }
        else if (packetId == PacketId.LoginEvent)
        {
            var playerId = reader.ReadUInt32();
            Debug.Log("OtherPlayerId: " + playerId);
            SpawnOtherPlayer(playerId);
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
        else if (packetId == PacketId.GameStateEvent)
        {
            GameState state;
            state.lastProcessSeqNumber = reader.ReadInt32();
            state.entitiesCount = reader.ReadInt32();
            state.entities = new EntityState[state.entitiesCount];
            // todo: check boundaries.
            for (int i = 0; i < state.entitiesCount; i++)
            {
                state.entities[i].id = reader.ReadInt32();
                state.entities[i].x = reader.ReadSingle();
                state.entities[i].y = reader.ReadSingle();
            }
            ApplyGameState(state);
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
    }

    private void UpdatePosition(uint playerId, float x, float y)
    {
        if (playerId == _myPlayerId)
            return;

        Debug.Log("UpdatePosition " + playerId);
        _players[playerId].transform.position = new Vector2(x, y);
    }
}
