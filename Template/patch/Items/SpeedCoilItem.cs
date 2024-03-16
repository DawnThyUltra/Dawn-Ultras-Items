using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YourThunderstoreTeam.util;

namespace YourThunderstoreTeam.patch.Items
{
    public class SpeedCoilItem : GrabbableObject
    {
        private const int RARITY = 70;
        private const float SPEED_INCREASE = 3f;

        private bool isSpeedBoostActive = false;


        public static void AddAsset(AssetBundle assetBundle)
        {
            Item speedCoil = assetBundle.LoadAsset<Item>("Speed Coil");
            LethalLib.Modules.Utilities.FixMixerGroups(speedCoil.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(speedCoil.spawnPrefab);
            LethalLib.Modules.Items.RegisterScrap(speedCoil, RARITY, LethalLib.Modules.Levels.LevelTypes.All);

            SpeedCoilItem speedCoilItem = speedCoil.spawnPrefab.AddComponent<SpeedCoilItem>();
            speedCoilItem.grabbable = true;
            speedCoilItem.grabbableToEnemies = true;
            speedCoilItem.isInFactory = true;
            speedCoilItem.itemProperties = speedCoil;
        }

        public override void EquipItem()
        {
            base.EquipItem();
            
            if (playerHeldBy is not null && !isSpeedBoostActive)
            {
                playerHeldBy.movementSpeed += SPEED_INCREASE;
                isSpeedBoostActive = true;
            }
        }

        public override void DestroyObjectInHand(PlayerControllerB playerHolding)
        {
            DisableSpeedBoost();
            base.DestroyObjectInHand(playerHolding);
        }

        public override void PocketItem()
        {
            base.PocketItem();
            DisableSpeedBoost();
        }

        public override void DiscardItem()
        {
            DisableSpeedBoost();
            base.DiscardItem();
        }


        private void DisableSpeedBoost()
        {
            if (isSpeedBoostActive && playerHeldBy is not null)
            {
                playerHeldBy.movementSpeed -= SPEED_INCREASE;
                isSpeedBoostActive = false;
            }
        }
    }
}
