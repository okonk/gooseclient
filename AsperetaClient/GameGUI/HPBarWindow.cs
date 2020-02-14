using System;
using System.Linq;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class HPBarWindow : StatBarWindow
    {
        public HPBarWindow() : base("HPbar")
        {
            GameClient.NetworkClient.PacketManager.Listen<StatusInfoPacket>(OnStatusInfo);
        }

        public void OnStatusInfo(object packet)
        {
            var p = (StatusInfoPacket)packet;

            this.value = p.CurrentHP;
            this.maxValue = p.MaxHP;
        }
    }
}
