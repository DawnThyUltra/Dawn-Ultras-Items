using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using YourThunderstoreTeam.util;

namespace YourThunderstoreTeam.patch.enemies
{
    [HarmonyPatch(typeof(EnemyAI))]
    public class EnemyAIPatch
    {
        //[HarmonyPatch("PlayerIsTargetable", MethodType.Normal)]
        //[HarmonyPostfix]
        //private static bool PostPlayerIsTargetable(bool __result, ref EnemyAI __instance, PlayerControllerB playerScript)
        //{
        //    if (!playerScript.isPlayerDead && PlayerControllerBPatch.IsPlayerInvisible(playerScript))
        //        return false;

        //    return __result;
        //}

        [HarmonyPatch("CheckLineOfSightForPlayer", MethodType.Normal)]
        [HarmonyPostfix]
        private static PlayerControllerB PostCheckLineOfSightForPlayer(PlayerControllerB __result)
        {
            return PlayerControllerBPatch.IsPlayerInvisible(__result) ? null : __result;
        }

        [HarmonyPatch("CheckLineOfSightForClosestPlayer", MethodType.Normal)]
        [HarmonyPostfix]
        private static PlayerControllerB PostCheckLineOfSightForClosestPlayer(PlayerControllerB __result)
        {
            return PlayerControllerBPatch.IsPlayerInvisible(__result) ? null : __result;
        }

        [HarmonyPatch("GetAllPlayersInLineOfSight", MethodType.Normal)]
        [HarmonyPostfix]
        private static PlayerControllerB[] GetAllPlayersInLineOfSight(PlayerControllerB[] __result)
        {
            if (__result is null || __result.Length == 0)
                return null;

            List<PlayerControllerB> newList = new List<PlayerControllerB>();

            foreach(PlayerControllerB player in __result)
            {
                if (player is not null && !player.isPlayerDead && !PlayerControllerBPatch.IsPlayerInvisible(player))
                    newList.Add(player);
            }
            Console.WriteLine("Count: "+ newList.Count);
            return newList.Count > 0 ? newList.ToArray() : null;
        }

        [HarmonyPatch("GetClosestPlayer", MethodType.Normal)]
        [HarmonyPostfix]
        private static PlayerControllerB PostGetClosestPlayer(PlayerControllerB __result)
        {
            return __result is null || PlayerControllerBPatch.IsPlayerInvisible(__result) ? null : __result;
        }

        [HarmonyPatch("TargetClosestPlayer", MethodType.Normal)]
        [HarmonyPostfix]
        private static bool PostTargetClosestPlayer(bool __result, ref EnemyAI __instance)
        {
            PlayerControllerB targetPlayer = __instance.targetPlayer;

            if (targetPlayer is not null && PlayerControllerBPatch.IsPlayerInvisible(targetPlayer))
                return false;

            return __result;
        }
    }
}
