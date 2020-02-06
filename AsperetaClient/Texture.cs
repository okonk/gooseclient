using System;
using SDL2;

namespace AsperetaClient
{
    class Texture
    {
        public IntPtr SDLTexture { get; set; }

        private SDL.SDL_Rect Rect;

        public int Width { get { return Rect.w; } }

        private int xOffset = 0;
        private int yOffset = 0;

        public Texture(IntPtr texture, int x, int y, int w, int h, bool needsOffset = true)
        {
            this.SDLTexture = texture;

            SDL.SDL_Rect tRect;
            tRect.x = x;
            tRect.y = y;
            tRect.w = w;
            tRect.h = h;

            Rect = tRect;

            if (needsOffset)
            {
                // offset to center the sprite on the tile
                if (Rect.h > Constants.TileSize)
                    yOffset = -Rect.h + Constants.TileSize;

                xOffset = (Constants.TileSize / 2) - Rect.w / 2;
            }
        }

        public void Render(int x, int y)
        {
            SDL.SDL_Rect dRect;
            dRect.x = x + xOffset;
            dRect.y = y + yOffset;
            dRect.w = Rect.w;
            dRect.h = Rect.h;

            SDL.SDL_RenderCopy(GameClient.Renderer, SDLTexture, ref Rect, ref dRect);
        }

        public void Render(int x, int y, int w, int h)
        {
            SDL.SDL_Rect dRect;
            dRect.x = x + xOffset;
            dRect.y = y + yOffset;
            dRect.w = w;
            dRect.h = h;

            SDL.SDL_RenderCopy(GameClient.Renderer, SDLTexture, ref Rect, ref dRect);
        }
    }
}
