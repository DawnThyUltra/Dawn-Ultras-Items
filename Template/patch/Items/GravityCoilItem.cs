using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using YourThunderstoreTeam.util;

namespace YourThunderstoreTeam.patch.Items
{
    public class GravityCoilItem : GrabbableObject
    {
        private const int RARITY = 70;
        private const float JUMP_INCREASE = 13f;

        private bool isGravBoostActive = false;


        public static void AddAsset(AssetBundle assetBundle)
        {
            Item gravityCoil = assetBundle.LoadAsset<Item>("Gravity Coil");
            LethalLib.Modules.Utilities.FixMixerGroups(gravityCoil.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(gravityCoil.spawnPrefab);
            LethalLib.Modules.Items.RegisterScrap(gravityCoil, RARITY, LethalLib.Modules.Levels.LevelTypes.All);

            GravityCoilItem GravityCoilItem = gravityCoil.spawnPrefab.AddComponent<GravityCoilItem>();
            GravityCoilItem.grabbable = true;
            GravityCoilItem.grabbableToEnemies = true;
            GravityCoilItem.isInFactory = true;
            GravityCoilItem.itemProperties = gravityCoil;
        }

        public override void EquipItem()
        {
            base.EquipItem();
            
            if (playerHeldBy is not null && !isGravBoostActive)
            {
                playerHeldBy.jumpForce += JUMP_INCREASE;
                isGravBoostActive = true;
            }
        }

        public override void Update()
        {
            base.Update();

            if (playerHeldBy is not null && isGravBoostActive)
            {
                playerHeldBy.takingFallDamage = false;
                playerHeldBy.fallValue = Mathf.Clamp(playerHeldBy.fallValue, -10f, playerHeldBy.jumpForce);
            }
        }

        public override void DestroyObjectInHand(PlayerControllerB playerHolding)
        {
            DisableGravBoost();
            base.DestroyObjectInHand(playerHolding);
        }

        public override void PocketItem()
        {
            base.PocketItem();
            DisableGravBoost();
        }

        public override void DiscardItem()
        {
            DisableGravBoost();
            base.DiscardItem();
        }


        private void DisableGravBoost()
        {
            if (isGravBoostActive && playerHeldBy is not null)
            {
                playerHeldBy.jumpForce -= JUMP_INCREASE;
                playerHeldBy.takingFallDamage = true;
                isGravBoostActive = false;
            }
        }
    }
}
