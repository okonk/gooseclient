using System;
using System.Collections.Generic;

namespace AsperetaClient
{
    class ExperienceBarPacket : PacketHandler
    {
        public int Percentage { get; set; }

        public long Experience { get; set; }

        public long ExperienceToNextLevel { get; set; }

        public override string Prefix { get; } = "TNL";

        public override object Parse(PacketParser p)
        {
            return new ExperienceBarPacket()
            {  
                Percentage = p.GetInt32(),
                Experience = p.GetInt64(),
                ExperienceToNextLevel = p.GetInt64()
            };
        }
    }
}
