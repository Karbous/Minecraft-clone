using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine;

// to save data for each chunk that was changed
[Serializable]
public class BlockData
{
    public Block.BlockType[,,] blockTypeMatrix;

    public BlockData()
    {
    }

    public BlockData(Block[,,] blocks)
    {
        blockTypeMatrix = new Block.BlockType[World.chunkSize, World.chunkSize, World.chunkSize];
        for (int z = 0; z < World.chunkSize; z++)
        {
            for (int y = 0; y < World.chunkSize; y++)
            {
                for (int x = 0; x < World.chunkSize; x++)
                {
                    blockTypeMatrix[x, y, z] = blocks[x, y, z].blockType;
                }
            }
        }
    }
}

// to save player location, rotation and height generating offsets
[Serializable]
public class WorldData
{
    // player position
    public float playerPositionX;
    public float playerPositionY;
    public float playerPositionZ;

    // player rotation
    public float playerRotationX;
    public float playerRotationY;
    public float playerRotationZ;
    public float playerRotationW;

    // offsets for generating world
    public float heightGeneratorOffsetX;
    public float heightGeneratorOffsetZ;

    public WorldData()
    {
    }

    public WorldData(
        float playerPositionX, float playerPositionY, float playerPositionZ,
        float playerRotationX, float playerRotationY, float playerRotationZ, float playerRotationW,
        float heightGeneratorOffsetX, float heightGeneratorOffsetZ
        )
    {
        this.playerPositionX = playerPositionX;
        this.playerPositionY = playerPositionY;
        this.playerPositionZ = playerPositionZ;

        this.playerRotationX = playerRotationX;
        this.playerRotationY = playerRotationY;
        this.playerRotationZ = playerRotationZ;
        this.playerRotationW = playerRotationW;

        this.heightGeneratorOffsetX = heightGeneratorOffsetX;
        this.heightGeneratorOffsetZ = heightGeneratorOffsetZ;
    }
}

public class SaveLoad
{
    public static BlockData blockData;
    public static WorldData worldData;

    static string tmpChunkSaveFolder = $"{Application.persistentDataPath}/tmpsavedata/";
    static string hardSaveFile = $"{Application.persistentDataPath}/hardsavedata/world.dat";
    static string hardSaveFolderForChunks = $"{Application.persistentDataPath}/hardsavedata/tmpChunks";


    // ***to save and load chunk data of chunks that were changed when game is running***

    //create chunk save file name
    static string ChunkFileName(Vector3 chunkPosition)
    {
        return $"{tmpChunkSaveFolder}Chunk_{chunkPosition.x}_{chunkPosition.y}_{chunkPosition.z}.dat";
    }

    // save chunk
    public static void SaveChunk(Vector3 chunkPosition, Block[,,] blocksInChunk)
    {
        string chunkFileName = ChunkFileName(chunkPosition);
        if (!File.Exists(chunkFileName))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(chunkFileName));
        }
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream file = File.Open(chunkFileName, FileMode.OpenOrCreate);
        blockData = new BlockData(blocksInChunk);
        binaryFormatter.Serialize(file, blockData);
        file.Close();
    }

    // load chunk
    public static bool LoadChunk(Vector3 chunkPosition)
    {
        string chunkFileName = ChunkFileName(chunkPosition);
        if (File.Exists(chunkFileName))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream file = File.Open(chunkFileName, FileMode.Open);
            blockData = new BlockData();
            blockData = (BlockData)binaryFormatter.Deserialize(file);
            file.Close();
            return true;
        }
        return false;
    }

    // ***to save a load world to keep its state after game is quit***

    // save world
    public static void SaveWorld(GameObject player)
    {
        //save changed chunks that were not destroyed
        foreach (KeyValuePair<string, Chunk> chunk in World.chunks)
        {
            if (chunk.Value.isChanged)
            {
                chunk.Value.SaveChunk();
            }
        }

        //copy chunk save files from tmp save folder to hard save folder
        DirectoryCopy(tmpChunkSaveFolder, hardSaveFolderForChunks);

        // save world state
        string worldFileName = hardSaveFile;
        if (!File.Exists(worldFileName))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(worldFileName));
        }
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream file = File.Open(worldFileName, FileMode.OpenOrCreate);
        worldData = new WorldData(
            player.transform.position.x, player.transform.position.y, player.transform.position.z,
            player.transform.rotation.x, player.transform.rotation.y, player.transform.rotation.z, player.transform.rotation.w,
            World.heightGeneratorOffsetX, World.heightGeneratorOffsetZ);
        binaryFormatter.Serialize(file, worldData);
        file.Close();
    }

    // load world
    public static bool LoadWorld()
    {
        //copy chunk save files from hard save folder to tmp save folder
        DirectoryCopy(hardSaveFolderForChunks, tmpChunkSaveFolder);

        // load world state
        string worldFileName = hardSaveFile;
        if (File.Exists(worldFileName))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream file = File.Open(worldFileName, FileMode.Open);
            worldData = new WorldData();
            worldData = (WorldData)binaryFormatter.Deserialize(file);
            file.Close();
            return true;
        }
        return false;
    }

    static void DirectoryCopy(string sourceDirName, string destDirName)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (dir.Exists)
        {
            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }
        }
    }

    public static void DeleteTmpChunkData()
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(tmpChunkSaveFolder);
        if (dir.Exists)
        {
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Get the files in the directory and delete them.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                file.Delete();
            }
        }
    }
}
