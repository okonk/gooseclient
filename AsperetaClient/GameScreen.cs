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

            LoadMap();
        }

        public void LoadMap()
        {
            GameClient.StateManager.AppendState(new MapLoadingScreen(mapNumber, mapName, this));
        }
        
        public override void Render(double dt)
        {
            int half_x = GameClient.ScreenWidth / 2 - Constants.TileSize;
            int half_y = GameClient.ScreenHeight / 2 - Constants.TileSize;
            int start_x = player != null ? player.PixelXi - half_x - (player.GetWidth() / 2) : 0;
            int start_y = player != null ? player.PixelYi - half_y : 0;

            Map.Render(start_x, start_y);
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

            Map.Update(dt);
        }

        public override void HandleEvent(SDL.SDL_Event ev)
        {
            
        }

        public void MoveKeyPressed(Direction direction)
        {
            if (player == null || player.Moving) return;

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
    }
}