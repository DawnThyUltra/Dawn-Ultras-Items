using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace YourThunderstoreTeam.patch.Items
{
    public class HealthpackItem : GrabbableObject
    {
        private const int HEALTHPACK_PRICE = 60;
        private const string HEALTHPACK_DESC = "A first aid kit containing the standard combat wound treatment, which is used by all UNSC combat personnel, ranging from Marines to Spartans.\n\nEach health pack contains biofoam, a stitch kit, polypseudomorphine, a sterile field generator, self-adhering antiseptic battle dressings, and other useful components.\n\nUse when you receive and injury (critical or not).\n\n";

        public AudioSource healthpackAudio;
        public AudioClip healSfx;


        public static void AddAsset(AssetBundle assetBundle)
        {
            Item healthpack = assetBundle.LoadAsset<Item>("Healthpack");
            LethalLib.Modules.Utilities.FixMixerGroups(healthpack.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(healthpack.spawnPrefab);
            LethalLib.Modules.Items.RegisterShopItem(shopItem: healthpack, price: HEALTHPACK_PRICE, itemInfo: new TerminalNode() { displayText = HEALTHPACK_DESC, clearPreviousText = true });

            HealthpackItem healthpackItemScript = healthpack.spawnPrefab.AddComponent<HealthpackItem>();
            healthpackItemScript.grabbable = true;
            healthpackItemScript.isInFactory = true;
            healthpackItemScript.grabbableToEnemies = true;
            healthpackItemScript.itemProperties = healthpack;
            healthpackItemScript.healthpackAudio = healthpack.spawnPrefab.GetComponent<AudioSource>();
            healthpackItemScript.healSfx = healthpackItemScript.healthpackAudio.clip;
        }


        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (buttonDown)
            {
                if (playerHeldBy is not null && playerHeldBy.health < 100)
                {
                    healthpackAudio.PlayOneShot(healSfx);
                    playerHeldBy.DamagePlayer(damageNumber: -100);

                    if (playerHeldBy.criticallyInjured)
                    {
                        playerHeldBy.MakeCriticallyInjured(enable: false);
                    }


                    itemUsedUp = true;
                    DestroyObjectInHand(playerHeldBy);
                }
            }
        }
    }
}
