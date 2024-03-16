using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YourThunderstoreTeam.util;

namespace YourThunderstoreTeam.patch.enemies
{
    [HarmonyPatch(typeof(MaskedPlayerEnemy))]
    public class MaskedPlayerEnemyPatch
    {
        [HarmonyPatch("killAnimation", MethodType.Normal)]
        [HarmonyPrefix]
        private static bool OnKillAnimation(ref MaskedPlayerEnemy __instance)
        {
            PlayerControllerB targetPlayer = __instance.targetPlayer;

            if (targetPlayer is not null)
            {
                bool isPlayerInvincible = PlayerControllerBPatch.IsPlayerInvincible(targetPlayer);
                return !isPlayerInvincible;
            }


            return true;
        }

        [HarmonyPatch("KillPlayerAnimationServerRpc", MethodType.Normal)]
        [HarmonyPrefix]
        private static bool OnKillPlayerAnimationServerRpc(ref MaskedPlayerEnemy __instance)
        {
            PlayerControllerB targetPlayer = __instance.targetPlayer;

            if (targetPlayer is not null)
            {
                bool isInvincible = PlayerControllerBPatch.IsPlayerInvincible(targetPlayer);
                return !isInvincible;
            }
            Utilities.PrintToChat("Returned true");
            return true;
        }
    }
}
