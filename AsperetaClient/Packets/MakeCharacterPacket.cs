using System;
using System.Collections.Generic;

namespace AsperetaClient
{
    class MakeCharacterPacket : PacketHandler
    {
        public int LoginId { get; set; }
        public int CharacterType { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Surname { get; set; }
        public string GuildName { get; set; }
        public int MapX { get; set; }
        public int MapY { get; set; }
        public int Facing { get; set; }
        public int HPPercent { get; set; }
        public int BodyId { get; set; }
        public int BodyState { get; set; }
        public int HairId { get; set; }
        public int[] EquippedItems { get; set; }
        public int HairR { get; set; }
        public int HairG { get; set; }
        public int HairB { get; set; }
        public int HairA { get; set; }
        public int Invisible { get; set; }
        public int FaceId { get; set; }

        public override string Prefix { get; } = "MKC";

        public override object Parse(PacketParser p)
        {
            return new MakeCharacterPacket()
            {  
                LoginId = p.GetInt32(),
                CharacterType = p.GetInt32(),
                Name = p.GetString(),
                Title = p.GetString(),
                Surname = p.GetString(),
                GuildName = p.GetString(),
                MapX = p.GetInt32() - 1,
                MapY = p.GetInt32() - 1,
                Facing = RemapFacing(p.GetInt32()),
                HPPercent = p.GetInt32(),
                BodyId = p.GetInt32(),
                BodyState = p.GetInt32(),
                HairId = p.GetInt32(),
                EquippedItems = ParseEquippedItems(p),
                HairR = p.GetInt32(),
                HairG = p.GetInt32(),
                HairB = p.GetInt32(),
                HairA = p.GetInt32(),
                Invisible = p.GetInt32(),
                FaceId = p.GetInt32()
            };
        }

        public int[] ParseEquippedItems(PacketParser p)
        {
            // Chest, Head, Legs, Feet, Shield, Weapon
            var equipped = new int[6 * 5];
            for (int i = 0; i < equipped.Length; )
            {
                equipped[i++] = p.GetInt32(); // item graphic id

                if (p.Peek() == '*')
                {
                    p.GetString(); // eat the string
                    equipped[i++] = 0; // r
                    equipped[i++] = 0; // g
                    equipped[i++] = 0; // b
                    equipped[i++] = 0; // a
                }
                else
                {
                    equipped[i++] = p.GetInt32(); // r
                    equipped[i++] = p.GetInt32(); // g
                    equipped[i++] = p.GetInt32(); // b
                    equipped[i++] = p.GetInt32(); // a
                }
            }

            return equipped;
        }

        public int RemapFacing(int facing)
        {
            if (facing == 3) return 2;
            if (facing == 4) return 3;
            if (facing == 2) return 4;

            return facing;
        }
    }
}
