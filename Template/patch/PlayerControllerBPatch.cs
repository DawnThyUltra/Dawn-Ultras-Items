using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;

namespace YourThunderstoreTeam.patch;

/// <summary>
/// Patch to modify the behavior of a player.
/// </summary>
[HarmonyPatch(typeof(PlayerControllerB))]
public class PlayerControllerBPatch
{
    /// <summary>
    /// A dictionary of player instance IDs that represent whether the players themselves are invincible.
    /// </summary>
    private static Dictionary<int, bool> InvinciblePlayerIDs { get; set; } = new Dictionary<int, bool>();


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


    [HarmonyPatch("Start", MethodType.Normal)]
    [HarmonyPrefix]
    private static bool OnStart(ref PlayerControllerB __instance)
    {
        InvinciblePlayerIDs.Add(__instance.GetInstanceID(), true);
        return true;
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