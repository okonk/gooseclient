using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AsperetaClient;

internal class ScriptManager
{
    private Dictionary<string, IScript> scriptMapping = new();

    private IReadOnlyCollection<IClientScript> scripts;

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
        foreach (var script in scripts)
        {
            script.OnGameScreenCreated(screen);
        }
    }
}
