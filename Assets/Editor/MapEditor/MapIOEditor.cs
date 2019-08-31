﻿using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using Rotorz.ReorderableList;

public class MapIOEditor : EditorWindow
{
    string editorVersion = "v1.9.5-prerelease";
    string[] landLayers = { "Ground", "Biome", "Alpha", "Topology" };
    string loadFile = "", saveFile = "", mapName = "", prefabSaveFile = "", mapPrefabSaveFile = "";
    int mapSize = 1000, mainMenuOptions = 0, toolsOptions = 0, mapToolsOptions = 0, heightMapOptions = 0, conditionalPaintOptions = 0, prefabOptions = 0;
    float heightToSet = 450f, offset = 0f;
    bool[] sides = new bool[4]; 
    bool checkHeight = true, setWaterMap = false;
    bool allLayers = false, ground = false, biome = false, alpha = false, topology = false, heightmap = false, prefabs = false, paths = false;
    float heightLow = 0f, heightHigh = 500f, slopeLow = 40f, slopeHigh = 60f;
    float slopeMinBlendLow = 25f, slopeMaxBlendLow = 40f, slopeMinBlendHigh = 60f, slopeMaxBlendHigh = 75f;
    float heightMinBlendLow = 0f, heightMaxBlendLow = 500f, heightMinBlendHigh = 500f, heightMaxBlendHigh = 1000f;
    float normaliseLow = 450f, normaliseHigh = 1000f;
    int z1 = 0, z2 = 0, x1 = 0, x2 = 0;
    bool blendSlopes = false, blendHeights = false, aboveTerrain = false;
    int textureFrom, textureToPaint, landLayerFrom, landLayerToPaint, topologyFrom, topologyToPaint;
    int layerConditionalInt, texture = 0, topologyTexture = 0, alphaTexture;
    bool deletePrefabs = false;
    bool checkHeightCndtl = false, checkSlopeCndtl = false, checkAlpha = false;
    float slopeLowCndtl = 45f, slopeHighCndtl = 60f;
    float heightLowCndtl = 500f, heightHighCndtl = 600f;
    bool autoUpdate = false;
    Vector2 scrollPos = new Vector2(0, 0), presetScrollPos = new Vector2(0, 0);

    float terraceErodeFeatureSize = 150f, terraceErodeInteriorCornerWeight = 1f;
    float blurDirection = 0f, filterStrength = 1f;

    int[] values = { 0, 1 };
    string[] activeTextureAlpha = { "Visible", "Invisible" };
    string[] activeTextureTopo = { "Active", "Inactive" };

    [MenuItem("Rust Map Editor/Main Menu", false, 0)]
    static void Initialize()
    {
        MapIOEditor window = (MapIOEditor)EditorWindow.GetWindow(typeof(MapIOEditor), false, "Rust Map Editor");
    }
    public void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
        GUIContent[] mainMenu = new GUIContent[4];
        mainMenu[0] = new GUIContent("File");
        mainMenu[1] = new GUIContent("Prefabs");
        mainMenu[2] = new GUIContent("Layers");
        mainMenu[3] = new GUIContent("Advanced");
        mainMenuOptions = GUILayout.Toolbar(mainMenuOptions, mainMenu);

        #region Menu
        switch (mainMenuOptions)
        {
            #region File
            case 0:
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                if (GUILayout.Button(new GUIContent("Load", "Opens a file viewer to find and open a Rust .map file."), EditorStyles.toolbarButton, GUILayout.MaxWidth(45)))
                {
                    loadFile = UnityEditor.EditorUtility.OpenFilePanel("Import Map File", loadFile, "map");
                    if (loadFile == "")
                    {
                        return;
                    }
                    var world = new WorldSerializationSDK();
                    MapIO.ProgressBar("Loading: " + loadFile, "Loading Land Heightmap Data ", 0.1f);
                    world.Load(loadFile);
                    MapIO.loadPath = loadFile;
                    MapIO.ProgressBar("Loading: " + loadFile, "Loading Land Heightmap Data ", 0.2f);
                    MapIO.Load(world);
                }
                if (GUILayout.Button(new GUIContent("Save", "Opens a file viewer to find and save a Rust .map file."), EditorStyles.toolbarButton, GUILayout.MaxWidth(45)))
                {
                    saveFile = UnityEditor.EditorUtility.SaveFilePanel("Export Map File", saveFile, mapName, "map");
                    if (saveFile == "")
                    {
                        return;
                    }
                    Debug.Log("Exported map " + saveFile);
                    MapIO.savePath = saveFile;
                    prefabSaveFile = saveFile;
                    MapIO.ProgressBar("Saving Map: " + saveFile, "Saving Heightmap ", 0.1f);
                    MapIO.Save(saveFile);
                }
                if (GUILayout.Button(new GUIContent("New", "Creates a new map " + mapSize.ToString() + " metres in size."), EditorStyles.toolbarButton, GUILayout.MaxWidth(45)))
                {
                    int newMap = EditorUtility.DisplayDialogComplex("Warning", "Creating a new map will remove any unsaved changes to your map.", "Create New Map", "Exit", "Save and Create New Map");
                    switch (newMap)
                    {
                        case 0:
                            MapIO.loadPath = "New Map";
                            MapIO.CreateNewMap(mapSize);
                            break;
                        case 1:
                            // User cancelled
                            break;
                        case 2:
                            saveFile = UnityEditor.EditorUtility.SaveFilePanel("Export Map File", saveFile, mapName, "map");
                            if (saveFile == "")
                            {
                                EditorUtility.DisplayDialog("Error", "Save Path is Empty", "Ok");
                                return;
                            }
                            Debug.Log("Exported map " + saveFile);
                            MapIO.Save(saveFile);
                            MapIO.loadPath = "New Map";
                            MapIO.CreateNewMap(mapSize);
                            break;
                    }
                }
                GUILayout.Label(new GUIContent("Size", "The size of the Rust Map to create upon new map. (1000-6000)"), GUILayout.MaxWidth(30));
                mapSize = Mathf.Clamp(EditorGUILayout.IntField(mapSize, GUILayout.MaxWidth(45)), 1000, 6000);
                EditorGUILayout.EndHorizontal();

                GUILayout.Label("Editor Info", EditorStyles.boldLabel, GUILayout.MaxWidth(75));
                GUILayout.Label("OS: " + SystemInfo.operatingSystem);
                GUILayout.Label("Unity Version: " + Application.unityVersion);
                GUILayout.Label("Editor Version: " + editorVersion);

                GUILayout.Label(new GUIContent("Links"), EditorStyles.boldLabel, GUILayout.MaxWidth(60));

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                if (GUILayout.Button(new GUIContent("Report Bug", "Opens up the editor bug report in GitHub."), EditorStyles.toolbarButton, GUILayout.MaxWidth(75)))
                {
                    Application.OpenURL("https://github.com/RustMapMaking/Editor/issues/new?assignees=Adsito&labels=bug&template=bug-report.md&title=%5BBUG%5D+Bug+name+goes+here");
                }
                if (GUILayout.Button(new GUIContent("Request Feature", "Opens up the editor feature request in GitHub."), EditorStyles.toolbarButton, GUILayout.MaxWidth(105)))
                {
                    Application.OpenURL("https://github.com/RustMapMaking/Editor/issues/new?assignees=Adsito&labels=enhancement&template=feature-request.md&title=%5BREQUEST%5D+Request+name+goes+here");
                }
                if (GUILayout.Button(new GUIContent("RoadMap", "Opens up the editor roadmap in GitHub."), EditorStyles.toolbarButton, GUILayout.MaxWidth(65)))
                {
                    Application.OpenURL("https://github.com/RustMapMaking/Editor/projects/1");
                }
                if (GUILayout.Button(new GUIContent("Wiki", "Opens up the editor wiki in GitHub."), EditorStyles.toolbarButton, GUILayout.MaxWidth(65)))
                {
                    Application.OpenURL("https://github.com/RustMapMaking/Editor/wiki");
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Label(new GUIContent("Settings"), EditorStyles.boldLabel, GUILayout.MaxWidth(60));

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                if (GUILayout.Button(new GUIContent("Save Changes", "Sets and saves the current settings."), EditorStyles.toolbarButton, GUILayout.MaxWidth(82)))
                {
                    Application.OpenURL("https://github.com/RustMapMaking/Editor/issues/new?assignees=Adsito&labels=bug&template=bug-report.md&title=%5BBUG%5D+Bug+name+goes+here");
                }
                if (GUILayout.Button(new GUIContent("Discard", "Discards the changes to the settings."), EditorStyles.toolbarButton, GUILayout.MaxWidth(82)))
                {
                    Application.OpenURL("https://github.com/RustMapMaking/Editor/issues/new?assignees=Adsito&labels=bug&template=bug-report.md&title=%5BBUG%5D+Bug+name+goes+here");
                }
                if (GUILayout.Button(new GUIContent("Default", "Sets the settings back to the default."), EditorStyles.toolbarButton, GUILayout.MaxWidth(82)))
                {
                    Application.OpenURL("https://github.com/RustMapMaking/Editor/issues/new?assignees=Adsito&labels=bug&template=bug-report.md&title=%5BBUG%5D+Bug+name+goes+here");
                }
                EditorGUILayout.EndHorizontal();

                break;
            #endregion
           
            #region Prefabs
            case 1:
                GUIContent[] prefabsOptionsMenu = new GUIContent[2];
                prefabsOptionsMenu[0] = new GUIContent("Asset Bundle");
                prefabsOptionsMenu[1] = new GUIContent("Prefab Tools");
                //prefabsOptionsMenu[1] = new GUIContent("Spawn Prefabs");
                prefabOptions = GUILayout.Toolbar(prefabOptions, prefabsOptionsMenu);

                switch (prefabOptions)
                {
                    case 0:
                        EditorGUILayout.LabelField("THIS IS AN EARLY PREVIEW. FULL OF BUGS ATM", EditorStyles.boldLabel);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("Load", "Loads all the prefabs from the Rust Asset Bundle for use in the editor. Prefabs paths to be loaded can be changed in " +
                            "AssetList.txt in the root directory"), GUILayout.MaxWidth(100)))
                        {
                            if (!MapIO.bundleFile.Contains(@"Bundles/Bundles"))
                            {
                                MapIO.bundleFile = MapIO.bundleFile = EditorUtility.OpenFilePanel("Select Bundle File", MapIO.bundleFile, "");
                                if (MapIO.bundleFile == "")
                                {
                                    return;
                                }
                                if (!MapIO.bundleFile.Contains(@"steamapps/common/Rust/Bundles/Bundles"))
                                {
                                    EditorUtility.DisplayDialog("ERROR: Bundle File Invalid", @"Bundle file path invalid. It should be located within steamapps\common\Rust\Bundles", "Ok");
                                    return;
                                }
                            }
                            MapIO.StartPrefabLookup();
                        }
                        if (GUILayout.Button(new GUIContent("Unload", "Unloads all the prefabs from the Rust Asset Bundle."), GUILayout.MaxWidth(100)))
                        {
                            if (MapIO.GetPrefabLookUp() != null)
                            {
                                MapIO.GetPrefabLookUp().Dispose();
                                MapIO.SetPrefabLookup(null);
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("ERROR: Can't unload prefabs", "No prefabs loaded.", "Ok");
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        MapIO.bundleFile = GUILayout.TextArea(MapIO.bundleFile);
                        break;
                    case 2:
                        if (GUILayout.Button(new GUIContent("Prefab List", "Opens a window to drag and drop prefabs onto the map."), GUILayout.MaxWidth(125)))
                        {
                            PrefabHierachyEditor.ShowWindow();
                        }
                        break;
                    case 1:
                        if (GUILayout.Button(new GUIContent("Remove Broken Prefabs", "Removes any prefabs known to prevent maps from loading. Use this is you are having" +
                                    " errors loading a map on a server.")))
                        {
                            MapIO.RemoveBrokenPrefabs();
                        }
                        deletePrefabs = EditorGUILayout.ToggleLeft(new GUIContent("Delete on Export.", "Deletes the prefabs after exporting them."), deletePrefabs, GUILayout.MaxWidth(300));
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("Export LootCrates", "Exports all lootcrates that don't yet respawn in Rust to a JSON for use with the LootCrateRespawn plugin." +
                            "If you don't delete them after export they will duplicate on first map load.")))
                        {
                            prefabSaveFile = EditorUtility.SaveFilePanel("Export LootCrates", prefabSaveFile, "LootCrateData", "json");
                            if (prefabSaveFile == "")
                            {
                                return;
                            }
                            MapIO.ExportLootCrates(prefabSaveFile, deletePrefabs);
                        }
                        if (GUILayout.Button(new GUIContent("Export Map Prefabs", "Exports all map prefabs to plugin data.")))
                        {
                            mapPrefabSaveFile = EditorUtility.SaveFilePanel("Export Map Prefabs", prefabSaveFile, "MapData", "json");
                            if (mapPrefabSaveFile == "")
                            {
                                return;
                            }
                            MapIO.ExportMapPrefabs(mapPrefabSaveFile, deletePrefabs);
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("Hide Prefabs in RustEdit", "Changes all the prefab categories to a semi-colon. Hides all of the prefabs from " +
                            "appearing in RustEdit. Use the break RustEdit Custom Prefabs button to undo.")))
                        {
                            MapIO.HidePrefabsInRustEdit();
                        }
                        if (GUILayout.Button(new GUIContent("Break RustEdit Custom Prefabs", "Breaks down all custom prefabs saved in the map file.")))
                        {
                            MapIO.BreakRustEditCustomPrefabs();
                        }
                        EditorGUILayout.EndHorizontal();
                        if (GUILayout.Button(new GUIContent("Group RustEdit Custom Prefabs", "Groups all custom prefabs saved in the map file.")))
                        {
                            MapIO.GroupRustEditCustomPrefabs();
                        }
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("Delete All Map Prefabs", "Removes all the prefabs from the map.")))
                        {
                            MapIO.RemoveMapObjects(true, false);
                        }
                        if (GUILayout.Button(new GUIContent("Delete All Map Paths", "Removes all the paths from the map.")))
                        {
                            MapIO.RemoveMapObjects(false, true);
                        }
                        EditorGUILayout.EndHorizontal();
                        break;
                    default:
                        prefabOptions = 0;
                        break;
                }
                break;
            #endregion
            #region Tools
            case 2:
                GUIContent[] toolsOptionsMenu = new GUIContent[3];
                toolsOptionsMenu[0] = new GUIContent("Map Tools");
                toolsOptionsMenu[1] = new GUIContent("Layer Tools");
                toolsOptionsMenu[2] = new GUIContent("Generation");
                toolsOptions = GUILayout.Toolbar(toolsOptions, toolsOptionsMenu);

                switch (toolsOptions)
                {
                    #region Map Tools
                    case 0:
                        GUIContent[] mapToolsMenu = new GUIContent[4];
                        mapToolsMenu[0] = new GUIContent("Transform");
                        mapToolsMenu[1] = new GUIContent("HeightMap");
                        mapToolsMenu[2] = new GUIContent("Textures");
                        mapToolsMenu[3] = new GUIContent("Misc");
                        mapToolsOptions = GUILayout.Toolbar(mapToolsOptions, mapToolsMenu);

                        switch (mapToolsOptions)
                        {
                            #region Rotate Map
                            case 0:
                                GUILayout.Label("Rotate Map", EditorStyles.boldLabel);
                                GUILayout.Label("Layers to Rotate", EditorStyles.boldLabel);
                                EditorGUILayout.BeginHorizontal();
                                allLayers = EditorGUILayout.ToggleLeft("Rotate All", allLayers, GUILayout.MaxWidth(75));

                                if (GUILayout.Button("Rotate 90°", GUILayout.MaxWidth(90))) // Calls every rotate function from MapIO. Rotates 90 degrees.
                                {
                                    if (heightmap == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Heightmap ", 0.05f);
                                        MapIO.RotateHeightmap(true);
                                    }
                                    if (paths == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Paths ", 0.075f);
                                        MapIO.RotatePaths(true);
                                    }
                                    if (prefabs == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Prefabs ", 0.1f);
                                        MapIO.RotatePrefabs(true);
                                    }
                                    if (ground == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Ground Textures ", 0.15f);
                                        MapIO.RotateLayer("ground", true);
                                    }
                                    if (biome == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Biome Textures ", 0.2f);
                                        MapIO.RotateLayer("biome", true);
                                    }
                                    if (alpha == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Alpha Textures ", 0.25f);
                                        MapIO.RotateLayer("alpha", true);
                                    }
                                    if (topology == true)
                                    {
                                        MapIO.RotateAllTopologymap(true);
                                    };
                                    EditorUtility.DisplayProgressBar("Rotating Map", "Finished ", 1f);
                                    EditorUtility.ClearProgressBar();
                                }
                                if (GUILayout.Button("Rotate 270°", GUILayout.MaxWidth(90))) // Calls every rotate function from MapIO. Rotates 270 degrees.
                                {
                                    if (heightmap == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Heightmap ", 0.05f);
                                        MapIO.RotateHeightmap(false);
                                    }
                                    if (paths == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Paths ", 0.075f);
                                        MapIO.RotatePaths(false);
                                    }
                                    if (prefabs == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Prefabs ", 0.1f);
                                        MapIO.RotatePrefabs(false);
                                    }
                                    if (ground == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Ground Textures ", 0.15f);
                                        MapIO.RotateLayer("ground", false);
                                    }
                                    if (biome == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Biome Textures ", 0.2f);
                                        MapIO.RotateLayer("biome", false);
                                    }
                                    if (alpha == true)
                                    {
                                        EditorUtility.DisplayProgressBar("Rotating Map", "Rotating Alpha Textures ", 0.25f);
                                        MapIO.RotateLayer("alpha", false);
                                    }
                                    if (topology == true)
                                    {
                                        MapIO.RotateAllTopologymap(false);
                                    };
                                    EditorUtility.DisplayProgressBar("Rotating Map", "Finished ", 1f);
                                    EditorUtility.ClearProgressBar();
                                }
                                EditorGUILayout.EndHorizontal();
                                if (allLayers == true)
                                {
                                    ground = true; biome = true; alpha = true; topology = true; heightmap = true; paths = true; prefabs = true;
                                }

                                EditorGUILayout.BeginHorizontal();
                                ground = EditorGUILayout.ToggleLeft("Ground", ground, GUILayout.MaxWidth(60));
                                biome = EditorGUILayout.ToggleLeft("Biome", biome, GUILayout.MaxWidth(60));
                                alpha = EditorGUILayout.ToggleLeft("Alpha", alpha, GUILayout.MaxWidth(60));
                                topology = EditorGUILayout.ToggleLeft("Topology", topology, GUILayout.MaxWidth(75));
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                                paths = EditorGUILayout.ToggleLeft("Paths", paths, GUILayout.MaxWidth(60));
                                prefabs = EditorGUILayout.ToggleLeft("Prefabs", prefabs, GUILayout.MaxWidth(60));
                                heightmap = EditorGUILayout.ToggleLeft("HeightMap", heightmap, GUILayout.MaxWidth(80));
                                EditorGUILayout.EndHorizontal();
                                break;
                            #endregion
                            #region HeightMap
                            case 1:
                                GUIContent[] heightMapMenu = new GUIContent[2];
                                heightMapMenu[0] = new GUIContent("Heights");
                                heightMapMenu[1] = new GUIContent("Filters");
                                heightMapOptions = GUILayout.Toolbar(heightMapOptions, heightMapMenu);

                                switch (heightMapOptions)
                                {
                                    case 0:
                                        GUILayout.Label("Heightmap Offset (Increase or Decrease)", EditorStyles.boldLabel);
                                        EditorGUILayout.BeginHorizontal();
                                        offset = EditorGUILayout.FloatField(offset, GUILayout.MaxWidth(40));
                                        checkHeight = EditorGUILayout.ToggleLeft(new GUIContent("Prevent Flattening", "Prevents the flattening effect if you raise or lower the heightmap" +
                                            " by too large a value."), checkHeight, GUILayout.MaxWidth(125));
                                        setWaterMap = EditorGUILayout.ToggleLeft(new GUIContent("Water Heightmap", "If toggled it will raise or lower the water heightmap as well as the " +
                                            "land heightmap."), setWaterMap, GUILayout.MaxWidth(125));
                                        EditorGUILayout.EndHorizontal();
                                        if (GUILayout.Button(new GUIContent("Offset Heightmap", "Raises or lowers the height of the entire heightmap by " + offset.ToString() + " metres. " +
                                            "A positive offset will raise the heightmap, a negative offset will lower the heightmap.")))
                                        {
                                            MapIO.OffsetHeightmap(offset, checkHeight, setWaterMap);
                                        }
                                        GUILayout.Label("Heightmap Minimum/Maximum Height", EditorStyles.boldLabel);
                                        EditorGUILayout.BeginHorizontal();
                                        heightToSet = EditorGUILayout.FloatField(heightToSet, GUILayout.MaxWidth(40));
                                        if (GUILayout.Button(new GUIContent("Set Minimum Height", "Raises any of the land below " + heightToSet.ToString() + " metres to " + heightToSet.ToString() +
                                            " metres."), GUILayout.MaxWidth(130)))
                                        {
                                            MapIO.SetMinimumHeight(heightToSet);
                                        }
                                        if (GUILayout.Button(new GUIContent("Set Maximum Height", "Lowers any of the land above " + heightToSet.ToString() + " metres to " + heightToSet.ToString() +
                                            " metres."), GUILayout.MaxWidth(130)))
                                        {
                                            MapIO.SetMaximumHeight(heightToSet);
                                        }
                                        EditorGUILayout.EndHorizontal();

                                        GUILayout.Label("Edge of Map Height", EditorStyles.boldLabel);
                                        EditorGUILayout.BeginHorizontal();
                                        sides[0] = EditorGUILayout.ToggleLeft("Top ", sides[0], GUILayout.MaxWidth(60));
                                        sides[3] = EditorGUILayout.ToggleLeft("Left ", sides[3], GUILayout.MaxWidth(60));
                                        sides[2] = EditorGUILayout.ToggleLeft("Bottom ", sides[2], GUILayout.MaxWidth(60));
                                        sides[1] = EditorGUILayout.ToggleLeft("Right ", sides[1], GUILayout.MaxWidth(60));
                                        EditorGUILayout.EndHorizontal();

                                        if (GUILayout.Button(new GUIContent("Set Edge Height", "Sets the very edge of the map to " + heightToSet.ToString() + " metres on any of the sides selected.")))
                                        {
                                            MapIO.SetEdgePixel(heightToSet, sides);
                                        }

                                        break;
                                    case 1:
                                        GUILayout.Label("Flip, Invert and Scale", EditorStyles.boldLabel);
                                        EditorGUILayout.BeginHorizontal();
                                        //EditorGUILayout.LabelField("Scale", GUILayout.MaxWidth(60));
                                        //mapScale = EditorGUILayout.Slider(mapScale, 0.01f, 10f);
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal();
                                        /*
                                        if (GUILayout.Button(new GUIContent("Rescale", "Scales the heightmap by " + mapScale.ToString() + " %.")))
                                        {
                                            script.scaleHeightmap(mapScale);
                                        }*/
                                        if (GUILayout.Button(new GUIContent("Invert", "Inverts the heightmap in on itself.")))
                                        {
                                            MapIO.InvertHeightmap();
                                        }
                                        EditorGUILayout.EndHorizontal();
                                        GUILayout.Label(new GUIContent("Normalise", "Moves the heightmap heights to between the two heights."), EditorStyles.boldLabel);
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField(new GUIContent("Low", "The lowest point on the map after being normalised."), GUILayout.MaxWidth(40));
                                        EditorGUI.BeginChangeCheck();
                                        normaliseLow = EditorGUILayout.Slider(normaliseLow, 0f, 1000f);
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField(new GUIContent("High", "The highest point on the map after being normalised."), GUILayout.MaxWidth(40));
                                        normaliseHigh = EditorGUILayout.Slider(normaliseHigh, 0f, 1000f);
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal();
                                        if (EditorGUI.EndChangeCheck() && autoUpdate == true)
                                        {
                                            MapIO.NormaliseHeightmap(normaliseLow / 1000f, normaliseHigh / 1000f);
                                        }
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal();
                                        if (GUILayout.Button(new GUIContent("Normalise", "Normalises the heightmap between these heights.")))
                                        {
                                            MapIO.NormaliseHeightmap(normaliseLow / 1000f, normaliseHigh / 1000f);
                                        }
                                        autoUpdate = EditorGUILayout.ToggleLeft(new GUIContent("Auto Update", "Automatically applies the changes to the heightmap on value change."), autoUpdate);
                                        EditorGUILayout.EndHorizontal();
                                        GUILayout.Label(new GUIContent("Smooth", "Smooth the entire terrain."), EditorStyles.boldLabel);
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField(new GUIContent("Strength", "The strength of the smoothing operation."), GUILayout.MaxWidth(85));
                                        filterStrength = EditorGUILayout.Slider(filterStrength, 0f, 1f);
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField(new GUIContent("Blur Direction", "The direction the terrain should blur towards. Negative is down, " +
                                            "positive is up."), GUILayout.MaxWidth(85));
                                        blurDirection = EditorGUILayout.Slider(blurDirection, -1f, 1f);
                                        EditorGUILayout.EndHorizontal();
                                        if (GUILayout.Button(new GUIContent("Smooth Map", "Smoothes the heightmap.")))
                                        {
                                            MapIO.SmoothHeightmap(filterStrength, blurDirection);
                                        }
                                        GUILayout.Label(new GUIContent("Terrace", "Terrace the entire terrain."), EditorStyles.boldLabel);
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField(new GUIContent("Feature Size", "The higher the value the more terrace levels generated."), GUILayout.MaxWidth(85));
                                        terraceErodeFeatureSize = EditorGUILayout.Slider(terraceErodeFeatureSize, 2f, 1000f);
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField(new GUIContent("Corner Weight", "The strength of the corners of the terrace."), GUILayout.MaxWidth(85));
                                        terraceErodeInteriorCornerWeight = EditorGUILayout.Slider(terraceErodeInteriorCornerWeight, 0f, 1f);
                                        EditorGUILayout.EndHorizontal();
                                        if (GUILayout.Button(new GUIContent("Terrace Map", "Terraces the heightmap.")))
                                        {
                                            MapIO.TerraceErodeHeightmap(terraceErodeFeatureSize, terraceErodeInteriorCornerWeight);
                                        }
                                        break;
                                }
                                break;
                            #endregion
                            #region Textures
                            case 2:
                                GUILayout.Label("Copy Textures", EditorStyles.boldLabel);
                                landLayerFrom = EditorGUILayout.Popup("Layer:", landLayerFrom, landLayers);
                                switch (landLayerFrom) // Get texture list from the currently selected landLayer.
                                {
                                    case 0:
                                        MapIO.groundLayerFrom = (TerrainSplatSDK.Enum)EditorGUILayout.EnumPopup("Texture To Copy:", MapIO.groundLayerFrom);
                                        textureFrom = TerrainSplatSDK.TypeToIndex((int)MapIO.groundLayerFrom);
                                        break;
                                    case 1:
                                        MapIO.biomeLayerFrom = (TerrainBiomeSDK.Enum)EditorGUILayout.EnumPopup("Texture To Copy:", MapIO.biomeLayerFrom);
                                        textureFrom = TerrainBiomeSDK.TypeToIndex((int)MapIO.biomeLayerFrom);
                                        break;
                                    case 2:
                                        textureFrom = EditorGUILayout.IntPopup("Texture To Copy:", textureFrom, activeTextureAlpha, values);
                                        break;
                                    case 3:
                                        MapIO.topologyLayerFrom = (TerrainTopologySDK.Enum)EditorGUILayout.EnumPopup("Topology To Copy:", MapIO.topologyLayerFrom);
                                        textureFrom = EditorGUILayout.IntPopup("Texture To Copy:", textureFrom, activeTextureTopo, values);
                                        break;
                                }
                                landLayerToPaint = EditorGUILayout.Popup("Layer:", landLayerToPaint, landLayers);
                                switch (landLayerToPaint) // Get texture list from the currently selected landLayer.
                                {
                                    default:
                                        Debug.Log("Layer doesn't exist");
                                        break;
                                    case 0:
                                        MapIO.groundLayerToPaint = (TerrainSplatSDK.Enum)EditorGUILayout.EnumPopup("Texture To Paint:", MapIO.groundLayerToPaint);
                                        textureToPaint = TerrainSplatSDK.TypeToIndex((int)MapIO.groundLayerToPaint);
                                        break;
                                    case 1:
                                        MapIO.biomeLayerToPaint = (TerrainBiomeSDK.Enum)EditorGUILayout.EnumPopup("Texture To Paint:", MapIO.biomeLayerToPaint);
                                        textureToPaint = TerrainBiomeSDK.TypeToIndex((int)MapIO.biomeLayerToPaint);
                                        break;
                                    case 2:
                                        textureToPaint = EditorGUILayout.IntPopup("Texture To Paint:", textureToPaint, activeTextureAlpha, values);
                                        break;
                                    case 3:
                                        MapIO.topologyLayerToPaint = (TerrainTopologySDK.Enum)EditorGUILayout.EnumPopup("Topology To Paint:", MapIO.topologyLayerToPaint);
                                        textureToPaint = EditorGUILayout.IntPopup("Texture To Paint:", textureToPaint, activeTextureTopo, values);
                                        break;
                                }
                                if (GUILayout.Button(new GUIContent("Copy textures to new layer", "Copies the Texture from the " + landLayers[landLayerFrom] + " layer and " +
                                    "paints it on the " + landLayers[landLayerToPaint] + " layer.")))
                                {
                                    MapIO.CopyTexture(landLayers[landLayerFrom], landLayers[landLayerToPaint], textureFrom, textureToPaint, TerrainTopologySDK.TypeToIndex((int)MapIO.topologyLayerFrom), TerrainTopologySDK.TypeToIndex((int)MapIO.topologyLayerToPaint));
                                }
                                GUILayout.Label("Conditional Paint", EditorStyles.boldLabel);

                                GUIContent[] conditionalPaintMenu = new GUIContent[5];
                                conditionalPaintMenu[0] = new GUIContent("Ground");
                                conditionalPaintMenu[1] = new GUIContent("Biome");
                                conditionalPaintMenu[2] = new GUIContent("Alpha");
                                conditionalPaintMenu[3] = new GUIContent("Topology");
                                conditionalPaintMenu[4] = new GUIContent("Terrain");
                                conditionalPaintOptions = GUILayout.Toolbar(conditionalPaintOptions, conditionalPaintMenu);

                                switch (conditionalPaintOptions)
                                {
                                    case 0: // Ground
                                        GUILayout.Label("Ground Texture", EditorStyles.boldLabel);
                                        MapIO.conditionalGround = (TerrainSplatSDK.Enum)EditorGUILayout.EnumFlagsField(MapIO.conditionalGround);
                                        break;
                                    case 1: // Biome
                                        GUILayout.Label("Biome Texture", EditorStyles.boldLabel);
                                        MapIO.conditionalBiome = (TerrainBiomeSDK.Enum)EditorGUILayout.EnumFlagsField(MapIO.conditionalBiome);
                                        break;
                                    case 2: // Alpha
                                        checkAlpha = EditorGUILayout.Toggle("Check Alpha:", checkAlpha);
                                        if (checkAlpha)
                                        {
                                            alphaTexture = EditorGUILayout.IntPopup("Alpha Texture:", alphaTexture, activeTextureAlpha, values);
                                        }
                                        break;
                                    case 3: // Topology
                                        GUILayout.Label("Topology Layer", EditorStyles.boldLabel);
                                        MapIO.conditionalTopology = (TerrainTopologySDK.Enum)EditorGUILayout.EnumFlagsField(MapIO.conditionalTopology);
                                        EditorGUILayout.Space();
                                        GUILayout.Label("Topology Texture", EditorStyles.boldLabel);
                                        topologyTexture = EditorGUILayout.IntPopup("Topology Texture:", topologyTexture, activeTextureTopo, values);
                                        break;
                                    case 4: // Terrain
                                        GUILayout.Label("Slope Range", EditorStyles.boldLabel);
                                        checkSlopeCndtl = EditorGUILayout.Toggle("Check Slopes:", checkSlopeCndtl);
                                        if (checkSlopeCndtl == true)
                                        {
                                            if (slopeLowCndtl > slopeHighCndtl)
                                            {
                                                slopeLowCndtl = slopeHighCndtl - 0.05f;
                                            }
                                            if (slopeLowCndtl < 0)
                                            {
                                                slopeLowCndtl = 0f;
                                            }
                                            if (slopeHighCndtl > 90f)
                                            {
                                                slopeHighCndtl = 90f;
                                            }
                                            EditorGUILayout.BeginHorizontal();
                                            slopeLowCndtl = EditorGUILayout.FloatField(slopeLowCndtl);
                                            slopeHighCndtl = EditorGUILayout.FloatField(slopeHighCndtl);
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.MinMaxSlider(ref slopeLowCndtl, ref slopeHighCndtl, 0f, 90f);
                                        }
                                        GUILayout.Label("Height Range", EditorStyles.boldLabel);
                                        checkHeightCndtl = EditorGUILayout.Toggle("Check Heights:", checkHeightCndtl);
                                        if (checkHeightCndtl == true)
                                        {
                                            if (heightLowCndtl > heightHighCndtl)
                                            {
                                                heightLowCndtl = heightHighCndtl - 0.05f;
                                            }
                                            if (heightLowCndtl < 0)
                                            {
                                                heightLowCndtl = 0f;
                                            }
                                            if (heightHighCndtl > 1000f)
                                            {
                                                heightHighCndtl = 1000f;
                                            }
                                            EditorGUILayout.BeginHorizontal();
                                            heightLowCndtl = EditorGUILayout.FloatField(heightLowCndtl);
                                            heightHighCndtl = EditorGUILayout.FloatField(heightHighCndtl);
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUILayout.MinMaxSlider(ref heightLowCndtl, ref heightHighCndtl, 0f, 1000f);
                                        }
                                        break;
                                }
                                GUILayout.Label("Texture To Paint:", EditorStyles.boldLabel);
                                layerConditionalInt = EditorGUILayout.Popup("Layer:", layerConditionalInt, landLayers);
                                switch (layerConditionalInt)
                                {
                                    default:
                                        Debug.Log("Layer doesn't exist");
                                        break;
                                    case 0:
                                        MapIO.groundLayerToPaint = (TerrainSplatSDK.Enum)EditorGUILayout.EnumPopup("Texture To Paint:", MapIO.groundLayerToPaint);
                                        texture = TerrainSplatSDK.TypeToIndex((int)MapIO.groundLayerToPaint);
                                        break;
                                    case 1:
                                        MapIO.biomeLayerToPaint = (TerrainBiomeSDK.Enum)EditorGUILayout.EnumPopup("Texture To Paint:", MapIO.biomeLayerToPaint);
                                        texture = TerrainBiomeSDK.TypeToIndex((int)MapIO.biomeLayerToPaint);
                                        break;
                                    case 2:
                                        texture = EditorGUILayout.IntPopup("Texture To Paint:", texture, activeTextureAlpha, values);
                                        break;
                                    case 3:
                                        MapIO.topologyLayerToPaint = (TerrainTopologySDK.Enum)EditorGUILayout.EnumPopup("Topology Layer:", MapIO.topologyLayerToPaint);
                                        texture = EditorGUILayout.IntPopup("Texture To Paint:", texture, activeTextureTopo, values);
                                        break;
                                }
                                if (GUILayout.Button(new GUIContent("Paint Conditional", "Paints the selected texture if it matches all of the conditions set.")))
                                {
                                    Conditions conditions = new Conditions();
                                    conditions.GroundConditions = MapIO.conditionalGround;
                                    conditions.BiomeConditions = MapIO.conditionalBiome;
                                    conditions.CheckAlpha = checkAlpha;
                                    conditions.AlphaTexture = alphaTexture;
                                    conditions.TopologyLayers = MapIO.conditionalTopology;
                                    conditions.TopologyTexture = topologyTexture;
                                    conditions.SlopeLow = slopeLowCndtl;
                                    conditions.SlopeHigh = slopeHighCndtl;
                                    conditions.HeightLow = heightLowCndtl;
                                    conditions.HeightHigh = heightHighCndtl;
                                    conditions.CheckHeight = checkHeightCndtl;
                                    conditions.CheckSlope = checkSlopeCndtl;
                                    MapIO.PaintConditional(landLayers[layerConditionalInt], texture, conditions);
                                }
                                break;
                            #endregion
                            #region Misc
                            case 3:
                                EditorGUILayout.BeginHorizontal();
                                if (GUILayout.Button(new GUIContent("Debug Alpha", "Sets the ground texture to rock wherever the terrain is invisible. Prevents the floating grass effect.")))
                                {
                                    MapIO.AlphaDebug();
                                }
                                if (GUILayout.Button(new GUIContent("Debug Water", "Raises the water heightmap to 500 metres if it is below.")))
                                {
                                    MapIO.DebugWaterLevel();
                                }
                                EditorGUILayout.EndHorizontal();
                                break;
                                #endregion
                        }
                        break;
                    #endregion
                    #region Layer Tools
                    case 1:
                        GUILayout.Label("Layer Tools", EditorStyles.boldLabel);

                        string oldLandLayer = MapIO.landLayer;

                        MapIO.landSelectIndex = EditorGUILayout.Popup("Layer:", MapIO.landSelectIndex, landLayers);
                        MapIO.landLayer = landLayers[MapIO.landSelectIndex];
                        if (MapIO.landLayer != oldLandLayer)
                        {
                            MapIO.ChangeLandLayer();
                            Repaint();
                        }
                        if (slopeMinBlendHigh > slopeMaxBlendHigh)
                        {
                            slopeMaxBlendHigh = slopeMinBlendHigh + 0.25f;
                            if (slopeMaxBlendHigh > 90f)
                            {
                                slopeMaxBlendHigh = 90f;
                            }
                        }
                        if (slopeMinBlendLow > slopeMaxBlendLow)
                        {
                            slopeMinBlendLow = slopeMaxBlendLow - 0.25f;
                            if (slopeMinBlendLow < 0f)
                            {
                                slopeMinBlendLow = 0f;
                            }
                        }
                        if (heightMinBlendLow > heightMaxBlendLow)
                        {
                            heightMinBlendLow = heightMaxBlendLow - 0.25f;
                            if (heightMinBlendLow < 0f)
                            {
                                heightMinBlendLow = 0f;
                            }
                        }
                        if (heightMinBlendHigh > heightMaxBlendHigh)
                        {
                            heightMaxBlendHigh = heightMinBlendHigh + 0.25f;
                            if (heightMaxBlendHigh > 1000f)
                            {
                                heightMaxBlendHigh = 1000f;
                            }
                        }
                        slopeMaxBlendLow = slopeLow;
                        slopeMinBlendHigh = slopeHigh;
                        heightMaxBlendLow = heightLow;
                        heightMinBlendHigh = heightHigh;
                        if (blendSlopes == false)
                        {
                            slopeMinBlendLow = slopeMaxBlendLow;
                            slopeMaxBlendHigh = slopeMinBlendHigh;
                        }
                        if (blendHeights == false)
                        {
                            heightMinBlendLow = heightLow;
                            heightMaxBlendHigh = heightHigh;
                        }
                        #region Ground Layer
                        if (MapIO.landLayer.Equals("Ground"))
                        {
                            MapIO.terrainLayer = (TerrainSplatSDK.Enum)EditorGUILayout.EnumPopup("Texture To Paint: ", MapIO.terrainLayer);
                            if (GUILayout.Button("Paint Whole Layer"))
                            {
                                MapIO.PaintLayer("Ground", TerrainSplatSDK.TypeToIndex((int)MapIO.terrainLayer));
                            }
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(new GUIContent("Rotate 90°", "Rotate this layer 90° ClockWise.")))
                            {
                                MapIO.RotateLayer("ground", true);
                            }
                            if (GUILayout.Button(new GUIContent("Rotate 270°", "Rotate this layer 90° Counter ClockWise.")))
                            {
                                MapIO.RotateLayer("ground", false);
                            }
                            EditorGUILayout.EndHorizontal();
                            aboveTerrain = EditorGUILayout.ToggleLeft("Paint only visible part of river.", aboveTerrain);
                            if (GUILayout.Button("Paint Rivers"))
                            {
                                MapIO.PaintRiver("Ground", aboveTerrain, TerrainSplatSDK.TypeToIndex((int)MapIO.terrainLayer));
                            }
                            GUILayout.Label("Slope Tools", EditorStyles.boldLabel); // From 0 - 90
                            EditorGUILayout.BeginHorizontal();
                            blendSlopes = EditorGUILayout.ToggleLeft("Toggle Blend Slopes", blendSlopes);
                            // Todo: Toggle for check between heightrange.
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("From: " + slopeLow.ToString() + "°", EditorStyles.boldLabel);
                            GUILayout.Label("To: " + slopeHigh.ToString() + "°", EditorStyles.boldLabel);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.MinMaxSlider(ref slopeLow, ref slopeHigh, 0f, 90f);
                            if (blendSlopes == true)
                            {
                                GUILayout.Label("Blend Low: " + slopeMinBlendLow + "°");
                                EditorGUILayout.MinMaxSlider(ref slopeMinBlendLow, ref slopeMaxBlendLow, 0f, 90f);
                                GUILayout.Label("Blend High: " + slopeMaxBlendHigh + "°");
                                EditorGUILayout.MinMaxSlider(ref slopeMinBlendHigh, ref slopeMaxBlendHigh, 0f, 90f);
                            }
                            if (GUILayout.Button(new GUIContent("Paint Slopes", "Paints the terrain on the " + MapIO.landLayer + " layer within the slope range.")))
                            {
                                MapIO.PaintSlope("Ground", slopeLow, slopeHigh, slopeMinBlendLow, slopeMaxBlendHigh, TerrainSplatSDK.TypeToIndex((int)MapIO.terrainLayer));
                            }
                            GUILayout.Label("Height Tools", EditorStyles.boldLabel); // From 0 - 90
                            blendHeights = EditorGUILayout.ToggleLeft("Toggle Blend Heights", blendHeights);
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("From: " + heightLow.ToString() + "m", EditorStyles.boldLabel);
                            GUILayout.Label("To: " + heightHigh.ToString() + "m", EditorStyles.boldLabel);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.MinMaxSlider(ref heightLow, ref heightHigh, 0f, 1000f);
                            if (blendHeights == true)
                            {
                                GUILayout.Label("Blend Low: " + heightMinBlendLow + "m");
                                EditorGUILayout.MinMaxSlider(ref heightMinBlendLow, ref heightMaxBlendLow, 0f, 1000f);
                                GUILayout.Label("Blend High: " + heightMaxBlendHigh + "m");
                                EditorGUILayout.MinMaxSlider(ref heightMinBlendHigh, ref heightMaxBlendHigh, 0f, 1000f);
                            }
                            if (GUILayout.Button(new GUIContent("Paint Heights", "Paints the terrain on the " + MapIO.landLayer + " layer within the height range.")))
                            {
                                MapIO.PaintHeight("Ground", heightLow, heightHigh, heightMinBlendLow, heightMaxBlendHigh, TerrainSplatSDK.TypeToIndex((int)MapIO.terrainLayer));
                            }
                            /*
                            GUILayout.Label("Noise scale, the futher left the smaller the blobs \n Replaces the current Ground textures");
                            GUILayout.Label(scale.ToString(), EditorStyles.boldLabel);
                            scale = GUILayout.HorizontalSlider(scale, 10f, 2000f);
                            if (GUILayout.Button("Generate random Ground map"))
                            {
                                script.generateEightLayersNoise("Ground", scale);
                            }*/
                        }
                        #endregion

                        #region Biome Layer
                        if (MapIO.landLayer.Equals("Biome"))
                        {
                            MapIO.biomeLayer = (TerrainBiomeSDK.Enum)EditorGUILayout.EnumPopup("Biome To Paint:", MapIO.biomeLayer);
                            if (GUILayout.Button("Paint Whole Layer"))
                            {
                                MapIO.PaintLayer("Biome", TerrainBiomeSDK.TypeToIndex((int)MapIO.biomeLayer));
                            }
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(new GUIContent("Rotate 90°", "Rotate this layer 90° ClockWise.")))
                            {
                                MapIO.RotateLayer("biome", true);
                            }
                            if (GUILayout.Button(new GUIContent("Rotate 270°", "Rotate this layer 90° Counter ClockWise.")))
                            {
                                MapIO.RotateLayer("biome", false);
                            }
                            EditorGUILayout.EndHorizontal();
                            aboveTerrain = EditorGUILayout.ToggleLeft("Paint only visible part of river.", aboveTerrain);
                            if (GUILayout.Button("Paint Rivers"))
                            {
                                MapIO.PaintRiver("Biome", aboveTerrain, 0);
                            }
                            GUILayout.Label("Slope Tools", EditorStyles.boldLabel); // From 0 - 90
                            EditorGUILayout.BeginHorizontal();
                            blendSlopes = EditorGUILayout.ToggleLeft("Toggle Blend Slopes", blendSlopes);
                            // Todo: Toggle for check between heightrange.
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("From: " + slopeLow.ToString() + "°", EditorStyles.boldLabel);
                            GUILayout.Label("To: " + slopeHigh.ToString() + "°", EditorStyles.boldLabel);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.MinMaxSlider(ref slopeLow, ref slopeHigh, 0f, 90f);
                            if (blendSlopes == true)
                            {
                                GUILayout.Label("Blend Low: " + slopeMinBlendLow + "°");
                                EditorGUILayout.MinMaxSlider(ref slopeMinBlendLow, ref slopeMaxBlendLow, 0f, 90f);
                                GUILayout.Label("Blend High: " + slopeMaxBlendHigh + "°");
                                EditorGUILayout.MinMaxSlider(ref slopeMinBlendHigh, ref slopeMaxBlendHigh, 0f, 90f);
                            }
                            if (GUILayout.Button(new GUIContent("Paint Slopes", "Paints the terrain on the " + MapIO.landLayer + " layer within the slope range.")))
                            {
                                MapIO.PaintSlope("Biome", slopeLow, slopeHigh, slopeMinBlendLow, slopeMaxBlendHigh, TerrainBiomeSDK.TypeToIndex((int)MapIO.biomeLayer));
                            }
                            GUILayout.Label("Height Tools", EditorStyles.boldLabel); // From 0 - 1000
                            blendHeights = EditorGUILayout.ToggleLeft("Toggle Blend Heights", blendHeights);
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("From: " + heightLow.ToString() + "m", EditorStyles.boldLabel);
                            GUILayout.Label("To: " + heightHigh.ToString() + "m", EditorStyles.boldLabel);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.MinMaxSlider(ref heightLow, ref heightHigh, 0f, 1000f);
                            if (blendHeights == true)
                            {
                                GUILayout.Label("Blend Low: " + heightMinBlendLow + "m");
                                EditorGUILayout.MinMaxSlider(ref heightMinBlendLow, ref heightMaxBlendLow, 0f, 1000f);
                                GUILayout.Label("Blend High: " + heightMaxBlendHigh + "m");
                                EditorGUILayout.MinMaxSlider(ref heightMinBlendHigh, ref heightMaxBlendHigh, 0f, 1000f);
                            }
                            if (GUILayout.Button(new GUIContent("Paint Heights", "Paints the terrain on the " + MapIO.landLayer + " layer within the height range.")))
                            {
                                MapIO.PaintHeight("Biome", heightLow, heightHigh, heightMinBlendLow, heightMaxBlendHigh, TerrainBiomeSDK.TypeToIndex((int)MapIO.biomeLayer));
                            }
                            z1 = EditorGUILayout.IntField("From Z ", z1);
                            z2 = EditorGUILayout.IntField("To Z ", z2);
                            x1 = EditorGUILayout.IntField("From X ", x1);
                            x2 = EditorGUILayout.IntField("To X ", x2);
                            if (GUILayout.Button("Paint Area"))
                            {
                                MapIO.PaintArea("Biome", z1, z2, x1, x2, TerrainBiomeSDK.TypeToIndex((int)MapIO.biomeLayer));
                            }
                            /*
                            GUILayout.Label("Noise scale, the futher left the smaller the blobs \n Replaces the current Biomes");
                            GUILayout.Label(scale.ToString(), EditorStyles.boldLabel);
                            scale = GUILayout.HorizontalSlider(scale, 500f, 5000f);
                            if (GUILayout.Button("Generate random Biome map"))
                            {
                                script.generateFourLayersNoise("Biome", scale);
                            }*/
                        }
                        #endregion

                        #region Alpha Layer
                        if (MapIO.landLayer.Equals("Alpha"))
                        {
                            GUILayout.Label("Green = Terrain Visible \nPurple = Terrain Invisible", EditorStyles.boldLabel);
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Paint Layer"))
                            {
                                MapIO.PaintLayer("Alpha", 1);
                            }
                            if (GUILayout.Button("Clear Layer"))
                            {
                                MapIO.PaintLayer("Alpha", 0);
                            }
                            if (GUILayout.Button("Invert Layer"))
                            {
                                MapIO.InvertLayer("Alpha");
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(new GUIContent("Rotate 90°", "Rotate this layer 90° ClockWise.")))
                            {
                                MapIO.RotateLayer("alpha", true);
                            }
                            if (GUILayout.Button(new GUIContent("Rotate 270°", "Rotate this layer 90° Counter ClockWise.")))
                            {
                                MapIO.RotateLayer("alpha", false);
                            }
                            EditorGUILayout.EndHorizontal();
                            GUILayout.Label("Slope Tools", EditorStyles.boldLabel); // From 0 - 90
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("From: " + slopeLow.ToString() + "°", EditorStyles.boldLabel);
                            GUILayout.Label("To: " + slopeHigh.ToString() + "°", EditorStyles.boldLabel);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.MinMaxSlider(ref slopeLow, ref slopeHigh, 0f, 90f);
                            if (GUILayout.Button(new GUIContent("Paint Slopes", "Paints the slopes on the " + MapIO.landLayer + " layer within the slope range.")))
                            {
                                MapIO.PaintSlope("Alpha", slopeLow, slopeHigh, slopeLow, slopeHigh, 0);
                            }
                            GUILayout.Label("Height Tools", EditorStyles.boldLabel); // From 0 - 1000
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("From: " + heightLow.ToString() + "m", EditorStyles.boldLabel);
                            GUILayout.Label("To: " + heightHigh.ToString() + "m", EditorStyles.boldLabel);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.MinMaxSlider(ref heightLow, ref heightHigh, 0f, 1000f);
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Paint range"))
                            {
                                MapIO.PaintHeight("Alpha", heightLow, heightHigh, heightLow, heightHigh, 0);
                            }
                            if (GUILayout.Button("Erase range"))
                            {
                                MapIO.PaintHeight("Alpha", heightLow, heightHigh, heightLow, heightHigh, 1);
                            }
                            EditorGUILayout.EndHorizontal();
                            z1 = EditorGUILayout.IntField("From Z ", z1);
                            z2 = EditorGUILayout.IntField("To Z ", z2);
                            x1 = EditorGUILayout.IntField("From X ", x1);
                            x2 = EditorGUILayout.IntField("To X ", x2);
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Paint Area"))
                            {
                                MapIO.PaintArea("Alpha", z1, z2, x1, x2, 0);
                            }
                            if (GUILayout.Button("Erase Area"))
                            {
                                MapIO.PaintArea("Alpha", z1, z2, x1, x2, 1);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        #endregion

                        #region Topology Layer
                        if (MapIO.landLayer.Equals("Topology"))
                        {
                            GUILayout.Label("Green = Active \nPurple = Inactive", EditorStyles.boldLabel);
                            MapIO.oldTopologyLayer = MapIO.topologyLayer;
                            MapIO.topologyLayer = (TerrainTopologySDK.Enum)EditorGUILayout.EnumPopup("Topology Layer:", MapIO.topologyLayer);
                            if (MapIO.topologyLayer != MapIO.oldTopologyLayer)
                            {
                                MapIO.ChangeLandLayer();
                                Repaint();
                            }
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Paint Layer"))
                            {
                                MapIO.PaintLayer("Topology", 0, TerrainTopologySDK.TypeToIndex((int)MapIO.topologyLayer));
                            }
                            if (GUILayout.Button("Clear Layer"))
                            {
                                MapIO.ClearLayer("Topology", MapIO.topologyLayer);
                            }
                            if (GUILayout.Button("Invert Layer"))
                            {
                                MapIO.InvertLayer("Topology", MapIO.topologyLayer);
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(new GUIContent("Rotate 90°", "Rotate this layer 90° ClockWise.")))
                            {
                                MapIO.RotateLayer("topology", true, MapIO.topologyLayer);
                            }
                            if (GUILayout.Button(new GUIContent("Rotate 270°", "Rotate this layer 90° Counter ClockWise.")))
                            {
                                MapIO.RotateLayer("topology", false, MapIO.topologyLayer);
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(new GUIContent("Rotate All 90°", "Rotate all Topology layers 90° ClockWise.")))
                            {
                                MapIO.RotateAllTopologymap(true);
                            }
                            if (GUILayout.Button(new GUIContent("Rotate All 270°", "Rotate all Topology layers 90° Counter ClockWise.")))
                            {
                                MapIO.RotateAllTopologymap(false);
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(new GUIContent("Invert All", "Invert all Topology layers.")))
                            {
                                MapIO.InvertAllTopologyLayers();
                            }
                            if (GUILayout.Button(new GUIContent("Clear All", "Clear all Topology layers.")))
                            {
                                MapIO.ClearAllTopologyLayers();
                            }
                            EditorGUILayout.EndHorizontal();
                            aboveTerrain = EditorGUILayout.ToggleLeft("Paint only visible part of river.", aboveTerrain);
                            if (GUILayout.Button("Paint Rivers"))
                            {
                                MapIO.PaintRiver("Topology", aboveTerrain, 0, TerrainTopologySDK.TypeToIndex((int)MapIO.topologyLayer));
                            }
                            GUILayout.Label("Slope Tools", EditorStyles.boldLabel); // From 0 - 90
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("From: " + slopeLow.ToString() + "°", EditorStyles.boldLabel);
                            GUILayout.Label("To: " + slopeHigh.ToString() + "°", EditorStyles.boldLabel);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.MinMaxSlider(ref slopeLow, ref slopeHigh, 0f, 90f);
                            if (GUILayout.Button(new GUIContent("Paint Slopes", "Paints the slopes on the " + MapIO.landLayer + " layer within the slope range.")))
                            {
                                MapIO.PaintSlope("Topology", slopeLow, slopeHigh, slopeLow, slopeHigh, 0, TerrainTopologySDK.TypeToIndex((int)MapIO.topologyLayer));
                            }
                            if (GUILayout.Button(new GUIContent("Erase Slopes", "Paints the slopes within the slope range with the INACTIVE topology texture.")))
                            {
                                MapIO.PaintSlope("Topology", slopeLow, slopeHigh, slopeLow, slopeHigh, 1, TerrainTopologySDK.TypeToIndex((int)MapIO.topologyLayer));
                            }
                            GUILayout.Label("Height Tools", EditorStyles.boldLabel); // From 0 - 1000
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("From: " + heightLow.ToString() + "m", EditorStyles.boldLabel);
                            GUILayout.Label("To: " + heightHigh.ToString() + "m", EditorStyles.boldLabel);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.MinMaxSlider(ref heightLow, ref heightHigh, 0f, 1000f);
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(new GUIContent("Paint Range", "Paints the slopes within the height range with the ACTIVE topology texture.")))
                            {
                                MapIO.PaintHeight("Topology", heightLow, heightHigh, heightLow, heightHigh, 0, TerrainTopologySDK.TypeToIndex((int)MapIO.topologyLayer));
                            }
                            if (GUILayout.Button(new GUIContent("Erase Range", "Paints the slopes within the height range with the INACTIVE topology texture.")))
                            {
                                MapIO.PaintHeight("Topology", heightLow, heightHigh, heightLow, heightHigh, 1, TerrainTopologySDK.TypeToIndex((int)MapIO.topologyLayer));
                            }
                            EditorGUILayout.EndHorizontal();
                            z1 = EditorGUILayout.IntField("From Z ", z1);
                            z2 = EditorGUILayout.IntField("To Z ", z2);
                            x1 = EditorGUILayout.IntField("From X ", x1);
                            x2 = EditorGUILayout.IntField("To X ", x2);
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Paint Area"))
                            {
                                MapIO.PaintArea("Topology", z1, z2, x1, x2, 0, TerrainTopologySDK.TypeToIndex((int)MapIO.topologyLayer));
                            }
                            if (GUILayout.Button("Erase Area"))
                            {
                                MapIO.PaintArea("Topology", z1, z2, x1, x2, 1, TerrainTopologySDK.TypeToIndex((int)MapIO.topologyLayer));
                            }
                            EditorGUILayout.EndHorizontal();
                            /*
                            GUILayout.Label("Noise scale, the futher left the smaller the blobs \n Replaces the current Topology");
                            GUILayout.Label(scale.ToString(), EditorStyles.boldLabel);
                            scale = GUILayout.HorizontalSlider(scale, 10f, 500f);
                            if (GUILayout.Button("Generate random topology map"))
                            {
                                script.generateTwoLayersNoise("Topology", scale, 1, 0);
                            }*/
                        }
                        break;
                    #endregion
                    default:
                        toolsOptions = 0;
                        break;
                    #endregion
                    #region Generation
                    case 2:
                        /*
                        if (GUILayout.Button(new GUIContent("Default Topologies", "Generates default topologies on the map and paints over existing topologies without wiping them.")))
                        {
                            script.AutoGenerateTopology(false);
                        }
                        if (GUILayout.Button(new GUIContent("Wipe & Default Topologies", "Generates default topologies on the map and paints over existing topologies after wiping them.")))
                        {
                            script.AutoGenerateTopology(true);
                        }
                        if (GUILayout.Button(new GUIContent("Default Ground Textures", "Generates default ground textures and paints over existing textures after wiping them.")))
                        {
                            script.AutoGenerateGround();
                        }
                        if (GUILayout.Button(new GUIContent("Default Biome Textures", "Generates default biome textures and paints over existing textures after wiping them.")))
                        {
                            script.AutoGenerateBiome();
                        }
                        scale = EditorGUILayout.Slider(scale, 1f, 2000f);
                        GUILayout.Label("Scale of the heightmap generation, \n the further left the less smoothed the terrain will be");
                        if (GUILayout.Button(new GUIContent("Generate Perlin Heightmap", "Really basic perlin doesn't do much rn.")))
                        {
                            script.generatePerlinHeightmap(scale);
                        }*/
                        GUILayout.Label(new GUIContent("Node Presets", "List of all the node presets in the project."), EditorStyles.boldLabel);
                        if (GUILayout.Button(new GUIContent("Refresh presets list.", "Refreshes the list of all the Node Presets in the project.")))
                        {
                            MapIO.RefreshAssetList();
                        }
                        presetScrollPos = GUILayout.BeginScrollView(presetScrollPos);
                        ReorderableListGUI.Title("Node Presets");
                        ReorderableListGUI.ListField(MapIO.generationPresetList, NodePresetDrawer, DrawEmpty);
                        GUILayout.EndScrollView();
                        break;
                        #endregion
                }
                break;
            #endregion
            default:
                mainMenuOptions = 0;
                break;
        }
        #endregion
        #region InspectorGUIInput
        Event e = Event.current;
        #endregion
        EditorGUILayout.EndScrollView();
    }
    #region OtherMenus
    [MenuItem("Rust Map Editor/Terrain Tools", false, 1)]
    static void OpenTerrainTools()
    {
        Selection.activeGameObject = GameObject.FindGameObjectWithTag("Land");
    }
    [MenuItem("Rust Map Editor/Wiki", false, 2)]
    static void OpenWiki()
    {
        Application.OpenURL("https://github.com/RustMapMaking/Rust-Map-Editor-Unity/wiki");
    }
    [MenuItem("Rust Map Editor/Discord", false, 3)]
    static void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/HPmTWVa");
    }
    #endregion
    #region Methods
    private string NodePresetDrawer(Rect position, string itemValue)
    {
        position.width -= 39;
        GUI.Label(position, itemValue);
        position.x = position.xMax;
        position.width = 39;
        if (GUI.Button(position, new GUIContent("Open", "Opens the Node Editor for the preset.")))
        {
            MapIO.RefreshAssetList();
            MapIO.nodePresetLookup.TryGetValue(itemValue, out Object preset);
            if (preset != null)
            {
                AssetDatabase.OpenAsset(preset.GetInstanceID());
            }
            else
            {
                Debug.LogError("The preset you are trying to open is null.");
            }
        }
        /*
        position.x = position.x + 40;
        position.width = 30;
        if (GUI.Button(position, "Run"))
        {
            MapIO.nodePresetLookup.TryGetValue(itemValue, out Object preset);
            if (preset != null)
            {
                var graph = (XNode.NodeGraph)AssetDatabase.LoadAssetAtPath(assetDirectory + itemValue + ".asset", typeof(XNode.NodeGraph));
                MapIO.NodeAsset(graph);
            }
        }
        */
        return itemValue;
    }
    private void DrawEmpty()
    {
        GUILayout.Label("No presets in list.", EditorStyles.miniLabel);
    }
    #endregion
}
public class PrefabHierachyEditor : EditorWindow
{
    [SerializeField] TreeViewState m_TreeViewState;

    PrefabHierachy m_TreeView;
    SearchField m_SearchField;

    void OnEnable()
    {
        if (m_TreeViewState == null)
            m_TreeViewState = new TreeViewState();

        m_TreeView = new PrefabHierachy(m_TreeViewState);
        m_SearchField = new SearchField();
        m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;
    }
    void OnGUI()
    {
        DoToolbar();
        DoTreeView();
    }
    void DoToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Space(100);
        GUILayout.FlexibleSpace();
        m_TreeView.searchString = m_SearchField.OnToolbarGUI(m_TreeView.searchString);
        GUILayout.EndHorizontal();
    }
    void DoTreeView()
    {
        Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
        m_TreeView.OnGUI(rect);
    }
    public static void ShowWindow()
    {
        var window = GetWindow<PrefabHierachyEditor>();
        window.titleContent = new GUIContent("Prefabs");
        window.Show();
    }
}