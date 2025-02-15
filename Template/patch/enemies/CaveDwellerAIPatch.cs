using GameNetcodeStuff;
using HarmonyLib;
using System;
using YourThunderstoreTeam.patch.Items;

namespace YourThunderstoreTeam.patch.enemies
{
    [HarmonyPatch(typeof(CaveDwellerAI))]
    public class CaveDwellerAIPatch
    {
        [HarmonyPatch("HitEnemy", MethodType.Normal)]
        [HarmonyPrefix]

        private static bool OnHitEnemy(ref CaveDwellerAI __instance, int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false, int hitID = -1)
        {
            Console.WriteLine("Hit ID: " + hitID);
            if (hitID == EnergySwordItem.HitId)
                __instance.KillEnemyOnOwnerClient();

            return true;
        }
    }
}
