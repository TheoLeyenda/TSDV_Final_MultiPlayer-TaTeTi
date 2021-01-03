using System;
using System.IO;

namespace Server
{
    public class Protocol
    {
        private void InitWriter(int size)
        {
            m_buffer = new byte[size];
            m_stream = new MemoryStream(m_buffer);
            m_writer = new BinaryWriter(m_stream);
        }

        private void InitReader(byte[] buffer)
        {
            m_stream = new MemoryStream(buffer);
            m_reader = new BinaryReader(m_stream);
        }

        public byte[] Serialize(byte code, uint value)
        {
            const int bufSize = sizeof(byte) + sizeof(int);
            InitWriter(bufSize);
            m_writer.Write(code);
            m_writer.Write(value);
            return m_buffer;
        }
        public byte[] Serialize(byte code, uint value, bool _isMyTurn)
        {
            const int bufSize = sizeof(byte) + sizeof(int) + sizeof(bool);
            InitWriter(bufSize);
            m_writer.Write(code);
            m_writer.Write(value);
            m_writer.Write(_isMyTurn);
            return m_buffer;
        }
        public byte[] Serialize(byte code, uint value, bool otherPlayerReady, string otherPlayerAlias, int countPlayersInRoom, int ID_ConnectedRoom)
        {
            const int bufSize = sizeof(byte) + sizeof(int) * 3 + 1500;
            InitWriter(bufSize);
            m_writer.Write(code);
            m_writer.Write(value);
            m_writer.Write(otherPlayerReady);
            m_writer.Write(otherPlayerAlias);
            m_writer.Write(countPlayersInRoom);
            m_writer.Write(ID_ConnectedRoom);
            return m_buffer;
        }

        public byte[] Serialize(byte code, uint value, int countRooms)
        {
            const int bufSize = sizeof(byte) + sizeof(int) * 2;
            InitWriter(bufSize);
            m_writer.Write(code);
            m_writer.Write(value);
            m_writer.Write(countRooms);
            return m_buffer;
        }
        public byte[] Serialize(byte code, uint value, string alias, bool readyOtherPlayer, int countPlayersConnected)
        {
            const int bufSize = sizeof(byte) + sizeof(int) * 2 + 1500;
            InitWriter(bufSize);
            m_writer.Write(code);
            m_writer.Write(value);
            m_writer.Write(alias);
            m_writer.Write(readyOtherPlayer);
            m_writer.Write(countPlayersConnected);
            return m_buffer;
        }
        public byte[] Serialize(byte code, uint value, uint ID, int ID_RoomConnect, uint ID_InRoom, string alias, int input, bool isMyTurn, bool inputOK)
        {
            const int bufSize = sizeof(byte) + sizeof(int) * 5 + 1500;
            InitWriter(bufSize);
            m_writer.Write(code);
            m_writer.Write(value);
            m_writer.Write(ID);
            m_writer.Write(ID_RoomConnect);
            m_writer.Write(ID_InRoom);
            m_writer.Write(alias);
            m_writer.Write(input);
            m_writer.Write(isMyTurn);
            m_writer.Write(inputOK);
            return m_buffer;
        }
        /*
         public string nameRoom;
         public int currentPlayers;
         public int maxPlayers;
         */
        public byte[] Serialize(byte code, uint value, string nameRoom, int currentPlayers, int maxPlayers, int ID_Room)
        {
            const int bufSize = sizeof(byte) + sizeof(int) * 4 + 1000;
            InitWriter(bufSize);
            m_writer.Write(code);
            m_writer.Write(value);
            m_writer.Write(nameRoom);
            m_writer.Write(currentPlayers);
            m_writer.Write(maxPlayers);
            m_writer.Write(ID_Room);
            return m_buffer;
        }

        public byte[] Serialize(byte code, uint value, float x, float y)
        {
            const int bufSize = sizeof(byte) + sizeof(int) + sizeof(float) + sizeof(float);
            InitWriter(bufSize);
            m_writer.Write(code);
            m_writer.Write(value);
            m_writer.Write(x);
            m_writer.Write(y);
            return m_buffer;
        }
        public byte[] Serialize(byte code, uint playerId, int ID_RoomConnect, uint ID_InRoom)
        {
            const int bufSize = sizeof(byte) + sizeof(int) * 3;
            InitWriter(bufSize);
            m_writer.Write(code);
            m_writer.Write(playerId);
            m_writer.Write(ID_RoomConnect);
            m_writer.Write(ID_InRoom);
            return m_buffer;
        }
        public void Deserialize(byte[] buf, out byte code, out int value)
        {
            InitReader(buf);
            m_stream.Write(buf, 0, buf.Length);
            m_stream.Position = 0;
            code = m_reader.ReadByte();
            value = m_reader.ReadInt32();
        }

        private BinaryWriter m_writer;
        private BinaryReader m_reader;
        private MemoryStream m_stream;
        private byte[] m_buffer;
    }
}
