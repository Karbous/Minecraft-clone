using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;
using System;


public class World : MonoBehaviour
{
    [SerializeField] Material cubeMaterial;
    [SerializeField] GameObject player;
    [SerializeField] GameObject loadingScreen;
    [SerializeField] GameObject inGameUI;
    [SerializeField] Slider loadingSlider;

    public static int chunkSize = 16;
    public static int radius = 6;

    public static ConcurrentDictionary<string, Chunk> chunks = new ConcurrentDictionary<string, Chunk>();

    public static List<string> chunksToRemove = new List<string>();

    public Vector3 lastPlayerPosition;

    public static float heightGeneratorOffsetX;
    public static float heightGeneratorOffsetZ;

    bool firstBuild = true;
    public bool finishedLoading = false;
    bool draw = false;
    int buildCoroutineCounter = 0;
    int drawCounter = 0;

    void BuildChunkAtPosition(int x, int y, int z)
    {
        Vector3 chunkPosition = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize);
        Chunk chunk;
        string chunkName = Chunk.ChunkName(chunkPosition);

        if (!chunks.TryGetValue(chunkName, out chunk))
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
            //StartCoroutine(BuildRecursiveWorld(x, y - 1, z, radius));
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
                loadingSlider.value = (float)drawCounter / (float)chunks.Count;
            }

            if (chunk.Value.chunkStatus == Chunk.ChunkStatus.DRAW)
            {
                chunk.Value.chunkStatus = Chunk.ChunkStatus.DONE;
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

                if (chunkToRemove.isChanged)
                {
                    chunkToRemove.SaveChunk();
                }
                Destroy(chunkToRemove.chunkGameObject);

                yield return null;
            }
        }
        chunksToRemove.Clear();
    }

    void BuildNearPlayer()
    {
        StopAllCoroutines();
        StartCoroutine(BuildRecursiveWorld(
            (int)(player.transform.position.x / chunkSize),
            (int)(player.transform.position.y / chunkSize),
            (int)(player.transform.position.z / chunkSize),
            radius
            ));
    }

    public void NewGame()
    {
        finishedLoading = false;
        firstBuild = true;
        draw = false;
        loadingSlider.value = 0;
        drawCounter = 0;
        loadingSlider.value = 0;

        chunksToRemove.Clear();
        chunks.Clear();

        heightGeneratorOffsetX = UnityEngine.Random.Range(10000, 30000);
        heightGeneratorOffsetZ = UnityEngine.Random.Range(10000, 30000);

        HeightGenerator.offsetX = heightGeneratorOffsetX;
        HeightGenerator.offsetZ = heightGeneratorOffsetZ;

        player.transform.position = new Vector3(
            0f,
            HeightGenerator.GenerateTerrainHeight(0f, 0f) + 1,
            0f
            );
        lastPlayerPosition = player.transform.position;

        BuildWorld();
    }

    public void LoadGame()
    {
        finishedLoading = false;
        firstBuild = true;
        draw = false;
        loadingSlider.value = 0;
        drawCounter = 0;
        loadingSlider.value = 0;


        chunksToRemove.Clear();
        chunks.Clear();

        if (SaveLoad.LoadWorld())
        {
            heightGeneratorOffsetX = SaveLoad.worldData.heightGeneratorOffsetX;
            heightGeneratorOffsetZ = SaveLoad.worldData.heightGeneratorOffsetZ;
            HeightGenerator.offsetX = heightGeneratorOffsetX;
            HeightGenerator.offsetZ = heightGeneratorOffsetZ;

            player.transform.position = new Vector3(
                SaveLoad.worldData.playerPositionX,
                SaveLoad.worldData.playerPositionY,
                SaveLoad.worldData.playerPositionZ
                );
            player.transform.rotation = new Quaternion(
                SaveLoad.worldData.playerRotationX,
                SaveLoad.worldData.playerRotationY,
                SaveLoad.worldData.playerRotationZ,
                SaveLoad.worldData.playerRotationW
                );

            lastPlayerPosition = player.transform.position;

            BuildWorld();
        }
        else
        {
            Debug.Log($"The game cannot be loaded! A save file may be missing: {Application.persistentDataPath}/hardsavedata/world.dat");
        }
    }

    public void SaveGame()
    {
        SaveLoad.SaveWorld(player);
    }


    public void BuildWorld()
    {
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
        finishedLoading = true;
    }

    void Update()
    {
        if (finishedLoading)
        {
            // only for the first build, wait until all chunks are drawn
            if (firstBuild && drawCounter >= chunks.Count)
            {
                player.SetActive(true);
                firstBuild = false;
                loadingScreen.SetActive(false);
                inGameUI.SetActive(true);
            }

            // if new chunks were generated, draw them
            if (buildCoroutineCounter <= 0 && draw)
            {
                StartCoroutine(DrawChunks());
                draw = false;
            }

            // if player walks certain distance, redraw the world around him
            if (Vector3.Distance(player.transform.position, lastPlayerPosition) > chunkSize / 2)
            {
                lastPlayerPosition = player.transform.position;
                BuildNearPlayer();
                AddChunksToRemove();
                StartCoroutine(RemoveOldChunks());
                draw = true;
            }
        }
    }
}
