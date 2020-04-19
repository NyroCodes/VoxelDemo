using UnityEngine;

public static class VoxelData
{
    public static readonly int right = 0;
    public static readonly int up = 1;
    public static readonly int forward = 2;
    public static readonly int left = 3;
    public static readonly int down = 4;
    public static readonly int back = 5;

    public static readonly Vector3[] vertices = new Vector3[8]
    {
        new Vector3(0.0f,0.0f,0.0f),
        new Vector3(1.0f,0.0f,0.0f),
        new Vector3(1.0f,1.0f,0.0f),
        new Vector3(0.0f,1.0f,0.0f),
        new Vector3(0.0f,0.0f,1.0f),
        new Vector3(1.0f,0.0f,1.0f),
        new Vector3(1.0f,1.0f,1.0f),
        new Vector3(0.0f,1.0f,1.0f),
    };

    public static readonly int[,] sides = new int[6, 6]
    {
        { 1, 2, 5, 6, 5, 2 }, // right
        { 3, 7, 2, 6, 2, 7 }, // up
        { 5, 6, 4, 7, 4, 6 }, // forward
        { 4, 7, 0, 3, 0, 7 }, // left
        { 4, 0, 5, 1, 5, 0 }, // down
        { 0, 3, 1, 2, 1, 3 }, // back
    };

    public static readonly Vector3[] neighbours = new Vector3[6] {
        Vector3.right,
        Vector3.up,
        Vector3.forward,
        Vector3.left,
        Vector3.down,
        Vector3.back
    };
}

public enum VoxelSides
{
    right, up, forward, left, down, back
}
