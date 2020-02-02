using System;
using System.Collections.Generic;
using SDL2;

namespace AsperetaClient
{
    abstract class GuiElement
    {
        public int X { get; set;}

        public int Y { get; set; }

        public int W { get; set; }

        public int H { get; set; }

        public List<GuiElement> Children { get; set; } = new List<GuiElement>();

        public GuiElement(int x, int y, int w, int h)
        {
            this.X = x;
            this.Y = y;
            this.W = w;
            this.H = h;
        }

        public abstract void Render(double dt);

        public virtual void Logic(double dt) { }

        public virtual void HandleEvent(SDL.SDL_Event ev) { }
    }
}
