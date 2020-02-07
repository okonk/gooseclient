using System;
using SDL2;

namespace AsperetaClient
{
    class Texture
    {
        public IntPtr SDLTexture { get; set; }

        private SDL.SDL_Rect Rect;

        public int Width { get { return Rect.w; } }

        public int XOffset { get; private set; }
        public int YOffset { get; private set; }

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
                    YOffset = -Rect.h + Constants.TileSize;

                XOffset = (Constants.TileSize / 2) - Rect.w / 2;
            }
        }

        public void Render(int x, int y)
        {
            SDL.SDL_Rect dRect;
            dRect.x = x + XOffset;
            dRect.y = y + YOffset;
            dRect.w = Rect.w;
            dRect.h = Rect.h;

            SDL.SDL_RenderCopy(GameClient.Renderer, SDLTexture, ref Rect, ref dRect);
        }

        public void Render(int x, int y, int w, int h)
        {
            SDL.SDL_Rect dRect;
            dRect.x = x + XOffset;
            dRect.y = y + YOffset;
            dRect.w = w;
            dRect.h = h;

            SDL.SDL_RenderCopy(GameClient.Renderer, SDLTexture, ref Rect, ref dRect);
        }
    }
}
