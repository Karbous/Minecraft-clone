using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;

public class World : MonoBehaviour
{
    [SerializeField] Material cubeMaterial;
    [SerializeField] GameObject player;
    //[SerializeField] GameObject loadingScreen;
    //[SerializeField] Slider loadingSlider;

    public static int chunkSize = 16;
    public static int radius = 5;

    public static ConcurrentDictionary<string, Chunk> chunks = new ConcurrentDictionary<string, Chunk>();
    public static ConcurrentDictionary<string, Chunk> savedChunks = new ConcurrentDictionary<string, Chunk>();

    public static List<string> chunksToRemove = new List<string>();
    public static List<string> chunksToSave = new List<string>();
    public Vector3 lastPlayerPosition;

    float heightGeneratorOffsetX;
    float heightGeneratorOffsetZ;

    bool firstBuild = true;
    bool draw = false;
    int buildCoroutineCounter = 0;
    int drawCounter = 0;

    void BuildChunkAtPosition(int x, int y, int z)
    {
        Vector3 chunkPosition = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize);
        Chunk chunk;
        string chunkName = Chunk.ChunkName(chunkPosition);

        if (savedChunks.TryGetValue(chunkName, out chunk))
        {
            chunk.chunkGameObject.transform.parent = transform;
            chunks.TryAdd(chunk.chunkGameObject.name, chunk);
        }
        else if (!chunks.TryGetValue(chunkName, out chunk))
        {
            chunk = new Chunk(chunkPosition, cubeMaterial);
            chunk.chunkGameObject.transform.parent = transform;
            chunks.TryAdd(chunk.chunkGameObject.name, chunk);
        }
    }

    IEnumerator BuildRecursiveWorld(int x, int y, int z, int radius)
    {
        buildCoroutineCounter++;

        radius--;

        if (radius <= 0)
        {
            buildCoroutineCounter--;
            yield break;
        }

        BuildChunkAtPosition(x, y, z + 1);
        StartCoroutine(BuildRecursiveWorld(x, y, z + 1, radius));
        yield return null;

        BuildChunkAtPosition(x, y, z - 1);
        StartCoroutine(BuildRecursiveWorld(x, y, z - 1, radius));
        yield return null;

        BuildChunkAtPosition(x + 1, y, z);
        StartCoroutine(BuildRecursiveWorld(x + 1, y, z, radius));
        yield return null;

        BuildChunkAtPosition(x - 1, y, z);
        StartCoroutine(BuildRecursiveWorld(x - 1, y, z, radius));
        yield return null;

        BuildChunkAtPosition(x, y + 1, z);
        StartCoroutine(BuildRecursiveWorld(x, y + 1, z, radius));
        yield return null;

        if (y > 0)
        {
            BuildChunkAtPosition(x, y - 1, z);
            StartCoroutine(BuildRecursiveWorld(x, y - 1, z, radius));
            yield return null;
        }

        buildCoroutineCounter--;
    }

    void AddChunksToRemove()
    {
        foreach (KeyValuePair<string, Chunk> chunk in chunks)
        {
            if (Vector3.Distance(player.transform.position, chunk.Value.chunkGameObject.transform.position) > radius * chunkSize)
            {
                chunk.Value.chunkStatus = Chunk.ChunkStatus.DESTROY;
                chunksToRemove.Add(chunk.Key);
            }
        }
    }


    IEnumerator DrawChunks()
    {
        foreach (KeyValuePair<string, Chunk> chunk in chunks)
        {
            if (firstBuild)
            {
                drawCounter++;
            }

            if (chunk.Value.chunkStatus == Chunk.ChunkStatus.DRAW)
            {
                chunk.Value.chunkStatus = Chunk.ChunkStatus.DONE;
                //TO DO check for DONE neighbors to redraw them
                chunk.Value.DrawChunk();
            }

            yield return null;
        }
    }

    IEnumerator RemoveOldChunks()
    {
        for (int i = 0; i < chunksToRemove.Count; i++)
        {
            string chunkToRemoveName = chunksToRemove[i];
            Chunk chunkToRemove;

            if (chunks.TryGetValue(chunkToRemoveName, out chunkToRemove))
            {
                chunks.TryRemove(chunkToRemoveName, out chunkToRemove);

                // not working!!!!
                /*
                if (chunksToSave.Contains(chunkToRemoveName))
                {
                    savedChunks.TryAdd(chunkToRemoveName, chunkToRemove);
                }
                */

                Destroy(chunkToRemove.chunkGameObject);

                yield return null;
            }
        }
        chunksToRemove.Clear();
    }

    void BuildNearPlayer()
    {
        //StopAllCoroutines();
        StartCoroutine(BuildRecursiveWorld(
            (int)(player.transform.position.x / chunkSize),
            (int)(player.transform.position.y / chunkSize),
            (int)(player.transform.position.z / chunkSize),
            radius
            ));
    }

    void Start()
    {
        //only for the new world
        heightGeneratorOffsetX = UnityEngine.Random.Range(10000, 30000);
        heightGeneratorOffsetZ = UnityEngine.Random.Range(10000, 30000);

        HeightGenerator.offsetX = heightGeneratorOffsetX;
        HeightGenerator.offsetZ = heightGeneratorOffsetZ;

        player.transform.position = new Vector3(
            player.transform.position.x,
            HeightGenerator.GenerateTerrainHeight(player.transform.position.x, player.transform.position.z) + 1,
            player.transform.position.z
            );
        lastPlayerPosition = player.transform.position;

        // TO DO when loading a save file, load heightGeneratorOffsetX, heightGeneratorOffsetZ, 


        //build starting chunk at player position
        BuildChunkAtPosition(
            (int)(player.transform.position.x / chunkSize),
            (int)(player.transform.position.y / chunkSize),
            (int)(player.transform.position.z / chunkSize)
            );

        //create rest of the chunks
        StartCoroutine(BuildRecursiveWorld(
            (int)(player.transform.position.x / chunkSize),
            (int)(player.transform.position.y / chunkSize),
            (int)(player.transform.position.z / chunkSize),
            radius
            ));

        draw = true;
    }

    void Update()
    {
        // if new chunks were generated, draw them
        if (buildCoroutineCounter <= 0 && draw)
        {
            StartCoroutine(DrawChunks());
            draw = false;
        }

        // only for the first build, whait until all chunks are drawn
        if (firstBuild && drawCounter >= chunks.Count)
        {
            player.SetActive(true);
            firstBuild = false;
            //TO DO disable load screen
        }


        if (Vector3.Distance(player.transform.position, lastPlayerPosition) > chunkSize/2)
        {
            lastPlayerPosition = player.transform.position;
            BuildNearPlayer();
            AddChunksToRemove();
            StartCoroutine(RemoveOldChunks());
            draw = true;
        }


    }
}
