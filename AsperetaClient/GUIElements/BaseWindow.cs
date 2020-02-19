using System;
using System.Linq;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class BaseWindow : BaseContainer
    {
        public string Name { get; private set; }

        public bool Hidden { get; set; } = false;

        protected Texture background;

        private int focusAlpha;
        private int unfocusAlpha;

        private bool mouseDown = false;
        private int lastMouseDragX = 0;
        private int lastMouseDragY = 0;

        public BaseWindow(string windowName)
        {
            this.Name = windowName;

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
            if (Hidden) return;

            background?.Render(X + xOffset, Y + yOffset, (this.HasFocus ? focusAlpha : unfocusAlpha));

            base.Render(dt, xOffset, yOffset);
        }

        public override bool HandleEvent(SDL.SDL_Event ev, int xOffset, int yOffset)
        {
            if (Hidden) return false;

            bool preventFurtherEvents = base.HandleEvent(ev, xOffset, yOffset);
            if (preventFurtherEvents)
                return true;

            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    if (ev.button.button == SDL.SDL_BUTTON_LEFT && Contains(xOffset, yOffset, ev.button.x, ev.button.y))
                    {
                        mouseDown = true;
                        lastMouseDragX = ev.button.x;
                        lastMouseDragY = ev.button.y;

                        return true;
                    }
                    break;
                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    if (ev.button.button == SDL.SDL_BUTTON_LEFT)
                    {
                        mouseDown = false;
                        lastMouseDragX = 0;
                        lastMouseDragY = 0;
                    }
                    break;
                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    if (mouseDown)
                    {
                        // Can't use the xrel since it's relative to desktop, not our window/scaling
                        this.Rect.x += (ev.motion.x - lastMouseDragX);
                        this.Rect.y += (ev.motion.y - lastMouseDragY);

                        lastMouseDragX = ev.motion.x;
                        lastMouseDragY = ev.motion.y;
                    }

                    // TODO: Window with focus also becomes on top of other windows
                    this.HasFocus = Contains(xOffset, yOffset, ev.motion.x, ev.motion.y);
                    break;
            }

            return false;
        }
    }
}
