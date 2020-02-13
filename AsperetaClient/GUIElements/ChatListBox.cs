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
            }

            return Colour.White;
        }
    }

    class ChatListBox : GuiElement
    {
        private int displayedLines;
        private int lastViewIndex = -1;

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

        public void AddLine(int colour, string text)
        {
            foreach (var line in WordWrap(text))
            {
                if (lastViewIndex == lines.Count - 1)
                {
                    lastViewIndex++;
                }

                lines.Add(new ChatLine(colour, line));
            }

            // TODO: Remove older lines when > some number of lines
 
        }

        private IEnumerable<string> WordWrap(string input)
        {
            const int INDENT_CHARS = 2;

            int availableSpace = this.W;
            int neededSpace = input.Length * GameClient.FontRenderer.CharWidth;
            int currentIndex = 0;

            bool indent = false;

            while (currentIndex < input.Length)
            {
                int x = X + Padding;

                if (neededSpace < availableSpace)
                {
                    yield return (indent ? "  " : "") + input.Substring(currentIndex);
                    yield break;
                }

                bool failedToSplit = true;
                for (int i = (availableSpace / GameClient.FontRenderer.CharWidth) - 1; i > 0; i--)
                {
                    if (input[currentIndex + i] == ' ')
                    {
                        if (indent)
                        {
                            yield return "  " + input.Substring(currentIndex, i - 2);
                            currentIndex += i - 1;
                        }
                        else
                        {
                            yield return input.Substring(currentIndex, i);
                            currentIndex += i + 1;

                            indent = true;
                        }

                        neededSpace = (input.Length - currentIndex + INDENT_CHARS) * GameClient.FontRenderer.CharWidth;

                        failedToSplit = false;

                        break;
                    }
                }

                if (failedToSplit)
                {
                    int splitPoint = (availableSpace / GameClient.FontRenderer.CharWidth) - 1;

                    if (indent)
                    {
                        yield return (indent ? "  " : "") + input.Substring(currentIndex, splitPoint - 2);
                        currentIndex += splitPoint - 2;
                    }
                    else
                    {
                        yield return (indent ? "  " : "") + input.Substring(currentIndex, splitPoint);
                        currentIndex += splitPoint;

                        indent = true;
                    }
                    
                    neededSpace = (input.Length - currentIndex + INDENT_CHARS) * GameClient.FontRenderer.CharWidth;
                }
            }
        }
    }
}
