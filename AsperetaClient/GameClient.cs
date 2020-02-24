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
        public static bool Running { get; set; }

        public static IntPtr Window { get; set; }

        public static IntPtr Renderer { get; set; }

        public static ResourceManager ResourceManager { get; set; }

        public static StateManager StateManager { get; set; } = new StateManager();

        public static int ScreenWidth { get; set; } = 800;

        public static int ScreenHeight { get; set; } = 600;

        public static IniFile GameSettings { get; set; }

        public static IniFile ServerInfoSettings { get; set; }

        public static IniFile WindowSettings { get; set; }

        public static IniFile ButtonSettings { get; set; }

        public static IniFile LoginScreenSettings { get; set; }

        public static IniFile UserSettings { get; set; }

        public static FontRenderer FontRenderer { get; set; }

        public static NetworkClient NetworkClient { get; set; } = new NetworkClient();

        public static string RealmName { get; set; }

        public static uint DRAG_DROP_EVENT_ID;

        public GameClient()
        {
            Running = true;
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
                GameSettings = new IniFile("Game.ini");
                int windowWidth = ScreenWidth;
                int windowHeight = ScreenHeight;
                SDL.SDL_WindowFlags flags = SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
                SDL.SDL_RendererFlags renderFlags = SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED;

                if (GameSettings.GetBool("INIT", "2xScale"))
                {
                    windowWidth = 640 * 2;
                    windowHeight = 480 * 2;
                }

                if (GameSettings.GetBool("INIT", "VSync"))
                {
                    renderFlags |= SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC;
                }

                if (GameSettings.GetBool("INIT", "Fullscreen"))
                {
                    flags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
                }

                if (GameSettings.GetBool("INIT", "FullscreenBorderless"))
                {
                    flags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
                }

                Window = IntPtr.Zero;
                Window = SDL.SDL_CreateWindow("Goose Client",
                    SDL.SDL_WINDOWPOS_CENTERED,
                    SDL.SDL_WINDOWPOS_CENTERED,
                    windowWidth,
                    windowHeight,
                    flags
                );

                if (Window == IntPtr.Zero)
                {
                    Console.WriteLine("Unable to create a window. SDL. Error: {0}", SDL.SDL_GetError());  
                }
                else
                {
                    Renderer = SDL.SDL_CreateRenderer(Window, -1, renderFlags);

                    SDL.SDL_SetRenderDrawBlendMode(Renderer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

                    ServerInfoSettings = new IniFile("serverinfo.ini");
                    WindowSettings = new IniFile($"skins/{GameSettings["INIT"]["Skin"]}/Window.ini");
                    ButtonSettings = new IniFile($"skins/{GameSettings["INIT"]["Skin"]}/Button.ini");

                    ResourceManager = new ResourceManager("data", Renderer);
                    FontRenderer = new FontRenderer();

                    StateManager.AppendState(new LoginScreen());

                    RegisterCustomEvents();

                    SDL.SDL_StartTextInput();

                    long lastUpdate = Stopwatch.GetTimestamp();

                    SDL.SDL_Event ev;

                    while (Running)
                    {
                        long timeNow = Stopwatch.GetTimestamp();
                        double timeDiff = (timeNow - lastUpdate) / (double)Stopwatch.Frequency;
                        lastUpdate = timeNow;

                        StateManager.Update(timeDiff);
                        NetworkClient.Update();

                        while (SDL.SDL_PollEvent(out ev) != 0)
                        {
                            StateManager.HandleEvent(ev);

                            switch (ev.type)
                            {
                                case SDL.SDL_EventType.SDL_QUIT:
                                    Running = false;
                                    break;
                            }
                        }

                        SDL.SDL_SetRenderDrawColor(GameClient.Renderer, 0, 0, 0, 0);
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

        public void RegisterCustomEvents()
        {
            DRAG_DROP_EVENT_ID = SDL.SDL_RegisterEvents(1);
        }

        public static void Quit()
        {
            var quitEvent = new SDL.SDL_Event();
            quitEvent.type = SDL.SDL_EventType.SDL_QUIT;
            
            SDL.SDL_PushEvent(ref quitEvent);
        }
    }
}
