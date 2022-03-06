using System;
using System.Collections.Generic;

namespace GooseClient
{
    class ExperienceBarPacket : PacketHandler
    {
        public int Percentage { get; set; }

        public long Experience { get; set; }

        public long ExperienceToNextLevel { get; set; }

        public override string Prefix { get; } = "TNL";

        public override object Parse(PacketParser p)
        {
            var packet = new ExperienceBarPacket()
            {
                Percentage = p.GetInt32(),
                Experience = p.GetInt64(),
                ExperienceToNextLevel = p.GetInt64()
            };

            // My extension to display uncapped experience value on newer clients
            if (p.LengthRemaining() > 0)
            {
                packet.ExperienceToNextLevel = p.GetInt64();
            }

            return packet;
        }
    }
}
