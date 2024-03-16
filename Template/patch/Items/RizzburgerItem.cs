using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using YourThunderstoreTeam.util;
using static LethalLib.Modules.ContentLoader;

namespace YourThunderstoreTeam.patch.Items
{
    public class RizzburgerItem: GrabbableObject
    {
        private const int RARITY = 30;
        private const float COOLDOWN = 10f;

        public AudioSource rizzburgerAudio;
        public AudioClip rizzSfx;


        public static void AddAsset(AssetBundle assetBundle)
        {
            Item rizzburger = assetBundle.LoadAsset<Item>("Rizzburger");
            LethalLib.Modules.Utilities.FixMixerGroups(rizzburger.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(rizzburger.spawnPrefab);
            LethalLib.Modules.Items.RegisterScrap(rizzburger, RARITY, LethalLib.Modules.Levels.LevelTypes.All);

            RizzburgerItem rizzburgerItem = rizzburger.spawnPrefab.AddComponent<RizzburgerItem>();
            rizzburgerItem.grabbable = true;
            rizzburgerItem.grabbableToEnemies = true;
            rizzburgerItem.isInFactory = true;
            rizzburgerItem.itemProperties = rizzburger;
            rizzburgerItem.rizzburgerAudio = rizzburger.spawnPrefab.GetComponent<AudioSource>();
            rizzburgerItem.rizzSfx = rizzburgerItem.rizzburgerAudio.clip;
            rizzburgerItem.useCooldown = COOLDOWN;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (buttonDown)
            {
                if (playerHeldBy is not null)
                {
                    rizzburgerAudio.PlayOneShot(rizzSfx);
                }
            }
        }


    }
}
