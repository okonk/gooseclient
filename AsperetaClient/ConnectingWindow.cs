using System;
using SDL2;

namespace AsperetaClient
{
    class ConnectingWindow : GuiElement
    {
        private Texture backgroundTexture;

        private Label messageLabel;

        private string username;
        private string password;

        public ConnectingWindow(string username, string password) : base(-1, -1, 200, 100)
        {
            this.ZIndex = 10;
            this.Padding = 10;

            backgroundTexture = GameClient.ResourceManager.GetTexture($"skins/{GameClient.GameSettings.Skin}/Working.bmp");

            var cancelButton = new Button(-1, 55, 66, 34, "Cancel");
            cancelButton.UpTexture = GameClient.ResourceManager.GetTexture($"skins/{GameClient.GameSettings.Skin}/blankbuttonup.bmp");
            cancelButton.DownTexture = GameClient.ResourceManager.GetTexture($"skins/{GameClient.GameSettings.Skin}/blankbuttondown.bmp");
            cancelButton.Clicked += CancelConnect;
            AddChild(cancelButton);

            messageLabel = new Label(10, 10, Colour.White, "Connecting...") { WordWrap = true };
            AddChild(messageLabel);

            this.username = username;
            this.password = password;

            GameClient.NetworkClient.Connected += OnConnected;
            GameClient.NetworkClient.ConnectionError += OnConnectionError;

            GameClient.NetworkClient.PacketHandler.Listen<LoginSuccessPacket>(OnLoginSuccess);
            GameClient.NetworkClient.PacketHandler.Listen<LoginFailPacket>(OnLoginFail);

            GameClient.NetworkClient.Connect();
        }

        public override void Update(double dt)
        {
            foreach (var gui in Children.ToArray())
            {
                gui.Update(dt);
            }
        }

        public override void Render(double dt, int xOffset, int yOffset)
        {
            SDL.SDL_Rect fullScreen;
            fullScreen.x = 0;
            fullScreen.y = 0;
            fullScreen.w = GameClient.ScreenWidth;
            fullScreen.h = GameClient.ScreenHeight;

            SDL.SDL_SetRenderDrawColor(GameClient.Renderer, 0, 0, 0, 180);
            SDL.SDL_RenderFillRect(GameClient.Renderer, ref fullScreen);

            backgroundTexture.Render(X + xOffset, Y + yOffset, W, H);

            foreach (var gui in Children)
            {
                gui.Render(dt, X + xOffset, Y + yOffset);
            }
        }

        public override bool HandleEvent(SDL.SDL_Event ev, int xOffset, int yOffset)
        {
            foreach (var gui in Children.ToArray())
            {
                bool preventFurtherEvents = gui.HandleEvent(ev, X + xOffset, Y + yOffset);
                if (preventFurtherEvents)
                    return true;
            }

            return true;
        }

        public void CancelConnect(Button b)
        {
            GameClient.NetworkClient.Disconnect();
            Close();
        }

        public void Close()
        {
            this.Parent.RemoveChild(this);
        }

        public void OnConnected()
        {
            messageLabel.Value = "Connected!";
            GameClient.NetworkClient.Login(username, password);
        }

        public void OnConnectionError(Exception e)
        {
            messageLabel.Value = $"Could not connect to server. {e.Message}";
        }

        public void OnLoginSuccess(object packet)
        {
            Close();
            GameClient.StateManager.AppendState(new GameScreen(((LoginSuccessPacket)packet).RealmName));
        }

        public void OnLoginFail(object packet)
        {
            messageLabel.Value = $"Could not connect to server. {((LoginFailPacket)packet).Message}";
        }
    }
}
