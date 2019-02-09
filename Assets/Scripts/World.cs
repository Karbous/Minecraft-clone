using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [SerializeField] Material cubeMaterial;
    [SerializeField] int columnHeight = 2;
    [SerializeField] int worldSize = 2;
    public static int chunkSize = 8;
    public static Dictionary<string, Chunk> chunks = new Dictionary<string, Chunk>();

    IEnumerator BuildWorld()
    {
        for (int z = 0; z < worldSize; z++)
        {
            for (int x = 0; x < worldSize; x++)
            {
                for (int y = 0; y < columnHeight; y++)
                {
                    Vector3 chunkPosition = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize);
                    Chunk newChunk = new Chunk(chunkPosition, cubeMaterial);
                    newChunk.chunkGameObject.transform.parent = transform;
                    chunks.Add(newChunk.chunkGameObject.name, newChunk);
                }
            }
        }
        foreach (KeyValuePair<string, Chunk> chunk in chunks)
        {
            chunk.Value.DrawChunk();
            yield return null;
        }
    }


    void Start()
    {
        StartCoroutine(BuildWorld());       
    }
}
