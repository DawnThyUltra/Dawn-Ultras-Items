using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static LethalLib.Modules.ContentLoader;

namespace YourThunderstoreTeam.patch.Items
{
    public class CheezburgerItem: GrabbableObject
    {
        private const int RARITY = 80;
        private bool isActive = false;

        public AudioSource cheezburgerAudio;
        public AudioClip mmmCheezburgerSfx;


        public static void AddAsset(AssetBundle assetBundle)
        {
            Item cheezburger = assetBundle.LoadAsset<Item>("Cheezburger");
            LethalLib.Modules.Utilities.FixMixerGroups(cheezburger.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(cheezburger.spawnPrefab);
            LethalLib.Modules.Items.RegisterScrap(cheezburger, RARITY, LethalLib.Modules.Levels.LevelTypes.All);

            CheezburgerItem cheezburgerItem = cheezburger.spawnPrefab.AddComponent<CheezburgerItem>();
            cheezburgerItem.grabbable = true;
            cheezburgerItem.grabbableToEnemies = true;
            cheezburgerItem.isInFactory = true;
            cheezburgerItem.itemProperties = cheezburger;
            cheezburgerItem.cheezburgerAudio = cheezburger.spawnPrefab.GetComponent<AudioSource>();
            cheezburgerItem.mmmCheezburgerSfx = cheezburgerItem.cheezburgerAudio.clip;
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (buttonDown)
            {
                if (playerHeldBy is not null && !isActive)
                {
                    Vector3 currentPosOffset = itemProperties.positionOffset;
                    Vector3 currentRotOffset = itemProperties.rotationOffset;
                    isActive = true;

                    itemProperties.positionOffset = new Vector3(0.03f, -0.375f, -0.33f);
                    itemProperties.rotationOffset = new Vector3(-90f, 9f, -90f);

                    cheezburgerAudio.PlayOneShot(mmmCheezburgerSfx);
                    ReturnToNormalOffsets(currentPosOffset, currentRotOffset);
                }
            }
        }

        private async void ReturnToNormalOffsets(Vector3 normalPos, Vector3 normalRot)
        {
            await Task.Delay(1000);

            itemProperties.positionOffset = normalPos;
            itemProperties.rotationOffset = normalRot;
            isActive = false;
        }
    }
}
