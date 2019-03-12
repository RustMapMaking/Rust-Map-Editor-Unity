﻿
using System.Collections.Generic;
using UnityEngine;
using static WorldSerialization;

public class WorldConverter {
    
	// #fuckErrn
	
    public struct MapInfo
    {
        public int resolution;
        public Vector3 size;
        public float[,,] splatMap;
        public float[,,] biomeMap;
        public float[,,] alphaMap;
        public TerrainInfo terrain;
        public TerrainInfo land;
        public TerrainInfo water;
        public TerrainMap<int> topology;
        public WorldSerialization.PrefabData[] prefabData;
        public WorldSerialization.PathData[] pathData;
    }

    public struct TerrainInfo
    {
        //put splatmaps in here if swamps and oceans add textures to water
        public float[,] heights;
    }

    
    public static MapInfo emptyWorld(int size)
    {
        MapInfo terrains = new MapInfo();

        
        var terrainSize = new Vector3(size, 1000, size);

        int resolution = Mathf.NextPowerOfTwo((int)(size * 0.50f));
        /*
        Terrain land = GameObject.FindGameObjectWithTag("Land").GetComponent<Terrain>();
        float[,] landHeight = land.terrainData.GetHeights(0, 0, (int)land.terrainData.size.x, (int)land.terrainData.size.y);
        Debug.Log(landHeight.GetLength(0));
        MapTransformations.resize(landHeight, size);

        Debug.Log(landHeight.GetLength(0));
        */
        var terrainMap  = new TerrainMap<short> (new byte[(int)Mathf.Pow((resolution + 1), 2) * 2 * 1], 1); //2 bytes 1 channel
        var heightMap   = new TerrainMap<short> (new byte[(int)Mathf.Pow((resolution + 1), 2) * 2 * 1], 1); //2 bytes 1 channel
        var waterMap    = new TerrainMap<short> (new byte[(int)Mathf.Pow((resolution + 1), 2) * 2 * 1], 1); //2 bytes 1 channel
        var splatMap    = new TerrainMap<byte>  (new byte[(int)Mathf.Pow(Mathf.Clamp(resolution,16,2048), 2) * 1 * 8], 8); //1 byte 8 channels
        var topologyMap = new TerrainMap<int>   (new byte[(int)Mathf.Pow(Mathf.Clamp(resolution, 16, 2048), 2) * 4 * 1], 1); //4 bytes 1 channel
        var biomeMap    = new TerrainMap<byte>  (new byte[(int)Mathf.Pow(Mathf.Clamp(resolution, 16, 2048), 2) * 1 * 4], 4); //1 bytes 4 channels
        var alphaMap    = new TerrainMap<byte>  (new byte[(int)Mathf.Pow(Mathf.Clamp(resolution, 16, 2048), 2) * 1 * 1], 1); //1 byte 1 channel

        float[,] landHeight = new float[resolution + 1, resolution + 1];
        for (int i = 0; i < resolution + 1; i++)
        {
            for (int j = 0; j < resolution + 1; j++)
            {
                landHeight[i, j] = 480f / 1000f;
            }
        }

        float[,] waterHeight = new float[resolution + 1, resolution + 1];
        for (int i = 0; i < resolution + 1; i++)
        {
            for (int j = 0; j < resolution + 1; j++)
            {
                waterHeight[i, j] = 500f / 1000f;
            }
        }
        byte[] landHeightBytes = TypeConverter.floatArrayToByteArray(landHeight);
        byte[] waterHeightBytes = TypeConverter.floatArrayToByteArray(waterHeight);


        terrainMap.FromByteArray(landHeightBytes);
        heightMap.FromByteArray(landHeightBytes);
        waterMap.FromByteArray(waterHeightBytes);
        /*
        Debug.Log(terrainMap.res + " " + terrainMap.BytesTotal());
        Debug.Log(heightMap.res + " " + heightMap.BytesTotal());
        Debug.Log(waterMap.res + " " + waterMap.BytesTotal());
        Debug.Log(splatMap.res + " " + splatMap.BytesTotal());
        Debug.Log(topologyMap.res + " " + topologyMap.BytesTotal());
        Debug.Log(biomeMap.res + " " + biomeMap.BytesTotal());
        Debug.Log(alphaMap.res + " " + alphaMap.BytesTotal());
        */

        terrains.topology = topologyMap;

        List<PathData> paths = new List<PathData>();
        //foreach (PathDataHolder pathHolder in GameObject.FindObjectsOfType<PathDataHolder>())
            //paths.Add(pathHolder.pathData);

        List<PrefabData> prefabs = new List<PrefabData>();
        //foreach (PrefabDataHolder prefabHolder in GameObject.FindObjectsOfType<PrefabDataHolder>())
            //prefabs.Add(prefabHolder.prefabData);

        terrains.pathData = paths.ToArray();
        terrains.prefabData = prefabs.ToArray();


        terrains.resolution = heightMap.res;
        terrains.size = terrainSize;

        terrains.terrain.heights = TypeConverter.shortMapToFloatArray(terrainMap);
        terrains.land.heights = TypeConverter.shortMapToFloatArray(terrainMap);
        terrains.water.heights = TypeConverter.shortMapToFloatArray(waterMap);

        terrains = convertMaps(terrains, splatMap, biomeMap, alphaMap);

        for (int i = 0; i < terrains.splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < terrains.splatMap.GetLength(1); j++)
            {
                terrains.splatMap[i, j, 4] = 1f;
            }
        }

        return terrains;
    }

    public static MapInfo convertMaps(MapInfo terrains, TerrainMap<byte> splatMap, TerrainMap<byte> biomeMap, TerrainMap<byte> alphaMap)
    {

        terrains.splatMap = new float[splatMap.res, splatMap.res, 8];
        for (int i = 0; i < terrains.splatMap.GetLength(0); i++)
        {
            for (int j = 0; j < terrains.splatMap.GetLength(1); j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    terrains.splatMap[i, j, k] = BitUtility.Byte2Float(splatMap[k, i, j]);
                }
            }
        }

        terrains.biomeMap = new float[biomeMap.res, biomeMap.res, 4];
        for (int i = 0; i < terrains.biomeMap.GetLength(0); i++)
        {
            for (int j = 0; j < terrains.biomeMap.GetLength(1); j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    terrains.biomeMap[i, j, k] = BitUtility.Byte2Float(biomeMap[k, i, j]);
                }
            }
        }
        terrains.alphaMap = new float[alphaMap.res, alphaMap.res, 2];
        for (int i = 0; i < terrains.alphaMap.GetLength(0); i++)
        {
            for (int j = 0; j < terrains.alphaMap.GetLength(1); j++)
            {
                if (alphaMap[0, i, j] > 0)
                {
                    terrains.alphaMap[i, j, 0] = BitUtility.Byte2Float(alphaMap[0, i, j]);
                }
                else
                {
                    terrains.alphaMap[i, j, 1] = 0xFF;
                }
            }
        }

        return terrains;
    }

    public static MapInfo worldToTerrain(WorldSerialization blob)
    {
        MapInfo terrains = new MapInfo();

        // Puts prefabs object in middle of map for any size up to 6000.
        var worldCentrePrefab = GameObject.FindGameObjectWithTag("Prefabs");
        worldCentrePrefab.transform.position = new Vector3(blob.world.size / 2, 500, blob.world.size / 2);
        var worldCentrePath = GameObject.FindGameObjectWithTag("Paths");
        worldCentrePath.transform.position = new Vector3(blob.world.size / 2, 500, blob.world.size / 2);

        if (blob.GetMap("terrain") == null)
            Debug.LogError("Old map file");

        var terrainSize = new Vector3(blob.world.size, 1000, blob.world.size);
        var terrainMap = new TerrainMap<short>(blob.GetMap("terrain").data, 1);
        var heightMap = new TerrainMap<short>(blob.GetMap("height").data, 1);
        var waterMap = new TerrainMap<short>(blob.GetMap("water").data, 1);
        var splatMap = new TerrainMap<byte>(blob.GetMap("splat").data, 8);
        var topologyMap = new TerrainMap<int>(blob.GetMap("topology").data, 1);
        var biomeMap = new TerrainMap<byte>(blob.GetMap("biome").data, 4);
        var alphaMap = new TerrainMap<byte>(blob.GetMap("alpha").data, 1);

        
        terrains.topology = topologyMap;

        /*
        Debug.Log(terrainMap.res + " " + terrainMap.BytesTotal());
        Debug.Log(heightMap.res + " " + heightMap.BytesTotal());
        Debug.Log(waterMap.res + " " + waterMap.BytesTotal());
        Debug.Log(splatMap.res + " " + splatMap.BytesTotal());
        Debug.Log(topologyMap.res + " " + topologyMap.BytesTotal());
        Debug.Log(biomeMap.res + " " + biomeMap.BytesTotal());
        Debug.Log(alphaMap.res + " " + alphaMap.BytesTotal());
        */


        terrains.pathData = blob.world.paths.ToArray();
        terrains.prefabData = blob.world.prefabs.ToArray();

        terrains.resolution = heightMap.res;
        terrains.size = terrainSize;

        terrains.terrain.heights = TypeConverter.shortMapToFloatArray(terrainMap);
        terrains.land.heights = TypeConverter.shortMapToFloatArray(terrainMap);
        terrains.water.heights = TypeConverter.shortMapToFloatArray(waterMap);

        terrains = convertMaps(terrains, splatMap, biomeMap, alphaMap);

        
        
        

        
        return terrains;
        
    }
    

    public static WorldSerialization terrainToWorld(Terrain land, Terrain water)
    {
        WorldSerialization world = new WorldSerialization();
        world.world.size = (uint) land.terrainData.size.x;

        byte[] landHeightBytes = TypeConverter.floatArrayToByteArray(land.terrainData.GetHeights(0, 0, land.terrainData.heightmapWidth, land.terrainData.heightmapHeight));

        byte[] waterHeightBytes = TypeConverter.floatArrayToByteArray(water.terrainData.GetHeights(0, 0, water.terrainData.heightmapWidth, water.terrainData.heightmapHeight));
    
        var textureResolution = Mathf.Clamp(Mathf.NextPowerOfTwo((int)(world.world.size * 0.50f)), 16, 2048);

        float[,,] splatMapValues = TypeConverter.singleToMulti(GameObject.FindGameObjectWithTag("Land").transform.Find("Ground").GetComponent<LandData>().splatMap, 8);
        byte[] splatBytes = new byte[textureResolution * textureResolution * 8];
        var splatMap = new TerrainMap<byte>(splatBytes, 8);

        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < textureResolution; j++)
            {
                for(int k = 0; k < textureResolution; k++)
                {
                    splatMap[i, j, k] = BitUtility.Float2Byte(splatMapValues[j, k, i]);
                }
            }
        }

        byte[] biomeBytes = new byte[textureResolution * textureResolution * 4];
        var biomeMap = new TerrainMap<byte>(biomeBytes, 4);
        float[,,] biomeArray = TypeConverter.singleToMulti(GameObject.FindGameObjectWithTag("Land").transform.Find("Biome").GetComponent<LandData>().splatMap, 4);

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < textureResolution; j++)
            {
                for (int k = 0; k < textureResolution; k++)
                {
                    biomeMap[i, j, k] = BitUtility.Float2Byte(biomeArray[j, k, i]);
                }
            }
        }


        byte[] alphaBytes = new byte[textureResolution * textureResolution * 1];
        var alphaMap = new TerrainMap<byte>(alphaBytes, 1);
        float[,,] alphaArray = TypeConverter.singleToMulti(GameObject.FindGameObjectWithTag("Land").transform.Find("Alpha").GetComponent<LandData>().splatMap, 2);
        for (int j = 0; j < textureResolution; j++)
        {
            for (int k = 0; k < textureResolution; k++)
            {
                 alphaMap[0, j, k] = BitUtility.Float2Byte(alphaArray[j, k, 0]);
            }
        }

        var topologyMap = GameObject.FindGameObjectWithTag("Topology").GetComponent<TopologyMesh>().getTerrainMap();


        world.AddMap("terrain", landHeightBytes);
        world.AddMap("height", landHeightBytes);
        world.AddMap("splat", splatMap.ToByteArray());
        world.AddMap("biome", biomeMap.ToByteArray());
        world.AddMap("topology", topologyMap.ToByteArray());
        world.AddMap("alpha", alphaMap.ToByteArray());
        world.AddMap("water", waterHeightBytes);

        PrefabDataHolder[] prefabs = GameObject.FindObjectsOfType<PrefabDataHolder>();

        foreach (PrefabDataHolder p in prefabs)
        {
            if (p.prefabData != null)
                world.world.prefabs.Insert(0,p.prefabData);
        }

        PathDataHolder[] paths = GameObject.FindObjectsOfType<PathDataHolder>();

        foreach (PathDataHolder p in paths)
        {
            if (p.pathData != null)
            {
                p.pathData.nodes = new VectorData[p.transform.childCount];
                for(int i = 0; i < p.transform.childCount; i++)
                {
                    Transform g = p.transform.GetChild(i);
                    p.pathData.nodes[i] = g.position - (0.5f * land.terrainData.size);
                }
                world.world.paths.Insert(0, p.pathData);
            }
        }

       
        return world;
    }

}
