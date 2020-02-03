using System;
using System.Collections.Generic;
using SDL2;

namespace AsperetaClient
{
    abstract class GuiElement
    {
        public SDL.SDL_Rect Rect;

        public int X { get { return Rect.x; } }

        public int Y { get { return Rect.y; } }

        public int W { get { return Rect.w; } }

        public int H { get { return Rect.h; } }

        public int Padding { get; set; }

        public SDL.SDL_Color BackgroundColour { get; set; }

        public SDL.SDL_Color ForegroundColour { get; set; }

        public List<GuiElement> Children { get; set; } = new List<GuiElement>();

        public bool HasFocus { get; set; }

        public GuiElement(int x, int y, int w, int h)
        {
            SDL.SDL_Rect rect;
            rect.x = x;
            rect.y = y;
            rect.w = w;
            rect.h = h;
            this.Rect = rect;

            this.HasFocus = false;
        }

        public GuiElement(int x, int y, int w, int h, SDL.SDL_Color backgroundColour, SDL.SDL_Color foregroundColour) : this(x, y, w, h)
        {
            this.BackgroundColour = backgroundColour;
            this.ForegroundColour = foregroundColour;
        }

        public abstract void Render(double dt);

        public virtual void Update(double dt) { }

        public virtual bool HandleEvent(SDL.SDL_Event ev) { return false; }
    }
}
