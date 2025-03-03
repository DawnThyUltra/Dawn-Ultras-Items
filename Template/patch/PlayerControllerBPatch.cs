using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace YourThunderstoreTeam.patch;

/// <summary>
/// Patch to modify the behavior of a player.
/// </summary>
[HarmonyPatch(typeof(PlayerControllerB))]
public class PlayerControllerBPatch
{
    /// <summary>
    /// A dictionary of player instance IDs that represent whether the players themselves are invincible. Not to be confused with <see cref="InvisiblePlayerIDs"/>.
    /// </summary>
    private static Dictionary<int, bool> InvinciblePlayerIDs { get; } = new Dictionary<int, bool>();
    /// <summary>
    /// A dictionary of player instance IDs that represent whether the players themselves are invisible. Not to be confused with <see cref="InvinciblePlayerIDs"/>.
    /// </summary>
    private static Dictionary<int, bool> InvisiblePlayerIDs { get; } = new Dictionary<int, bool>();
    
    private static Dictionary<int, Renderer[]> PlayerRenderers { get; } = new Dictionary<int, Renderer[]>();

    #region Invincibility
    /// <summary>
    /// Determines whether the player should take damage.<br/><br/>
    /// 
    /// Called when the player takes damage.
    /// </summary>
    /// <param name="__instance">The player instance.</param>
    /// <returns>Whether the player can be damaged.</returns>
    [HarmonyPatch("DamagePlayer", MethodType.Normal)]
    [HarmonyPrefix]
    private static bool OnPlayerDamage(ref PlayerControllerB __instance)
    {
        bool canTakeDamage = !IsPlayerInvincible(__instance);

        return canTakeDamage;
    }

    /// <summary>
    /// Determines whether the player should die.<br/><br/>
    /// 
    /// Called when the player is about to DIE!!!
    /// </summary>
    /// <param name="__instance">The player instance.</param>
    /// <param name="__args">The arguments of <see cref="PlayerControllerB.KillPlayer(UnityEngine.Vector3, bool, CauseOfDeath, int)"/>.</param>
    /// <returns>Whether the player can DIE.</returns>
    [HarmonyPatch("KillPlayer", MethodType.Normal)]
    [HarmonyPrefix]
    private static bool OnPlayerDeath(ref PlayerControllerB __instance, object[] __args)
    {
        CauseOfDeath causeOfDeath = (CauseOfDeath)__args[2];
        bool canDie = !(CanResistCauseOfDeath(causeOfDeath) && IsPlayerInvincible(__instance));

        return canDie;
    }

    /// <summary>
    /// Returns whether the cause of death can be ignored if a player is invincible.
    /// </summary>
    /// <param name="causeOfDeath">The cause of a player's would-be (or soon-to-be) death.</param>
    /// <returns>Whether the cause of death can be ignored if a player is invincible.</returns>
    private static bool CanResistCauseOfDeath(CauseOfDeath causeOfDeath)
    {
        switch(causeOfDeath)
        {
            case CauseOfDeath.Drowning: return false;
            case CauseOfDeath.Abandoned: return false;
        }
        
        return true;
    }

    /// <summary>
    /// Toggles whether a player can be damaged and killed or not.
    /// </summary>
    /// <param name="player">The player instance.</param>
    /// <param name="isInvincible">Whether the player can be damaged or not.</param>
    public static void TogglePlayerInvincibility(PlayerControllerB player, bool isInvincible)
    {
        if (IsPlayerInvincible(player) != isInvincible)
            InvinciblePlayerIDs[player.GetInstanceID()] = isInvincible;
    }

    /// <summary>
    /// Returns whether a player can be damaged or killed by most causes.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>Whether the player is invincible.</returns>
    public static bool IsPlayerInvincible(PlayerControllerB player)
    {
        return IsPlayerInvincible(player.GetInstanceID());
    }

    /// <summary>
    /// Returns whether a player can be damaged or killed by most causes.
    /// </summary>
    /// <param name="playerObjectId">The unity object ID of a player to check.</param>
    /// <returns>Whether the player is invincible.</returns>
    public static bool IsPlayerInvincible(int playerObjectId)
    {
        InvinciblePlayerIDs.TryGetValue(playerObjectId, out bool isInvincible);

        if (!InvinciblePlayerIDs.ContainsKey(playerObjectId))
            InvinciblePlayerIDs.Add(playerObjectId, false);

        return isInvincible;
    }
    #endregion

    #region Invisibility
    /// <summary>
    /// Toggles whether the player can be detected by line of sight.
    /// </summary>
    /// <param name="player">The player instance.</param>
    /// <param name="isInvisible">Whether the player can be detected by line of sight.</param>
    public static void TogglePlayerInvisibility(PlayerControllerB player, bool isInvisible)
    {
        int instanceId = player.GetInstanceID();

        if (IsPlayerInvisible(player) != isInvisible)
            InvisiblePlayerIDs[instanceId] = isInvisible;

        if (PlayerRenderers.TryGetValue(instanceId, out Renderer[] renderers))
        {
            Console.WriteLine("Renderer count: "+renderers.Length);
            foreach (Renderer renderer in renderers)
            {
                
                foreach (Material material in renderer.materials)
                {
                    Console.WriteLine("Setting material alpha to " + (isInvisible ? 0 : 1));

                    //https://discussions.unity.com/t/change-rendering-mode-via-script/667727/3
                    if (isInvisible)
                    {
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.EnableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    }
                    else
                    {
                        material.SetOverrideTag("RenderType", "");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        material.SetInt("_ZWrite", 1);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = -1;
                    }

                    //material.color = new Color(material.color.r, material.color.g, material.color.b, isInvisible ? 0.7f : 1f);
                }
            }
        }
    }

    /// <summary>
    /// Returns whether the player can be detected by line of sight.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>Whether the player can be detected by line of sight.</returns>
    public static bool IsPlayerInvisible(PlayerControllerB player)
    {
        return IsPlayerInvisible(player.GetInstanceID());
    }

    /// <summary>
    /// Returns whether a player with the matching ID can be detected by line of sight.
    /// </summary>
    /// <param name="playerObjectId">The Unity instance ID of the player.</param>
    /// <returns>Whether the player with the matching ID can be detected by line of sight.</returns>
    public static bool IsPlayerInvisible(int playerObjectId)
    {
        InvisiblePlayerIDs.TryGetValue(playerObjectId, out bool isInvisible);

        if (!InvisiblePlayerIDs.ContainsKey(playerObjectId))
            InvisiblePlayerIDs.Add(playerObjectId, false);

        return isInvisible;
    }
    #endregion


    [HarmonyPatch("Start", MethodType.Normal)]
    [HarmonyPrefix]
    private static bool OnStart(ref PlayerControllerB __instance)
    {
        PlayerRenderers.Add(__instance.GetInstanceID(), __instance.GetComponentsInChildren<Renderer>());

        TogglePlayerInvincibility(__instance, false);
        TogglePlayerInvisibility(__instance, false);

        return true;
    }

    [HarmonyPatch("IVisibleThreat.GetVisibility", MethodType.Normal)]
    [HarmonyPostfix]
    private static float PostGetVisibility(float __result, ref PlayerControllerB __instance)
    {
        return __instance && IsPlayerInvisible(__instance) ? 0f : __result;
    }
    /// <summary>
    /// Method called when the player jumps.
    ///
    /// Check the link below for more information about Harmony patches.
    /// Class patches: https://github.com/BepInEx/HarmonyX/wiki/Class-patches
    /// Patch parameters: https://github.com/BepInEx/HarmonyX/wiki/Patch-parameters
    /// </summary>
    /// <param name="__instance">Instance that called the method.</param>
    /// <returns>True if the original method should be called, false otherwise.</returns>
    //[HarmonyPatch("PlayerJump")]
    //[HarmonyPrefix]
    //private static bool OnPlayerJump(ref PlayerControllerB __instance)
    //{
    //    HUDManager.Instance.AddTextToChatOnServer("isJumping: " + __instance.isJumping);
    //    // When a player jumps, set isJumping to false to prevent the player from jumping.
    //    __instance.isJumping = false;
    //    return false;
    //}

    
}