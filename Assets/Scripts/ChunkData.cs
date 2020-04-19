using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkData
{
    // chunk size
    public static readonly int[] SIZE = new int[3] { 16, 16, 16 };

    // just a rename for each individual axis
    public static readonly int SIZE_X = SIZE[0];
    public static readonly int SIZE_Y = SIZE[1];
    public static readonly int SIZE_Z = SIZE[2];
}
