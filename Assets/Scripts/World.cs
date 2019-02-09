using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class World : MonoBehaviour
{
    [SerializeField] Material cubeMaterial;
    [SerializeField] GameObject player;
    [SerializeField] GameObject loadingScreen;
    [SerializeField] Slider loadingSlider;

    public static int columnHeight = 8;
    public static int chunkSize = 16;
    public static int horizontalRadius = 1;

    public static Dictionary<string, Chunk> chunks = new Dictionary<string, Chunk>();

    IEnumerator BuildWorld()
    {
        float totalChunks = (Mathf.Pow((horizontalRadius * 2) + 1, 2) * columnHeight) * 2;
        int chunksToLoad = 0;

        for (int z = -horizontalRadius; z <= horizontalRadius; z++)
        {
            for (int x = -horizontalRadius; x <= horizontalRadius; x++)
            {
                for (int y = 0; y < columnHeight; y++)
                {
                    Vector3 chunkPosition = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize);
                    Chunk newChunk = new Chunk(chunkPosition, cubeMaterial);
                    newChunk.chunkGameObject.transform.parent = transform;
                    chunks.Add(newChunk.chunkGameObject.name, newChunk);

                    chunksToLoad++;
                    loadingSlider.value = chunksToLoad / totalChunks;

                    yield return null;
                }
            }
        }
        foreach (KeyValuePair<string, Chunk> chunk in chunks)
        {
            chunk.Value.DrawChunk();
            chunksToLoad++;
            loadingSlider.value = chunksToLoad / totalChunks;
            yield return null;
        }

        loadingScreen.SetActive(false);
        player.SetActive(true);
    }

    void Start()
    {
        StartCoroutine(BuildWorld());
    }
}
