using System;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class Tooltip : GuiElement
    {
        public override int W { get { return GameClient.FontRenderer.CharWidth * Value.Length + this.Padding * 2; } }

        public override int H { get { return GameClient.FontRenderer.CharHeight + 4; } }

        public string Value { get; set; }

        public Tooltip(int x, int y, Colour backgroundColour, Colour foregroundColour, string value) : base(x, y, 0, 0, backgroundColour, foregroundColour)
        {
            this.Padding = 6;
            this.Value = value;
        }

        public override void Render(double dt, int xOffset, int yOffset)
        {
            int x = X;
            int y = Y;

            SDL.SDL_SetRenderDrawColor(GameClient.Renderer, BackgroundColour.R, BackgroundColour.G, BackgroundColour.B, BackgroundColour.A);
            SDL.SDL_Rect dRect;
            dRect.x = x;
            dRect.y = y;
            dRect.w = W;
            dRect.h = H;
            SDL.SDL_RenderFillRect(GameClient.Renderer, ref dRect);
            SDL.SDL_SetRenderDrawColor(GameClient.Renderer, ForegroundColour.R, ForegroundColour.G, ForegroundColour.B, ForegroundColour.A);
            SDL.SDL_RenderDrawRect(GameClient.Renderer, ref dRect);

            GameClient.FontRenderer.RenderText(Value, x + Padding, y + 3, ForegroundColour);
        }
    }
}