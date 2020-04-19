using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;



public class Chunk : MonoBehaviour
{
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    // render stuff
    private List<Quad> quads = new List<Quad>();

    private Vector3[] vertices;
    private Vector3[] normals;
    private int[] triangles;
    private Vector3[] uvs;

    // flags
    private bool updateMesh;

    private Voxel[,,] voxels = new Voxel[ChunkData.SIZE_X, ChunkData.SIZE_Y, ChunkData.SIZE_Z];

    // data storage
    private Dictionary<Vector3, Voxel> voxelsOld = new Dictionary<Vector3, Voxel>();

    void Start()
    {
        // generate data
        GenerateVoxels();
    }

    void FixedUpdate()
    {
        UpdateVoxels();

        if (updateMesh)
        {
            GenerateGreedyMesh();
            updateMesh = false;
        }
    }

    private void GenerateVoxels()
    {
        for (int x = 0; x < ChunkData.SIZE_X; x++)
            for (int z = 0; z < ChunkData.SIZE_Z; z++)
                for (int y = 0; y < ChunkData.SIZE_Y; y++)
                    if (Random.value > 0.7f)
                    {
                        voxels[x, y, z] = new Voxel();
                        voxelsOld.Add(new Vector3(x, y, z), voxels[x, y, z]);
                    }
                    else
                        voxels[x, y, z] = null;

        updateMesh = true;
    }

    private void UpdateVoxels()
    {
        foreach (KeyValuePair<Vector3, Voxel> voxel in voxelsOld)
        {
            if (voxel.Value.update)
            {
                bool visible = voxel.Value.visible;
                voxel.Value.visible = false;

                for (int s = 0; s < VoxelData.neighbours.Length; s++)
                {
                    if (!voxelsOld.ContainsKey(voxel.Key + VoxelData.neighbours[s]))
                    {
                        voxel.Value.visible = true;
                        voxel.Value.visibleSides[s] = true;
                    }
                }

                voxel.Value.update = false;

                if (visible != voxel.Value.visible)
                {
                    updateMesh = true;
                }
            }
        }
    }

    private int GetVoxelType(Voxel voxel, int side)
    {
        if (voxel != null && voxel.visibleSides[side])
            return voxel.type;
        return -1;
    }

    private void GenerateGreedyMesh()
    {

        Debug.Log("Neues Mesh");

        Profiler.BeginSample("Quading");

        quads.Clear();

        // loop through x, y, z - axis
        for (int d = 0; d < 3; d++)
        {
            // the other 2 axis
            int u = (d + 1) % 3;
            int i; // loop index for u
            int v = (d + 2) % 3;
            int j; // loop index for v

            // array to hold coordinates
            int[] x = new int[3] { 0, 0, 0 };
            // array to handle layer coordinates
            int[] y = new int[3] { 0, 0, 0 };
            // array to handle orientation 
            int[] o = new int[3] { 0, 0, 0 };

            int w; // quad width
            int h; // quad height

            // mask to find same type
            int[,] mask = new int[2, ChunkData.SIZE[u] * ChunkData.SIZE[v]];
            int n; // mask counter
            int layer; // mask layer

            // loop through current main axis
            for (x[d] = 0; x[d] < ChunkData.SIZE[d]; x[d]++) // noch schecken
            {

                // create masks from block types
                n = 0;
                for (x[v] = 0; x[v] < ChunkData.SIZE[v]; x[v]++)
                    for (x[u] = 0; x[u] < ChunkData.SIZE[u]; x[u]++)
                    {
                        Profiler.BeginSample("Voxeln");
                        mask[0, n] = GetVoxelType(voxels[x[0], x[1], x[2]], d);
                        mask[1, n] = GetVoxelType(voxels[x[0], x[1], x[2]], d + 3);
                        n++;
                        Profiler.EndSample();
                    }

                // create quads from masks

                for (layer = 0; layer < 2; layer++)
                {
                    n = 0;
                    o = new int[3] { 0, 0, 0 };
                    o[d] = 1 - (layer * 2);

                    for (j = 0; j < ChunkData.SIZE[v]; j++)
                        for (i = 0; i < ChunkData.SIZE[u];)
                        {
                            if (i < ChunkData.SIZE[u] && mask[layer, n] > 0)
                            {
                                // compute width
                                for (w = 1; i + w < ChunkData.SIZE[u] && mask[layer, n] == mask[layer, n + w]; w++) { }


                                // compute height
                                bool done = false;
                                for (h = 1; j + h < ChunkData.SIZE[v]; h++)
                                {
                                    for (int k = 0; k < w; k++)
                                    {
                                        // exit loop when the type doesn't matches
                                        if (mask[layer, n] != mask[layer, n + k + h * ChunkData.SIZE[u]])
                                        {
                                            done = true;
                                            break;
                                        }
                                    }
                                    if (done) break;
                                }


                                // add quad
                                y[d] = x[d] + (1 - layer);
                                y[u] = i;
                                y[v] = j;

                                // set deltas
                                int[] du = { 0, 0, 0 }; du[u] = w;
                                int[] dv = { 0, 0, 0 }; dv[v] = h;

                                Quad newQuad = new Quad();

                                newQuad.orientation = new Vector3(o[0], o[1], o[2]);

                                if (layer == 0)
                                {
                                    newQuad.vertices = new Vector3[4]
                                    {
                                        new Vector3(y[0]                , y[1]                , y[2]                ),
                                        new Vector3(y[0]         + dv[0], y[1]         + dv[1], y[2]         + dv[2]),
                                        new Vector3(y[0] + du[0]        , y[1] + du[1]        , y[2] + du[2]        ),
                                        new Vector3(y[0] + du[0] + dv[0], y[1] + du[1] + dv[1], y[2] + du[2] + dv[2])
                                    };

                                }
                                else
                                {
                                    newQuad.vertices = new Vector3[4]
                                    {
                                        new Vector3(y[0]         + dv[0], y[1]         + dv[1], y[2]         + dv[2]),
                                        new Vector3(y[0]                , y[1]                , y[2]                ),
                                        new Vector3(y[0] + du[0] + dv[0], y[1] + du[1] + dv[1], y[2] + du[2] + dv[2]),
                                        new Vector3(y[0] + du[0]        , y[1] + du[1]        , y[2] + du[2]        )
                                    };
                                }

                                if (newQuad.orientation == Vector3.up )
                                    newQuad.uvs = new Vector3[4]
                                    {
                                        new Vector3(0, 0, 1 ),
                                        new Vector3(0, h, 1 ),
                                        new Vector3(w, 0, 1 ),
                                        new Vector3(w, h, 1 )
                                    };

                                if (newQuad.orientation == Vector3.down )
                                    newQuad.uvs = new Vector3[4]
                                    {
                                        new Vector3(0, 0, 1 ),
                                        new Vector3(0, h, 1 ),
                                        new Vector3(w, 0, 1 ),
                                        new Vector3(w, h, 1 )
                                    };

                                if (newQuad.orientation == Vector3.forward )
                                    newQuad.uvs = new Vector3[4]
                                    {
                                        new Vector3(0, 0, 1 ),
                                        new Vector3(0, h, 1 ),
                                        new Vector3(w, 0, 1 ),
                                        new Vector3(w, h, 1 )
                                    };

                                if (newQuad.orientation == Vector3.back )
                                    newQuad.uvs = new Vector3[4]
                                    {
                                        new Vector3(0, h, 1 ),
                                        new Vector3(0, 0, 1 ),
                                        new Vector3(w, h, 1 ),
                                        new Vector3(w, 0, 1 )
                                    };

                                if (newQuad.orientation == Vector3.left )
                                    newQuad.uvs = new Vector3[4]
                                    {
                                        new Vector3(0, 0, 1 ),
                                        new Vector3(h, 0, 1 ),
                                        new Vector3(0, w, 1 ),
                                        new Vector3(h, w, 1 )
                                    };

                                if (newQuad.orientation == Vector3.right )
                                    newQuad.uvs = new Vector3[4]
                                    {
                                        new Vector3(h, 0, 1 ),
                                        new Vector3(0, 0, 1 ),
                                        new Vector3(h, w, 1 ),
                                        new Vector3(0, w, 1 )
                                    };

                                quads.Add(newQuad);

                                for (int l = 0; l < h; l++)
                                    for (int k = 0; k < w; k++)
                                    {
                                        mask[layer, n + k + l * ChunkData.SIZE[u]] = -1;
                                    }

                                i += w;
                                n += w;
                            }
                            else
                            {
                                i++;
                                n++;
                            }
                        }
                }
            }
        }

        Profiler.EndSample();
        Profiler.BeginSample("Meshing");

        vertices = new Vector3[quads.Count * 4];
        normals = new Vector3[quads.Count * 4];
        triangles = new int[quads.Count * 6];
        uvs = new Vector3[quads.Count * 4];

        int c = 0;
        int t = 0;
        int[] a = new int[4] { 0, 0, 0, 0 };

        foreach (Quad quad in quads)
        {
            for (int i = 0; i < quad.vertices.Length; i++)
            {
                vertices[c] = quad.vertices[i] + transform.position;
                normals[c] = quad.orientation;
                uvs[c] = quad.uvs[i];
                a[i] = c;
                c++;
            }

            triangles[t++] = a[0];
            triangles[t++] = a[2];
            triangles[t++] = a[1];
            triangles[t++] = a[3];
            triangles[t++] = a[1];
            triangles[t++] = a[2];
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.SetUVs(0, uvs);
        meshFilter.mesh = mesh;

        Profiler.EndSample();
    }
}
