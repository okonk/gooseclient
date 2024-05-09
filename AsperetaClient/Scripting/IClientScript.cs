using System;

namespace AsperetaClient;


public interface IClientScript
{
    void OnLoaded() { }

    void OnGameScreenCreated(GameScreen screen) { }



}
