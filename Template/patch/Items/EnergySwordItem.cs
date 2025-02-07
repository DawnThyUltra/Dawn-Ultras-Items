using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace YourThunderstoreTeam.patch.Items
{
    public class EnergySwordItem : GrabbableObject
    {
        private const int PRICE = 100;
        private const string DESC = "A sword with a blade made of plasma used by high-ranking Covenant Elites. Allows the user to lunge from a distance towards their target to deliver a high-damage slash.";

        private const int _layerMask = 1084754248;
        private RaycastHit[] _scannedObjects;
        private List<RaycastHit> _scannedObjectsList;
        private IHittable _lungeHittable;
        private RaycastHit _lungeRayHit;
        private Vector3 _lungeOrigin;
        private bool _lunging = false;
        private GameObject _reticle;
        private GameObject _reticleTargetLocked;
        private bool _reticleEnabled = false;
        private bool _targetLocked = false;
        private bool _cooldownActive = false;

        #region Announcer
        private int _killCount = 0;

        public AudioSource AnnouncerAudioSource;
        public AudioClip Betrayal;
        public AudioClip KillingSpree;
        public AudioClip KillingFrenzy;
        public AudioClip RunningRiot;
        public AudioClip Rampage;
        #endregion

        #region Sound Effects
        public AudioSource AudioSource;
        public AudioClip SwordHitSfx;
        public AudioClip SwordHitEnvSfx;
        public AudioClip SwordSwingSfx;
        #endregion


        public static float LungeMinDist
        {
            get { return 2f; }
        }
        public static float LungeMaxDist
        {
            get { return 12f; }
        }


        public static void AddAsset(AssetBundle assetBundle)
        {
            Item energySword = assetBundle.LoadAsset<Item>("EnergySword");
            LethalLib.Modules.Utilities.FixMixerGroups(energySword.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(energySword.spawnPrefab);
            LethalLib.Modules.Items.RegisterShopItem(shopItem: energySword, price: PRICE, itemInfo: new TerminalNode() { displayText = DESC, clearPreviousText = true });

            EnergySwordItem energySwordScript = energySword.spawnPrefab.AddComponent<EnergySwordItem>();
            energySwordScript.grabbable = true;
            energySwordScript.isInFactory = true;
            energySwordScript.grabbableToEnemies = true;
            energySwordScript.itemProperties = energySword;
            energySwordScript.AudioSource = energySword.spawnPrefab.GetComponent<AudioSource>();
            energySwordScript.AnnouncerAudioSource = energySword.spawnPrefab.GetComponentInChildren<AudioSource>();
            energySwordScript.SwordSwingSfx = assetBundle.LoadAsset<AudioClip>("Energy_sword_melee.wav");
            energySwordScript.SwordHitSfx = assetBundle.LoadAsset<AudioClip>("Energy_sword_hit.wav");
            energySwordScript.SwordHitEnvSfx = assetBundle.LoadAsset<AudioClip>("Energy_sword_hit_env.wav");

            #region Announcer Lines
            energySwordScript.Betrayal = assetBundle.LoadAsset<AudioClip>("Betrayal.mp3");
            energySwordScript.KillingSpree = assetBundle.LoadAsset<AudioClip>("Killing_Spree.mp3");
            energySwordScript.KillingFrenzy = assetBundle.LoadAsset<AudioClip>("Killing_Frenzy.mp3");
            energySwordScript.RunningRiot = assetBundle.LoadAsset<AudioClip>("Running_Riot.mp3");
            energySwordScript.Rampage = assetBundle.LoadAsset<AudioClip>("Rampage.mp3");
            #endregion
        }

        public override void Update()
        {
            base.Update();
            float dt = Time.deltaTime;

            if (playerHeldBy is not null && !playerHeldBy.isPlayerDead)
            {
                if (_lunging)
                {
                    //Vector3 currentPos = Vector3.RotateTowards(playerHeldBy.transform.position, _lungeRayHit.transform.position, 360f * dt, 0.0f);
                    if (Vector3.Distance(playerHeldBy.transform.position, _lungeOrigin) >= _lungeRayHit.distance - LungeMinDist)
                    {
                        _lunging = false;

                        if (_lungeHittable.Hit(4, playerHeldBy.transform.forward, playerHeldBy, false, 2))
                            TryAddKill(_lungeRayHit);

                        SwingSword();
                        AudioSource.PlayOneShot(SwordHitSfx);
                    }
                    else
                    {
                        playerHeldBy.transform.position = Vector3.MoveTowards(playerHeldBy.transform.position, _lungeRayHit.transform.position, 50f * dt);
                    }
                }
                else
                {
                    if (_reticle is not null && _reticleTargetLocked is not null)
                    {
                        (_targetLocked, _, _) = ScanForTarget();
                        _reticle.SetActive(!_targetLocked && _reticleEnabled);
                        _reticleTargetLocked.SetActive(_targetLocked && _reticleEnabled);
                    }
                }
            }
        }

        public override void EquipItem()
        {;
            base.EquipItem();
            LoadReticle();
        }

        public override void PocketItem()
        {
            base.PocketItem();
            ToggleReticle(false);
        }

        public override void DiscardItem()
        {
            base.DiscardItem();
            ToggleReticle(false);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(_reticle);
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            if (buttonDown && !_cooldownActive && !_lunging)
            {
                (bool success, IHittable? hit, RaycastHit? hit2) = ScanForTarget();
                _cooldownActive = true;

                if (success && hit is not null && hit2 is not null)
                {
                    _lungeHittable = hit;
                    _lungeRayHit = (RaycastHit)hit2;
                    _lungeOrigin = playerHeldBy.transform.position;
                    _lunging = true;

                    Console.WriteLine("Lunging, dist: {0}", _lungeRayHit.distance);
                }
                else
                {
                    SwingSword();
                }
            }
        }

        private (bool, IHittable?, RaycastHit?) ScanForTarget()
        {
            Vector3 forward = playerHeldBy.gameplayCamera.transform.forward;

            _scannedObjects = Physics.RaycastAll(
                        playerHeldBy.gameplayCamera.transform.position,
                        forward,
                        LungeMaxDist,
                        _layerMask
                    );
            _scannedObjectsList = _scannedObjects.OrderBy((RaycastHit hit) => hit.distance).ToList();

            for (int i = 0; i < _scannedObjectsList.Count; i++)
            {
                RaycastHit hit = _scannedObjectsList[i];
                Collider collider = hit.collider;
                GameObject gameObj = collider.gameObject;
                
                if (gameObj.TryGetComponent(out IHittable component) && hit.transform != playerHeldBy.transform && !Physics.Linecast(playerHeldBy.transform.position, hit.point, out var _, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                {
                    bool canScan = true;
                    
                    if (hit.transform.TryGetComponent(out EnemyAICollisionDetect enemyAICollision))
                    {
                        canScan = !enemyAICollision.mainScript.isEnemyDead;
                    }
                    else if (hit.transform.TryGetComponent(out PlayerControllerB player))
                    {
                        canScan = !player.isPlayerDead;
                    }

                    return (canScan, component, hit);
                }
            }

            return (false, null, null);
        }


        private void ToggleReticle(bool enabled)
        {
            _reticleEnabled = enabled;

            if (_reticle is not null)
            {
                _reticle.SetActive(enabled);
                _reticleTargetLocked.SetActive(false);
            } 
        }

        private void LoadReticle()
        {
            if (_reticle is null)
            {
                GameObject canvas = GameObject.Find("Systems").gameObject.transform.Find("UI").gameObject.transform.Find("Canvas").gameObject;
                Transform reticleTransform = gameObject.transform.Find("Reticle");
                Transform reticleTargetTransform = gameObject.transform.Find("ReticleTargetLocked");

                if (reticleTransform is not null)
                {
                    _reticle = Instantiate(reticleTransform.gameObject, canvas.transform);
                    _reticleTargetLocked = Instantiate(reticleTargetTransform.gameObject, canvas.transform);

                    RectTransform rectTransform = _reticle.GetComponent<RectTransform>();
                    RectTransform rectTransform2 = _reticleTargetLocked.GetComponent<RectTransform>();

                    Vector2 newSizeDelta = new Vector2(30, 30);
                    rectTransform.set_sizeDelta_Injected(ref newSizeDelta);
                    rectTransform2.set_sizeDelta_Injected(ref newSizeDelta);
                }
            }

            ToggleReticle(true);
        }

        private async void SwingSword()
        {
            playerHeldBy.playerBodyAnimator.SetTrigger("UseHeldItem1");
            AudioSource.PlayOneShot(SwordSwingSfx);

            await Task.Delay(900);
            _cooldownActive = false;
        }

        private async void TryAddKill(RaycastHit rayHit)
        {
            bool isEnemyDead = false;
            bool isAlly = false;

            if (rayHit.transform.TryGetComponent(out EnemyAICollisionDetect enemyAICollision))
                isEnemyDead = enemyAICollision.mainScript.isEnemyDead;
            else if (rayHit.transform.TryGetComponent(out PlayerControllerB player))
            {
                isEnemyDead = player.isPlayerDead;
                isAlly = true;
            }
                

            if (isEnemyDead)
            {
                
            }
        }
    }
}
