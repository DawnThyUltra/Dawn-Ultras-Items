using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static LethalLib.Modules.ContentLoader;

namespace YourThunderstoreTeam.patch.Items
{
    public class PizzaItem: GrabbableObject
    {
        private const int RARITY = 80;
        private bool isActive = false;

        public AudioSource AudioSource;
        public AudioClip EatSfx;


        public static void AddAsset(AssetBundle assetBundle)
        {
            Item pizza = assetBundle.LoadAsset<Item>("Pizza");
            LethalLib.Modules.Utilities.FixMixerGroups(pizza.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(pizza.spawnPrefab);
            LethalLib.Modules.Items.RegisterScrap(pizza, RARITY, LethalLib.Modules.Levels.LevelTypes.All);

            PizzaItem pizzaItem = pizza.spawnPrefab.AddComponent<PizzaItem>();
            pizzaItem.grabbable = true;
            pizzaItem.grabbableToEnemies = true;
            pizzaItem.isInFactory = true;
            pizzaItem.itemProperties = pizza;
            pizzaItem.AudioSource = pizza.spawnPrefab.GetComponent<AudioSource>();
            pizzaItem.EatSfx = pizzaItem.AudioSource.clip;
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

                    AudioSource.PlayOneShot(EatSfx);
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
