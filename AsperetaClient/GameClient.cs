using SDL2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace AsperetaClient
{
    class GameClient : IDisposable
    {
        public bool Running { get; set; }

        public IntPtr Window { get; set; }

        public IntPtr Renderer { get; set; }

        public GameClient()
        {
            this.Running = true;
        }

        public void Run()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine("Unable to initialize SDL. Error: {0}", SDL.SDL_GetError());
            }
            else
            {
                this.Window = IntPtr.Zero;
                this.Window = SDL.SDL_CreateWindow("Goose Client",
                    SDL.SDL_WINDOWPOS_CENTERED,
                    SDL.SDL_WINDOWPOS_CENTERED,
                    0,
                    0,
                    SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE
                );

                if (this.Window == IntPtr.Zero)
                {
                    Console.WriteLine("Unable to create a window. SDL. Error: {0}", SDL.SDL_GetError());  
                }
                else
                {
                    this.Renderer = SDL.SDL_CreateRenderer(this.Window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

                    var game = new GameScreen(this);
                    long lastUpdate = Stopwatch.GetTimestamp();

                    SDL.SDL_Event ev;

                    while (this.Running)
                    {
                        long timeNow = Stopwatch.GetTimestamp();
                        double timeDiff = (timeNow - lastUpdate) / (double)Stopwatch.Frequency;
                        lastUpdate = timeNow;

                        game.Update(timeDiff);

                        while (SDL.SDL_PollEvent(out ev) != 0)
                        {
                            switch (ev.type)
                            {
                                case SDL.SDL_EventType.SDL_QUIT:
                                    this.Running = false;
                                    break;
                            }

                            game.HandleEvent(ev);
                        }

                        //SDL.SDL_SetRenderDrawColor(Renderer, 255, 255, 255, 255);
                        SDL.SDL_RenderClear(this.Renderer);

                        game.Render(timeDiff);

                        SDL.SDL_RenderPresent(this.Renderer);
                        SDL.SDL_Delay(1);
                    }
                }

                SDL.SDL_DestroyRenderer(this.Renderer);
                SDL.SDL_DestroyWindow(this.Window);
                SDL.SDL_Quit();
            }
        }

        public void Dispose()
        {

        }
    }
}
