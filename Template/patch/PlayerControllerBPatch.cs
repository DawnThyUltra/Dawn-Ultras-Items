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
    public static bool IsInvincible { get; private set; } = false;


    [HarmonyPatch("DamagePlayer", MethodType.Normal)]
    [HarmonyPrefix]
    private static bool OnPlayerDamage(ref PlayerControllerB __instance)
    {
        bool canTakeDamage = !IsInvincible;

        return canTakeDamage;
    }

    [HarmonyPatch("KillPlayer", MethodType.Normal)]
    [HarmonyPrefix]
    private static bool OnPlayerDeath(ref PlayerControllerB __instance, object[] __args)
    {
        CauseOfDeath causeOfDeath = (CauseOfDeath)__args[2];
        bool canDie = !(CanResistCauseOfDeath(causeOfDeath) && IsInvincible);

        return canDie;
    }

    private static bool CanResistCauseOfDeath(CauseOfDeath causeOfDeath)
    {
        switch(causeOfDeath)
        {
            case CauseOfDeath.Drowning: return false;
            case CauseOfDeath.Abandoned: return false;
        }
        
        return true;
    }

    private static void PrintToChat(string message)
    {
        HUDManager.Instance.AddTextToChatOnServer(message);
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
