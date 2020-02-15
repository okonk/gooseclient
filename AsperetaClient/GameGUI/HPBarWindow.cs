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

        public override bool HandleEvent(SDL.SDL_Event ev, int xOffset, int yOffset)
        {
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_F6)
                    {
                        this.Hidden = !this.Hidden;
                        return true;
                    }
                    break;
            }

            return base.HandleEvent(ev, xOffset, yOffset);
        }
    }
}
