using System;
using System.Collections.Generic;

namespace AsperetaClient
{
    class LoginSuccessPacket : PacketParser
    {
        public string RealmName { get; set; }

        public override string Prefix { get; } = "LOK";

        public override object Parse(string packet)
        {
            return new LoginSuccessPacket() { RealmName = packet.Substring(Prefix.Length) };
        }
    }
}
