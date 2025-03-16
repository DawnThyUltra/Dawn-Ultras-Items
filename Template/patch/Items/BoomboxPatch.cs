using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace YourThunderstoreTeam.patch.Items
{
    [HarmonyPatch(typeof(BoomboxItem))]
    public class BoomboxPatch
    {
        [HarmonyPatch(nameof(BoomboxItem.Start), MethodType.Normal)]
        [HarmonyPrefix]
        private static bool OnStart( ref BoomboxItem __instance)
        {
            List<AudioClip> songs = new List<AudioClip>(__instance.musicAudios);
            
            for (int i = 1; i <= 10; i++)
            {
                AudioClip currentSong = Plugin.DawnUltrasItemsAssets.LoadAsset<AudioClip>(string.Format("Song{0}", i.ToString()));
                songs.Add(currentSong);
            }

            __instance.musicAudios = songs.ToArray();
            return true;
        }
    }
}
