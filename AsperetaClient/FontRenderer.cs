using System;
using SDL2;

namespace AsperetaClient
{
    class FontRenderer
    {
        private IntPtr FontTexture { get; set; }
        private int[] CharOffsets = new int[255];

        public int CharHeight { get { return 11; } }
        public int CharWidth { get { return 6; } }

        public const string Letters = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

        public FontRenderer()
        {
            this.FontTexture = GameClient.ResourceManager.GetSDLTexture(GameClient.ResourceManager.AdfManager.Files[101]);

            SDL.SDL_SetTextureBlendMode(this.FontTexture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

            int offset = 0;
            foreach (var c in Letters)
            {
                CharOffsets[(int)c] = offset;
                offset += CharWidth;
            }
        }

        public void RenderText(string text, int x, int y, Colour colour, int limitX = 0)
        {
            SDL.SDL_SetTextureColorMod(this.FontTexture, colour.R, colour.G, colour.B);
            SDL.SDL_SetTextureAlphaMod(this.FontTexture, colour.A);

            SDL.SDL_Rect dRect;
            dRect.y = y;
            dRect.w = CharWidth;
            dRect.h = CharHeight;

            SDL.SDL_Rect sRect;
            sRect.y = 0;
            sRect.w = CharWidth;
            sRect.h = CharHeight;

            foreach (var c in text)
            {
                if (c == ' ')
                {
                    x += CharWidth;
                    continue;
                }

                int i = c;
                if (c > 256)
                    i = '`';

                sRect.x = CharOffsets[i];
                dRect.x = x;

                SDL.SDL_RenderCopy(GameClient.Renderer, FontTexture, ref sRect, ref dRect);

                x += CharWidth;

                if (limitX > 0 && x > limitX)
                    return;
            }
        }
    }
}
