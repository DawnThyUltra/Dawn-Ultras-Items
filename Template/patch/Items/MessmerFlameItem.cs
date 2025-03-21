using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using YourThunderstoreTeam.patch.Other;
using static LethalLib.Modules.ContentLoader;

namespace YourThunderstoreTeam.patch.Items
{
    public class MessmerFlameItem: GrabbableObject
    {
        private const int RARITY = 80;

        public static GameObject FireSerpent;

        public AudioSource AudioSource;


        public static void AddAsset(AssetBundle assetBundle)
        {
            Item item = assetBundle.LoadAsset<Item>("MessmersFlame");
            LethalLib.Modules.Utilities.FixMixerGroups(item.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab);
            LethalLib.Modules.Items.RegisterScrap(item, RARITY, LethalLib.Modules.Levels.LevelTypes.All);

            MessmerFlameItem messmerFlameItem = item.spawnPrefab.AddComponent<MessmerFlameItem>();
            messmerFlameItem.grabbable = true;
            messmerFlameItem.grabbableToEnemies = true;
            messmerFlameItem.isInFactory = true;
            messmerFlameItem.itemProperties = item;
            messmerFlameItem.AudioSource = item.spawnPrefab.GetComponent<AudioSource>();

            FireSerpent = assetBundle.LoadAsset<GameObject>("FireSerpent");
            FireSerpent.AddComponent<FireSerpent>();   
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (buttonDown)
            {
                if (playerHeldBy is not null)
                {
                    GameObject fireSerpentClone = GameObject.Instantiate(original: FireSerpent, parent: RoundManager.Instance.mapPropsContainer.transform, position: transform.position, rotation: playerHeldBy.transform.rotation);
                    FireSerpent fireSerpentScript = fireSerpentClone.GetComponent<FireSerpent>();

                    fireSerpentScript.player = playerHeldBy;
                }
            }
        }
    }
}
