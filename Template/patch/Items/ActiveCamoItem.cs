using System;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace YourThunderstoreTeam.patch.Items
{
    public class ActiveCamoItem: GrabbableObject
    {
        private const int RARITY = 30;
        private const float DURATION = 60f; // Duration in seconds

        private float currentTime = 0f;
        private bool isInvis = false;

        public AudioSource AudioSource;
        public AudioClip UseSfx;
        public Light Light;


        public static void AddAsset(AssetBundle assetBundle)
        {
            Item activeCamoScrap = assetBundle.LoadAsset<Item>("ActiveCamo");
            LethalLib.Modules.Utilities.FixMixerGroups(activeCamoScrap.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(activeCamoScrap.spawnPrefab);
            LethalLib.Modules.Items.RegisterScrap(activeCamoScrap, RARITY, LethalLib.Modules.Levels.LevelTypes.All);

            ActiveCamoItem activeCamoItem = activeCamoScrap.spawnPrefab.AddComponent<ActiveCamoItem>();
            activeCamoItem.grabbable = true;
            activeCamoItem.grabbableToEnemies = true;
            activeCamoItem.isInFactory = true;
            activeCamoItem.itemProperties = activeCamoScrap;
            activeCamoItem.AudioSource = activeCamoScrap.spawnPrefab.GetComponent<AudioSource>();
            activeCamoItem.UseSfx = activeCamoItem.AudioSource.clip;
            activeCamoItem.Light = activeCamoScrap.spawnPrefab.GetComponentInChildren<Light>();
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            try
            {
                if (buttonDown)
                {
                    if (playerHeldBy is not null && !itemUsedUp && !PlayerControllerBPatch.IsPlayerInvisible(playerHeldBy))
                    {
                        itemUsedUp = true;
                        isInvis = true;
                        Light.enabled = false;
                        AudioSource.PlayOneShot(UseSfx);

                        PlayerControllerBPatch.TogglePlayerInvisibility(playerHeldBy, DURATION);
                        DestroyObjectInHand(playerHeldBy);
                    }
                }
            }
            catch(Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public override void Update()
        {
            base.Update();

            try
            {
                if (isInvis)
                {
                    currentTime += Time.deltaTime;

                    if (playerHeldBy is not null && PlayerControllerBPatch.IsPlayerInvisible(playerHeldBy))
                    {
                        if (currentTime >= DURATION || playerHeldBy.isPlayerDead)
                        {
                            isInvis = false;
                            PlayerControllerBPatch.TogglePlayerInvisibility(playerHeldBy , false);
                        }
                    }
                }
            }
            catch(Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public override void EquipItem()
        {
            base.EquipItem();
            Light.enabled = true;
        }

        public override void PocketItem()
        {
            base.PocketItem();
            Light.enabled = false;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Light.enabled = false;
        }
    }
}
