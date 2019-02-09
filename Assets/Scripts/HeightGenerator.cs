using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightGenerator
{
    static int terrainMaxHeight = (int)(World.chunkSize * World.columnHeight * 0.7f);
    static int octaves = 4;
    static float persistence = 0.5f;
    static float terrainSmooth = 0.01f;

    static int stoneMaxHeight = terrainMaxHeight - 25;
    static float stoneSmooth = terrainSmooth * 2;

    static int unbreakableMaxHeight = (int)(World.chunkSize * World.columnHeight * 0.1f);
    static float unbreakableSmooth = terrainSmooth * 0.8f;


    public static int GenerateTerrainHeight(float x, float z)
    {
        return (int)Mathf.Lerp(0, terrainMaxHeight, fractalBrownianMotion(x * terrainSmooth, z * terrainSmooth, octaves, persistence));
    }

    public static int GenerateStoneHeight(float x, float z)
    {
        return (int)Mathf.Lerp(0, stoneMaxHeight, fractalBrownianMotion(x * stoneSmooth, z * stoneSmooth, octaves, persistence));
    }

    public static int GenerateUnbreakableHeight(float x, float z)
    {
        return (int)Mathf.Lerp(0, unbreakableMaxHeight, fractalBrownianMotion(x * unbreakableSmooth, z * unbreakableSmooth, octaves, persistence));
    }


    static float fractalBrownianMotion(float x, float z, int octaves, float persistance)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;
        float offset = 32000;
        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise((x + offset) * frequency, (z + offset) * frequency) * amplitude;

            maxValue += amplitude;

            amplitude *= persistance;
            frequency *= 2;
        }

        return total / maxValue;
    }
}
