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

        public int MoveSpeed { get; set; } = 400; // This is an illutia move speed value. I think this is milliseconds per tile? Default illutia is 320.



        public bool Moving { get; set; }

        public bool Attacking { get; set; }


        public double MoveSpeedX { get; set; }

        public double MoveSpeedY { get; set; }

        public Character(int tileX, int tileY, int bodyId, int bodyState, Direction facing)
        {
            this.TileX = tileX;
            this.TileY = tileY;
            this.PixelX = tileX * Constants.TileSize;
            this.PixelY = tileY * Constants.TileSize;
            this.BodyId = bodyId;
            this.BodyState = bodyState;
            this.Facing = facing;

            this.Moving = false;
            this.Attacking = false;

            this.UpdateAnimations();
        }

        public void UpdateAnimations()
        {
            var compiledAnimations = GameClient.ResourceManager.AdfManager.CompiledEnc.CompiledAnimations.FirstOrDefault(a => a.Id == this.BodyId && a.Type == AnimationType.Body);
            this.BodyAnimation = GameClient.ResourceManager.GetAnimation(compiledAnimations.AnimationIndexes[(this.BodyState - 1) + ((int)this.Facing) * 4]);
            this.BodyAnimation.Interval *= (MoveSpeed / 1000d); // Needed to display the full animation when moving 1 tile
            this.BodyAnimation.SetAnimating(Moving | Attacking);
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