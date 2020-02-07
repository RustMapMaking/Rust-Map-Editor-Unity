﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using RustMapEditor.Variables;
using static RustMapEditor.Data.LandData;
using static RustMapEditor.Maths.Array;
using static WorldConverter;
using static PrefabManager;

public static class MapIO
{
    public static float progressValue = 1f;
    public static Texture terrainFilterTexture;
    public static Vector2 heightmapCentre = new Vector2(0.5f, 0.5f);

    [InitializeOnLoadMethod]
    public static void Start()
    {
        terrainFilterTexture = Resources.Load<Texture>("Textures/Brushes/White128");
        RefreshPresetsList(); // Refreshes the node gen presets.
        EditorApplication.update += OnProjectLoad;
    }
    /// <summary>Executes once when the project finished loading.</summary>
    static void OnProjectLoad()
    {
        if (land != null)
        {
            EditorApplication.update -= OnProjectLoad;
            CreateMap(1000);
        }
    }

    public static void CentreSceneView(SceneView sceneView)
    {
        if (sceneView != null)
        {
            sceneView.orthographic = false;
            sceneView.pivot = new Vector3(500f, 600f, 500f);
            sceneView.rotation = Quaternion.Euler(25f, 0f, 0f);
        }
    }

    public static void SetCullingDistances(Camera camera, float prefabDist, float pathDist)
    {
        float[] distances = new float[32];
        distances[8] = prefabDist;
        distances[9] = pathDist;
        camera.layerCullDistances = distances;
    }

    /// <summary>Displays a popup progress bar, the progress is also visible in the taskbar.</summary>
    /// <param name="title">The Progress Bar title.</param>
    /// <param name="info">The info to be displayed next to the loading bar.</param>
    /// <param name="progress">The progress amount. Between 0f - 1f.</param>
    public static void ProgressBar(string title, string info, float progress)
    {
        EditorUtility.DisplayProgressBar(title, info, progress);
    }

    /// <summary>Clears the popup progress bar. Needs to be called otherwise it will persist in the editor.</summary>
    public static void ClearProgressBar()
    {
        progressValue = 0;
        EditorUtility.ClearProgressBar();
    }
    
    public static List<int> GetEnumSelection<T>(T enumGroup)
    {
        List<int> selectedEnums = new List<int>();
        for (int i = 0; i < Enum.GetValues(typeof(T)).Length; i++)
        {
            int layer = 1 << i;
            if ((Convert.ToInt32(enumGroup) & layer) != 0)
            {
                selectedEnums.Add(i);
            }
        }
        return selectedEnums;
    }
    public static void RotateMap(Selections.Objects objectSelection, bool CW)
    {
        foreach (var item in GetEnumSelection(objectSelection))
        {
            switch (item)
            {
                case 0:
                    RotateLayer(LandLayers.Ground, CW);
                    break;
                case 1:
                    RotateLayer(LandLayers.Biome, CW);
                    break;
                case 2:
                    RotateLayer(LandLayers.Alpha, CW);
                    break;
                case 3:
                    RotateTopologyLayers((TerrainTopology.Enum)TerrainTopology.EVERYTHING, CW);
                    break;
                case 4:
                    RotateTerrains(CW, Selections.Terrains.Land);
                    break;
                case 5:
                    RotateTerrains(CW, Selections.Terrains.Water);
                    break;
                case 6:
                    RotatePrefabs(CW);
                    break;
                case 7:
                    RotatePaths(CW);
                    break;
            }
        }
    }
    /// <summary>Removes all the map objects from the scene.</summary>
    /// <param name="prefabs">Delete Prefab objects.</param>
    /// <param name="paths">Delete Path objects.</param>
    public static void RemoveMapObjects(bool prefabs = true, bool paths = true)
    {
        if (prefabs)
        {
            foreach (PrefabDataHolder g in GameObject.FindGameObjectWithTag("Prefabs").GetComponentsInChildren<PrefabDataHolder>())
            {
                if (g != null)
                {
                    GameObject.DestroyImmediate(g.gameObject);
                }
            }
            foreach (CustomPrefabData p in GameObject.FindGameObjectWithTag("Prefabs").GetComponentsInChildren<CustomPrefabData>())
            {
                GameObject.DestroyImmediate(p.gameObject);
            }
        }
        if (paths)
        {
            foreach (PathDataHolder g in GameObject.FindGameObjectWithTag("Paths").GetComponentsInChildren<PathDataHolder>())
            {
                GameObject.DestroyImmediate(g.gameObject);
            }
        }
    }
    /// <summary>Rotates prefabs 90°.</summary>
    /// <param name="CW">True = 90°, False = 270°</param>
    public static void RotatePrefabs(bool CW)
    {
        var prefabRotate = GameObject.FindGameObjectWithTag("Prefabs");
        if (CW)
        {
            prefabRotate.transform.Rotate(0, 90, 0, Space.World);
            prefabRotate.GetComponent<LockObject>().UpdateTransform();
        }
        else
        {
            prefabRotate.transform.Rotate(0, -90, 0, Space.World);
            prefabRotate.GetComponent<LockObject>().UpdateTransform();
        }
    }
    /// <summary>Rotates paths 90°.</summary>
    /// <param name="CW">True = 90°, False = 270°</param>
    public static void RotatePaths(bool CW)
    {
        var pathRotate = GameObject.FindGameObjectWithTag("Paths");
        if (CW)
        {
            pathRotate.transform.Rotate(0, 90, 0, Space.World);
            pathRotate.GetComponent<LockObject>().UpdateTransform();
        }
        else
        {
            pathRotate.transform.Rotate(0, -90, 0, Space.World);
            pathRotate.GetComponent<LockObject>().UpdateTransform();
        }
    }
    /// <summary>Rotates the selected terrains.</summary>
    /// <param name="CW">True = 90°, False = 270°</param>
    public static void RotateTerrains(bool CW, Selections.Terrains terrains, Dimensions dmns = null)
    {
        foreach (var item in GetEnumSelection(terrains))
        {
            switch (item)
            {
                case 0:
                    land.terrainData.SetHeights(0, 0, Rotate(land.terrainData.GetHeights(0, 0, GetHeightMapResolution(), GetHeightMapResolution()), CW, dmns));
                    break;
                case 1:
                    water.terrainData.SetHeights(0, 0, Rotate(water.terrainData.GetHeights(0, 0, GetHeightMapResolution(), GetHeightMapResolution()), CW, dmns));
                    break;
            }
        }
    }
    /// <summary>Sets the selected terrains to the height set.</summary>
    /// <param name="height">The height to set.</param>
    public static void SetHeightmap(float height, Selections.Terrains terrains, Dimensions dmns = null)
    {
        height /= 1000f;
        foreach (var item in GetEnumSelection(terrains))
        {
            switch (item)
            {
                case 0:
                    land.terrainData.SetHeights(0, 0, SetValues(land.terrainData.GetHeights(0, 0, GetHeightMapResolution(), GetHeightMapResolution()), height, dmns));
                    break;
                case 1:
                    water.terrainData.SetHeights(0, 0, SetValues(water.terrainData.GetHeights(0, 0, GetHeightMapResolution(), GetHeightMapResolution()), height, dmns));
                    break;
            }
        }
    }
    /// <summary>Inverts the selected terrains.</summary>
    public static void InvertHeightmap(Selections.Terrains terrains, Dimensions dmns = null)
    {
        foreach (var item in GetEnumSelection(terrains))
        {
            switch (item)
            {
                case 0:
                    land.terrainData.SetHeights(0, 0, Invert(land.terrainData.GetHeights(0, 0, GetHeightMapResolution(), GetHeightMapResolution()), dmns));
                    break;
                case 1:
                    water.terrainData.SetHeights(0, 0, Invert(water.terrainData.GetHeights(0, 0, GetHeightMapResolution(), GetHeightMapResolution()), dmns));
                    break;
            }
        }
    }
    /// <summary> Normalises the terrain between two heights.</summary>
    /// <param name="normaliseLow">The lowest height the HeightMap should be.</param>
    /// <param name="normaliseHigh">The highest height the HeightMap should be.</param>
    public static void NormaliseHeightmap(float normaliseLow, float normaliseHigh, Selections.Terrains terrains, Dimensions dmns = null)
    {
        normaliseLow /= 1000f; normaliseHigh /= 1000f;
        foreach (var item in GetEnumSelection(terrains))
        {
            switch (item)
            {
                case 0:
                    land.terrainData.SetHeights(0, 0, Normalise(land.terrainData.GetHeights(0, 0, GetHeightMapResolution(), GetHeightMapResolution()), normaliseLow, normaliseHigh, dmns));
                    break;
                case 1:
                    water.terrainData.SetHeights(0, 0, Normalise(water.terrainData.GetHeights(0, 0, GetHeightMapResolution(), GetHeightMapResolution()), normaliseLow, normaliseHigh, dmns));
                    break;
            }
        }
    }
    /// <summary>Terraces the HeightMap.</summary>
    /// <param name="featureSize">The height of each terrace.</param>
    /// <param name="interiorCornerWeight">The weight of the terrace effect.</param>
    public static void TerraceErodeHeightmap(float featureSize, float interiorCornerWeight)
    {
        Material mat = new Material((Shader)AssetDatabase.LoadAssetAtPath("Packages/com.unity.terrain-tools/Shaders/TerraceErosion.shader", typeof(Shader)));
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(land, heightmapCentre, land.terrainData.size.x, 0.0f);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(land, brushXform.GetBrushXYBounds());
        Vector4 brushParams = new Vector4(1.0f, featureSize, interiorCornerWeight, 0.0f);
        mat.SetTexture("_BrushTex", terrainFilterTexture);
        mat.SetVector("_BrushParams", brushParams);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
        TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Filter - TerraceErosion");
    }
    /// <summary>Smooths the terrain.</summary>
    /// <param name="filterStrength">The strength of the smoothing.</param>
    /// <param name="blurDirection">The direction the smoothing should preference. Between -1f - 1f.</param>
    public static void SmoothHeightmap(float filterStrength, float blurDirection)
    {
        Material mat = TerrainPaintUtility.GetBuiltinPaintMaterial();
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(land, heightmapCentre, land.terrainData.size.x, 0.0f);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(land, brushXform.GetBrushXYBounds());
        Vector4 brushParams = new Vector4(filterStrength, 0.0f, 0.0f, 0.0f);
        mat.SetTexture("_BrushTex", terrainFilterTexture);
        mat.SetVector("_BrushParams", brushParams);
        Vector4 smoothWeights = new Vector4(Mathf.Clamp01(1.0f - Mathf.Abs(blurDirection)), Mathf.Clamp01(-blurDirection), Mathf.Clamp01(blurDirection), 0.0f);
        mat.SetVector("_SmoothWeights", smoothWeights);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, (int)TerrainPaintUtility.BuiltinPaintMaterialPasses.SmoothHeights);
        TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Filter - Smooth Heights");
    }
    /// <summary>Increases or decreases the terrain by the offset.</summary>
    /// <param name="offset">The amount to offset by. Negative values offset down.</param>
    /// <param name="clampOffset">Check if offsetting the heightmap would exceed the min-max values.</param>
    public static void OffsetHeightmap(float offset, bool clampOffset, Selections.Terrains terrains)
    {
        offset /= 1000f;
        foreach (var item in GetEnumSelection(terrains))
        {
            switch (item)
            {
                case 0:
                    land.terrainData.SetHeights(0, 0, Offset(land.terrainData.GetHeights(0, 0, GetHeightMapResolution(), GetHeightMapResolution()), offset, clampOffset));
                    break;
                case 1:
                    water.terrainData.SetHeights(0, 0, Offset(water.terrainData.GetHeights(0, 0, GetHeightMapResolution(), GetHeightMapResolution()), offset, clampOffset));
                    break;
            }
        }
    }
    /// <summary>Sets the HeightMap level to the minimum if it's below.</summary>
    /// <param name="minimumHeight">The minimum height to set.</param>
    /// <param name="maximumHeight">The maximum height to set.</param>
    public static void ClampHeightmap(float minimumHeight, float maximumHeight, Selections.Terrains terrains)
    {
        minimumHeight /= 1000f; maximumHeight /= 1000f;
        switch (terrains)
        {
            case Selections.Terrains.Land:
                land.terrainData.SetHeights(0, 0, ClampValues(land.terrainData.GetHeights(0, 0, GetHeightMapResolution(), GetHeightMapResolution()), minimumHeight, maximumHeight));
                break;
            case Selections.Terrains.Water:
                water.terrainData.SetHeights(0, 0, ClampValues(water.terrainData.GetHeights(0, 0, GetHeightMapResolution(), GetHeightMapResolution()), minimumHeight, maximumHeight));
                break;
        }
    }
    /// <summary>Returns the height of the HeightMap at the selected coords.</summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    public static float GetHeight(int x, int z)
    {
        float xNorm = (float)x / (float)land.terrainData.alphamapHeight;
        float yNorm = (float)z / (float)land.terrainData.alphamapHeight;
        float height = land.terrainData.GetInterpolatedHeight(xNorm, yNorm);
        return height;
    }
    /// <summary>Returns a 2D array of the height values.</summary>
    public static float[,] GetHeights()
    {
        return land.terrainData.GetInterpolatedHeights(0, 0, land.terrainData.alphamapHeight, land.terrainData.alphamapHeight, 1f / (float)land.terrainData.alphamapHeight, 1f / (float)land.terrainData.alphamapHeight);
    }
    public static float[,] GetWaterHeights()
    {
        return water.terrainData.GetInterpolatedHeights(0, 0, water.terrainData.alphamapHeight, water.terrainData.alphamapHeight, 1f / (float)water.terrainData.alphamapHeight, 1f / (float)water.terrainData.alphamapHeight);
    }
    /// <summary>Returns the slope of the HeightMap at the selected coords.</summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    public static float GetSlope(int x, int z)
    {
        float xNorm = (float)x / land.terrainData.alphamapHeight;
        float yNorm = (float)z / land.terrainData.alphamapHeight;
        float slope = land.terrainData.GetSteepness(xNorm, yNorm);
        return slope;
    }
    #region SplatMap Methods
    /// <summary>Texture count in layer chosen, used for determining the size of the splatmap array.</summary>
    /// <param name="landLayer">The LandLayer to return the texture count from. (Ground, Biome, Alpha, Topology)</param>
    public static int TextureCount(LandLayers landLayer)
    {
        switch (landLayer)
        {
            case LandLayers.Ground:
                return 8;
            case LandLayers.Biome:
                return 4;
            default:
                return 2;
        }
    }
    /// <summary>Returns the value of a texture at the selected coords.</summary>
    /// <param name="landLayer">The LandLayer of the texture. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="texture">The texture to get.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static float GetTexture(LandLayers landLayer, int texture, int x, int z, int topology = 0)
    {
        return GetSplatMap(landLayer, topology)[x, z, texture];
    }
    /// <summary>Rotates the selected layer.</summary>
    /// <param name="landLayerToPaint">The LandLayer to rotate. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="CW">True = 90°, False = 270°</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void RotateLayer(LandLayers landLayerToPaint, bool CW, int topology = 0)
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Ground:
            case LandLayers.Biome:
            case LandLayers.Topology:
                SetData(Rotate(GetSplatMap(landLayerToPaint, topology), CW), landLayerToPaint, topology);
                SetLayer(landLayer, TerrainTopology.TypeToIndex((int)topologyLayer));
                break;
            case LandLayers.Alpha:
                SetData(Rotate(GetAlphaMap(), CW), landLayerToPaint);
                break;
        }
    }
    /// <summary>Rotates the selected topologies.</summary>
    /// <param name="topologyLayers">The Topology layers to rotate.</param>
    /// <param name="CW">True = 90°, False = 270°</param>
    public static void RotateTopologyLayers(TerrainTopology.Enum topologyLayers, bool CW)
    {
        List<int> topologyElements = GetEnumSelection(topologyLayers);
        
        for (int i = 0; i < topologyElements.Count; i++)
        {
            progressValue += 1f / topologyElements.Count;
            ProgressBar("Rotating Topologies", "Rotating: " + ((TerrainTopology.Enum)TerrainTopology.IndexToType(i)).ToString(), progressValue);
            RotateLayer(LandLayers.Topology, CW, i);
        }
        ClearProgressBar();
    }
    /// <summary>Paints if all the conditions passed in are true.</summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="texture">The texture to paint.</param>
    /// <param name="conditions">The conditions to check.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintConditional(LandLayers landLayerToPaint, int texture, Conditions conditions, int topology = 0)
    {
        int splatRes = GetSplatMapResolution();
        bool[,] conditionsMet = new bool[splatRes, splatRes]; // Paints wherever the conditionsmet is false.

        ProgressBar("Paint Conditional", "Checking Ground", 0f);
        for(int i = 0; i < TerrainSplat.COUNT; i++)
        {
            if (conditions.GroundConditions.CheckLayer[i])
            {
                conditionsMet = CheckConditions(GetSplatMap(LandLayers.Ground), conditionsMet, i, conditions.GroundConditions.Weight[i]);
            }
        };
        ProgressBar("Paint Conditional", "Checking Biome", 0.2f);
        for (int i = 0; i < TerrainBiome.COUNT; i++)
        {
            if (conditions.BiomeConditions.CheckLayer[i])
            {
                conditionsMet = CheckConditions(GetSplatMap(LandLayers.Biome), conditionsMet, i, conditions.BiomeConditions.Weight[i]);
            }
        }
        ProgressBar("Paint Conditional", "Checking Alpha", 0.3f);
        if (conditions.AlphaConditions.CheckAlpha)
        {
            conditionsMet = CheckConditions(GetAlphaMap(), conditionsMet, (conditions.AlphaConditions.Texture == 0) ? true : false);
        }
        ProgressBar("Paint Conditional", "Checking Topology", 0.5f);
        for (int i = 0; i < TerrainTopology.COUNT; i++)
        {
            if (conditions.TopologyConditions.CheckLayer[i])
            {
                conditionsMet = CheckConditions(GetSplatMap(LandLayers.Topology, i), conditionsMet, (int)conditions.TopologyConditions.Texture[i], 0.5f);
            }
        }
        ProgressBar("Paint Conditional", "Checking Heights", 0.7f);
        if (conditions.TerrainConditions.CheckHeights)
        {
            conditionsMet = CheckConditions(GetHeights(), conditionsMet, conditions.TerrainConditions.Heights.HeightLow, conditions.TerrainConditions.Heights.HeightHigh);
        }
        ProgressBar("Paint Conditional", "Checking Slopes", 0.8f);
        if (conditions.TerrainConditions.CheckSlopes)
        {
            conditionsMet = CheckConditions(GetSlopes(), conditionsMet, conditions.TerrainConditions.Slopes.SlopeLow, conditions.TerrainConditions.Slopes.SlopeHigh);
        }
        ProgressBar("Paint Conditional", "Painting", 0.9f);
        switch (landLayerToPaint)
        {
            case LandLayers.Ground:
            case LandLayers.Biome:
            case LandLayers.Topology:
                float[,,] splatMapToPaint = GetSplatMap(landLayerToPaint, topology);
                int textureCount = TextureCount(landLayerToPaint);
                Parallel.For(0, splatRes, i =>
                {
                    for (int j = 0; j < splatRes; j++)
                    {
                        if (conditionsMet[i, j] == false)
                        {
                            for (int k = 0; k < textureCount; k++)
                            {
                                splatMapToPaint[i, j, k] = 0f;
                            }
                            splatMapToPaint[i, j, texture] = 1f;
                        }
                    }
                });
                SetData(splatMapToPaint, landLayerToPaint, topology);
                SetLayer(landLayerToPaint, topology);
                break;
            case LandLayers.Alpha:
                bool[,] alphaMapToPaint = GetAlphaMap();
                Parallel.For(0, splatRes, i =>
                {
                    for (int j = 0; j < splatRes; j++)
                    {
                        alphaMapToPaint[i, j] = (conditionsMet[i, j] == false) ? conditionsMet[i, j] : alphaMapToPaint[i, j];
                    }
                });
                SetData(alphaMapToPaint, landLayerToPaint);
                break;
        }
        ClearProgressBar();
    }
    /// <summary>Paints the layer wherever the height conditions are met.</summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="heightLow">The minimum height to paint at 100% weight.</param>
    /// <param name="heightHigh">The maximum height to paint at 100% weight.</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintHeight(LandLayers landLayerToPaint, float heightLow, float heightHigh, int t, int topology = 0)
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Ground:
            case LandLayers.Biome:
            case LandLayers.Topology:
                SetData(SetRange(GetSplatMap(landLayerToPaint, topology), GetHeights(), t, heightLow, heightHigh), landLayerToPaint, topology);
                SetLayer(landLayer, TerrainTopology.TypeToIndex((int)topologyLayer));
                break;
            case LandLayers.Alpha:
                bool value = (t == 0) ? true : false;
                SetData(SetRange(GetAlphaMap(), GetHeights(), value, heightLow, heightHigh), landLayerToPaint);
                break;
        }
    }
    /// <summary>Paints the layer wherever the height conditions are met with a weighting determined by the range the height falls in.</summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="heightLow">The minimum height to paint at 100% weight.</param>
    /// <param name="heightHigh">The maximum height to paint at 100% weight.</param>
    /// <param name="heightBlendLow">The minimum height to start to paint. The texture weight will increase as it gets closer to the heightlow.</param>
    /// <param name="heightBlendHigh">The maximum height to start to paint. The texture weight will increase as it gets closer to the heighthigh.</param>
    /// <param name="t">The texture to paint.</param>
    public static void PaintHeightBlend(LandLayers landLayerToPaint, float heightLow, float heightHigh, float heightBlendLow, float heightBlendHigh, int t)
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Ground:
            case LandLayers.Biome:
                SetData(SetRangeBlend(GetSplatMap(landLayerToPaint), GetHeights(), t, heightLow, heightHigh, heightBlendLow, heightBlendHigh), landLayerToPaint);
                SetLayer(landLayer);
                break;
        }
    }
    /// <summary>Sets whole layer to the active texture.</summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintLayer(LandLayers landLayerToPaint, int t, int topology = 0)
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Ground:
            case LandLayers.Biome:
            case LandLayers.Topology:
                SetData(SetValues(GetSplatMap(landLayerToPaint), t), landLayerToPaint, topology);
                SetLayer(landLayer, TerrainTopology.TypeToIndex((int)topologyLayer));
                break;
            case LandLayers.Alpha:
                SetData(SetValues(GetAlphaMap(), true), landLayerToPaint);
                break;
        }
    }
    /// <summary>Paints the selected Topology layers.</summary>
    /// <param name="topologyLayers">The Topology layers to clear.</param>
    public static void PaintTopologyLayers(TerrainTopology.Enum topologyLayers)
    {
        List<int> topologyElements = GetEnumSelection(topologyLayers);
        progressValue = 1f / topologyElements.Count;
        for (int i = 0; i < topologyElements.Count; i++)
        {
            progressValue += 1f / topologyElements.Count;
            ProgressBar("Painting Topologies", "Painting: " + ((TerrainTopology.Enum)TerrainTopology.IndexToType(i)).ToString(), progressValue);
            PaintLayer(LandLayers.Topology, 0, i);
        }
        ClearProgressBar();
    }
    /// <summary>Sets whole layer to the inactive texture. Alpha and Topology only.</summary>
    /// <param name="landLayerToPaint">The LandLayer to clear. (Alpha, Topology)</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void ClearLayer(LandLayers landLayerToPaint, int topology = 0)
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Topology:
                SetData(SetValues(GetSplatMap(landLayerToPaint, topology), 1), landLayerToPaint, topology);
                SetLayer(landLayer, TerrainTopology.TypeToIndex((int)topologyLayer));
                break;
            case LandLayers.Alpha:
                SetData(SetValues(GetAlphaMap(), false), landLayerToPaint);
                break;
        }
    }
    /// <summary>Clears the selected Topology layers.</summary>
    /// <param name="topologyLayers">The Topology layers to clear.</param>
    public static void ClearTopologyLayers(TerrainTopology.Enum topologyLayers)
    {
        List<int> topologyElements = GetEnumSelection(topologyLayers);
        progressValue = 1f / topologyElements.Count;
        for (int i = 0; i < topologyElements.Count; i++)
        {
            progressValue += 1f / topologyElements.Count;
            ProgressBar("Clearing Topologies", "Clearing: " + ((TerrainTopology.Enum)TerrainTopology.IndexToType(i)).ToString(), progressValue);
            ClearLayer(LandLayers.Topology, i);
        }
        ClearProgressBar();
    }
    /// <summary>Inverts the active and inactive textures. Alpha and Topology only.</summary>
    /// <param name="landLayerToPaint">The LandLayer to invert. (Alpha, Topology)</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void InvertLayer(LandLayers landLayerToPaint, int topology = 0)
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Topology:
                SetData(Invert(GetSplatMap(landLayerToPaint, topology)), landLayerToPaint, topology);
                SetLayer(landLayer, TerrainTopology.TypeToIndex((int)topologyLayer));
                break;
            case LandLayers.Alpha:
                SetData(Invert(GetAlphaMap()), landLayerToPaint);
                break;
        }
    }
    /// <summary>Inverts the selected Topology layers.</summary>
    /// <param name="topologyLayers">The Topology layers to invert.</param>
    public static void InvertTopologyLayers(TerrainTopology.Enum topologyLayers)
    {
        List<int> topologyElements = GetEnumSelection(topologyLayers);
        for (int i = 0; i < topologyElements.Count; i++)
        {
            progressValue += 1f / topologyElements.Count;
            ProgressBar("Inverting Topologies", "Inverting: " + ((TerrainTopology.Enum)TerrainTopology.IndexToType(i)).ToString(), progressValue);
            InvertLayer(LandLayers.Topology, i);
        }
        ClearProgressBar();
    }
    /// <summary>Paints the layer wherever the slope conditions are met. Includes option to blend.</summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="slopeLow">The minimum slope to paint at 100% weight.</param>
    /// <param name="slopeHigh">The maximum slope to paint at 100% weight.</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintSlope(LandLayers landLayerToPaint, float slopeLow, float slopeHigh, int t, int topology = 0) // Paints slope based on the current slope input, the slope range is between 0 - 90
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Ground:
            case LandLayers.Biome:
            case LandLayers.Topology:
                SetData(SetRange(GetSplatMap(landLayerToPaint, topology), GetSlopes(), t, slopeLow, slopeHigh), landLayerToPaint, topology);
                SetLayer(landLayer, TerrainTopology.TypeToIndex((int)topologyLayer));
                break;
            case LandLayers.Alpha:
                bool value = (t == 0) ? true : false;
                SetData(SetRange(GetAlphaMap(), GetSlopes(), value, slopeLow, slopeHigh), landLayerToPaint);
                break;
        }
    }
    /// <summary> Paints the layer wherever the slope conditions are met. Includes option to blend.</summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="slopeLow">The minimum slope to paint at 100% weight.</param>
    /// <param name="slopeHigh">The maximum slope to paint at 100% weight.</param>
    /// <param name="minBlendLow">The minimum slope to start to paint. The texture weight will increase as it gets closer to the slopeLow.</param>
    /// <param name="maxBlendHigh">The maximum slope to start to paint. The texture weight will increase as it gets closer to the slopeHigh.</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintSlopeBlend(LandLayers landLayerToPaint, float slopeLow, float slopeHigh, float minBlendLow, float maxBlendHigh, int t) // Paints slope based on the current slope input, the slope range is between 0 - 90
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Ground:
            case LandLayers.Biome:
                SetData(SetRangeBlend(GetSplatMap(landLayerToPaint), GetSlopes(), t, slopeLow, slopeHigh, minBlendLow, maxBlendHigh), landLayerToPaint);
                SetLayer(landLayer);
                break;
        }
    }
    /// <summary>
    /// Paints area within these splatmap coords, Maps will always have a splatmap resolution between 512 - 2048 resolution, to the nearest Power of Two (512, 1024, 2048).
    /// Paints from bottom left to top right corner of map if world rotation is 0° in the editor.
    /// </summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintArea(LandLayers landLayerToPaint, Dimensions dmns, int t, int topology = 0)
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Ground:
            case LandLayers.Biome:
            case LandLayers.Topology:
                SetData(SetValues(GetSplatMap(landLayerToPaint, topology), t, dmns), landLayerToPaint, topology);
                SetLayer(landLayer, TerrainTopology.TypeToIndex((int)topologyLayer));
                break;
            case LandLayers.Alpha:
                bool value = (t == 0) ? true : false;
                SetData(SetValues(GetAlphaMap(), value, dmns), landLayerToPaint);
                break;
        }
    }
    /// <summary>Paints the splats wherever the water is above 500 and is above the terrain.</summary>
    /// <param name="landLayerToPaint">The LandLayer to paint. (Ground, Biome, Alpha, Topology)</param>
    /// <param name="aboveTerrain">Check if the watermap is above the terrain before painting.</param>
    /// <param name="t">The texture to paint.</param>
    /// <param name="topology">The Topology layer, if selected.</param>
    public static void PaintRiver(LandLayers landLayerToPaint, bool aboveTerrain, int t, int topology = 0)
    {
        switch (landLayerToPaint)
        {
            case LandLayers.Ground:
            case LandLayers.Biome:
            case LandLayers.Topology:
                SetData(SetRiver(GetSplatMap(landLayerToPaint, topology), GetHeights(), GetWaterHeights(), aboveTerrain, t), landLayerToPaint, topology);
                SetLayer(landLayer, TerrainTopology.TypeToIndex((int)topologyLayer));
                break;
            case LandLayers.Alpha:
                bool value = (t == 0) ? true : false;
                SetData(SetRiver(GetAlphaMap(), GetHeights(), GetWaterHeights(), aboveTerrain, value), landLayerToPaint);
                break;
            
        }
    }
    #endregion
    /// <summary>Changes all the prefab categories to a the RustEdit custom prefab format. Hide's prefabs from appearing in RustEdit.</summary>
    public static void HidePrefabsInRustEdit()
    {
        PrefabDataHolder[] prefabDataHolders = GameObject.FindObjectsOfType<PrefabDataHolder>();
        ProgressBar("Hide Prefabs in RustEdit", "Hiding prefabs: ", 0f);
        int prefabsHidden = 0;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        for (int i = 0; i < prefabDataHolders.Length; i++)
        {
            progressValue += 1f / prefabDataHolders.Length;
            if (sw.Elapsed.TotalSeconds > 0.1f)
            {
                sw.Restart();
                ProgressBar("Hide Prefabs in RustEdit", "Hiding prefabs: " + i + " / " + prefabDataHolders.Length, progressValue);
            }
            prefabDataHolders[i].prefabData.category = @":\RustEditHiddenPrefab:" + prefabsHidden + ":";
            prefabsHidden++;
        }
        Debug.Log("Hid " + prefabsHidden + " prefabs.");
        ClearProgressBar();
    }
    /// <summary>Breaks down RustEdit custom prefabs back into the individual prefabs.</summary>
    public static void BreakRustEditCustomPrefabs()
    {
        PrefabDataHolder[] prefabDataHolders = GameObject.FindObjectsOfType<PrefabDataHolder>();
        ProgressBar("Break RustEdit Custom Prefabs", "Scanning prefabs", 0f);
        int prefabsBroken = 0;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        for (int i = 0; i < prefabDataHolders.Length; i++)
        {
            progressValue += 1f / prefabDataHolders.Length;
            if (sw.Elapsed.TotalSeconds > 0.1f)
            {
                sw.Restart();
                ProgressBar("Break RustEdit Custom Prefabs", "Scanning prefabs: " + i + " / " + prefabDataHolders.Length, progressValue);
            }
            if (prefabDataHolders[i].prefabData.category.Contains(':'))
            {
                prefabDataHolders[i].prefabData.category = "Decor";
                prefabsBroken++;
            }
        }
        Debug.Log("Broke down " + prefabsBroken + " prefabs.");
        ClearProgressBar();
    }
    /// <summary>Parents all the RustEdit custom prefabs in the map to parent gameobjects.</summary>
    public static void GroupRustEditCustomPrefabs()
    {
        PrefabDataHolder[] prefabDataHolders = GameObject.FindObjectsOfType<PrefabDataHolder>();
        Transform prefabHierachy = GameObject.FindGameObjectWithTag("Prefabs").transform;
        Dictionary<string, GameObject> prefabParents = new Dictionary<string, GameObject>();
        ProgressBar("Group RustEdit Custom Prefabs", "Scanning prefabs", 0f);
        progressValue = 1f / prefabDataHolders.Length;
        for (int i = 0; i < prefabDataHolders.Length; i++)
        {
            progressValue += 1f / prefabDataHolders.Length;
            ProgressBar("Group RustEdit Custom Prefabs", "Scanning prefabs: " + i + " / " + prefabDataHolders.Length, progressValue);
            if (prefabDataHolders[i].prefabData.category.Contains(':'))
            {
                var categoryFields = prefabDataHolders[i].prefabData.category.Split(':');
                if (!prefabParents.ContainsKey(categoryFields[1]))
                {
                    GameObject customPrefabParent = new GameObject(categoryFields[1]);
                    customPrefabParent.transform.SetParent(prefabHierachy);
                    customPrefabParent.transform.localPosition = prefabDataHolders[i].transform.localPosition;
                    customPrefabParent.AddComponent<CustomPrefabData>();
                    prefabParents.Add(categoryFields[1], customPrefabParent);
                }
                if (prefabParents.TryGetValue(categoryFields[1], out GameObject prefabParent))
                {
                    prefabDataHolders[i].gameObject.transform.SetParent(prefabParent.transform);
                }
            }
        }
        ClearProgressBar();
    }
    /// <summary>Exports information about all the map prefabs to a JSON file.</summary>
    /// <param name="mapPrefabFilePath">The JSON file path and name.</param>
    /// <param name="deletePrefabs">Deletes the prefab after the data is exported.</param>
    public static void ExportMapPrefabs(string mapPrefabFilePath, bool deletePrefabs)
    {
        List<PrefabExport> mapPrefabExports = new List<PrefabExport>();
        PrefabDataHolder[] prefabDataHolders = GameObject.FindObjectsOfType<PrefabDataHolder>();
        ProgressBar("Export Map Prefabs", "Exporting...", 0f);
        progressValue = 1f / prefabDataHolders.Length;
        for (int i = 0; i < prefabDataHolders.Length; i++)
        {
            progressValue += 1f / prefabDataHolders.Length;
            ProgressBar("Export Map Prefabs", "Exporting prefab: " + i + " / " + prefabDataHolders.Length, progressValue);
            mapPrefabExports.Add(new PrefabExport()
            {
                PrefabNumber = i,
                PrefabID = prefabDataHolders[i].prefabData.id,
                PrefabPosition = prefabDataHolders[i].transform.localPosition.ToString(),
                PrefabScale = prefabDataHolders[i].transform.localScale.ToString(),
                PrefabRotation = prefabDataHolders[i].transform.rotation.ToString()
            });
            if (deletePrefabs)
            {
                GameObject.DestroyImmediate(prefabDataHolders[i].gameObject);
            }
        }
        using (StreamWriter streamWriter = new StreamWriter(mapPrefabFilePath, false))
        {
            streamWriter.WriteLine("{");
            foreach (PrefabExport prefabDetail in mapPrefabExports)
            {
                streamWriter.WriteLine("   \"" + prefabDetail.PrefabNumber + "\": \"" + prefabDetail.PrefabID + ":" + prefabDetail.PrefabPosition + ":" + prefabDetail.PrefabScale + ":" + prefabDetail.PrefabRotation + "\",");
            }
            streamWriter.WriteLine("   \"Prefab Count\": " + prefabDataHolders.Length);
            streamWriter.WriteLine("}");
        }
        mapPrefabExports.Clear();
        ClearProgressBar();
        Debug.Log("Exported " + prefabDataHolders.Length + " prefabs.");
    }
    /// <summary>Exports lootcrates to a JSON for use with Oxide.</summary>
    /// <param name="prefabFilePath">The path to save the JSON.</param>
    /// <param name="deletePrefabs">Delete the lootcrates after exporting.</param>
    public static void ExportLootCrates(string prefabFilePath, bool deletePrefabs)
    {
        List<PrefabExport> prefabExports = new List<PrefabExport>();
        PrefabDataHolder[] prefabs = GameObject.FindObjectsOfType<PrefabDataHolder>();
        int lootCrateCount = 0;
        foreach (PrefabDataHolder p in prefabs)
        {
            switch (p.prefabData.id)
            {
                case 1603759333:
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabPath = "assets/bundled/prefabs/radtown/crate_basic.prefab",
                        PrefabPosition = "(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + ")",
                        PrefabRotation = p.transform.rotation.ToString()
                    });
                    if (deletePrefabs == true)
                    {
                        GameObject.DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
                case 3286607235:
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabPath = "assets/bundled/prefabs/radtown/crate_elite.prefab",
                        PrefabPosition = "(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + ")",
                        PrefabRotation = p.transform.rotation.ToString()
                    });
                    if (deletePrefabs == true)
                    {
                        GameObject.DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
                case 1071933290:
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabPath = "assets/bundled/prefabs/radtown/crate_mine.prefab",
                        PrefabPosition = "(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + ")",
                        PrefabRotation = p.transform.rotation.ToString()
                    });
                    if (deletePrefabs == true)
                    {
                        GameObject.DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
                case 2857304752:
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabPath = "assets/bundled/prefabs/radtown/crate_normal.prefab",
                        PrefabPosition = "(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + ")",
                        PrefabRotation = p.transform.rotation.ToString()
                    });
                    if (deletePrefabs == true)
                    {
                        GameObject.DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
                case 1546200557:
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabPath = "assets/bundled/prefabs/radtown/crate_normal_2.prefab",
                        PrefabPosition = "(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + ")",
                        PrefabRotation = p.transform.rotation.ToString()
                    });
                    if (deletePrefabs == true)
                    {
                        GameObject.DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
                case 2066926276:
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabPath = "assets/bundled/prefabs/radtown/crate_normal_2_food.prefab",
                        PrefabPosition = "(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + ")",
                        PrefabRotation = p.transform.rotation.ToString()
                    });
                    if (deletePrefabs == true)
                    {
                        GameObject.DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
                case 1791916628:
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabPath = "assets/bundled/prefabs/radtown/crate_normal_2_medical.prefab",
                        PrefabPosition = "(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + ")",
                        PrefabRotation = p.transform.rotation.ToString()
                    });
                    if (deletePrefabs == true)
                    {
                        GameObject.DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
                case 1892026534:
                    p.transform.Rotate(Vector3.zero, 180f);
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabPath = "assets/bundled/prefabs/radtown/crate_underwater_advanced.prefab",
                        PrefabPosition = "(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + ")",
                        PrefabRotation = p.transform.rotation.ToString()
                    });
                    if (deletePrefabs == true)
                    {
                        GameObject.DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
                case 3852690109:
                    prefabExports.Add(new PrefabExport()
                    {
                        PrefabNumber = lootCrateCount,
                        PrefabPath = "assets/bundled/prefabs/radtown/crate_underwater_basic.prefab",
                        PrefabPosition = "(" + p.transform.localPosition.z + ", " + p.transform.localPosition.y + ", " + p.transform.localPosition.x * -1 + ")",
                        PrefabRotation = p.transform.rotation.ToString()
                    });
                    if (deletePrefabs == true)
                    {
                        GameObject.DestroyImmediate(p.gameObject);
                    }
                    lootCrateCount++;
                    break;
            }
        }
        using (StreamWriter streamWriter = new StreamWriter(prefabFilePath, false))
        {
            streamWriter.WriteLine("{");
            foreach (PrefabExport prefabDetail in prefabExports)
            {
                streamWriter.WriteLine("   \"" + prefabDetail.PrefabNumber + "\": \"" + prefabDetail.PrefabPath + ":" + prefabDetail.PrefabPosition + ":" + prefabDetail.PrefabRotation + "\",");
            }
            streamWriter.WriteLine("   \"Prefab Count\": " + lootCrateCount);
            streamWriter.WriteLine("}");
        }
        prefabExports.Clear();
        Debug.Log("Exported " + lootCrateCount + " lootcrates.");
    }
    /// <summary>Centres the Prefab and Path parent objects.</summary>
    static void CentreSceneObjects(MapInfo terrains)
    {
        GameObject.FindGameObjectWithTag("Prefabs").GetComponent<LockObject>().SetPosition(new Vector3(terrains.size.x / 2, 500, terrains.size.z / 2));
        GameObject.FindGameObjectWithTag("Paths").GetComponent<LockObject>().SetPosition(new Vector3(terrains.size.x / 2, 500, terrains.size.z / 2));
    }
    /// <summary>Loads and sets the Land and Water terrains.</summary>
    public static void LoadTerrains(MapInfo terrains)
    {
        land.terrainData.heightmapResolution = terrains.terrainRes;
        land.terrainData.size = terrains.size;

        water.terrainData.heightmapResolution = terrains.terrainRes;
        water.terrainData.size = terrains.size;

        land.terrainData.SetHeights(0, 0, terrains.land.heights);
        water.terrainData.SetHeights(0, 0, terrains.water.heights);

        land.terrainData.alphamapResolution = terrains.splatRes;
        land.terrainData.baseMapResolution = terrains.splatRes;
        water.terrainData.alphamapResolution = terrains.splatRes;
        water.terrainData.baseMapResolution = terrains.splatRes;
    }
    /// <summary>Loads and sets up the map Prefabs.</summary>
    static void LoadPrefabs(MapInfo terrains, string loadPath = "")
    {
        Transform prefabsParent = GameObject.FindGameObjectWithTag("Prefabs").transform;
        ProgressBar("Loading: ", "Spawning Prefabs ", 0.8f);
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        for (int i = 0; i < terrains.prefabData.Length; i++)
        {
            progressValue += 1f / terrains.prefabData.Length;
            if (sw.Elapsed.TotalSeconds > 0.1f)
            {
                sw.Restart();
                ProgressBar("Loading: " + loadPath, "Spawning Prefabs: " + i + " / " + terrains.prefabData.Length, progressValue);
            }
            Spawn(PrefabManager.Load(terrains.prefabData[i].id), terrains.prefabData[i], prefabsParent);
        }
    }
    /// <summary>Loads and sets up the map Paths.</summary>
    static void LoadPaths(MapInfo terrains, string loadPath = "")
    {
        var terrainPosition = 0.5f * terrains.size;
        Transform pathsParent = GameObject.FindGameObjectWithTag("Paths").transform;
        GameObject pathObj = Resources.Load<GameObject>("Paths/Path");
        GameObject pathNodeObj = Resources.Load<GameObject>("Paths/PathNode");
        ProgressBar("Loading:" + loadPath, "Spawning Paths ", 0.99f);
        for (int i = 0; i < terrains.pathData.Length; i++)
        {
            Vector3 averageLocation = Vector3.zero;
            for (int j = 0; j < terrains.pathData[i].nodes.Length; j++)
            {
                averageLocation += terrains.pathData[i].nodes[j];
            }
            averageLocation /= terrains.pathData[i].nodes.Length;
            GameObject newObject = GameObject.Instantiate(pathObj, averageLocation + terrainPosition, Quaternion.identity, pathsParent);

            List<GameObject> pathNodes = new List<GameObject>();
            for (int j = 0; j < terrains.pathData[i].nodes.Length; j++)
            {
                GameObject newNode = GameObject.Instantiate(pathNodeObj, newObject.transform);
                newNode.transform.position = terrains.pathData[i].nodes[j] + terrainPosition;
                pathNodes.Add(newNode);
            }
            newObject.GetComponent<PathDataHolder>().pathData = terrains.pathData[i];
        }
    }
    static void LoadSplatMaps(MapInfo terrains)
    {
        TopologyData.InitMesh(terrains.topology);
        SetData(terrains.splatMap, LandLayers.Ground);
        SetData(terrains.biomeMap, LandLayers.Biome);
        SetData(terrains.alphaMap, LandLayers.Alpha);
        Parallel.For(0, TerrainTopology.COUNT, i =>
        {
            SetData(TopologyData.GetTopologyLayer(TerrainTopology.IndexToType(i)), LandLayers.Topology, i);
        });
    }
    /// <summary>Loads and sets up the map.</summary>
    static void LoadMapInfo(MapInfo terrains, string loadPath = "")
    {
        ProgressBar("Loading: " + loadPath, "Preparing Map", 0.25f);
        RemoveMapObjects();
        CentreSceneView(SceneView.lastActiveSceneView);
        SetCullingDistances(SceneView.lastActiveSceneView.camera, MapEditorSettings.prefabRenderDistance, MapEditorSettings.pathRenderDistance);
        CentreSceneObjects(terrains);
        LoadTerrains(terrains);
        LoadSplatMaps(terrains);
        LoadPrefabs(terrains, loadPath);
        LoadPaths(terrains, loadPath);
        ProgressBar("Loading: " + loadPath, "Setting Layers", 0.75f);
        SetLayer(LandLayers.Alpha); // Sets the terrain holes.
        SetLayer(LandLayers.Ground, TerrainTopology.TypeToIndex((int)topologyLayer)); // Sets the alphamaps to Ground.
        ClearProgressBar();
    }
    /// <summary>Loads a WorldSerialization and calls LoadMapInfo.</summary>
    /// <param name="loadPath">The path of the map, used by the progress bars.</param>
    public static void Load(WorldSerialization world, string loadPath = "")
    {
        ProgressBar("Loading: " + loadPath, "Loading Map", 0.1f);
        LoadMapInfo(WorldToTerrain(world), loadPath);
    }
    /// <summary>Saves the map.</summary>
    /// <param name="path">The path to save to.</param>
    public static void Save(string path)
    {
        SaveLayer(TerrainTopology.TypeToIndex((int)topologyLayer));
        ProgressBar("Saving Map: " + path, "Saving Prefabs ", 0.4f);
        WorldSerialization world = WorldConverter.TerrainToWorld(land, water);
        ProgressBar("Saving Map: " + path, "Saving to disk ", 0.8f);
        world.Save(path);
        ClearProgressBar();
    }
    /// <summary>Creates a new flat terrain.</summary>
    /// <param name="size">The size of the terrain.</param>
    public static void CreateMap(int size)
    {
        LoadMapInfo(EmptyMap(size), "New Map");
        PaintLayer(LandLayers.Ground, 4);
        PaintLayer(LandLayers.Biome, 1);
        SetHeightmap(503f, Selections.Terrains.Land);
        SetHeightmap(500f, Selections.Terrains.Water);
    }
    public static List<string> generationPresetList = new List<string>();
    public static Dictionary<string, UnityEngine.Object> nodePresetLookup = new Dictionary<string, UnityEngine.Object>();
    /// <summary>Refreshes and adds the new NodePresets in the generationPresetList.</summary>
    public static void RefreshPresetsList()
    {
        var list = AssetDatabase.FindAssets("t:" + NodeAsset.nodeAssetName);
        generationPresetList.Clear();
        nodePresetLookup.Clear();
        foreach (var item in list)
        {
            var itemName = AssetDatabase.GUIDToAssetPath(item).Split('/');
            var itemNameSplit = itemName[itemName.Length - 1].Replace(".asset", "");
            generationPresetList.Add(itemNameSplit);
            nodePresetLookup.Add(itemNameSplit, AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(item), typeof(NodePreset)));
        }
    }
}