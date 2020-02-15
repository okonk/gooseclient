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

        private BaseContainer uiContainer;

        public GameScreen(int mapNumber, string mapName)
        {
            this.mapNumber = mapNumber;
            this.mapName = mapName;

            GameClient.NetworkClient.PacketManager.Listen<MakeCharacterPacket>(OnMakeCharacter);
            GameClient.NetworkClient.PacketManager.Listen<SetYourCharacterPacket>(OnSetYourCharacter);
            GameClient.NetworkClient.PacketManager.Listen<PingPacket>(OnPing);
            GameClient.NetworkClient.PacketManager.Listen<MoveCharacterPacket>(OnMoveCharacter);
            GameClient.NetworkClient.PacketManager.Listen<ChangeHeadingPacket>(OnChangeHeading);
            GameClient.NetworkClient.PacketManager.Listen<SetYourPositionPacket>(OnSetYourPosition);
            GameClient.NetworkClient.PacketManager.Listen<VitalsPercentagePacket>(OnVitalsPercentage);
            GameClient.NetworkClient.PacketManager.Listen<SendCurrentMapPacket>(OnSendCurrentMap);
            GameClient.NetworkClient.PacketManager.Listen<EraseCharacterPacket>(OnEraseCharacter);
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
            this.uiContainer = new BaseContainer();

            this.uiContainer.AddChild(new ChatWindow());
            this.uiContainer.AddChild(new FpsWindow());
            this.uiContainer.AddChild(new BuffBarWindow());
            this.uiContainer.AddChild(new HotkeyBarWindow());
            this.uiContainer.AddChild(new HPBarWindow());
            this.uiContainer.AddChild(new MPBarWindow());
            this.uiContainer.AddChild(new XPBarWindow());

            this.uiContainer.AddChild(new InventoryWindow());
        }
        
        public override void Render(double dt)
        {
            int half_x = GameClient.ScreenWidth / 2 - Constants.TileSize;
            int half_y = GameClient.ScreenHeight / 2 - Constants.TileSize;
            int start_x = player != null ? player.PixelXi - half_x - (player.GetWidth() / 2) : 0;
            int start_y = player != null ? player.PixelYi - half_y : 0;

            Map.Render(start_x, start_y);
            this.uiContainer.Render(dt, 0, 0);
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

            this.uiContainer.Update(dt);
        }

        public override void HandleEvent(SDL.SDL_Event ev)
        {
            this.uiContainer.HandleEvent(ev, 0, 0);
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

        public void OnMakeCharacter(object packet)
        {
            var p = (MakeCharacterPacket)packet;

            var character = new Character(p);
            Map.AddCharacter(character);
        }

        public void OnSetYourCharacter(object packet)
        {
            var p = (SetYourCharacterPacket)packet;

            this.player = this.Map.Characters.FirstOrDefault(c => c.LoginId == p.LoginId);
        }

        public void OnPing(object packet)
        {
            GameClient.NetworkClient.Pong();
        }

        public void OnMoveCharacter(object packet)
        {
            var p = (MoveCharacterPacket)packet;

            var character = this.Map.Characters.FirstOrDefault(c => c.LoginId == p.LoginId);

            Map.MoveCharacter(character, p.MapX, p.MapY);
        }

        public void OnChangeHeading(object packet)
        {
            var p = (ChangeHeadingPacket)packet;
            var character = this.Map.Characters.FirstOrDefault(c => c.LoginId == p.LoginId);
            character.Facing = (Direction)p.Facing;
        }

        public void OnSetYourPosition(object packet)
        {
            var p = (SetYourPositionPacket)packet;

            if (Map[player.TileX, player.TileY].Character == player)
                Map[player.TileX, player.TileY].Character = null;

            player.SetPosition(p.MapX, p.MapY);
            Map[player.TileX, player.TileY].Character = player;
        }

        public void OnVitalsPercentage(object packet)
        {
            var p = (VitalsPercentagePacket)packet;
            var character = this.Map.Characters.FirstOrDefault(c => c.LoginId == p.LoginId);
            character.HPPercentage = p.HPPercentage;
            character.MPPercentage = p.MPPercentage;
            character.ShouldRenderHPMPBars = true;
            character.RenderHPMPBarsTime = 0;
        }

        public void OnSendCurrentMap(object packet)
        {
            var sendCurrentMapPacket = (SendCurrentMapPacket)packet;
            this.mapNumber = sendCurrentMapPacket.MapNumber;
            this.mapName = sendCurrentMapPacket.MapName;
            LoadMap();
        }

        public void OnEraseCharacter(object packet)
        {
            var p = (EraseCharacterPacket)packet;

            var character = this.Map.Characters.FirstOrDefault(c => c.LoginId == p.LoginId);
            if (character == null) return;

            if (Map[character.TileX, character.TileY].Character == character)
                Map[character.TileX, character.TileY].Character = null;

            Map.Characters.Remove(character);
        }
    }
}