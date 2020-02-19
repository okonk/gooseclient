using System;
using System.Linq;
using SDL2;

namespace AsperetaClient
{
    enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }

    class Character
    {
        public int LoginId { get; set; }

        public string Title { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        public int TileX { get; set; }
        public int TileY { get; set; }

        public double PixelX { get; set; }
        public double PixelY { get; set; }

        public int PixelXi { get { return (int)PixelX; } }
        public int PixelYi { get { return (int)PixelY; } }

        public int[][] DisplayedEquipment { get; set; }

        public int BodyState { get; set; }

        public int BodyId { get; set; }

        private Direction facing;
        public Direction Facing
        { 
            get { return facing; }
            set
            {
                if (facing != value)
                {
                    facing = value;
                    UpdateAnimations();
                }
            }
        }

        public Animation[] EquippedAnimations { get; set; }

        public Animation SpellAnimation { get; set; }

        public int MoveSpeed { get; set; } = 400; // This is an illutia move speed value. I think this is milliseconds per tile? Default illutia is 320.
        
        public int FaceId { get; set; }

        public int HairId { get; set; }
        public Colour HairColour { get; set; }

        public int HPPercentage { get; set; }
        public int MPPercentage { get; set; }
        public bool ShouldRenderHPMPBars { get; set; }
        public double RenderHPMPBarsTime { get; set; }



        public bool Moving { get; set; }

        public bool Attacking { get; set; }


        public double MoveSpeedX { get; set; }

        public double MoveSpeedY { get; set; }

        public Character(MakeCharacterPacket p)
        {
            this.Moving = false;
            this.Attacking = false;

            this.LoginId = p.LoginId;
            this.Title = p.Title;
            this.Name = p.Name;
            this.Surname = p.Surname;
            this.TileX = p.MapX;
            this.TileY = p.MapY;
            this.PixelX = this.TileX * Constants.TileSize;
            this.PixelY = this.TileY * Constants.TileSize;
            this.BodyId = p.BodyId;
            this.BodyState = p.BodyState;
            this.facing = (Direction)p.Facing;
            this.FaceId = p.FaceId;
            this.HairId = p.HairId;
            this.HairColour = new Colour(p.HairR, p.HairG, p.HairB, p.HairA);
            this.HPPercentage = p.HPPercent;
            this.MPPercentage = 0;

            this.ShouldRenderHPMPBars = true;
            this.RenderHPMPBarsTime = 0;

            this.DisplayedEquipment = p.DisplayedEquipment;
            this.EquippedAnimations = new Animation[9];

            this.UpdateAnimations();
        }

        public enum DrawAnimations
        {
            Body,
            Face,
            Hair,
            Feet,
            Legs,
            Chest,
            Head,
            Shield,
            Weapon
        }

        public void UpdateAnimation(int id, DrawAnimations slot, AnimationType type, Colour colour = null)
        {
            if (id == 0)
            {
                this.EquippedAnimations[(int)slot] = null;
                return;
            }

            var compiledAnimation = GameClient.ResourceManager.AdfManager.CompiledEnc.CompiledAnimations.FirstOrDefault(a => a.Id == id && a.Type == type);
            if (compiledAnimation == null)
            {
                Console.WriteLine($"Couldn't find animation id {id} of type {type} for slot {slot}");
                this.EquippedAnimations[(int)slot] = null;
                return;
            }

            var animation = GameClient.ResourceManager.GetAnimation(compiledAnimation.AnimationIndexes[(this.BodyState - 1) + ((int)this.Facing) * 4]);
            //animation.Interval *= (MoveSpeed / 1000d); // Needed to display the full animation when moving 1 tile
            animation.SetAnimating(Moving | Attacking);
            animation.Colour = colour;

            this.EquippedAnimations[(int)slot] = animation;
        }

        public void UpdateAnimations()
        {
            UpdateAnimation(this.BodyId, DrawAnimations.Body, AnimationType.Body);
            UpdateAnimation(this.FaceId, DrawAnimations.Face, AnimationType.Hair);
            UpdateAnimation(this.HairId, DrawAnimations.Hair, AnimationType.Hair, HairColour);
            UpdateAnimation(this.DisplayedEquipment[0][0], DrawAnimations.Chest, AnimationType.Chest, EquipmentColour(0));
            UpdateAnimation(this.DisplayedEquipment[1][0], DrawAnimations.Head, AnimationType.Helm, EquipmentColour(1));
            UpdateAnimation(this.DisplayedEquipment[2][0], DrawAnimations.Legs, AnimationType.Legs, EquipmentColour(2));
            UpdateAnimation(this.DisplayedEquipment[3][0], DrawAnimations.Feet, AnimationType.Feet, EquipmentColour(3));
            UpdateAnimation(this.DisplayedEquipment[4][0], DrawAnimations.Shield, AnimationType.Hand, EquipmentColour(4));
            UpdateAnimation(this.DisplayedEquipment[5][0], DrawAnimations.Weapon, AnimationType.Hand, EquipmentColour(5));
        }

        public Colour EquipmentColour(int i)
        {
            var equip = this.DisplayedEquipment[i];
            if (equip[0] == 0 || equip[4] == 0)
                return null;

            return new Colour(equip[1], equip[2], equip[3], equip[4]);
        }

        public void Update(double dt)
        {
            if (Moving)
            {
                this.PixelX += (dt * MoveSpeedX);
                this.PixelY += (dt * MoveSpeedY);

                if ((MoveSpeedX < 0 && PixelX <= TileX * Constants.TileSize)
                    || (MoveSpeedX > 0 && PixelX >= TileX * Constants.TileSize)
                    || (MoveSpeedY < 0 && PixelY <= TileY * Constants.TileSize)
                    || (MoveSpeedY > 0 && PixelY >= TileY * Constants.TileSize)) {

                    Moving = false;
                    PixelX = TileX * Constants.TileSize;
                    PixelY = TileY * Constants.TileSize;
                    MoveSpeedX = 0;
                    MoveSpeedY = 0;

                    this.UpdateAnimations();
                }
            }

            foreach (var animation in this.EquippedAnimations)
            {
                animation?.Update(dt);
            }

            if (SpellAnimation != null)
            {
                SpellAnimation.Update(dt);
                if (SpellAnimation.Finished)
                    SpellAnimation = null;
            }

            if (ShouldRenderHPMPBars)
            {
                RenderHPMPBarsTime += dt;

                if (RenderHPMPBarsTime >= 2)
                    ShouldRenderHPMPBars = false;
            }
        }

        public void Render(int x_offset, int y_offset)
        {
            switch (Facing)
            {
                case Direction.Right:
                case Direction.Up:
                    EquippedAnimations[(int)DrawAnimations.Shield]?.Render(this.PixelXi - x_offset, this.PixelYi - y_offset);
                    EquippedAnimations[(int)DrawAnimations.Weapon]?.Render(this.PixelXi - x_offset, this.PixelYi - y_offset);
                    for (int i = 0; i < EquippedAnimations.Length - 2; i++)
                    {
                        EquippedAnimations[i]?.Render(this.PixelXi - x_offset, this.PixelYi - y_offset);
                    }
                    break;
                case Direction.Down:
                case Direction.Left:
                    foreach (var animation in this.EquippedAnimations)
                    {
                        animation?.Render(this.PixelXi - x_offset, this.PixelYi - y_offset);
                    }
                    break;
            }

            SpellAnimation?.Render(this.PixelXi - x_offset, this.PixelYi - y_offset);
        }

        public void RenderName(int x_offset, int y_offset)
        {
            string name = (string.IsNullOrWhiteSpace(Title) ? "" : Title + " ") + Name + (string.IsNullOrWhiteSpace(Surname) ? "" : " " + Surname);
            int x = (this.PixelXi - x_offset) + Constants.TileSize / 2 - (name.Length * GameClient.FontRenderer.CharWidth) / 2;
            int y = this.PixelYi - y_offset + this.GetYOffset() - GameClient.FontRenderer.CharHeight - 7;

            GameClient.FontRenderer.RenderText(this.Name, x + 1, y + 1, Colour.Black);
            GameClient.FontRenderer.RenderText(this.Name, x, y, Colour.White);
        }

        public void RenderHPMPBars(int x_offset, int y_offset)
        {
            if (!ShouldRenderHPMPBars) return;

            const int BAR_LENGTH = 24;
            const int HP_BAR_HEIGHT = 3;
            const int MP_BAR_HEIGHT = 2;

            int x = (this.PixelXi - x_offset) + Constants.TileSize / 2 - BAR_LENGTH / 2;
            int y = this.PixelYi - y_offset + this.GetYOffset() - 8;

            SDL.SDL_Rect rect;
            rect.x = x;
            rect.y = y;
            rect.w = BAR_LENGTH;
            rect.h = HP_BAR_HEIGHT;

            // hp bar background
            SDL.SDL_SetRenderDrawColor(GameClient.Renderer, 1, 1, 1, 255);
            SDL.SDL_RenderFillRect(GameClient.Renderer, ref rect);

            // hp bar
            rect.w = (int)(BAR_LENGTH * (HPPercentage / 100d));
            SDL.SDL_SetRenderDrawColor(GameClient.Renderer, 0, 252, 0, 255);
            SDL.SDL_RenderFillRect(GameClient.Renderer, ref rect);

            // mp bar
            rect.y = rect.y + HP_BAR_HEIGHT;
            rect.w = (int)(BAR_LENGTH * (MPPercentage / 100d));
            rect.h = MP_BAR_HEIGHT;
            SDL.SDL_SetRenderDrawColor(GameClient.Renderer, 0, 0, 248, 255);
            SDL.SDL_RenderFillRect(GameClient.Renderer, ref rect);
        }

        public int GetWidth()
        {
            return this.EquippedAnimations[0]?.GetWidth() ?? 32;
        }

        public int GetHeight()
        {
            return this.EquippedAnimations[0]?.GetHeight() ?? 32;
        }

        public int GetXOffset()
        {
            return this.EquippedAnimations[0]?.GetXOffset() ?? 32;
        }

        public int GetYOffset()
        {
            return this.EquippedAnimations[0]?.GetYOffset() ?? 32;
        }

        public void MoveTo(int destX, int destY)
        {
            Moving = true;
            MoveSpeedX = 0;
            MoveSpeedY = 0;

            if (destY < TileY)
            {
                Facing = Direction.Up;
                MoveSpeedY = (-1000d * Constants.TileSize) / MoveSpeed;
            }
            else if (destX > TileX)
            {
                Facing = Direction.Right;
                MoveSpeedX = (1000d * Constants.TileSize) / MoveSpeed;
            }
            else if (destY > TileY)
            {
                Facing = Direction.Down;
                MoveSpeedY = (1000d * Constants.TileSize) / MoveSpeed;
            }
            else if (destX < TileX)
            {
                Facing = Direction.Left;
                MoveSpeedX = (-1000d * Constants.TileSize) / MoveSpeed;
            }

            TileX = destX;
            TileY = destY;

            this.UpdateAnimations();
        }

        public void SetPosition(int tileX, int tileY)
        {
            TileX = tileX;
            TileY = tileY;
            Moving = false;
            PixelX = TileX * Constants.TileSize;
            PixelY = TileY * Constants.TileSize;
            MoveSpeedX = 0;
            MoveSpeedY = 0;
        }

        public void UpdateCharacter(UpdateCharacterPacket p)
        {
            this.BodyId = p.BodyId;
            this.BodyState = p.BodyState;
            this.FaceId = p.FaceId;
            this.HairId = p.HairId;
            this.HairColour = new Colour(p.HairR, p.HairG, p.HairB, p.HairA);

            this.DisplayedEquipment = p.DisplayedEquipment;

            this.UpdateAnimations();
        }
    }
}