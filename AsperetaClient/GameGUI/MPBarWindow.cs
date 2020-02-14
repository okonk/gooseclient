using System;
using System.Linq;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class MPBarWindow : StatBarWindow
    {
        public MPBarWindow() : base("MPbar")
        {
            GameClient.NetworkClient.PacketManager.Listen<StatusInfoPacket>(OnStatusInfo);
        }

        public void OnStatusInfo(object packet)
        {
            var p = (StatusInfoPacket)packet;

            this.value = p.CurrentMP;
            this.maxValue = p.MaxMP;
        }
    }
}
