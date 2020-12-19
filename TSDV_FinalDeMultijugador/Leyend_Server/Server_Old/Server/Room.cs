using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class Room
    {
        public class ClientInParty
        {
            public bool isReady;
            public Server.Client client;
            public ClientInParty(bool _isReady, Server.Client _client)
            {
                isReady = _isReady;
                client = _client;
            }
        }
        public enum StateRoom
        {
            WaitingStart,
            InGame,
            EndGame,
        }
        public StateRoom stateRoom;
        public int maxClient;
        public int countClient;
        public List<ClientInParty> clientsInRoom;
        public bool isFullCapacity { set; get; }
        public TaTeTi TaTeTiGame;
        public string nameRoom = "Room sin nombre";
        public Room(int _maxClient, string _nameRoom)
        {
            stateRoom = StateRoom.WaitingStart;
            maxClient = _maxClient;
            countClient = 0;
            clientsInRoom = new List<ClientInParty>();
            isFullCapacity = false;
            TaTeTiGame = new TaTeTi(this);
            nameRoom = _nameRoom;
        }
        public void OnWaitingStartGame()
        {
            //ESTA FUNCION SE EJECUTARA MIENTRAS SE ESPERA QUE EMPIECE LA PARTIDA.

            //EL INPUT QUE SE RESIVA SERA PARA:
            //-1 = NADA NUEVO.
            //9 = LISTO PARA JUGAR. (Si los dos estan listos empieza la partida)
            //10 = SALIR DE LA ROOM (LLAMA A LA FUNCION DisconnectClient(int ID_InRoom) Y SACA AL CLIENTE ESPECIFICADO DE LA ROOM)
            for (int i = 0; i < clientsInRoom.Count; i++)
            {
                if (clientsInRoom[i] != null)
                {
                    if (clientsInRoom[i].client.input != -1)
                    {
                        switch (clientsInRoom[i].client.input)
                        {
                            case 9:
                                if (!clientsInRoom[i].isReady)
                                    clientsInRoom[i].isReady = true;
                                clientsInRoom[i].client.input = -1;
                                break;
                            case 10:
                                clientsInRoom[i].client.input = -1;
                                DisconnectClient(i);
                                break;
                        }
                    }
                }
            }
            if (CheckClientsReady())
            {
                stateRoom = StateRoom.InGame;
                TaTeTiGame.gameState = TaTeTi.GameState.InGame;
            }

        }
        public void OnGaming()
        {
            // ESTA FUNCION SE EJECUTARA MIENTRAS ESTOY EN PARTIDA.

            //EL INPUT QUE SE RESIVA SERA PARA:
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

            //10 = EL JUGADOR SALE DE LA ROOM SE LE DA LA VICTORIA AUTOMATICAMENTE AL OTRO JUGADOR Y PASO AL ESTADO StateRoom.ExitGame

            //SIEMPRE QUE TERMINE UNA PARTIDA SE PASA AL ESTADO StateRoom.ExitGame.

            //LOS PUNTAJES DE LOS JUGADORES SE MANTENDRAN SIEMPRE Y CUANDO EL CLIENTE EN CUESTON NO ABANDONE LA ROOM.

            for (int i = 0; i < clientsInRoom.Count; i++)
            {
                if (clientsInRoom[i] != null)
                {
                    if (clientsInRoom[i].client.input != -1)
                    {
                        switch (clientsInRoom[i].client.input)
                        {
                            case 10:
                                TaTeTiGame.WinByDefault(i);
                                clientsInRoom[i].client.input = -1;
                                DisconnectClient(i);
                                break;
                        }
                    }
                }
            }

            // ESTA FUNCION SE ENCARGA POR DENTRO DE CHEKEAR EL INPUT DEL JUGADOR PARA EL JUEGO (LOS INPUTS QUE VALEN ENTRE 0 Y 8).
            TaTeTiGame.Update();

            if (TaTeTiGame.gameState == TaTeTi.GameState.FinishGame)
                stateRoom = StateRoom.EndGame;
            
        }
        public void OnEndGaming()
        {
            // ESTA FUNCION SE EJECUTA CUANDO TERMINA LA PARTIDA.

            //TE ENVIA DIRECTAMENTE AL ESTADO ON StateRoom.WaitingStart.
            //SE EJECUTA UNA FUNCION EN LOS CLIENTES PARA VOLVER A LA PANTALLA DE ESPERA DE LA ROOM.

            //11 = INPUT QUE INFORMA AL JUGADOR QUE GANO LA PARTIDA.
            //12 = INPUT QUE INFORMA AL JUGADOR QUE PERDIO LA PARTIDA.
            //13 = INPUT QUE INFORMA AL JUGADOR QUE EMPATO LA PARTIDA.
            for (int i = 0; i < clientsInRoom.Count; i++)
            {
                switch (TaTeTiGame.players[i].result)
                {
                    case TaTeTi.ResultGame.WinPlayer:
                        clientsInRoom[i].client.input = 11;
                        break;
                    case TaTeTi.ResultGame.LosePlayer:
                        clientsInRoom[i].client.input = 12;
                        break;
                    case TaTeTi.ResultGame.Tie:
                        clientsInRoom[i].client.input = 13;
                        break;
                }
                clientsInRoom[i].isReady = false;
            }
            stateRoom = StateRoom.WaitingStart;

        }
        public void UpdateRoom()
        {
            switch (stateRoom)
            {
                case StateRoom.WaitingStart:
                    OnWaitingStartGame();
                    break;
                case StateRoom.InGame:
                    OnGaming();
                    break;
                case StateRoom.EndGame:
                    OnEndGaming();
                    break;
            }
        }
        public uint ConnectClient(Server.Client _client)
        {
            uint id_room = 0;
            if (countClient < maxClient)
            {
                clientsInRoom.Add(new ClientInParty(false, _client));
                countClient++;
                id_room = (uint)clientsInRoom.Count - 1;
            }
            return id_room;
        }
        public void DisconnectClient(int ID_InRoom)
        {
            if (countClient > 0)
            {
                clientsInRoom.Remove(clientsInRoom[ID_InRoom]);
                countClient--;
            }
            TaTeTiGame.ResetDataPlayer(ID_InRoom);
        }

        public bool CheckClientsReady()
        {
            for (int i = 0; i < clientsInRoom.Count; i++)
            {
                if (!clientsInRoom[i].isReady)
                    return false;
            }
            return true;
        }

        public int GetCountPlayersInThisRoom()
        {
            return clientsInRoom.Count;
        }
        public int GetMaxCountPlayersInThisRoom()
        {
            return maxClient;
        }
    }
}
