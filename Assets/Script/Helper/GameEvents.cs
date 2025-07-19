using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    public static Action GameStart; 

    public static Action GameOver;
    public static Action RestartGameAction;
}

public enum GameState
{
    Starting,
    Playing,
    Paused,
    GameOver
}


