using System;
using System.Linq;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class FpsWindow : BaseWindow
    {
        private double recordingTime = 0;
        private int numberOfFrames = 0;

        private Label fpsLabel;

        public FpsWindow() : base("FPS")
        {
            var objoff = GameClient.WindowSettings.GetCoords("FPS", "objoff");
            fpsLabel = new Label(objoff.ElementAt(0) + 2, objoff.ElementAt(1) + 1, Colour.White, "FPS 100");
            this.AddChild(fpsLabel);
        }

        public override void Update(double dt)
        {
            base.Update(dt);

            recordingTime += dt;
            numberOfFrames++;

            if (recordingTime > 1)
            {
                var fps = Math.Ceiling(numberOfFrames / recordingTime);

                recordingTime = 0;
                numberOfFrames = 0;

                fpsLabel.Value = $"FPS {fps:0}";
            }
        }

        public override bool HandleEvent(SDL.SDL_Event ev, int xOffset, int yOffset)
        {
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_F5)
                    {
                        this.Hidden = !this.Hidden;
                        return true;
                    }
                    break;
            }

            return base.HandleEvent(ev, xOffset, yOffset);
        }
    }
}
