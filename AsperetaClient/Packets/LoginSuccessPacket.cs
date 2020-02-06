using System;
using System.Collections.Generic;

namespace AsperetaClient
{
    class LoginSuccessPacket : PacketHandler
    {
        public string RealmName { get; set; }

        public override string Prefix { get; } = "LOK";

        public override object Parse(PacketParser p)
        {
            return new LoginSuccessPacket() { RealmName = p.GetRemaining() };
        }
    }
}
