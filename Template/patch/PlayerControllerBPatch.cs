using GameNetcodeStuff;
using HarmonyLib;
using System.Reflection;

namespace YourThunderstoreTeam.patch;

/// <summary>
/// Patch to modify the behavior of a player.
/// </summary>
[HarmonyPatch(typeof(PlayerControllerB))]
public class PlayerControllerBPatch
{
    /// <summary>
    /// Whether the player can die (to most causes).
    /// </summary>
    public static bool IsInvincible { get; private set; } = false;


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
        bool canTakeDamage = !IsInvincible;

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
        bool canDie = !(CanResistCauseOfDeath(causeOfDeath) && IsInvincible);

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
