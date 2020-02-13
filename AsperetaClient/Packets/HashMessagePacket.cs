using System;
using System.Collections.Generic;

namespace AsperetaClient
{
    class HashMessagePacket : PacketHandler
    {
        public string Message { get; set; }

        public override string Prefix { get; } = "#";

        public override object Parse(PacketParser p)
        {
            return new HashMessagePacket()
            {  
                Message = p.GetRemaining()
            };
        }
    }
}
