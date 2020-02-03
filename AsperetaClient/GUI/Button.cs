using System;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class Button : GuiElement
    {
        public Texture UpTexture { get; set; }
        public Texture DownTexture { get; set; }

        private bool pressed = false;

        public Action<Button> Clicked { get; set; }

        public Button(int x, int y, int w, int h) : base(x, y, w, h)
        {

        }

        public override void Update(double dt)
        {

        }

        public override void Render(double dt)
        {
            if (pressed && DownTexture != null)
                DownTexture.Render(this.X, this.Y);
            else
                UpTexture.Render(this.X, this.Y);
        }

        public override bool HandleEvent(SDL.SDL_Event ev)
        {
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    if (ev.button.x >= this.X && ev.button.x <= this.X + this.W &&
                        ev.button.y >= this.Y && ev.button.y <= this.Y + this.H)
                    {
                        pressed = true;
                    }

                    break;
                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    if (ev.button.x >= this.X && ev.button.x <= this.X + this.W &&
                        ev.button.y >= this.Y && ev.button.y <= this.Y + this.H)
                    {
                        if (Clicked != null)
                            Clicked(this);
                    }

                    pressed = false;

                    break;
            }

            return false;
        }
    }
}
