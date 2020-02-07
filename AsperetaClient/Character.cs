using System;
using System.Linq;

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

        public string Name { get; set; }

        public int TileX { get; set; }
        public int TileY { get; set; }

        public double PixelX { get; set; }
        public double PixelY { get; set; }

        public int PixelXi { get { return (int)PixelX; } }
        public int PixelYi { get { return (int)PixelY; } }

        public int[] DisplayedEquipmentIds { get; set; }

        public int BodyState { get; set; }

        public int BodyId { get; set; }

        public Direction Facing { get; set; }

        public Animation BodyAnimation { get; set; }

        public Animation FaceAnimation { get; set; }

        public Animation HairAnimation { get; set; }

        public int MoveSpeed { get; set; } = 400; // This is an illutia move speed value. I think this is milliseconds per tile? Default illutia is 320.
        
        public int FaceId { get; set; }

        public int HairId { get; set; }
        public int HairR { get; set; }
        public int HairG { get; set; }
        public int HairB { get; set; }
        public int HairA { get; set; }



        public bool Moving { get; set; }

        public bool Attacking { get; set; }


        public double MoveSpeedX { get; set; }

        public double MoveSpeedY { get; set; }

        public Character(MakeCharacterPacket p)
        {
            this.Moving = false;
            this.Attacking = false;

            this.LoginId = p.LoginId;
            this.Name = p.Name;
            this.TileX = p.MapX;
            this.TileY = p.MapY;
            this.PixelX = this.TileX * Constants.TileSize;
            this.PixelY = this.TileY * Constants.TileSize;
            this.BodyId = p.BodyId;
            this.BodyState = p.BodyState;
            this.Facing = (Direction)p.Facing;
            this.FaceId = p.FaceId;
            this.HairId = p.HairId;
            this.HairR = p.HairR;
            this.HairG = p.HairG;
            this.HairB = p.HairB;
            this.HairA = p.HairA;

            this.UpdateAnimations();
        }

        public void UpdateAnimations()
        {
            var compiledAnimations = GameClient.ResourceManager.AdfManager.CompiledEnc.CompiledAnimations.FirstOrDefault(a => a.Id == this.BodyId && a.Type == AnimationType.Body);
            this.BodyAnimation = GameClient.ResourceManager.GetAnimation(compiledAnimations.AnimationIndexes[(this.BodyState - 1) + ((int)this.Facing) * 4]);
            this.BodyAnimation.Interval *= (MoveSpeed / 1000d); // Needed to display the full animation when moving 1 tile
            this.BodyAnimation.SetAnimating(Moving | Attacking);

            compiledAnimations = GameClient.ResourceManager.AdfManager.CompiledEnc.CompiledAnimations.FirstOrDefault(a => a.Id == this.HairId && a.Type == AnimationType.Hair);
            this.HairAnimation = GameClient.ResourceManager.GetAnimation(compiledAnimations.AnimationIndexes[(this.BodyState - 1) + ((int)this.Facing) * 4]);
            this.HairAnimation.Interval *= (MoveSpeed / 1000d); // Needed to display the full animation when moving 1 tile
            this.HairAnimation.SetAnimating(Moving | Attacking);

            compiledAnimations = GameClient.ResourceManager.AdfManager.CompiledEnc.CompiledAnimations.FirstOrDefault(a => a.Id == this.FaceId && a.Type == AnimationType.Hair);
            this.FaceAnimation = GameClient.ResourceManager.GetAnimation(compiledAnimations.AnimationIndexes[(this.BodyState - 1) + ((int)this.Facing) * 4]);
            this.FaceAnimation.Interval *= (MoveSpeed / 1000d); // Needed to display the full animation when moving 1 tile
            this.FaceAnimation.SetAnimating(Moving | Attacking);
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

            this.BodyAnimation.Update(dt);
        }

        public void Render(double dt, int x_offset, int y_offset)
        {
            this.BodyAnimation.Render(dt, this.PixelXi - x_offset, this.PixelYi - y_offset);
            this.FaceAnimation.Render(dt, this.PixelXi - x_offset, this.PixelYi - y_offset);
            this.HairAnimation.Render(dt, this.PixelXi - x_offset, this.PixelYi - y_offset);
        }

        public void RenderName(int x_offset, int y_offset)
        {
            string name = Name;
            int x = (this.PixelXi - x_offset) + Constants.TileSize / 2 - (name.Length * GameClient.FontRenderer.CharWidth) / 2;
            int y = this.PixelYi - y_offset + this.BodyAnimation.GetYOffset() - GameClient.FontRenderer.CharHeight - 10;

            GameClient.FontRenderer.RenderText(this.Name, x + 1, y + 1, Colour.Black);
            GameClient.FontRenderer.RenderText(this.Name, x, y, Colour.White);
        }

        public int GetWidth()
        {
            return this.BodyAnimation.GetWidth();
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
    }
}