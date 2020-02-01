using System;
using System.Linq;

namespace AsperetaClient
{
    internal class Tile
    {
        public bool Blocked { get; set; }
        public Texture[] Layers { get; set; }

        public Tile(Texture[] layers)
        {
            this.Layers = layers;
        }
    }

    internal class Map
    {
        const int NUM_LAYERS = 4;

        public MapFile MapFile { get; private set; }
        public int MapNumber { get { return MapFile.MapNumber; } }
        public int Width { get { return MapFile.Width; } }
        public int Height { get { return MapFile.Height; } }
        public Tile[] Tiles { get; set; }

        public Map(MapFile fileData)
        {
            this.MapFile = fileData;
            this.Tiles = new Tile[this.Width * this.Height];
        }

        public Tile this[int x, int y]
        {
            get { return this.Tiles[y * this.Width + x]; }
            set { this.Tiles[y * this.Width + x] = value; }
        }

        public void Load(ResourceManager resourceManager)
        {
            for (int y = 0; y < this.Height; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    var tileData = MapFile[x, y];
                    var tile = new Tile(tileData.Layers.Select(l => l > 0 ? resourceManager.GetTexture(l) : null).ToArray());

                    this[x, y] = tile;
                }
            }
        }

        public void Render(IntPtr renderer, int start_x, int start_y)
        {
            for (int l = 0; l < NUM_LAYERS; l++)
            {
                for (int y = start_y / Constants.TileSize; y < this.Height; y++)
                {
                    if (y < 0) y = 0; // always start at the first tile
                    // break when this tile is off the bottom of the screen
                    if (y * Constants.TileSize - start_y > Constants.ScreenHeight) break;

                    for (int x = start_x / Constants.TileSize; x < this.Width; x++)
                    {
                        if (x < 0) x = 0; // always start at the first tile
                        // break when this tile is off the right of the screen
                        if (x * Constants.TileSize - start_x > Constants.ScreenWidth) break;

                        var tile = this[x, y];
                        var graphic = tile.Layers[l];

                        if (graphic != null)
                            graphic.Render(renderer, x * 32 - start_x, y * 32 - start_y);
                    }
                }
            }
        }
    }
}