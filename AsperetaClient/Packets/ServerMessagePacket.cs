using System;
using System.Collections.Generic;

namespace AsperetaClient
{
    class ServerMessagePacket : PacketHandler
    {
        public int Colour { get; set; }

        public string Message { get; set; }

        public override string Prefix { get; } = "$";

        public override object Parse(PacketParser p)
        {
            return new ServerMessagePacket()
            {  
                Colour = Convert.ToInt32(p.GetSubstring(1)),
                Message = p.GetRemaining()
            };
        }
    }
}
