using System;
using SDL2;

namespace AsperetaClient
{
    class Texture
    {
        public IntPtr SDLTexture { get; set; }

        private SDL.SDL_Rect Rect;

        public int Width { get { return Rect.w; } }

        public Texture(IntPtr texture, int x, int y, int w, int h)
        {
            this.SDLTexture = texture;

            SDL.SDL_Rect tRect;
            tRect.x = x;
            tRect.y = y;
            tRect.w = w;
            tRect.h = h;

            Rect = tRect;
        }

        public void Render(int x, int y)
        {
            int y_off = 0;
            if (Rect.h > Constants.TileSize)
                y_off = Rect.h - Constants.TileSize;

            // center the sprite on the tile
            int x_offset = x;
            int y_offset = y;
            int draw_x = x_offset + (Constants.TileSize / 2) - Rect.w / 2;

            SDL.SDL_Rect dRect;
            dRect.x = draw_x;
            dRect.y = (y_offset - y_off);
            dRect.w = Rect.w;
            dRect.h = Rect.h;

            SDL.SDL_RenderCopy(GameClient.Renderer, SDLTexture, ref Rect, ref dRect);
        }
    }
}
