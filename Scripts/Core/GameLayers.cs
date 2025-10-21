using UnityEngine;

/// <summary>
/// A static class to cache LayerMask information for performance.
/// </summary>
public static class GameLayers
{
    public static readonly int Enemies = LayerMask.NameToLayer("Enemies");
    public static readonly int EnemiesMask = 1 << Enemies;
}

