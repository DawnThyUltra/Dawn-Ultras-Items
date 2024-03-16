using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace YourThunderstoreTeam.patch
{
    [HarmonyPatch(typeof(MaskedPlayerEnemy))]
    public class MaskedPlayerEnemyPatch
    {
        [HarmonyPatch("killAnimation", MethodType.Normal)]
        [HarmonyPrefix]
        private static bool OnKillAnimation(ref MaskedPlayerEnemy __instance)
        {
            PlayerControllerB? targetPlayer = __instance.targetPlayer;

            if (targetPlayer is not null) 
                return !PlayerControllerBPatch.IsPlayerInvincible(targetPlayer);

            return true;
        }
    }
}
