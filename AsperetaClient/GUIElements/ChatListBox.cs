using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    enum ChatType
    {
        Chat = 1,
        Guild,
        Group,
        Melee,
        Spells,
        Tell,
        Server,
        Client = 8,
    }

    class ChatLine
    {
        public Colour Colour { get; set; }
        public string Text { get; set; }

        public ChatLine(Colour colour, string text)
        {
            this.Colour = colour;
            this.Text = text;
        }
    }

    class ChatListBox : GuiElement
    {
        private static Dictionary<ChatType, Colour> chatColours = new Dictionary<ChatType, Colour>();

        private int displayedLines;
        private int lastViewIndex = -1;

        public bool FilterPickupMessages { get; set; } = false;

        private List<ChatLine> lines = new List<ChatLine>();

        public ChatListBox(int x, int y, int w, int h, int numLines) : base(x, y, w, h)
        {
            this.displayedLines = numLines;
            this.Padding = 6;

            var filterColours = GameClient.UserSettings["FilterColors"];
            foreach (var kvp in filterColours)
            {
                if (Enum.TryParse<ChatType>(kvp.Key, true, out ChatType chatType))
                {
                    chatColours[chatType] = ParseColour(kvp.Value);
                }
            }

            if (!chatColours.ContainsKey(ChatType.Client))
                chatColours[ChatType.Client] = Colour.Blue;
        }

        private Colour ParseColour(string colourString)
        {
            switch (colourString.ToLowerInvariant())
            {
                case "white": return Colour.White;
                case "black": return Colour.Black;
                case "yellow": return Colour.Yellow;
                case "green": return Colour.Green;
                case "red": return Colour.Red;
                case "blue": return Colour.Blue;
                case "purple": return Colour.Purple;
                default:
                    var splits = colourString.Split(',');
                    if (splits.Length >= 3)
                    {
                        byte.TryParse(splits[0], out byte r);
                        byte.TryParse(splits[1], out byte g);
                        byte.TryParse(splits[2], out byte b);
                        byte a = 255;
                        if (splits.Length > 3)
                            byte.TryParse(splits[3], out a);
                        return new Colour(r, g, b, a);
                    }
                    else
                    {
                        return Colour.White;
                    }
            }
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

        public void AddText(ChatType chatType, string text)
        {
            if (FilterPickupMessages && chatType == ChatType.Group && text.Contains("picked up") && !text.StartsWith("[group]")) return;

            foreach (var line in GameClient.FontRenderer.WordWrap(text, this.W, "  "))
            {
                if (lastViewIndex == lines.Count - 1)
                {
                    lastViewIndex++;
                }

                lines.Add(new ChatLine(chatColours[chatType], line));
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
