using System;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    enum CubeSide { BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK };
    public enum BlockType { GRASS, DIRT, STONE, UNBREAKABLE, AIR };
    int[] maxBlockHealth = { 2, 2, 4, -1, 0 };
    public enum CrackBlockType { NOCRACK, CRACK1, CRACK2, CRACK3 }


    public BlockType blockType;
    CrackBlockType crackBlockType;
    public Chunk parentChunk;
    public Vector3 blockPosition;
    int currentBlockHealth;

    // UVs of the particular texture in texture atlas
    Vector2[,] blockUVs =
    { 
		/*GRASS TOP*/		{new Vector2( 0.125f, 0.375f ), new Vector2( 0.1875f, 0.375f), new Vector2( 0.125f, 0.4375f ),new Vector2( 0.1875f, 0.4375f )},
		/*GRASS SIDE*/		{new Vector2( 0.1875f, 0.9375f ), new Vector2( 0.25f, 0.9375f), new Vector2( 0.1875f, 1.0f ),new Vector2( 0.25f, 1.0f )},
		/*DIRT*/			{new Vector2( 0.125f, 0.9375f ), new Vector2( 0.1875f, 0.9375f), new Vector2( 0.125f, 1.0f ),new Vector2( 0.1875f, 1.0f )},
		/*STONE*/			{new Vector2( 0.0f, 0.875f ), new Vector2( 0.0625f, 0.875f), new Vector2( 0.0f, 0.9375f ),new Vector2( 0.0625f, 0.9375f )},
		/*UNBREAKABLE*/	    {new Vector2( 0.0625f, 0.5f ), new Vector2( 0.125f, 0.5f), new Vector2( 0.0625f, 0.5625f ),new Vector2( 0.125f, 0.5625f )},

        /*NOCRACK*/         {new Vector2( 0.6875f, 0f ), new Vector2( 0.75f, 0f), new Vector2( 0.6875f, 0.0625f ),new Vector2( 0.75f, 0.0625f )},
        /*CRACK1*/          {new Vector2( 0.0625f, 0.0f ), new Vector2( 0.125f, 0.0f), new Vector2( 0.0625f, 0.0625f ),new Vector2( 0.125f, 0.0625f )},
        /*CRACK2*/          {new Vector2( 0.125f, 0.0f ), new Vector2( 0.1875f, 0.0f), new Vector2( 0.125f, 0.0625f ),new Vector2( 0.1875f, 0.0625f )},
        /*CRACK3*/          {new Vector2( 0.1875f, 0.0f ), new Vector2( 0.25f, 0.0f), new Vector2( 0.1875f, 0.0625f ),new Vector2( 0.25f, 0.0625f )},
    };

    // constructor
    public Block(BlockType _blockType, Vector3 _blockPosition, Chunk _parentChunk)
    {
        blockType = _blockType;
        parentChunk = _parentChunk;
        blockPosition = _blockPosition;
        currentBlockHealth = maxBlockHealth[(int)blockType];
        crackBlockType = CrackBlockType.NOCRACK;
    }

    public void CreateVisibleQuads()
    {
        // if the block is AIR, don't draw anything
        if (blockType == BlockType.AIR)
        {
            return;
        }

        // if the side has a solid neighbour, don't draw that side
        if (!HasSolidNeighbour((int)blockPosition.x, (int)blockPosition.y, (int)blockPosition.z + 1))
        {
            CreateQuad(CubeSide.FRONT);
        }
        if (!HasSolidNeighbour((int)blockPosition.x, (int)blockPosition.y, (int)blockPosition.z - 1))
        {
            CreateQuad(CubeSide.BACK);
        }
        if (!HasSolidNeighbour((int)blockPosition.x, (int)blockPosition.y + 1, (int)blockPosition.z))
        {
            CreateQuad(CubeSide.TOP);
        }
        if (!HasSolidNeighbour((int)blockPosition.x, (int)blockPosition.y - 1, (int)blockPosition.z))
        {
            CreateQuad(CubeSide.BOTTOM);
        }
        if (!HasSolidNeighbour((int)blockPosition.x + 1, (int)blockPosition.y, (int)blockPosition.z))
        {
            CreateQuad(CubeSide.RIGHT);
        }
        if (!HasSolidNeighbour((int)blockPosition.x - 1, (int)blockPosition.y, (int)blockPosition.z))
        {
            CreateQuad(CubeSide.LEFT);
        }
    }

    public void CreateGhostBlock()
    {
        CreateQuad(CubeSide.FRONT);
        CreateQuad(CubeSide.BACK);
        CreateQuad(CubeSide.TOP);
        CreateQuad(CubeSide.BOTTOM);
        CreateQuad(CubeSide.RIGHT);
        CreateQuad(CubeSide.LEFT);
    }

    bool HasSolidNeighbour(int x, int y, int z)
    {
        // get the blocks to search in
        Block[,,] blocks;

        // check in neighbour chunk if block is on the edge of chunk
        if (
            x < 0 || x >= World.chunkSize ||
            y < 0 || y >= World.chunkSize ||
            z < 0 || z >= World.chunkSize
            )
        {
            Chunk neigborChunk;

            // get neighbour chunk position
            Vector3 neighbourChunkPosition = parentChunk.chunkGameObject.transform.position
                                            + new Vector3(
                                                (x - (int)blockPosition.x) * World.chunkSize,
                                                (y - (int)blockPosition.y) * World.chunkSize,
                                                (z - (int)blockPosition.z) * World.chunkSize
                                                );
            // get neihbour chunk name using its location
            string neighbourChunkName = Chunk.ChunkName(neighbourChunkPosition);

            // get local position of block in neighbour chunk
            x = GetBlockLocalPosition(x);
            y = GetBlockLocalPosition(y);
            z = GetBlockLocalPosition(z);

            // try find the neighbour chunk in chunk dictionary
            if (World.chunks.TryGetValue(neighbourChunkName, out neigborChunk))
            {
                blocks = neigborChunk.blocksInChunk;
            }
            else
            {
                return false;
            }
        }
        else
        {
            blocks = parentChunk.blocksInChunk;
        }

        return blocks[x, y, z].IsBlockSolid();
    }

    int GetBlockLocalPosition(int i)
    {
        if (i == -1)
        {
            i = World.chunkSize - 1;
        }
        else if (i == World.chunkSize)
        {
            i = 0;
        }

        return i;
    }

    public bool IsBlockSolid()
    {
        if (blockType == BlockType.AIR)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void SetBlockType(BlockType newBlockType)
    {
        blockType = newBlockType;
        crackBlockType = CrackBlockType.NOCRACK;
        currentBlockHealth = maxBlockHealth[(int)blockType];
    }

    public bool BlockIsDestroyed()
    {
        // if block is unbreakable
        if (currentBlockHealth == -1)
        {
            return false;
        }

        // damage the block
        currentBlockHealth--;
        crackBlockType++;
        parentChunk.isChanged = true;

        if (currentBlockHealth <= 0)
        {
            // destroy the block
            SetBlockType(BlockType.AIR);
            parentChunk.Redraw();
            return true;
        }

        parentChunk.Redraw();
        return false;
    }

    public bool BuildBlock(BlockType blockType)
    {
        SetBlockType(blockType);
        parentChunk.isChanged = true;
        parentChunk.Redraw();
        return true;
    }

    void CreateQuad(CubeSide cubeSide)
    {
        Mesh mesh = new Mesh();

        Vector3[] verticies = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        Vector2[] UVs = new Vector2[4];
        List<Vector2> crackUVs = new List<Vector2>();
        int[] triangles = new int[6];

        // all possible vertices
        Vector3 vertex0 = new Vector3(-0.5f, -0.5f, 0.5f);
        Vector3 vertex1 = new Vector3(0.5f, -0.5f, 0.5f);
        Vector3 vertex2 = new Vector3(0.5f, -0.5f, -0.5f);
        Vector3 vertex3 = new Vector3(-0.5f, -0.5f, -0.5f);
        Vector3 vertex4 = new Vector3(-0.5f, 0.5f, 0.5f);
        Vector3 vertex5 = new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 vertex6 = new Vector3(0.5f, 0.5f, -0.5f);
        Vector3 vertex7 = new Vector3(-0.5f, 0.5f, -0.5f);

        // all possible UVs
        Vector2 UV00;
        Vector2 UV10;
        Vector2 UV01;
        Vector2 UV11;

        // set UVs acording to block type
        switch (blockType)
        {
            case BlockType.GRASS:
                if (cubeSide == CubeSide.TOP)
                {
                    UV00 = blockUVs[0, 0];
                    UV10 = blockUVs[0, 1];
                    UV01 = blockUVs[0, 2];
                    UV11 = blockUVs[0, 3];
                }
                else if (cubeSide == CubeSide.BOTTOM)
                {
                    UV00 = blockUVs[2, 0];
                    UV10 = blockUVs[2, 1];
                    UV01 = blockUVs[2, 2];
                    UV11 = blockUVs[2, 3];
                }
                else
                {
                    UV00 = blockUVs[1, 0];
                    UV10 = blockUVs[1, 1];
                    UV01 = blockUVs[1, 2];
                    UV11 = blockUVs[1, 3];
                }
                break;
            case BlockType.DIRT:
                UV00 = blockUVs[2, 0];
                UV10 = blockUVs[2, 1];
                UV01 = blockUVs[2, 2];
                UV11 = blockUVs[2, 3];
                break;
            case BlockType.STONE:
                UV00 = blockUVs[3, 0];
                UV10 = blockUVs[3, 1];
                UV01 = blockUVs[3, 2];
                UV11 = blockUVs[3, 3];
                break;
            case BlockType.UNBREAKABLE:
                UV00 = blockUVs[4, 0];
                UV10 = blockUVs[4, 1];
                UV01 = blockUVs[4, 2];
                UV11 = blockUVs[4, 3];
                break;
            default:
                // error texture if block type unknown
                UV00 = blockUVs[0, 0];
                UV10 = blockUVs[0, 0];
                UV01 = blockUVs[0, 0];
                UV11 = blockUVs[0, 0];
                break;
        }

        crackUVs.Add(blockUVs[(int)(crackBlockType) + 5, 3]);
        crackUVs.Add(blockUVs[(int)(crackBlockType) + 5, 2]);
        crackUVs.Add(blockUVs[(int)(crackBlockType) + 5, 0]);
        crackUVs.Add(blockUVs[(int)(crackBlockType) + 5, 1]);

        // construct all quads of the cube
        switch (cubeSide)
        {
            case CubeSide.BOTTOM:
                verticies = new Vector3[] { vertex0, vertex1, vertex2, vertex3 };
                normals = new Vector3[] { Vector3.down, Vector3.down, Vector3.down, Vector3.down };
                UVs = new Vector2[] { UV11, UV01, UV00, UV10 };
                triangles = new int[] { 3, 1, 0, 3, 2, 1 };
                break;
            case CubeSide.TOP:
                verticies = new Vector3[] { vertex7, vertex6, vertex5, vertex4 };
                normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
                UVs = new Vector2[] { UV11, UV01, UV00, UV10 };
                triangles = new int[] { 3, 1, 0, 3, 2, 1 };
                break;
            case CubeSide.LEFT:
                verticies = new Vector3[] { vertex7, vertex4, vertex0, vertex3 };
                normals = new Vector3[] { Vector3.left, Vector3.left, Vector3.left, Vector3.left };
                UVs = new Vector2[] { UV11, UV01, UV00, UV10 };
                triangles = new int[] { 3, 1, 0, 3, 2, 1 };
                break;
            case CubeSide.RIGHT:
                verticies = new Vector3[] { vertex5, vertex6, vertex2, vertex1 };
                normals = new Vector3[] { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
                UVs = new Vector2[] { UV11, UV01, UV00, UV10 };
                triangles = new int[] { 3, 1, 0, 3, 2, 1 };
                break;
            case CubeSide.FRONT:
                verticies = new Vector3[] { vertex4, vertex5, vertex1, vertex0 };
                normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
                UVs = new Vector2[] { UV11, UV01, UV00, UV10 };
                triangles = new int[] { 3, 1, 0, 3, 2, 1 };
                break;
            case CubeSide.BACK:
                verticies = new Vector3[] { vertex6, vertex7, vertex3, vertex2 };
                normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
                UVs = new Vector2[] { UV11, UV01, UV00, UV10 };
                triangles = new int[] { 3, 1, 0, 3, 2, 1 };
                break;
            default:
                break;
        }

        // create mesh
        mesh.vertices = verticies;
        mesh.normals = normals;
        mesh.uv = UVs;
        mesh.SetUVs(1, crackUVs);
        mesh.triangles = triangles;

        mesh.RecalculateBounds();

        // create quad
        GameObject quad = new GameObject("quad");
        quad.transform.parent = parentChunk.chunkGameObject.transform;
        quad.transform.position = blockPosition;

        // add mesh filter
        MeshFilter meshFilter = quad.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }
}
