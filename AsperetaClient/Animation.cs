using System;

namespace AsperetaClient
{
    class Animation
    {
        public bool Animating { get; set; }

        public bool Loop { get; set; }

        public bool Finished { get; set; }

        public Texture[] Frames { get; set; }

        public double Interval { get; set; }

        public int CurrentFrame { get; set; }

        public double CurrentTime { get; set; }

        public Animation()
        {
            this.Animating = true;
            this.Loop = false;
            this.Finished = false;
            this.Interval = 10;
            this.CurrentFrame = 0;
            this.CurrentTime = 0;
        }

        public void Update(double dt)
        {
            if (!this.Animating) return;

            this.CurrentTime += dt;

            if (this.CurrentTime > Interval)
            {
                this.CurrentTime -= Interval;
                this.CurrentFrame++;
            }

            if (this.CurrentFrame >= Frames.Length)
            {
                this.CurrentFrame = 0;
                this.Finished = !this.Loop;
            }
        }

        public void Render(double dt, int x, int y)
        {
            this.Frames[this.CurrentFrame].Render(x, y);
        }

        public void SetAnimating(bool animating)
        {
            this.Animating = animating;
            this.CurrentFrame = 0;
            this.CurrentTime = 0;
            this.Finished = false;
        }

        public int GetWidth()
        {
            return this.Frames[this.CurrentFrame].Width;
        }
    }
}
