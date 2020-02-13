using System;
using System.Collections.Generic;
using SDL2;

namespace AsperetaClient
{
    class LoginScreen : State
    {
        private Texture background;
        private Texture loginBox;

        private TextBox usernameTextbox;
        private TextBox passwordTextbox;

        private BaseContainer guiContainer = new BaseContainer();

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
            GameClient.ScreenWidth = 800;
            GameClient.ScreenHeight = 600;
            SDL.SDL_RenderSetLogicalSize(GameClient.Renderer, GameClient.ScreenWidth, GameClient.ScreenHeight);

            background = GameClient.ResourceManager.GetTexture($"skins/{GameClient.GameSettings["INIT"]["Skin"]}/backdrop.bmp");
            loginBox = GameClient.ResourceManager.GetTexture($"skins/{GameClient.GameSettings["INIT"]["Skin"]}/login_box.bmp");

            usernameTextbox = new TextBox(608, 366, 125, 31, new Colour(192, 255, 192), Colour.Black) { HasFocus = true };
            passwordTextbox = new TextBox(608, 408, 125, 31, new Colour(192, 255, 192), Colour.Black) { PasswordMask = '*' };
            passwordTextbox.EnterPressed += Connect;

            usernameTextbox.SetValue(GameClient.GameSettings["INIT"]["Name"]);
            if (GameClient.GameSettings["INIT"].ContainsKey("Password"))
                passwordTextbox.SetValue(GameClient.GameSettings["INIT"]["Password"]);

            guiContainer.AddChild(usernameTextbox);
            guiContainer.AddChild(passwordTextbox);

            var loginButton = new Button(524, 450, 83, 43)
            { 
                UpTexture = GameClient.ResourceManager.GetTexture($"skins/{GameClient.GameSettings["INIT"]["Skin"]}/login_button.bmp")
            };
            loginButton.Clicked += Connect;

            var exitButton = new Button(640, 450, 83, 43)
            {
                UpTexture = GameClient.ResourceManager.GetTexture($"skins/{GameClient.GameSettings["INIT"]["Skin"]}/exit_button.bmp")
            };
            exitButton.Clicked += (_) => { GameClient.Running = false; };

            guiContainer.AddChild(loginButton);
            guiContainer.AddChild(exitButton);
        }

        public override void Update(double dt)
        {
            guiContainer.Update(dt);
        }

        public override void Render(double dt)
        {
            background.Render(0, 0);
            loginBox.Render(464, 328);

            guiContainer.Render(dt, 0, 0);
        }

        public override void HandleEvent(SDL.SDL_Event ev)
        {
            guiContainer.HandleEvent(ev, 0, 0);
        }

        public void Connect(Button b)
        {
            Connect();
        }

        public void Connect()
        {
            if (usernameTextbox.Value.Length <= 1 || passwordTextbox.Value.Length <= 1)
                return;

            GameClient.NetworkClient = new NetworkClient();
            GameClient.NetworkClient.ReceiveError += OnReceiveError;

            var connectingWindow = new ConnectingWindow(usernameTextbox.Value, passwordTextbox.Value);
            guiContainer.AddChild(connectingWindow);
        }

        public void OnReceiveError(Exception e)
        {
            Console.WriteLine($"Socket error receiving data: {e}");
            GameClient.StateManager.RemoveState();
        }
    }
}
