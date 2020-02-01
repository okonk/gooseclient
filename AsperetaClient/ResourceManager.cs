using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using SDL2;
using System.Linq;

namespace AsperetaClient
{
    class ResourceManager
    {
        public AdfManager AdfManager { get; set; }

        public Dictionary<int, IntPtr> AdfFileToSDLTexture { get; set; }

        public Dictionary<int, Texture> FrameIdToTexture { get; set; }

        public IntPtr Renderer { get; set; }

        public ResourceManager(string dataPath, IntPtr renderer)
        {
            this.AdfManager = new AdfManager(dataPath);
            this.AdfFileToSDLTexture = new Dictionary<int, IntPtr>();
            this.FrameIdToTexture = new Dictionary<int, Texture>();
            this.Renderer = renderer;
        }

        public Texture GetTexture(int frameId)
        {
            var adfFile = this.AdfManager.FrameToFile[frameId];

            Texture frameTexture;
            if (this.FrameIdToTexture.TryGetValue(frameId, out frameTexture))
                return frameTexture;

            IntPtr texture;
            if (!this.AdfFileToSDLTexture.TryGetValue(adfFile.FileNumber, out texture))
            {
                var handle = GCHandle.Alloc(adfFile.FileData, GCHandleType.Pinned);
                var ptr = handle.AddrOfPinnedObject();

                var sdlRWops = SDL.SDL_RWFromConstMem(ptr, adfFile.FileData.Length);
                handle.Free();

                var surface = SDL_image.IMG_Load_RW(sdlRWops, 1);
                SDL.SDL_SetColorKey(surface, 1, 0);
                texture = SDL.SDL_CreateTextureFromSurface(this.Renderer, surface);

                SDL.SDL_FreeSurface(surface);
            }

            var frame = adfFile.Frames[frameId];
            frameTexture = new Texture(texture, frame.X, frame.Y, frame.W, frame.H);

            this.FrameIdToTexture[frameId] = frameTexture;
            return frameTexture;
        }

        public Animation GetAnimation(int animationId)
        {
            var animationData = this.AdfManager.Animations[animationId];
            var animation = new Animation();
            animation.Frames = animationData.Frames.Select(f => GetTexture(f)).ToArray();
            animation.Interval = 1.0d / animationData.Interval;

            return animation;
        }
    }
}
