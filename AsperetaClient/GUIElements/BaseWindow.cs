using System;
using System.Linq;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class BaseWindow : BaseContainer
    {
        public bool Hidden { get; set; } = false;

        protected Texture background;

        private int focusAlpha;
        private int unfocusAlpha;

        public BaseWindow(string windowName)
        {
            background = GameClient.ResourceManager.GetTexture($"skins/{GameClient.GameSettings["INIT"]["Skin"]}/{GameClient.WindowSettings[windowName]["image"]}");

            var winloc = GameClient.UserSettings.GetCoords(windowName, "winloc");
            SDL.SDL_Rect rect;
            rect.x = winloc.ElementAt(0);
            rect.y = winloc.ElementAt(1);
            rect.w = background.W;
            rect.h = background.H;

            this.Rect = rect;

            this.HasFocus = false;
            this.ZIndex = -1;

            Hidden = GameClient.UserSettings[windowName]["startup"] == "0";

            var alphas = GameClient.WindowSettings.GetCoords(windowName, "focus");
            unfocusAlpha = alphas.ElementAt(0);
            focusAlpha = alphas.ElementAt(1);
        }

        public override void Render(double dt, int xOffset, int yOffset)
        {
            background?.Render(X + xOffset, Y + yOffset, unfocusAlpha);

            base.Render(dt, xOffset, yOffset);
        }

        public override bool HandleEvent(SDL.SDL_Event ev, int xOffset, int yOffset)
        {
            switch (ev.type)
            {
                
            }

            return false;
        }
    }
}
