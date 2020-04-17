using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class Chunk : MonoBehaviour
{
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    // flags
    private bool updateMesh;

    // data storage
    private Dictionary<Vector3, Voxel> voxels = new Dictionary<Vector3, Voxel>();

    // mesh storage
    private Hashtable verticesDictionary;
    private List<Vector3> chunkVertices;
    private List<int> chunkTriangles;

    void Start()
    {
        verticesDictionary = new Hashtable();
        chunkVertices = new List<Vector3>();
        chunkTriangles = new List<int>();

        GenerateVoxels();
    }

    void FixedUpdate()
    {
        UpdateVoxels();

        if (updateMesh)
        {
            GenerateMesh();
            // updateMesh = false;
        }
    }

    private void GenerateVoxels()
    {
        for (int x = 0; x < ChunkData.sizeX; x++)
        {
            for (int z = 0; z < ChunkData.sizeZ; z++)
            {
                for (int y = 0; y < ChunkData.sizeY; y++)
                {
                    if (Random.value > 0.5f)
                    {
                        voxels.Add(new Vector3(x, y, z) + transform.position, new Voxel());
                    }
                }
            }
        }

        updateMesh = true;
    }

    private void UpdateVoxels()
    {
        foreach (KeyValuePair<Vector3, Voxel> voxel in voxels)
        {
            if (voxel.Value.update)
            {
                bool visible = voxel.Value.visible;
                voxel.Value.visible = false;

                for (int s = 0; s < VoxelData.neighbours.Length; s++)
                {
                    if (!voxels.ContainsKey(voxel.Key + VoxelData.neighbours[s]))
                    {
                        voxel.Value.visible = true;
                        voxel.Value.renderSides[s] = true;
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

    private void GenerateMesh()
    {
        Profiler.BeginSample("My Sample");

        verticesDictionary.Clear();
        chunkVertices.Clear();
        chunkTriangles.Clear();

        foreach (KeyValuePair<Vector3, Voxel> voxel in voxels)
        {
            if (voxel.Value.visible)
            {
                AddVoxel(voxel.Key, voxel.Value);
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = chunkVertices.ToArray();
        mesh.triangles = chunkTriangles.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        Profiler.EndSample();
    }

    private void AddVoxel(Vector3 position, Voxel voxel)
    {
        int[] offset = new int[8];
        Vector3 newVertice;
        int maxVertices = VoxelData.vertices.GetLength(0);

        for (int v = 0; v < maxVertices; v++)
        {
            newVertice = VoxelData.vertices[v] + position;

            chunkVertices.Add(newVertice);
            offset[v] = chunkVertices.Count - 1;
        }

        int maxVoxelSides = VoxelData.sides.GetLength(0);
        int maxVoxelTriangleVertices = VoxelData.sides.GetLength(1);

        for (int s = 0; s < maxVoxelSides; s++)
        {
            //if (true)
            if (voxel.renderSides[s])
            {
                for (int v = 0; v < maxVoxelTriangleVertices; v++)
                {
                    chunkTriangles.Add(offset[VoxelData.sides[s, v]]);
                }
            }
        }
    }
}
