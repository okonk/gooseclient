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

        public Dictionary<string, Texture> FileToTexture { get; set; }

        public IntPtr Renderer { get; set; }

        public ResourceManager(string dataPath, IntPtr renderer)
        {
            this.AdfManager = new AdfManager(dataPath);
            this.AdfFileToSDLTexture = new Dictionary<int, IntPtr>();
            this.FrameIdToTexture = new Dictionary<int, Texture>();
            this.FileToTexture = new Dictionary<string, Texture>();
            this.Renderer = renderer;
        }

        public IntPtr GetSDLTexture(AdfFile adfFile)
        {
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

                this.AdfFileToSDLTexture[adfFile.FileNumber] = texture;
            }

            return texture;
        }

        public Texture GetTexture(int frameId, bool usedInMap = false, bool usedInSpell = false)
        {
            var adfFile = this.AdfManager.FrameToFile[frameId];

            Texture frameTexture;
            if (this.FrameIdToTexture.TryGetValue(frameId, out frameTexture))
                return frameTexture;

            var texture = GetSDLTexture(adfFile);

            var frame = adfFile.Frames[frameId];
            frameTexture = new Texture(texture, frame.X, frame.Y, frame.W, frame.H, mapOffset: usedInMap, spellOffset: usedInSpell);

            this.FrameIdToTexture[frameId] = frameTexture;
            return frameTexture;
        }

        public Animation GetAnimation(int animationId, bool spellAnimation = false)
        {
            if (!this.AdfManager.Animations.TryGetValue(animationId, out AnimationData animationData))
                return null;

            var animation = new Animation();
            animation.Frames = animationData.Frames.Select(f => GetTexture(f, usedInSpell: spellAnimation)).ToArray();
            animation.Interval = 1.0d / animationData.Interval / 2;

            return animation;
        }

        public Texture GetTexture(string filepath)
        {
            Texture texture;
            if (this.FileToTexture.TryGetValue(filepath, out texture))
                return texture;

            var fileBytes = File.ReadAllBytes(filepath);
            var handle = GCHandle.Alloc(fileBytes, GCHandleType.Pinned);
            var ptr = handle.AddrOfPinnedObject();

            var sdlRWops = SDL.SDL_RWFromConstMem(ptr, fileBytes.Length);
            handle.Free();

            var surfacePointer = SDL_image.IMG_Load_RW(sdlRWops, 1);
            SDL.SDL_SetColorKey(surfacePointer, 1, 0);
            var sdlTexture = SDL.SDL_CreateTextureFromSurface(this.Renderer, surfacePointer);

            var surfaceStruct = Marshal.PtrToStructure<SDL.SDL_Surface>(surfacePointer);

            texture = new Texture(sdlTexture, 0, 0, surfaceStruct.w, surfaceStruct.h, needsOffset: false);

            SDL.SDL_FreeSurface(surfacePointer);

            this.FileToTexture[filepath] = texture;
            return texture;
        }
    }
}
