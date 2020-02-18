using System;
using System.Linq;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class GameScreen : State
    {
        private int mapNumber;
        private string mapName;
        public Map Map { get; set; }

        private Character player;


        private bool moveKeyDown = false;
        private Direction moveKeyDirection = Direction.Down;
        private double moveKeyPressedTime = 0;

        private RootPanel uiRoot;

        public GameScreen(int mapNumber, string mapName)
        {
            this.mapNumber = mapNumber;
            this.mapName = mapName;

            
            GameClient.NetworkClient.PacketManager.Listen<SetYourCharacterPacket>(OnSetYourCharacter);
            GameClient.NetworkClient.PacketManager.Listen<PingPacket>(OnPing);
            GameClient.NetworkClient.PacketManager.Listen<SetYourPositionPacket>(OnSetYourPosition);
            GameClient.NetworkClient.PacketManager.Listen<SendCurrentMapPacket>(OnSendCurrentMap);
        }

        public override void Resuming()
        {
            GameClient.ScreenWidth = 640;
            GameClient.ScreenHeight = 480;
            SDL.SDL_RenderSetLogicalSize(GameClient.Renderer, GameClient.ScreenWidth, GameClient.ScreenHeight);
        }

        public override void Starting()
        {
            Resuming();

            CreateGui();

            LoadMap();
        }

        public void LoadMap()
        {
            GameClient.StateManager.AppendState(new MapLoadingScreen(mapNumber, mapName, this));
        }

        public void CreateGui()
        {
            this.uiRoot = new RootPanel();

            this.uiRoot.AddChild(new ChatWindow());
            this.uiRoot.AddChild(new FpsWindow());
            this.uiRoot.AddChild(new BuffBarWindow());
            this.uiRoot.AddChild(new HotkeyBarWindow());
            this.uiRoot.AddChild(new HPBarWindow());
            this.uiRoot.AddChild(new MPBarWindow());
            this.uiRoot.AddChild(new XPBarWindow());

            this.uiRoot.AddChild(new CharacterWindow());
            this.uiRoot.AddChild(new SpellbookWindow());
            this.uiRoot.AddChild(new InventoryWindow());
        }
        
        public override void Render(double dt)
        {
            int half_x = GameClient.ScreenWidth / 2 - Constants.TileSize;
            int half_y = GameClient.ScreenHeight / 2 - Constants.TileSize;
            int start_x = player != null ? player.PixelXi - half_x - (player.GetWidth() / 2) : 0;
            int start_y = player != null ? player.PixelYi - half_y : 0;

            Map.Render(start_x, start_y);
            this.uiRoot.Render(dt, 0, 0);
        }

        public override void Update(double dt)
        {
            var keysPtr = SDL.SDL_GetKeyboardState(out int keysLength);
            byte[] keys = new byte[keysLength];
            Marshal.Copy(keysPtr, keys, 0, keysLength);

            if (keys[(int)SDL.SDL_Scancode.SDL_SCANCODE_UP] == 1)
            {
                MoveKeyPressed(Direction.Up);
            }
            else if (keys[(int)SDL.SDL_Scancode.SDL_SCANCODE_RIGHT] == 1)
            {
                MoveKeyPressed(Direction.Right);
            }
            else if (keys[(int)SDL.SDL_Scancode.SDL_SCANCODE_DOWN] == 1)
            {
                MoveKeyPressed(Direction.Down);
            }
            else if (keys[(int)SDL.SDL_Scancode.SDL_SCANCODE_LEFT] == 1)
            {
                MoveKeyPressed(Direction.Left);
            }
            else
            {
                moveKeyDown = false;
                moveKeyPressedTime = 0;
            }

            Map.Update(dt);

            if (moveKeyDown)
            {
                moveKeyPressedTime += dt;
            }

            this.uiRoot.Update(dt);
        }

        public override void HandleEvent(SDL.SDL_Event ev)
        {
            this.uiRoot.HandleEvent(ev, 0, 0);
        }

        public void MoveKeyPressed(Direction direction)
        {
            if (player == null || player.Moving) return;

            bool delay = true;
            if (!moveKeyDown || moveKeyDirection != direction)
            {
                moveKeyDown = true;
                moveKeyDirection = direction;
                moveKeyPressedTime = 0;

                if (player.Facing != direction)
                {
                    player.Facing = direction;
                    GameClient.NetworkClient.Facing(direction);
                    return;
                }
                else
                {
                    delay = false;
                }
            }

            if (delay && moveKeyPressedTime < 0.1) return;

            int destX = player.TileX;
            int destY = player.TileY;

            switch (direction)
            {
                case Direction.Up:
                    destY -= 1;
                    break;
                case Direction.Right:
                    destX += 1;
                    break;
                case Direction.Down:
                    destY += 1;
                    break;
                case Direction.Left:
                    destX -= 1;
                    break;
            }

            if (Map.CanMoveTo(destX, destY))
            {
                Map.MoveCharacter(player, destX, destY);
                GameClient.NetworkClient.Move(direction);
            }
        }

        public void OnSetYourCharacter(object packet)
        {
            var p = (SetYourCharacterPacket)packet;

            this.player = this.Map.Characters.FirstOrDefault(c => c.LoginId == p.LoginId);

            GameClient.UserName = this.player.Name;
        }

        public void OnPing(object packet)
        {
            GameClient.NetworkClient.Pong();
        }

        public void OnSendCurrentMap(object packet)
        {
            var sendCurrentMapPacket = (SendCurrentMapPacket)packet;
            this.mapNumber = sendCurrentMapPacket.MapNumber;
            this.mapName = sendCurrentMapPacket.MapName;
            LoadMap();
        }

        public void OnSetYourPosition(object packet)
        {
            var p = (SetYourPositionPacket)packet;

            if (Map[player.TileX, player.TileY].Character == player)
                Map[player.TileX, player.TileY].Character = null;

            player.SetPosition(p.MapX, p.MapY);
            Map[player.TileX, player.TileY].Character = player;
        }
    }
}