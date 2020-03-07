using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class ChatLine
    {
        public Colour Colour { get; set; }
        public string Text { get; set; }

        public ChatLine(int colourInt, string text)
        {
            this.Colour = IntToColour(colourInt);
            this.Text = text;
        }

        public Colour IntToColour(int colour)
        {
            switch (colour)
            {
                case 1:
                    return Colour.White;
                case 2: // guild // TODO: Map these based on config
                case 3: // group
                case 6: // tell
                    return Colour.Yellow;
                case 7: // server
                    return Colour.Green;
                case 8: // Dunno what this actually is, using it for my client messages now...
                    return Colour.Blue;
            }

            return Colour.White;
        }
    }

    class ChatListBox : GuiElement
    {
        private int displayedLines;
        private int lastViewIndex = -1;

        public bool FilterPickupMessages { get; set; } = false;

        private List<ChatLine> lines = new List<ChatLine>();

        public ChatListBox(int x, int y, int w, int h, int numLines) : base(x, y, w, h)
        {
            this.displayedLines = numLines;
            this.Padding = 6;


        }

        public override void Update(double dt)
        {

        }

        public override void Render(double dt, int xOffset, int yOffset)
        {
            if (lastViewIndex == -1) return;

            int y = Math.Max(0, displayedLines - lastViewIndex - 1) * GameClient.FontRenderer.CharHeight;

            for (int i = Math.Max(0, lastViewIndex - displayedLines + 1); i <= lastViewIndex; i++)
            {
                GameClient.FontRenderer.RenderText(
                    lines[i].Text, 
                    X + Padding + xOffset, 
                    Y + yOffset + y + 1, 
                    lines[i].Colour);

                y += GameClient.FontRenderer.CharHeight;
            }
        }

        public override bool HandleEvent(SDL.SDL_Event ev, int xOffset, int yOffset)
        {
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_PAGEUP)
                    {
                        lastViewIndex = Math.Max(0, lastViewIndex - 6);
                    }
                    else if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_PAGEDOWN)
                    {
                        lastViewIndex = Math.Min(lines.Count - 1, lastViewIndex + 6);
                    }
                    break;
            }

            return false;
        }

        public void AddText(int colour, string text)
        {
            if (FilterPickupMessages && colour == 3 && text.Contains("picked up") && !text.StartsWith("[group]")) return;

            foreach (var line in GameClient.FontRenderer.WordWrap(text, this.W, "  "))
            {
                if (lastViewIndex == lines.Count - 1)
                {
                    lastViewIndex++;
                }

                lines.Add(new ChatLine(colour, line));
            }

            if (lines.Count > 500)
            {
                int toRemove = lines.Count - 500;
                lines.RemoveRange(0, toRemove);
                lastViewIndex = Math.Max(0, lastViewIndex - toRemove);
            }
        }
    }
}
