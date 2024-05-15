using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsperetaClient.Scripting.GameState;

namespace AsperetaClient;

internal class ScriptManager
{
    private Dictionary<string, IScript> scriptMapping = new();

    private IReadOnlyCollection<IClientScript> scripts;

    public GameState GameState { get; private set; }

    private GameScreen gameScreen;

    public ScriptManager()
    {
        LoadScripts();
    }

    public void LoadScripts()
    {
        foreach (var file in Directory.EnumerateFiles("Scripts", "*.csx", new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive }))
        {
            var script = new Script<IClientScript>(file);
            scriptMapping[file] = script;
        }

        scripts = scriptMapping.Values.Cast<Script<IClientScript>>().Select(s => s.Object).ToList();
    }

    public void OnGameScreenCreated(GameScreen screen)
    {
        this.gameScreen = screen;
        this.GameState = new();

        foreach (var script in scripts)
        {
            script.GameState = this.GameState;
            script.OnGameScreenCreated(screen);
        }
    }

    public void OnMapLoaded(Map map)
    {
        foreach (var script in scripts)
        {
            script.OnMapLoaded(map);
        }
    }

    public void ReloadScripts()
    {
        foreach (var script in scripts)
        {
            script.Stop();
        }

        LoadScripts();

        foreach (var script in scripts)
        {
            script.GameState = this.GameState;
            script.OnGameScreenCreated(gameScreen);
        }

        foreach (var script in scripts)
        {
            script.OnReloaded();
        }

        Console.WriteLine("Scripts reloaded");
    }
}
