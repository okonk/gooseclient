using System;
using System.Runtime.InteropServices;
using SDL2;

namespace AsperetaClient
{
    class GameScreen
    {
        private GameClient GameClient { get; set; }

        private ResourceManager ResourceManager { get; set; }

        private Map map;

        private Character player;

        public GameScreen(GameClient client)
        {
            this.GameClient = client;
            this.ResourceManager = new ResourceManager("AsperetaClient/bin/Debug/netcoreapp3.1/data", client.Renderer);

            map = new Map(AsperetaMapLoader.Load(1));
            map.Load(this.ResourceManager);

            player = new Character(50, 50, 100, 1, Direction.Down, this.ResourceManager);
        }

        public void Render(double dt)
        {
            int half_x = Constants.ScreenWidth / 2 - Constants.TileSize;
            int half_y = Constants.ScreenHeight / 2 - Constants.TileSize;
            int start_x = player.PixelXi - half_x - (player.GetWidth() / 2);
            int start_y = player.PixelYi - half_y;

            map.Render(GameClient.Renderer, start_x, start_y);

            player.Render(dt, GameClient.Renderer, start_x, start_y);
        }

        public void Update(double dt)
        {
            player.Update(dt);
        }

        public void HandleEvent(SDL.SDL_Event ev)
        {

        }
    }
}