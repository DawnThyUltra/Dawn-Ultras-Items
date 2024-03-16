using GameNetcodeStuff;
using HarmonyLib;

namespace YourThunderstoreTeam.patch.enemies
{
    [HarmonyPatch(typeof(FlowermanAI))]
    public class FlowermanPatch
    {
        [HarmonyPatch("KillPlayerAnimationServerRpc", MethodType.Normal)]
        [HarmonyPrefix]
        private static bool OnKillPlayerAnimationServerRpc(ref FlowermanAI __instance)
        {
            PlayerControllerB? targetPlayer = __instance.targetPlayer;

            if (targetPlayer is not null)
            {
                bool isInvincible = PlayerControllerBPatch.IsPlayerInvincible(targetPlayer);
                return !isInvincible;
            }

            return true;
        }
    }
}
