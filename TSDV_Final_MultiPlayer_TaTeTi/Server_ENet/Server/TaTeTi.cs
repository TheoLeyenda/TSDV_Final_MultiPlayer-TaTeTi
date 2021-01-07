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
            //public int scorePlayer;
            public int team;
            public ResultGame result;
            public Player(int _scorePlayer, int _team, ResultGame _result)
            {
                //scorePlayer = _scorePlayer;
                team = _team;
                result = _result;
            }
        }
        private int ID_Turn;
        //private int ID_WinPlayer;
        //private int ID_LosePlayer;
        public List<Player> players = new List<Player>();

        public GameState gameState;
        public int[] table = new int[9];

        public override void Start()
        {
            InitTurn();
            ResetTable();
            gameState = GameState.None;
            ResetDataPlayersInRoom();
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
                myRoom.clientsInRoom[id_player].client.input = Program.INPUT_INVALIDO;
                return;
            }
            if (myRoom.clientsInRoom[id_player].client.input == Program.NADA_NUEVO)
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
                myRoom.clientsInRoom[id_player].client.input = Program.INPUT_VALIDO;
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
                myRoom.clientsInRoom[id_player].client.input = Program.INPUT_INVALIDO;
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
                //players[ID_PlayerCheck].scorePlayer++;
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
                    Console.WriteLine("");
                    Console.WriteLine("EMPATE!");
                    players[ID_PlayerCheck].result = ResultGame.Tie;
                    players[ID_OtherPlayer].result = ResultGame.Tie;
                    gameState = GameState.FinishGame;
                }
            }

            for (int i = 0; i < table.Length; i++)
            {
                Console.Write(table[i]);
                if ((i+1) % 3 == 0)
                {
                    Console.WriteLine("");
                }
            }
            Console.WriteLine("");
            //Console.WriteLine("");

        }
        public void WinByDefault(int id_LosePlayer)
        {
            players[0].result = ResultGame.WinPlayer;
            gameState = GameState.FinishGame;
        }
        public void InitTurn()
        {
            Random random = new Random();
            int aux = random.Next(100); // devuelve un numero entre el 0 y el 1, Si no funciona poner en vez de 1, el 2 en los parametros.
            if (aux > 50)
                ID_Turn = 0; //LUEGO DE CORREGIR LOS BUGS DEL TURNO (NO APARECE EL NOMBRE DE QUIEN ES EL TURNO) PONER EN 0
            else if (aux <= 50)
                ID_Turn = 1;
        }
        public void ResetTable()
        {
            for (int i = 0; i < table.Length; i++)
            {
                table[i] = 0;
            }
        }

        /*public int GetID_WinPlayer()
        {
            return ID_WinPlayer;
        }
        public int GetID_LosePlayer()
        {
            return ID_LosePlayer;
        }*/
        public void ResetDataPlayersInRoom()
        {
            players.Clear();
            players.Add(new Player (0, 1, ResultGame.None));
            players.Add(new Player (0, 2, ResultGame.None));
        }
        public void ResetDataPlayer(int id_player)
        {
            //players[id_player].scorePlayer = 0;
            players[id_player].result = ResultGame.None;
        }
        public int GetID_Turn() { return ID_Turn; }
        public void SetID_Turn(int _ID_Turn) => ID_Turn = _ID_Turn;
    }
}
