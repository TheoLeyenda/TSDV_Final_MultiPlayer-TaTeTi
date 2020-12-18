using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class TaTeTi : Game
    {
        public enum ResultGame
        {
            None,
            Tie,
            WinPlayer,
            LosePlayer,
        }
        public enum GameState
        {
            None,
            InGame,
            FinishGame,
        }
        public TaTeTi(Room _myRoom) : base(_myRoom){}
        public class Player
        {
            public int scorePlayer;
            public int team;
            public ResultGame result;
            public Player(int _scorePlayer, int _team, ResultGame _result)
            {
                scorePlayer = _scorePlayer;
                team = _team;
                result = _result;
            }
        }
        private int ID_Turn;
        private int ID_WinPlayer;
        private int ID_LosePlayer;
        public List<Player> players = new List<Player>();

        public GameState gameState;
        public int[] table = new int[9];

        public override void Start()
        {
            InitTurn();
            ResetTable();
            gameState = GameState.None;
        }

        public override void Update()
        {
            switch (gameState)
            {
                case GameState.None:
                    break;
                case GameState.InGame:
                    for (int i = 0; i < myRoom.clientsInRoom.Count; i++)
                    {
                        CheckTurn(i);
                    }
                    CheckResultGame(0, 1);
                    CheckResultGame(1, 0);
                    break;
                case GameState.FinishGame:
                    break;
               
            }
        }

        public override void DestroyGame()
        {
            ResetTable();
        }

        public void CheckTurn(int id_player)
        {
            //HACER QUE LE LLEGUE EL INPUT DE LOS JUGADORES Y VERIFICAR SI EL INPUT ES VALIDO EN CASO AFIRMATIVO
            //PASAR DE TURNO DICIENDOLE AL CLIENTE QUE YA NO ES SU TURNO, UPDATEANDO EL TA TE TI VISUAL EN AMBOS CLIENTES.

            //Y EN CASO NEGATIVO DEVOLVER UN EVENTO AL CLIENTE DE QUE NO PASO NADA.
            if (id_player != ID_Turn)
            {
                myRoom.clientsInRoom[id_player].client.input = 15;
                return;
            }
            if (myRoom.clientsInRoom[id_player].client.input == -1)
            {
                return;
            }
            if (myRoom.clientsInRoom[id_player].client.input >= table.Length || myRoom.clientsInRoom[id_player].client.input < 0)
            {
                return;
            }

            int input = myRoom.clientsInRoom[id_player].client.input;

            if (table[input] != players[0].team && table[input] != players[1].team)
            {
                table[input] = players[id_player].team;
                myRoom.clientsInRoom[id_player].client.input = 14;
                switch (ID_Turn)
                {
                    case 0:
                        ID_Turn = 1;
                        break;
                    case 1:
                        ID_Turn = 0;
                        break;
                }
            }
            else
            {
                myRoom.clientsInRoom[id_player].client.input = 15;
            }

        }
        public void CheckResultGame(int ID_PlayerCheck, int ID_OtherPlayer)
        {
            if (
            (table[0] == players[ID_PlayerCheck].team
            && table[1] == players[ID_PlayerCheck].team
            && table[2] == players[ID_PlayerCheck].team)
            || 
            (table[3] == players[ID_PlayerCheck].team
            && table[4] == players[ID_PlayerCheck].team
            && table[5] == players[ID_PlayerCheck].team)
            ||
            (table[6] == players[ID_PlayerCheck].team
            && table[7] == players[ID_PlayerCheck].team
            && table[8] == players[ID_PlayerCheck].team)
            ||
            (table[0] == players[ID_PlayerCheck].team
            && table[3] == players[ID_PlayerCheck].team
            && table[6] == players[ID_PlayerCheck].team)
            ||
            (table[1] == players[ID_PlayerCheck].team
            && table[4] == players[ID_PlayerCheck].team
            && table[7] == players[ID_PlayerCheck].team)
            ||
            (table[2] == players[ID_PlayerCheck].team
            && table[5] == players[ID_PlayerCheck].team
            && table[8] == players[ID_PlayerCheck].team)
            ||
            (table[0] == players[ID_PlayerCheck].team
            && table[4] == players[ID_PlayerCheck].team
            && table[8] == players[ID_PlayerCheck].team)
            ||
            (table[2] == players[ID_PlayerCheck].team
            && table[4] == players[ID_PlayerCheck].team
            && table[6] == players[ID_PlayerCheck].team)
            )
            {
                players[ID_PlayerCheck].scorePlayer++;
                players[ID_PlayerCheck].result = ResultGame.WinPlayer;

                players[ID_OtherPlayer].result = ResultGame.LosePlayer;

                gameState = GameState.FinishGame;
            }

            if (players[ID_PlayerCheck].result == ResultGame.None && players[ID_OtherPlayer].result == ResultGame.None)
            {
                bool isTie = true;
                for (int i = 0; i < table.Length; i++)
                {
                    if (table[i] != players[ID_PlayerCheck].team && table[i] != players[ID_OtherPlayer].team)
                    {
                        isTie = false;
                        break;
                    }
                }
                if (isTie)
                {
                    players[ID_PlayerCheck].result = ResultGame.Tie;
                    players[ID_OtherPlayer].result = ResultGame.Tie;
                    gameState = GameState.FinishGame;
                }
            }

        }
        public void WinByDefault(int id_LosePlayer)
        {
            switch (id_LosePlayer)
            {
                case 0:
                    players[1].result = ResultGame.WinPlayer;
                    players[1].scorePlayer++;
                    break;
                case 1:
                    players[0].result = ResultGame.WinPlayer;
                    players[0].scorePlayer++;
                    break;
            }
            gameState = GameState.FinishGame;
            ResetDataPlayer(id_LosePlayer);
        }
        public void InitTurn()
        {
            Random random = new Random();
            ID_Turn = random.Next(1); // devuelve un numero entre el 0 y el 1, Si no funciona poner en vez de 1, el 2 en los parametros.
        }
        public void ResetTable()
        {
            for (int i = 0; i < table.Length; i++)
            {
                table[i] = 0;
            }
        }

        public int GetID_WinPlayer()
        {
            return ID_WinPlayer;
        }
        public int GetID_LosePlayer()
        {
            return ID_LosePlayer;
        }
        public void ResetDataPlayersInRoom()
        {
            players.Clear();
            players.Add(new Player (0, 1, ResultGame.None));
            players.Add(new Player (0, 2, ResultGame.None));
        }
        public void ResetDataPlayer(int id_player)
        {
            players[id_player].scorePlayer = 0;
            players[id_player].result = ResultGame.None;
        }
        public int GetID_Turn() { return ID_Turn; }
        public void SetID_Turn(int _ID_Turn) => ID_Turn = _ID_Turn;
    }
}
