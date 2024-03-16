using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;
using YourThunderstoreTeam.patch;
using YourThunderstoreTeam.service;
using LethalLib;
using UnityEngine.Audio;
using YourThunderstoreTeam.patch.Items;
using YourThunderstoreTeam.patch.enemies;

namespace YourThunderstoreTeam;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency(LethalLib.Plugin.ModGUID)]
//[BepInDependency(LethalDevMode.MyPluginInfo.PLUGIN_GUID)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; set; }

    public static ManualLogSource Log => Instance.Logger;

    private readonly Harmony _harmony = new(PluginInfo.PLUGIN_GUID);

    public TemplateService Service;

    public Plugin()
    {
        Instance = this;
    }

    public static AssetBundle DawnUltrasItemsAssets;

    private void Awake()
    {
        Service = new TemplateService();

        Log.LogInfo($"Applying patches...");
        ApplyPluginPatch();
        Log.LogInfo($"Patches applied");

        string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


        DawnUltrasItemsAssets = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "dawnultrasitemsbundle"));

        if (DawnUltrasItemsAssets == null)
        {
            Log.LogError("Failed to load custom assets");
            return;
        }

        Log.LogInfo("Adding Dawn Ultra's items...");
        AddItems();
        Log.LogInfo("Dawn Ultra's items added");
    }

    /// <summary>
    /// Applies the patch to the game.
    /// </summary>
    private void ApplyPluginPatch()
    {
        _harmony.PatchAll(typeof(ShipLightsPatch));
        _harmony.PatchAll(typeof(PlayerControllerBPatch));
        _harmony.PatchAll(typeof(MaskedPlayerEnemyPatch));
        _harmony.PatchAll(typeof(ForestGiantPatch));
        _harmony.PatchAll(typeof(FlowermanPatch));
    }

    /// <summary>
    /// Adds all items from the DawnUltrasItems Asset Bundle into the game.
    /// </summary>
    private void AddItems()
    {
        HealthpackItem.AddAsset(DawnUltrasItemsAssets);
        CheezburgerItem.AddAsset(DawnUltrasItemsAssets);
        RizzburgerItem.AddAsset(DawnUltrasItemsAssets);
    }
}
