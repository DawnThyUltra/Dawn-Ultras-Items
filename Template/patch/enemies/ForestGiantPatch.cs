using GameNetcodeStuff;
using HarmonyLib;

namespace YourThunderstoreTeam.patch.enemies
{
    [HarmonyPatch(typeof(ForestGiantAI))]
    public class ForestGiantPatch
    {
        [HarmonyPatch("GrabPlayerServerRpc", MethodType.Normal)]
        [HarmonyPrefix]
        private static bool OnGrabPlayerServerRpc(ref ForestGiantAI __instance)
        {
            PlayerControllerB targetPlayer = __instance.targetPlayer;

            if (targetPlayer is not null)
            {
                bool isInvincible = PlayerControllerBPatch.IsPlayerInvincible(targetPlayer);
                return !isInvincible;
            }

            return true;
        }
    }
}
