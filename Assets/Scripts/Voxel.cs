using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel {
    // block type like stone or sand
    public int type = 1;

    // block is visible and which side is visiable
    public bool visible = true;
    public bool[] visibleSides = new bool[6];

    // should be updated
    public bool update = true;

    // see through
    public bool transparent = false;
}