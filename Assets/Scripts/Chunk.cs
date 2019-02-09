using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    /*public*/ Material cubeMaterial;
    public Block[,,] blocksInChunk;
    public GameObject chunkGameObject;

    //constructor
    public Chunk (Vector3 chunkPosition, Material cubeMaterial)
    {
        chunkGameObject = new GameObject(ChunkName(chunkPosition));
        chunkGameObject.transform.position = chunkPosition;
        this.cubeMaterial = cubeMaterial;
        BuildChunk();
    }

    public static string ChunkName(Vector3 chunkPosition)
    {
        return $"{chunkPosition.x}_{chunkPosition.y}_{chunkPosition.z}";
    }

    void BuildChunk()
    {
        blocksInChunk = new Block[World.chunkSize, World.chunkSize, World.chunkSize];

        //create blocks and add them to 3D array
        for (int z = 0; z < World.chunkSize; z++)
        {
            for (int y = 0; y < World.chunkSize; y++)
            {
                for (int x = 0; x < World.chunkSize; x++)
                {
                    Vector3 newBlockPosition = new Vector3(x, y, z);

                    // temporary, only to see if making holes(air) in chunk will render them correctly
                    if (UnityEngine.Random.Range(0, 100) < 50)
                    {
                        blocksInChunk[x, y, z] = new Block(Block.BlockType.DIRT, newBlockPosition, this);
                    }
                    else
                    {
                        blocksInChunk[x, y, z] = new Block(Block.BlockType.AIR, newBlockPosition, this);
                    }
                }
            }
        }
    }

    public void DrawChunk()
    { 
        //draw all blocks in chunk
        for (int z = 0; z < World.chunkSize; z++)
        {
            for (int y = 0; y < World.chunkSize; y++)
            {
                for (int x = 0; x < World.chunkSize; x++)
                {
                    blocksInChunk[x, y, z].CreateVisibleQuads();
                }
            }
        }
        CombineQuads();
    }

    void CombineQuads()
    {
        //combine all children meshes
        MeshFilter[] meshFilters = chunkGameObject.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        //create new mesh on this game object and add combined children meshes to it
        MeshFilter meshFilter = chunkGameObject.gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh.CombineMeshes(combine);

        //create new mesh renderer on this game object
        MeshRenderer meshRenderer = chunkGameObject.gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = cubeMaterial;

        //delete all uncombined children quads
        foreach (Transform quad in chunkGameObject.gameObject.transform)
        {
            UnityEngine.Object.Destroy(quad.gameObject);
        }
    }
}
