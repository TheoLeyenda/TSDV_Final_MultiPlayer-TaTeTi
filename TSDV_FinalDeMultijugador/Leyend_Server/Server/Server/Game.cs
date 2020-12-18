using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Game
    {
        public Room myRoom;
        public Game(Room _myRoom) { myRoom = _myRoom; }
        public virtual void Start(){ }
        public virtual void Update() { }
        public virtual void DestroyGame() { }
    }
}
