using System;
using System.Linq;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    abstract class StatBarWindow : BaseWindow
    {
        protected long value;

        protected long maxValue;

        private Texture barTexture;

        private int barX;
        private int barY;
        private int barW;
        private int barH;

        public StatBarWindow(string windowName) : base(windowName)
        {
            var barImage = GameClient.WindowSettings[this.Name]["image2"];
            barTexture = GameClient.ResourceManager.GetTexture($"skins/{GameClient.GameSettings["INIT"]["Skin"]}/{barImage}");

            var barXY = GameClient.WindowSettings.GetCoords(this.Name, "objoff");
            var barWH = GameClient.WindowSettings.GetCoords(this.Name, "objdim");

            barX = barXY.ElementAt(0);
            barY = barXY.ElementAt(1);
            barW = barWH.ElementAt(0);
            barH = barWH.ElementAt(1);
        }

        public override void Render(double dt, int xOffset, int yOffset)
        {
            if (Hidden) return;

            base.Render(dt, xOffset, yOffset);

            int w = (int)(barW * GetPercentage());

            barTexture.RenderClipped(X + barX + xOffset, barY + Y + yOffset, w, barH);

            var label = value.ToString();
            int labelX = barX + barW - (label.Length * GameClient.FontRenderer.CharWidth);

            GameClient.FontRenderer.RenderText(label, labelX + X + xOffset, barY + Y + yOffset + (barH / 2) - GameClient.FontRenderer.CharHeight / 2, Colour.White);
        }

        protected virtual double GetPercentage()
        {
            return ((double)value / maxValue);
        }
    }
}
