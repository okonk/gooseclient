using System;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class Label : GuiElement
    {
        public override int W { get { return GameClient.FontRenderer.CharWidth * Value.Length; } }

        public override int H { get { return GameClient.FontRenderer.CharHeight; } }

        public string Value { get; set; }

        public bool DrawShadow { get; set; } = false;

        public bool WordWrap { get; set; } = false;

        public Label(int x, int y, Colour foregroundColour, string value) : base(x, y, 0, 0)
        {
            this.ForegroundColour = foregroundColour;
            this.Value = value;
        }

        public override void Render(double dt, int xOffset, int yOffset)
        {
            if (WordWrap)
            {
                RenderWrapped(xOffset, yOffset);
            }
            else
            {
                int x = xOffset + X;
                int y = yOffset + Y;

                if (DrawShadow)
                {
                    // TODO: Make shadow colour configurable
                    GameClient.FontRenderer.RenderText(Value, x - 1, y + 1, Colour.Black);
                }

                GameClient.FontRenderer.RenderText(Value, x, y, ForegroundColour);
            }
        }

        public void RenderWrapped(int xOffset, int yOffset)
        {
            int availableSpace = (this.Parent == null ? GameClient.ScreenWidth : this.Parent.W - this.Parent.Padding) - this.X;
            int neededSpace = Value.Length * GameClient.FontRenderer.CharWidth;
            int currentIndex = 0;

            int y = yOffset + Y;

            while (currentIndex < Value.Length)
            {
                int x = xOffset + X;

                if (neededSpace < availableSpace)
                {
                    GameClient.FontRenderer.RenderText(Value.Substring(currentIndex), x, y, ForegroundColour);
                    return;
                }

                bool failedToSplit = true;
                for (int i = (availableSpace / GameClient.FontRenderer.CharWidth) - 1; i > 0; i--)
                {
                    if (Value[currentIndex + i] == ' ')
                    {
                        GameClient.FontRenderer.RenderText(Value.Substring(currentIndex, i), x, y, ForegroundColour);

                        currentIndex += i + 1;
                        neededSpace = (Value.Length - currentIndex) * GameClient.FontRenderer.CharWidth;

                        failedToSplit = false;

                        break;
                    }
                }

                if (failedToSplit)
                {
                    int splitPoint = (availableSpace / GameClient.FontRenderer.CharWidth) - 1;
                    GameClient.FontRenderer.RenderText(Value.Substring(currentIndex, splitPoint), x, y, ForegroundColour);

                    currentIndex += splitPoint;
                    neededSpace = (Value.Length - currentIndex) * GameClient.FontRenderer.CharWidth;
                }

                y += GameClient.FontRenderer.CharHeight;
            }
        }
    }
}
