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
    //14 = JUGADA VALIDA.
    //15 = JUGADA INVALIDA.
    //16 = CONECTAR A LA ROOM
    
    public class Server
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
        
        public const int maxClientPerRoom = 2;

        static Host _server = new Host();
        public static Dictionary<uint, Client> _playersInLobby = new Dictionary<uint, Client>();
        public static List<Room> _rooms = new List<Room>();
        public enum PacketId : byte
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
        
        static void Main(string[] args)
        {
            const ushort port = 6005;
            const int maxClients = 100;

            ENet.Library.Initialize();

            _server = new Host();
            Address address = new Address();

            address.Port = port;
            _server.Create(address, maxClients);

            for (int i = 0; i < maxClients / maxClientPerRoom; i++)
            {
                _rooms.Add(new Room(maxClientPerRoom, "Sala " + (i + 1)));
            }
            for (int i = 0; i < _rooms.Count; i++)
            {
                _rooms[i].TaTeTiGame.ResetDataPlayersInRoom();
            }

            Console.WriteLine($"Circle ENet Server started on {port}");

            UpdateServer();

        }

        static void UpdateServer()
        {
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
                }
                for (int i = 0; i < _rooms.Count; i++)
                {
                    _rooms[i].UpdateRoom();
                }

                _server.Flush();
            }

        }

        static void HandlePacket(ref Event netEvent)
        {
            var readBuffer = new byte[1024];
            var readStream = new MemoryStream(readBuffer);
            var reader = new BinaryReader(readStream);

            readStream.Position = 0;
            netEvent.Packet.CopyTo(readBuffer);
            var packetId = (PacketId)reader.ReadByte();


            if (packetId != PacketId.InputUpdateRequest)
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
                _playersInLobby.Add(playerId, new Client(playerId, -1, 0, "sin alias", -1, false, false));
            }
            else if (packetId == PacketId.InputUpdateRequest)
            {
                var playerID = reader.ReadUInt32();
                var ID = reader.ReadUInt32();
                var ID_RoomConnect = reader.ReadInt32();
                var ID_InRoom = reader.ReadUInt32();
                var alias = reader.ReadString();
                var input = reader.ReadInt32();
                var isMyTurn = reader.ReadBoolean();
                var inputOK = reader.ReadBoolean();

                //AQUI RECIVO UN MENSAJE DEL CLIENTE.


                //INPUT 16 = CONECTAR A UNA ROOM.
                _playersInLobby[playerID].alias = alias;
                if (input == 16)
                {
                    ID_InRoom = _rooms[(int)ID_RoomConnect].ConnectClient(_playersInLobby[playerID]);
                    input = -1;
                }
                else if (ID_RoomConnect > -1)
                {
                    _rooms[(int)ID_RoomConnect].clientsInRoom[(int)ID_InRoom].client.input = input;

                    _rooms[(int)ID_RoomConnect].UpdateRoom();

                    input = _rooms[(int)ID_RoomConnect].clientsInRoom[(int)ID_InRoom].client.input;
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
            else if (packetId == PacketId.DataRoomsUpdateRequest)
            {
                var playerID = reader.ReadUInt32();

                for (int i = 0; i < _rooms.Count; i++)
                {
                    BroadcastDataRoomsUpdateEvent(playerID, _rooms[i].nameRoom, _rooms[i].GetCountPlayersInThisRoom(), _rooms[i].GetMaxCountPlayersInThisRoom(), i);
                }
            }
            else if (packetId == PacketId.CountRoomsUpdateRequest)
            {
                var playerID = reader.ReadUInt32();

                BroadcastCountRoomsUpdateEvent(playerID, _rooms.Count);
            }
            else if (packetId == PacketId.PositionUpdateRequest)
            {
                var playerId = reader.ReadUInt32();
                var x = reader.ReadSingle();
                var y = reader.ReadSingle();
                //Console.WriteLine($"ID: {playerId}, Pos: {x}, {y}");
                BroadcastPositionUpdateEvent(playerId, x, y);
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

        static void BroadcastCountRoomsUpdateEvent(uint playerId, int countRooms)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)PacketId.CountRoomsUpdateEvent, playerId, countRooms);
            var packet = default(Packet);
            packet.Create(buffer);
            _server.Broadcast(0, ref packet);
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

        static void BroadcastInputUpdateEvent(uint playerId, uint ID, int ID_RoomConnect, uint ID_InRoom, string alias, int input, bool isMyTurn, bool inputOK)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)PacketId.InputUpdateEvent, playerId, ID, ID_RoomConnect, ID_InRoom, alias, input, isMyTurn, inputOK);
            var packet = default(Packet);
            packet.Create(buffer);
            _server.Broadcast(0, ref packet);

            //RESPONDO EL MENSAJE DEL CLIENTE
        }
        static void BroadcastDataRoomsUpdateEvent(uint playerId, string nameRoom, int currentPlayers, int maxPlayers, int ID_Room)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)PacketId.DataRoomsUpdateEvent, playerId,nameRoom, currentPlayers, maxPlayers, ID_Room);
            var packet = default(Packet);
            packet.Create(buffer);
            _server.Broadcast(0, ref packet);
        }
        static void BroadcastPositionUpdateEvent(uint playerId, float x, float y)
        {
            var protocol = new Protocol();
            var buffer = protocol.Serialize((byte)PacketId.PositionUpdateEvent, playerId, x, y);
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

        public void DestroyServer()
        {
            Library.Deinitialize();
        }
    }
    
    
}
