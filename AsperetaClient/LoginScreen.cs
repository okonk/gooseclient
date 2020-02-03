using System;
using System.Collections.Generic;
using SDL2;

namespace AsperetaClient
{
    class LoginScreen : State
    {
        private int width = 800;
        private int height = 600;

        private Texture background;
        private Texture loginBox;

        private List<GuiElement> guiElements = new List<GuiElement>();

        // login box: 464, 328
        // login button: 544, 472
        // exit button: 648, 472
        // username textbox: 624, 354, 133, 26
        // password textbox: 625, 416, 133, 26
        // back colour: H00002474 // Abgr
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

            SDL.SDL_Color backgroundColour;
            backgroundColour.r = 192;
            backgroundColour.g = 255;
            backgroundColour.b = 192;
            backgroundColour.a = 255;

            SDL.SDL_Color foregroundColour;
            foregroundColour.r = 0;
            foregroundColour.g = 0;
            foregroundColour.b = 0;
            foregroundColour.a = 255;

            var usernameTextbox = new TextBox(608, 366, 125, 31, backgroundColour, foregroundColour) { HasFocus = true };
            var passwordTextbox = new TextBox(608, 408, 125, 31, backgroundColour, foregroundColour) { PasswordMask = '*' };

            guiElements.Add(usernameTextbox);
            guiElements.Add(passwordTextbox);

            var loginButton = new Button(524, 450, 83, 43)
            { 
                UpTexture = GameClient.ResourceManager.GetTexture($"skins/{GameClient.GameSettings.Skin}/login_button.bmp"),
                Clicked = (_) => { GameClient.StateManager.AppendState(new GameScreen()); }
            };

            var exitButton = new Button(640, 450, 83, 43)
            {
                UpTexture = GameClient.ResourceManager.GetTexture($"skins/{GameClient.GameSettings.Skin}/exit_button.bmp")
            };

            guiElements.Add(loginButton);
            guiElements.Add(exitButton);
        }

        public override void Update(double dt)
        {
            foreach (var gui in guiElements)
            {
                gui.Update(dt);
            }
        }

        public override void Render(double dt)
        {
            background.Render(0, 0);
            loginBox.Render(464, 328);

            foreach (var gui in guiElements)
            {
                gui.Render(dt);
            }
        }

        public override void HandleEvent(SDL.SDL_Event ev)
        {
            foreach (var gui in guiElements)
            {
                bool preventFurtherEvents = gui.HandleEvent(ev);
                if (preventFurtherEvents)
                    return;
            }
        }
    }
}
