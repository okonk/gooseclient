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

        public override bool HandleEvent(SDL.SDL_Event ev, int xOffset, int yOffset)
        {
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_F7)
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
