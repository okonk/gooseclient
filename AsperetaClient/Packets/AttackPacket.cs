using System;
using System.Collections.Generic;

namespace AsperetaClient
{
    class AttackPacket : PacketHandler
    {
        public int LoginId { get; set; }

        public override string Prefix { get; } = "ATT";

        public override object Parse(PacketParser p)
        {
            return new AttackPacket()
            {  
                LoginId = p.GetInt32(),
            };
        }
    }
}
