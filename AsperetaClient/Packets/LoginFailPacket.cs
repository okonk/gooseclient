using System;
using System.Collections.Generic;

namespace AsperetaClient
{
    class LoginFailPacket : PacketParser
    {
        public string Message { get; set; }

        public override string Prefix { get; } = "LNO";

        public override object Parse(string packet)
        {
            return new LoginFailPacket() { Message = packet.Substring(Prefix.Length) };
        }
    }
}
