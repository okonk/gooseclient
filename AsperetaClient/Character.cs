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

        public bool Moving { get; set; }

        public bool Attacking { get; set; }

        public Character(int tileX, int tileY, int bodyId, int bodyState, Direction facing, ResourceManager resourceManager)
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

            this.UpdateAnimations(resourceManager);
        }

        public void UpdateAnimations(ResourceManager resourceManager)
        {
            var compiledAnimations = resourceManager.AdfManager.CompiledEnc.CompiledAnimations.FirstOrDefault(a => a.Id == this.BodyId && a.Type == AnimationType.Body);
            this.BodyAnimation = resourceManager.GetAnimation(compiledAnimations.AnimationIndexes[(this.BodyState - 1) + ((int)this.Facing) * 4]);
            this.BodyAnimation.SetAnimating(Moving | Attacking);
        }

        public void Update(double dt)
        {
            this.BodyAnimation.Update(dt);
        }

        public void Render(double dt, IntPtr renderer, int x_offset, int y_offset)
        {
            this.BodyAnimation.Render(dt, renderer, this.PixelXi - x_offset, this.PixelYi - y_offset);
        }

        public int GetWidth()
        {
            return this.BodyAnimation.GetWidth();
        }
    }
}