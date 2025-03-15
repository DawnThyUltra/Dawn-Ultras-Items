using GameNetcodeStuff;
using HarmonyLib;

namespace YourThunderstoreTeam.patch;


[HarmonyPatch(typeof(UnlockableSuit))]
public class UnlockableSuitPatch
{
    [HarmonyPatch(nameof(UnlockableSuit.SwitchSuitForPlayer), MethodType.Normal)]
    [HarmonyPrefix]
    private static bool OnSwitchSuitForPlayer(UnlockableSuit __instance, PlayerControllerB player)
    {
        return player is not null && !PlayerControllerBPatch.IsPlayerInvisible(player);
    }
}
