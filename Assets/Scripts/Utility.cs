using System;
using UnityEngine;

public static class Utility
{
    public static Vector3Int RoundToVector3Int(this Vector3 v)
    {
        int x = Mathf.RoundToInt(v.x);
        int y = Mathf.RoundToInt(v.y);
        int z = Mathf.RoundToInt(v.z);
        return new Vector3Int(x, y, z);
    }

    public static Vector3Int FloorToVector3Int(this Vector3 v)
    {
        int x = Mathf.FloorToInt(v.x);
        int y = Mathf.FloorToInt(v.y);
        int z = Mathf.FloorToInt(v.z);
        return new Vector3Int(x, y, z);
    }

    public static Vector3Int CeilToVector3Int(this Vector3 v)
    {
        int x = Mathf.CeilToInt(v.x);
        int y = Mathf.CeilToInt(v.y);
        int z = Mathf.CeilToInt(v.z);
        return new Vector3Int(x, y, z);
    }

    public static float Cross(this Vector2 v, Vector2 other)
    {
        return v.x*other.y-v.y*other.x;
    }
}
