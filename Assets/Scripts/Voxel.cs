using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel {
    public bool update = true;
    public bool visible = true;
    public bool transparent = false;
    public bool[] renderSides = new bool[6];
}