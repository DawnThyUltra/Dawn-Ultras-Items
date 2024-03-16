using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using YourThunderstoreTeam.util;

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
            bool isPlayerInvincible = PlayerControllerBPatch.IsPlayerInvincible(targetPlayer);

            if (targetPlayer is not null) 
                return !isPlayerInvincible;

            return true;
        }
    }
}
