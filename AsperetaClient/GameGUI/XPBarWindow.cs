using System;
using System.Linq;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class XPBarWindow : StatBarWindow
    {
        private int percentage = 0;

        public XPBarWindow() : base("XPbar")
        {
            GameClient.NetworkClient.PacketManager.Listen<ExperienceBarPacket>(OnExperienceBar);
        }

        public void OnExperienceBar(object packet)
        {
            var p = (ExperienceBarPacket)packet;

            this.value = p.ExperienceToNextLevel;
            this.percentage = p.Percentage;
        }

        protected override double GetPercentage()
        {
            return percentage / 100d;
        }

        public override bool HandleEvent(SDL.SDL_Event ev, int xOffset, int yOffset)
        {
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_F9)
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
