using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightGenerator
{
    static int octaves = 4;
    static float persistence = 0.5f;

    public static float offsetX = 0f;
    public static float offsetZ = 0f;

    static int terrainMaxHeight = 100;
    static float terrainSmooth = 0.01f;

    static int stoneMaxHeight = terrainMaxHeight - 25;
    static float stoneSmooth = terrainSmooth * 1.8f;

    static int unbreakableMaxHeight = 10;
    static float unbreakableSmooth = terrainSmooth * 0.8f;

    // terrain height for dirt
    public static int GenerateTerrainHeight(float x, float z)
    {
        return (int)Mathf.Lerp(0, terrainMaxHeight, fractalBrownianMotion(x * terrainSmooth, z * terrainSmooth, octaves, persistence));
    }

    // terrain height for stone
    public static int GenerateStoneHeight(float x, float z)
    {
        return (int)Mathf.Lerp(0, stoneMaxHeight, fractalBrownianMotion(x * stoneSmooth, z * stoneSmooth, octaves, persistence));
    }

    // terrain height for unbreakable stone
    public static int GenerateUnbreakableHeight(float x, float z)
    {
        return (int)Mathf.Lerp(0, unbreakableMaxHeight, fractalBrownianMotion(x * unbreakableSmooth, z * unbreakableSmooth, octaves, persistence));
    }


    static float fractalBrownianMotion(float x, float z, int octaves, float persistance)
    {
        float total = 0f;
        float frequency = 1f;
        float amplitude = 1f;
        float maxValue = 0f;
        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise((x + offsetX) * frequency, (z + offsetZ) * frequency) * amplitude;

            maxValue += amplitude;

            amplitude *= persistance;
            frequency *= 2;
        }

        return total / maxValue;
    }
}
