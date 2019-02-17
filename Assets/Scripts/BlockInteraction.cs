using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockInteraction : MonoBehaviour
{
    Block.BlockType[] blockTypeToBuild = { Block.BlockType.AIR, Block.BlockType.GRASS, Block.BlockType.DIRT, Block.BlockType.STONE };
    int currentBuildMode = 0;

    Block previousHitBlock = null;
    GameObject ghostBlockGameObject = null;

    [SerializeField] GameObject camera;
    [SerializeField] Material ghostMaterial;
    [SerializeField] Sprite[] buildSprites = new Sprite[4];
    [SerializeField] Image buildImage;

    void Start()
    {
        buildImage.sprite = buildSprites[currentBuildMode];
    }

    private void Update()
    {
        //change buld mode
        if (
            Input.GetKeyDown(KeyCode.Alpha0) ||
            Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Alpha2) ||
            Input.GetKeyDown(KeyCode.Alpha3)

            )
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                currentBuildMode = 0;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                currentBuildMode = 1;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                currentBuildMode = 2;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                currentBuildMode = 3;
            }

            // change UI image
            buildImage.sprite = buildSprites[currentBuildMode];

            // destroy the ghost block
            Destroy(ghostBlockGameObject);

            // draw new ghost block according to the new build mode
            if (currentBuildMode != 0 && previousHitBlock != null)
            {
                Chunk ghostBlock = new Chunk(previousHitBlock.parentChunk.chunkGameObject.transform.position, ghostMaterial, previousHitBlock.blockPosition, blockTypeToBuild[currentBuildMode]);
                ghostBlockGameObject = ghostBlock.chunkGameObject;
            }
        }


        // add new block
        if (currentBuildMode != 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, 5))
            {

                Chunk hitChunk;
                if (!World.chunks.TryGetValue(hit.collider.gameObject.name, out hitChunk))
                {
                    return;
                }

                Block hitBlock = GetBlock(hit.point + hit.normal / 2f);

                if (hitBlock != previousHitBlock)
                {
                    //destroy old ghost block
                    Destroy(ghostBlockGameObject);

                    previousHitBlock = hitBlock;

                    //draw new ghost block
                    Chunk ghostBlock = new Chunk(hitBlock.parentChunk.chunkGameObject.transform.position, ghostMaterial, hitBlock.blockPosition, blockTypeToBuild[currentBuildMode]);
                    ghostBlockGameObject = ghostBlock.chunkGameObject;
                }


                if (Input.GetMouseButtonDown(1))
                {
                    //build the block
                    hitBlock.BuildBlock(blockTypeToBuild[currentBuildMode]);
                }
            }
        }

        // destroy the block
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, 5))
            {
                // get the chunk that was hit
                Chunk hitChunk;
                if (!World.chunks.TryGetValue(hit.collider.gameObject.name, out hitChunk))
                {
                    return;
                }

                Block blockToDestroy = GetBlock(hit.point - hit.normal / 2f);

                if (blockToDestroy.BlockIsDestroyed())
                {
                    RedrawNeighborChunks(hitChunk.chunkGameObject.transform.position, blockToDestroy.blockPosition);
                }
            }
        }
    }

    private void RedrawNeighborChunks(Vector3 chunkPosition, Vector3 blockPosition)
    {
        // if the change block is on the edge of the chunk, alo redraw the neighbor chunk

        List<string> chunksToUpdate = new List<string>();

        if (blockPosition.x == 0)
        {
            chunksToUpdate.Add(Chunk.ChunkName(new Vector3(chunkPosition.x - World.chunkSize, chunkPosition.y, chunkPosition.z)));
        }
        if (blockPosition.x == World.chunkSize - 1)
        {
            chunksToUpdate.Add(Chunk.ChunkName(new Vector3(chunkPosition.x + World.chunkSize, chunkPosition.y, chunkPosition.z)));
        }
        if (blockPosition.y == 0)
        {
            chunksToUpdate.Add(Chunk.ChunkName(new Vector3(chunkPosition.x, chunkPosition.y - World.chunkSize, chunkPosition.z)));
        }
        if (blockPosition.y == World.chunkSize - 1)
        {
            chunksToUpdate.Add(Chunk.ChunkName(new Vector3(chunkPosition.x, chunkPosition.y + World.chunkSize, chunkPosition.z)));
        }
        if (blockPosition.z == 0)
        {
            chunksToUpdate.Add(Chunk.ChunkName(new Vector3(chunkPosition.x, chunkPosition.y, chunkPosition.z - World.chunkSize)));
        }
        if (blockPosition.z == World.chunkSize - 1)
        {
            chunksToUpdate.Add(Chunk.ChunkName(new Vector3(chunkPosition.x, chunkPosition.y, chunkPosition.z + World.chunkSize)));
        }

        foreach (string chunkName in chunksToUpdate)
        {
            Chunk chunkToUpdate;

            if (World.chunks.TryGetValue(chunkName, out chunkToUpdate))
            {
                chunkToUpdate.Redraw();
            }
        }
    }

    static Block GetBlock(Vector3 position)
    {
        // gets the correct block to be build (or destroyed), even if the block to be build is in different chunk than the chunk that was hit

        int chunkX, chunkY, chunkZ;

        if (position.x > 0)
        {
            chunkX = (int)(Mathf.Round(position.x) / World.chunkSize) * World.chunkSize;
        }
        else
        {
            chunkX = (int)((Mathf.Round(position.x - World.chunkSize)+1) / World.chunkSize) * World.chunkSize;
        }
        if (position.y > 0)
        {
            chunkY = (int)(Mathf.Round(position.y) / World.chunkSize) * World.chunkSize;
        }
        else
        {
            chunkY = (int)((Mathf.Round(position.y - World.chunkSize)+1) / World.chunkSize) * World.chunkSize;
        }
        if (position.z > 0)
        {
            chunkZ = (int)(Mathf.Round(position.z) / World.chunkSize) * World.chunkSize;
        }
        else
        {
            chunkZ = (int)((Mathf.Round(position.z - World.chunkSize)+1) / World.chunkSize) * World.chunkSize;
        }

        int blockX = (int)Mathf.Abs(Mathf.Round(position.x) - chunkX);
        int blockY = (int)Mathf.Abs(Mathf.Round(position.y) - chunkY);
        int blockZ = (int)Mathf.Abs(Mathf.Round(position.z) - chunkZ);

        string chunkName = Chunk.ChunkName(new Vector3(chunkX, chunkY, chunkZ));
        Chunk chunk;

        if (World.chunks.TryGetValue(chunkName, out chunk))
        {
            return chunk.blocksInChunk[blockX, blockY, blockZ];
        }
        else
        {
            return null;
        }
    }
}


