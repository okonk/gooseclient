using System;
using System.Collections.Generic;

namespace GooseClient
{
    class WeaponSpeedPacket : PacketHandler
    {
        public int Speed { get; set; }

        public override string Prefix { get; } = "WPS";

        public override object Parse(PacketParser p)
        {
            return new WeaponSpeedPacket()
            {
                Speed = p.GetInt32()
            };
        }
    }
}
