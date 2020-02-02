using System;
using SDL2;

namespace AsperetaClient
{
    class LoginScreen : State
    {
        private int width = 800;
        private int height = 600;

        private Texture background;
        private Texture loginBox;

        private TextBox usernameTextbox;

        // login box: 464, 328
        // login button: 544, 472
        // exit button: 648, 472
        // username textbox: 624, 354, 133, 26
        // password textbox: 625, 416, 133, 26
        // back colour: H00002474
        // foreground colour: H0080FFFF


        // old asp username: 608; 366; 125; 31, comic sans ms 12, foregound black, background: &H00C0FFC0&
        // old asp password: 608; 408; 125; 31
        // login button: 524; 450; 83; 43
        // exit button: 640; 450; 83; 43
        // login box: 464; 4920; 4800; 3000

        public override void Starting()
        {
            SDL.SDL_RenderSetLogicalSize(GameClient.Renderer, width, height);
            background = GameClient.ResourceManager.GetTexture($"skins/{GameClient.GameSettings.Skin}/backdrop.bmp");
            loginBox = GameClient.ResourceManager.GetTexture($"skins/{GameClient.GameSettings.Skin}/login_box.bmp");

            usernameTextbox = new TextBox(width - 150, height - 300, 100, 30);
        }

        public override void Render(double dt)
        {
            background.Render(0, 0);
            loginBox.Render(464, 328);


        }

        public override void HandleEvent(SDL.SDL_Event ev)
        {
            if (ev.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN)
            {
                GameClient.StateManager.AppendState(new GameScreen());
            }
        }
    }
}
