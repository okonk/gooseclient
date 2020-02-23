using System;
using System.Collections.Generic;
using System.Linq;
using SDL2;

namespace AsperetaClient
{
    internal class MapObject
    {
        public Texture Graphic { get; set; }
        public Colour Colour { get; set; }
        public string Name { get; set; }
        public int StackSize { get; set; }
    }

    internal class Tile
    {
        public bool Blocked { get; set; }
        public Texture[] Layers { get; set; }
        public Character Character { get; set; }
        public SpellTileAnimation SpellAnimation { get; set; }
        public MapObject MapObject { get; set; }

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

    internal class SpellTileAnimation
    {
        public int TileX { get; set; }

        public int TileY { get; set; }

        public Animation Animation { get; set; }

        public SpellTileAnimation(int x, int y, Animation animation)
        {
            this.TileX = x;
            this.TileY = y;
            this.Animation = animation;
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

        public List<SpellTileAnimation> SpellAnimations { get; set; } = new List<SpellTileAnimation>();

        public bool Loaded { get; private set; } = false;

        public bool Targeting { get; private set; } = false;
        private Character spellCastTarget;
        private int spellCastSlotNumber;
        private Character player;

        public Map(MapFile fileData)
        {
            this.MapFile = fileData;
            this.Tiles = new Tile[this.Width * this.Height];

            GameClient.NetworkClient.PacketManager.Listen<MakeCharacterPacket>(OnMakeCharacter);
            GameClient.NetworkClient.PacketManager.Listen<MoveCharacterPacket>(OnMoveCharacter);
            GameClient.NetworkClient.PacketManager.Listen<ChangeHeadingPacket>(OnChangeHeading);
            GameClient.NetworkClient.PacketManager.Listen<VitalsPercentagePacket>(OnVitalsPercentage);
            GameClient.NetworkClient.PacketManager.Listen<EraseCharacterPacket>(OnEraseCharacter);
            GameClient.NetworkClient.PacketManager.Listen<UpdateCharacterPacket>(OnUpdateCharacter);
            GameClient.NetworkClient.PacketManager.Listen<SetYourCharacterPacket>(OnSetYourCharacter);
            GameClient.NetworkClient.PacketManager.Listen<SpellCharacterPacket>(OnSpellCharacter);
            GameClient.NetworkClient.PacketManager.Listen<SpellTilePacket>(OnSpellTile);
            GameClient.NetworkClient.PacketManager.Listen<MapObjectPacket>(OnMapObject);
            GameClient.NetworkClient.PacketManager.Listen<EraseObjectPacket>(OnEraseObject);
            GameClient.NetworkClient.PacketManager.Listen<BattleTextPacket>(OnBattleText);
            GameClient.NetworkClient.PacketManager.Listen<AttackPacket>(OnAttack);
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
                    var tile = new Tile(tileData.Blocked, tileData.Layers.Select(l => l > 0 ? GameClient.ResourceManager.GetTexture(l, usedInMap: true) : null).ToArray());

                    this[x, y] = tile;
                }

                if (y % 10 == 0)
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

            for (int i = 0; i < SpellAnimations.Count; i++)
            {
                var tile = SpellAnimations[i];
                tile.Animation.Update(dt);
                if (tile.Animation.Finished)
                {
                    if (this[tile.TileX, tile.TileY].SpellAnimation == tile)
                    {
                        this[tile.TileX, tile.TileY].SpellAnimation = null;
                    }

                    SpellAnimations.RemoveAt(i);
                    i--;
                }
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
                            if (tile.MapObject != null)
                                tile.MapObject.Graphic.Render(x * Constants.TileSize - start_x, y * Constants.TileSize - start_y, tile.MapObject.Colour);

                            if (tile.Character != null)
                                tile.Character.Render(start_x, start_y);

                            if (tile.SpellAnimation != null)
                                tile.SpellAnimation.Animation.Render(x * Constants.TileSize - start_x, y * Constants.TileSize - start_y);
                        }

                        if (graphic != null)
                            graphic.Render(x * Constants.TileSize - start_x, y * Constants.TileSize - start_y);
                    }
                }
            }

            foreach (var character in Characters)
            {
                if (Targeting && character == spellCastTarget)
                {
                    RenderSpellTargetBox(start_x, start_y);
                }

                character.RenderName(start_x, start_y);
                character.RenderHPMPBars(start_x, start_y);
                character.RenderBattleText(start_x, start_y);
            }
        }

        public void RenderSpellTargetBox(int start_x, int start_y)
        {
            var rect = new SDL.SDL_Rect();
            rect.x = spellCastTarget.PixelXi + spellCastTarget.GetXOffset() - start_x;
            rect.y = spellCastTarget.PixelYi + spellCastTarget.GetYOffset() - start_y;
            rect.w = spellCastTarget.GetWidth();
            rect.h = spellCastTarget.GetHeight();

            SDL.SDL_SetRenderDrawColor(GameClient.Renderer, 255, 255, 255, 255);
            SDL.SDL_RenderDrawRect(GameClient.Renderer, ref rect);

            rect.x += 1;
            rect.y += 1;
            rect.w -= 2;
            rect.h -= 2;

            SDL.SDL_SetRenderDrawColor(GameClient.Renderer, 0, 100, 248, 255);
            SDL.SDL_RenderDrawRect(GameClient.Renderer, ref rect);
        }

        public bool HandleEvent(SDL.SDL_Event ev)
        {
            switch (ev.type)
            {
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_COMMA)
                    {
                        GameClient.NetworkClient.Get();
                        return true;
                    }

                    if (!Targeting) return false;

                    if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_UP)
                    {
                        SetNextSpellCastTarget(searchDown: false);
                        return true;
                    }
                    else if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_RIGHT)
                    {
                        SetNextSpellCastTarget(searchDown: true);
                        return true;
                    }
                    else if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_DOWN)
                    {
                        SetNextSpellCastTarget(searchDown: true);
                        return true;
                    }
                    else if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_LEFT)
                    {
                        SetNextSpellCastTarget(searchDown: false);
                        return true;
                    }
                    else if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_HOME)
                    {
                        spellCastTarget = player;
                        return true;
                    }
                    else if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_RETURN || ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_KP_ENTER)
                    {
                        Targeting = false;
                        GameClient.NetworkClient.Cast(spellCastSlotNumber, spellCastTarget.LoginId);
                        return true;
                    }
                    else if (ev.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE)
                    {
                        Targeting = false;
                        return true;
                    }

                    break;
            }

            return false;
        }

        // Esssentially what the Asp client does is searches up or down going row by row with a wrap around to opposite end of screen
        private void SetNextSpellCastTarget(bool searchDown)
        {
            const int RangeX = 10;
            const int RangeY = 8;

            int currentPosition = spellCastTarget.TileY * this.Width + spellCastTarget.TileX;
            int lowestPosition = currentPosition;
            int highestPosition = currentPosition;
            int closestPosition = currentPosition;

            foreach (var character in Characters)
            {
                // Filter out things off screen and current target
                if (character == spellCastTarget || 
                    Math.Abs(character.TileX - player.TileX) > RangeX || 
                    Math.Abs(character.TileY - player.TileY) > RangeY)
                {
                    continue;
                }

                int characterPosition = character.TileY * this.Width + character.TileX;

                if (characterPosition < lowestPosition)
                    lowestPosition = characterPosition;
                else if (characterPosition > highestPosition)
                    highestPosition = characterPosition;

                if ((searchDown && characterPosition > currentPosition && (closestPosition == currentPosition || currentPosition - closestPosition < currentPosition - characterPosition))
                    || (!searchDown && characterPosition < currentPosition && (closestPosition == currentPosition || closestPosition - currentPosition < characterPosition - currentPosition)))
                {
                    closestPosition = characterPosition;
                }
            }

            int nextTarget = closestPosition;
            if (nextTarget == currentPosition)
            {
                nextTarget = searchDown ? lowestPosition : highestPosition;
            }

            if (nextTarget != currentPosition)
            {
                spellCastTarget = Tiles[nextTarget].Character ?? player;
            }
        }

        public bool ValidTile(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        public bool CanMoveTo(int x, int y)
        {
            if (!ValidTile(x, y))
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

        public void OnMakeCharacter(object packet)
        {
            var p = (MakeCharacterPacket)packet;

            var character = new Character(p);
            AddCharacter(character);
        }

        public void OnMoveCharacter(object packet)
        {
            var p = (MoveCharacterPacket)packet;

            var character = this.Characters.FirstOrDefault(c => c.LoginId == p.LoginId);
            if (character == null) return;

            MoveCharacter(character, p.MapX, p.MapY);
        }

        public void OnChangeHeading(object packet)
        {
            var p = (ChangeHeadingPacket)packet;
            var character = this.Characters.FirstOrDefault(c => c.LoginId == p.LoginId);
            if (character == null) return;

            character.Facing = (Direction)p.Facing;
        }

        public void OnVitalsPercentage(object packet)
        {
            var p = (VitalsPercentagePacket)packet;
            var character = this.Characters.FirstOrDefault(c => c.LoginId == p.LoginId);
            if (character == null) return;

            character.HPPercentage = p.HPPercentage;
            character.MPPercentage = p.MPPercentage;
            character.ShouldRenderHPMPBars = true;
            character.RenderHPMPBarsTime = 0;
        }

        public void OnEraseCharacter(object packet)
        {
            var p = (EraseCharacterPacket)packet;

            var character = this.Characters.FirstOrDefault(c => c.LoginId == p.LoginId);
            if (character == null) return;

            if (this[character.TileX, character.TileY].Character == character)
                this[character.TileX, character.TileY].Character = null;

            Characters.Remove(character);

            if (character == spellCastTarget)
                spellCastTarget = player;
        }

        public void OnUpdateCharacter(object packet)
        {
            var p = (UpdateCharacterPacket)packet;

            var character = this.Characters.FirstOrDefault(c => c.LoginId == p.LoginId);
            if (character == null) return;

            character.UpdateCharacter(p);
        }

        public void OnSetYourCharacter(object packet)
        {
            var p = (SetYourCharacterPacket)packet;

            this.player = this.Characters.FirstOrDefault(c => c.LoginId == p.LoginId);
        }

        public void OnCastSpell(SpellSlot slot)
        {
            if (Targeting) return;

            if (slot.Targetable)
            {
                Targeting = true;
                spellCastTarget = spellCastTarget ?? this.player;
                spellCastSlotNumber = slot.SlotNumber;
            }
            else
            {
                GameClient.NetworkClient.Cast(slot.SlotNumber, this.player.LoginId);
            }
        }

        public void OnSpellCharacter(object packet)
        {
            var p = (SpellCharacterPacket)packet;

            var character = this.Characters.FirstOrDefault(c => c.LoginId == p.LoginId);
            if (character == null) return;

            var animation = GameClient.ResourceManager.GetAnimation(p.AnimationId, spellAnimation: true);
            character.SpellAnimation = animation;
        }

        public void OnSpellTile(object packet)
        {
            var p = (SpellTilePacket)packet;

            if (!ValidTile(p.TileX, p.TileY))
                return;

            var animation = GameClient.ResourceManager.GetAnimation(p.AnimationId, spellAnimation: true);
            var spellTileAnimation = new SpellTileAnimation(p.TileX, p.TileY, animation);
            SpellAnimations.Add(spellTileAnimation);
            this[p.TileX, p.TileY].SpellAnimation = spellTileAnimation;
        }

        public void OnMapObject(object packet)
        {
            var p = (MapObjectPacket)packet;

            var mapObject = new MapObject
            {
                Graphic = GameClient.ResourceManager.GetTexture(p.GraphicId),
                Colour = new Colour(p.GraphicR, p.GraphicG, p.GraphicB, p.GraphicA),
                Name = p.ItemName,
                StackSize = p.StackSize
            };

            this[p.TileX, p.TileY].MapObject = mapObject;
        }

        public void OnEraseObject(object packet)
        {
            var p = (EraseObjectPacket)packet;

            this[p.TileX, p.TileY].MapObject = null;
        }

        public void OnBattleText(object packet)
        {
            var p = (BattleTextPacket)packet;

            var character = this.Characters.FirstOrDefault(c => c.LoginId == p.LoginId);
            if (character == null) return;

            character.AddBattleText(p.BattleTextType, p.Text);
        }

        public void OnAttack(object packet)
        {
            var p = (AttackPacket)packet;

            var character = this.Characters.FirstOrDefault(c => c.LoginId == p.LoginId);
            if (character == null) return;

            character.Attack();
        }

        public void OnRightClick(int offsetX, int offsetY, int clickX, int clickY)
        {
            foreach (var character in Characters)
            {
                int charX = character.PixelXi - offsetX + character.GetXOffset();
                int charY = character.PixelYi - offsetY + character.GetYOffset();

                if (clickX >= charX && clickX <= charX + character.GetWidth() && 
                    clickY >= charY && clickY <= charY + character.GetHeight())
                {
                    GameClient.NetworkClient.RightClick(character.TileX, character.TileY);
                    return;
                }
            }
        }
    }
}