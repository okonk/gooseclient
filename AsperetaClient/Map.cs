using System;
using System.Collections.Generic;
using System.Linq;

namespace AsperetaClient
{
    internal class Tile
    {
        public bool Blocked { get; set; }
        public Texture[] Layers { get; set; }

        public Character Character { get; set; }

        public bool IsBlocked()
        {
            return Blocked || Character != null;
        }

        public Tile(bool blocked, Texture[] layers)
        {
            this.Blocked = blocked;
            this.Layers = layers;
        }
    }

    internal class Map
    {
        const int NUM_LAYERS = 4;

        // Draw this many tiles additionally to stop trees popping in/out
        const int OVERDRAW = 5 * Constants.TileSize;

        public MapFile MapFile { get; private set; }
        public int MapNumber { get { return MapFile.MapNumber; } }
        public int Width { get { return MapFile.Width; } }
        public int Height { get { return MapFile.Height; } }
        public Tile[] Tiles { get; set; }

        public List<Character> Characters { get; set; } = new List<Character>();

        public bool Loaded { get; private set; } = false;

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

        public IEnumerable<int> Load()
        {
            for (int y = 0; y < this.Height; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    var tileData = MapFile[x, y];
                    var tile = new Tile(tileData.Blocked, tileData.Layers.Select(l => l > 0 ? GameClient.ResourceManager.GetTexture(l) : null).ToArray());

                    this[x, y] = tile;
                }

                yield return (int)((y / (double)this.Height) * 100);
            }

            Loaded = true;
        }

        public void Update(double dt)
        {
            foreach (var character in Characters)
            {
                character.Update(dt);
            }
        }

        public void Render(int start_x, int start_y)
        {
            for (int l = 0; l < NUM_LAYERS; l++)
            {
                for (int y = (start_y - OVERDRAW) / Constants.TileSize; y < this.Height; y++)
                {
                    if (y < 0) y = 0; // always start at the first tile
                    // break when this tile is off the bottom of the screen
                    if (y * Constants.TileSize - start_y - OVERDRAW > GameClient.ScreenHeight) break;

                    for (int x = (start_x - OVERDRAW) / Constants.TileSize; x < this.Width; x++)
                    {
                        if (x < 0) x = 0; // always start at the first tile
                        // break when this tile is off the right of the screen
                        if (x * Constants.TileSize - start_x - OVERDRAW > GameClient.ScreenWidth) break;

                        var tile = this[x, y];
                        var graphic = tile.Layers[l];

                        if (l == 2)
                        {
                            if (tile.Character != null)
                                tile.Character.Render(0, start_x, start_y);
                        }

                        if (graphic != null)
                            graphic.Render(x * Constants.TileSize - start_x, y * Constants.TileSize - start_y);
                    }
                }
            }

            foreach (var character in Characters)
            {
                character.RenderName(start_x, start_y);
                character.RenderHPMPBars(start_x, start_y);
            }
        }

        public bool CanMoveTo(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return false;

            return !this[x, y].IsBlocked();
        }

        public void MoveCharacter(Character character, int destX, int destY)
        {
            this[character.TileX, character.TileY].Character = null;
            this[destX, destY].Character = character;

            character.MoveTo(destX, destY);
        }

        public void AddCharacter(Character character)
        {
            this[character.TileX, character.TileY].Character = character;
            this.Characters.Add(character);
        }
    }
}