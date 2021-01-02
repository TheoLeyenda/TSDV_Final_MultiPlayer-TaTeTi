using System;
using System.Collections.Generic;
using System.IO;
using ENet;

namespace Server
{
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
    //14 = INPUT VALIDO.
    //15 = INPUT INVALIDO.
    //16 = CONECTAR A LA ROOM


    public class Program
    {
        public const int NADA_NUEVO = -1;
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
            public string myToken;

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

        private const int TAM_BUFFER = 4080;
        static Host _server = new Host();
        public static Dictionary<uint, Client> _playersInLobby = new Dictionary<uint, Client>();
        public static List<Room> _rooms = new List<Room>();
        public const int maxClientPerRoom = 2;
        public const int maxClients = 100;

        static void Main(string[] args)
        {
            const ushort port = 6005;
            
            Library.Initialize();

            _server = new Host();
            Address address = new Address();

            address.Port = port;
            _server.Create(address, maxClients);

            Console.WriteLine($"Circle ENet Server started on {port}");

            for (int i = 0; i < maxClients / maxClientPerRoom; i++)
            {
                _rooms.Add(new Room(maxClientPerRoom, "Sala " + (i + 1)));
            }
            for (int i = 0; i < _rooms.Count; i++)
            {
                _rooms[i].TaTeTiGame.ResetDataPlayersInRoom();
            }

            Event netEvent;
            while (!Console.KeyAvailable)
            {
                bool polled = false;

                while (!polled)
                {
                    if (_server.CheckEvents(out netEvent) <= 0)
                    {
                        if (_server.Service(15, out netEvent) <= 0)
                            break;

                        polled = true;
                    }

                    switch (netEvent.Type)
                    {
                        case EventType.None:
                            break;

                        case EventType.Connect:
                            Console.WriteLine("Client connected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                            netEvent.Peer.Timeout(32, 1000, 4000);
                            break;

                        case EventType.Disconnect:
                            Console.WriteLine("Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                            HandleLogout(netEvent.Peer.ID);
                            break;

                        case EventType.Timeout:
                            Console.WriteLine("Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                            HandleLogout(netEvent.Peer.ID);
                            break;

                        case EventType.Receive:
                            //Console.WriteLine("Packet received from - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP + ", Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);
                            HandlePacket(ref netEvent);
                            netEvent.Packet.Dispose();
                            break;
                           
                    }
                    for (int i = 0; i < _rooms.Count; i++)
                        _rooms[i].UpdateRoom();
                }
                for (int i = 0; i < _rooms.Count; i++)
                    _rooms[i].UpdateRoom();

                _server.Flush();
            }
            Library.Deinitialize();
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
        }

        static void HandlePacket(ref Event netEvent)
        {
            var readBuffer = new byte[TAM_BUFFER];
            var readStream = new MemoryStream(readBuffer);
            var reader = new BinaryReader(readStream);

            readStream.Position = 0;
            netEvent.Packet.CopyTo(readBuffer);
            var packetId = (PacketId)reader.ReadByte();

            if (packetId != PacketId.PositionUpdateRequest)
                Console.WriteLine($"HandlePacket received: {packetId}");

            if (packetId == PacketId.LoginRequest)
            {
                var playerId = netEvent.Peer.ID;
                SendLoginResponse(ref netEvent, playerId);
                BroadcastLoginEvent(playerId);
                foreach (var p in _playersInLobby)
                {
                    SendLoginEvent(ref netEvent, p.Key);
                }
                _playersInLobby.Add(playerId, new Client(playerId, -1, 0, "Falta Jugador...", -1, false, false));
            }
            else if (packetId == PacketId.PositionUpdateRequest)
            {
                var playerId = reader.ReadUInt32();
                var x = reader.ReadSingle();
                var y = reader.ReadSingle();
                //Console.WriteLine($"ID: {playerId}, Pos: {x}, {y}");
                BroadcastPositionUpdateEvent(playerId, x, y);
            }
            else if (packetId == PacketId.TestMensaggeRequest)
            {
                var playerId = reader.ReadUInt32();
                Console.WriteLine($"Mensaje recibido del ID: {playerId}");
                BroadcastTestMensaggeRequest(playerId);
            }
            else if (packetId == PacketId.CountRoomsRequest)
            {
                var playerId = reader.ReadUInt32();
                Console.WriteLine($"Solicitud de Cantidad de Rooms recibida del ID: {playerId}");
                BroadcastCountRooms(playerId);
            }
            else if (packetId == PacketId.DataRoomsUpdateRequest)
            {
                var playerId = reader.ReadUInt32();

                Console.WriteLine($"Solicitud de Data de Rooms recibida del ID: {playerId}");

                for (int i = 0; i < _rooms.Count; i++)
                {
                    BroadcastDataRoomsUpdateEvent(playerId, _rooms[i].nameRoom, _rooms[i].GetCountPlayersInThisRoom(), _rooms[i].GetMaxCountPlayersInThisRoom(), i);
                }
            }
            else if (packetId == PacketId.DataWaitReadyRoomRequest)
            {
                var playerId = reader.ReadUInt32();
                var ID_RoomConnect = reader.ReadInt32();
                var readyMyPlayer = reader.ReadBoolean();
                var myAlias = reader.ReadString();

                string _aliasOtherPlayer;
                bool _readyOtherPlayer;
                int _countPlayersConnected;

                for (int i = 0; i < _rooms[ID_RoomConnect].clientsInRoom.Count; i++)
                {
                    Console.WriteLine(_rooms[ID_RoomConnect].clientsInRoom[i].client.ID + " = " + playerId);
                    if (_rooms[ID_RoomConnect].clientsInRoom[i].client.ID == playerId)
                    {
                        _rooms[ID_RoomConnect].clientsInRoom[i].isReady = readyMyPlayer;
                        _rooms[ID_RoomConnect].clientsInRoom[i].client.alias = myAlias;
                    }
                    else
                    {
                        _aliasOtherPlayer = _rooms[ID_RoomConnect].clientsInRoom[i].client.alias;
                        _readyOtherPlayer = _rooms[ID_RoomConnect].clientsInRoom[i].isReady;
                        _countPlayersConnected = _rooms[ID_RoomConnect].countClient;
                        var ID = _rooms[ID_RoomConnect].clientsInRoom[i].client.ID;
                        BroadcastDataWaitReadyRoomEvent(playerId, readyMyPlayer, myAlias, _countPlayersConnected, ID_RoomConnect);
                        BroadcastDataWaitReadyRoomEvent(ID, _readyOtherPlayer, _aliasOtherPlayer, _countPlayersConnected, ID_RoomConnect);
                    }
                }
            }
            else if (packetId == PacketId.DataTurnRequest)
            {
                var playerId = reader.ReadUInt32();
                var ID_RoomConnect = reader.ReadInt32();
                var ID_InRoom = reader.ReadUInt32();

                Console.WriteLine(ID_InRoom + "=" + _rooms[ID_RoomConnect].TaTeTiGame.GetID_Turn());
                bool isMyTurn = (ID_InRoom == _rooms[ID_RoomConnect].TaTeTiGame.GetID_Turn());

                BroadcastDataTurnEvent(playerId, isMyTurn);
            }
            else if (packetId == PacketId.InputUpdateRequest)
            {
                var playerID = reader.ReadUInt32();
                reader.ReadUInt32();
                var ID = playerID;
                var ID_RoomConnect = reader.ReadInt32();
                var ID_InRoom = reader.ReadUInt32();
                var alias = reader.ReadString();
                var input = reader.ReadInt32();
                var isMyTurn = reader.ReadBoolean();
                var inputOK = reader.ReadBoolean();

                //AQUI RECIVO UN MENSAJE DEL CLIENTE.

                //INPUT 16 = CONECTAR A UNA ROOM.
                _playersInLobby[playerID].alias = alias;

                if (input == CONECTAR_A_LA_ROOM && _rooms[(int)ID_RoomConnect].clientsInRoom.Count < _rooms[(int)ID_RoomConnect].maxClient)
                {
                    ID_InRoom = _rooms[(int)ID_RoomConnect].ConnectClient(_playersInLobby[playerID]);
                    input = INPUT_VALIDO;
                    Console.WriteLine("El Player " + playerID + " se ha conectado a la room " + ID_InRoom);
                }
                else if (input == CONECTAR_A_LA_ROOM && _rooms[(int)ID_RoomConnect].clientsInRoom.Count >= _rooms[(int)ID_RoomConnect].maxClient)
                {
                    input = INPUT_INVALIDO;
                }
                else if (ID_RoomConnect > -1 && input != CONECTAR_A_LA_ROOM)
                {
                    _rooms[(int)ID_RoomConnect].clientsInRoom[(int)ID_InRoom].client.input = input;

                    _rooms[(int)ID_RoomConnect].UpdateRoom();

                    bool inputJugada = (input >= JUGAR_POSICION_0_DEL_TABLERO && input <= JUGAR_POSICION_8_DEL_TABLERO);
                    var auxInput = input;

                    input = _rooms[(int)ID_RoomConnect].clientsInRoom[(int)ID_InRoom].client.input;

                    if (inputJugada && input == INPUT_VALIDO)
                    {
                        for (int i = 0; i < _rooms[(int)ID_RoomConnect].clientsInRoom.Count; i++)
                        {
                            var currentID = _rooms[(int)ID_RoomConnect].clientsInRoom[i].client.ID_InRoom;
                            Client currentClient = _rooms[(int)ID_RoomConnect].clientsInRoom[i].client;

                            if (currentID == _rooms[(int)ID_RoomConnect].TaTeTiGame.GetID_Turn())
                            {
                                Console.WriteLine("ENTRE AL TURNO 1");
                                isMyTurn = true;
                            }
                            else
                            {
                                Console.WriteLine("ENTRE AL TURNO 2");
                                isMyTurn = false;
                            }

                            if (currentClient.ID != playerID)
                            {
                                BroadcastInputUpdateEvent(currentClient.ID, currentClient.ID, ID_RoomConnect, ID_InRoom, alias, auxInput, isMyTurn, inputOK);
                            }
                        }
                    }

                }

                _playersInLobby[playerID].ID = ID;
                _playersInLobby[playerID].ID_RoomConnect = ID_RoomConnect;
                _playersInLobby[playerID].ID_InRoom = ID_InRoom;
                _playersInLobby[playerID].alias = alias;
                _playersInLobby[playerID].input = input;
                _playersInLobby[playerID].isMyTurn = isMyTurn;
                _playersInLobby[playerID].inputOK = inputOK;

                BroadcastInputUpdateEvent(playerID, ID, ID_RoomConnect, ID_InRoom, alias, input, isMyTurn, inputOK);
            }
        }

        static void SendLoginResponse(ref Event netEvent, uint playerId)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)PacketId.LoginResponse, playerId);
            var packet = default(Packet);
            packet.Create(buffer);
            netEvent.Peer.Send(0, ref packet);
        }

        static void SendLoginEvent(ref Event netEvent, uint playerId)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)PacketId.LoginEvent, playerId);
            var packet = default(Packet);
            packet.Create(buffer);
            netEvent.Peer.Send(0, ref packet);
        }

        static void BroadcastLoginEvent(uint playerId)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)PacketId.LoginEvent, playerId);
            var packet = default(Packet);
            packet.Create(buffer);
            _server.Broadcast(0, ref packet);
        }

        static void BroadcastLogoutEvent(uint playerId)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)PacketId.LogoutEvent, playerId);
            var packet = default(Packet);
            packet.Create(buffer);
            _server.Broadcast(0, ref packet);
        }

        static void BroadcastDataTurnEvent(uint playerId, bool _isMyTurn)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)PacketId.DataTurnEvent, playerId, _isMyTurn);
            var packet = default(Packet);
            packet.Create(buffer);
            _server.Broadcast(0, ref packet);
        }

        static void BroadcastDataWaitReadyRoomEvent(uint playerId, bool otherPlayerReady, string otherPlayerAlias, int countPlayersInRoom, int ID_ConnectedRoom)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)PacketId.DataWaitReadyRoomEvent, playerId, otherPlayerReady, otherPlayerAlias, countPlayersInRoom, ID_ConnectedRoom);
            var packet = default(Packet);
            packet.Create(buffer);
            _server.Broadcast(0, ref packet);
        }

        public static void BroadcastInputUpdateEvent(uint playerId, uint ID, int ID_RoomConnect, uint ID_InRoom, string alias, int input, bool isMyTurn, bool inputOK)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)PacketId.InputUpdateEvent, playerId, ID, ID_RoomConnect, ID_InRoom, alias, input, isMyTurn, inputOK);
            var packet = default(Packet);
            packet.Create(buffer);
            _server.Broadcast(0, ref packet);

            //RESPONDO EL MENSAJE DEL CLIENTE
        }

        static void BroadcastPositionUpdateEvent(uint playerId, float x, float y)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)PacketId.PositionUpdateEvent, playerId, x, y);
            var packet = default(Packet);
            packet.Create(buffer);
            _server.Broadcast(0, ref packet);
        }
        static void BroadcastTestMensaggeRequest(uint playerId)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)PacketId.TestMensaggeEvent, playerId);
            var packet = default(Packet);
            packet.Create(buffer);
            _server.Broadcast(0, ref packet);
        }
        static void BroadcastCountRooms(uint playerId)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)PacketId.CountRoomsEvent, playerId, _rooms.Count);
            var packet = default(Packet);
            packet.Create(buffer);
            _server.Broadcast(0, ref packet);
        }
        static void BroadcastDataRoomsUpdateEvent(uint playerId, string nameRoom, int currentPlayers, int maxPlayers, int ID_Room)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)PacketId.DataRoomsUpdateEvent, playerId, nameRoom, currentPlayers, maxPlayers, ID_Room);
            var packet = default(Packet);
            packet.Create(buffer);
            _server.Broadcast(0, ref packet);
        }
        static void HandleLogout(uint playerId)
        {
            if (!_playersInLobby.ContainsKey(playerId))
                return;

            _playersInLobby.Remove(playerId);
            BroadcastLogoutEvent(playerId);
        }
    }
}
