using UnityEngine;
using UnityEngine.Events;

public static class EventController
{
    public static event UnityAction OnLevelComplete;
    public static void RaiseLevelComplete() => OnLevelComplete?.Invoke();

    public static event UnityAction OnGameComplete;
    public static void RaiseGameComplete() => OnGameComplete?.Invoke();
}
