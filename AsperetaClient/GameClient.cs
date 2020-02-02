using SDL2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace AsperetaClient
{
    class GameClient : IDisposable
    {
        public bool Running { get; set; }

        public static IntPtr Window { get; set; }

        public static IntPtr Renderer { get; set; }

        public static ResourceManager ResourceManager { get; set; }

        public static StateManager StateManager { get; set; } = new StateManager();

        public static int ScreenWidth { get; set; } = 640;

        public static int ScreenHeight { get; set; } = 480;

        public static GameSettings GameSettings { get; set; } = new GameSettings();

        public GameClient()
        {
            this.Running = true;
        }

        public void Run()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine("Unable to initialize SDL. Error: {0}", SDL.SDL_GetError());
            }
            else
            {
                Window = IntPtr.Zero;
                Window = SDL.SDL_CreateWindow("Goose Client",
                    SDL.SDL_WINDOWPOS_CENTERED,
                    SDL.SDL_WINDOWPOS_CENTERED,
                    800,
                    600,
                    SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE
                );

                if (Window == IntPtr.Zero)
                {
                    Console.WriteLine("Unable to create a window. SDL. Error: {0}", SDL.SDL_GetError());  
                }
                else
                {
                    Renderer = SDL.SDL_CreateRenderer(Window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

                    ResourceManager = new ResourceManager("data", Renderer);

                    StateManager.AppendState(new LoginScreen());

                    long lastUpdate = Stopwatch.GetTimestamp();

                    SDL.SDL_Event ev;

                    while (this.Running)
                    {
                        long timeNow = Stopwatch.GetTimestamp();
                        double timeDiff = (timeNow - lastUpdate) / (double)Stopwatch.Frequency;
                        lastUpdate = timeNow;

                        StateManager.Update(timeDiff);

                        while (SDL.SDL_PollEvent(out ev) != 0)
                        {
                            switch (ev.type)
                            {
                                case SDL.SDL_EventType.SDL_QUIT:
                                    this.Running = false;
                                    break;
                            }

                            StateManager.HandleEvent(ev);
                        }

                        SDL.SDL_RenderClear(Renderer);

                        StateManager.Render(timeDiff);

                        SDL.SDL_RenderPresent(Renderer);
                        SDL.SDL_Delay(1);
                    }
                }

                SDL.SDL_DestroyRenderer(Renderer);
                SDL.SDL_DestroyWindow(Window);
                SDL.SDL_Quit();
            }
        }

        public void Dispose()
        {

        }
    }
}
