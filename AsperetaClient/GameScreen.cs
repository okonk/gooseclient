using System;
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
            int start_x = player.PixelXi - half_x - (player.GetWidth() / 2);
            int start_y = player.PixelYi - half_y;

            Map.Render(start_x, start_y);

            player.Render(dt, start_x, start_y);
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

            player.Update(dt);
        }

        public override void HandleEvent(SDL.SDL_Event ev)
        {
            
        }

        public void MoveKeyPressed(Direction direction)
        {
            if (player.Moving) return;

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

            player = new Character(p.MapX, p.MapY, p.BodyId, p.BodyState, (Direction)p.Facing);
        }
    }
}