using System;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class BaseWindow : GuiElement
    {


        public BaseWindow(int x, int y, int w, int h) : base(x, y, w, h)
        {

        }

        public override void Update(double dt)
        {

        }

        public override void Render(double dt, int xOffset, int yOffset)
        {
  
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
