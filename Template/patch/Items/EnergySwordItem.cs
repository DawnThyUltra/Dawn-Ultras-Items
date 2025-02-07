using GameNetcodeStuff;
using HarmonyLib;
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
        private const int PRICE = 150;
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
        private int _tempKillCount = 0;
        private float _lastKillTimestamp = Time.time;
        private Queue<AudioClip> _announcerQueue = new Queue<AudioClip>();
        private bool _announcing = false;

        public AudioSource AnnouncerAudioSource;
        public AudioClip DoubleKill;
        public AudioClip TripleKill;
        public AudioClip Overkill;
        public AudioClip Killtacular;
        public AudioClip Killtrocity;
        public AudioClip Killamanjaro;
        public AudioClip Killtastrophe;
        public AudioClip Killpocalypse;
        public AudioClip Killionaire;
        public AudioClip Betrayal;
        public AudioClip KillingSpree;
        public AudioClip KillingFrenzy;
        public AudioClip RunningRiot;
        public AudioClip Rampage;
        public AudioClip Untouchable;
        public AudioClip Invincible;
        public AudioClip Inconceivable;
        public AudioClip Unfrigginbelievable;
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
            energySwordScript.AnnouncerAudioSource = energySword.spawnPrefab.gameObject.transform.Find("Announcer").gameObject.GetComponent<AudioSource>();
            energySwordScript.SwordSwingSfx = assetBundle.LoadAsset<AudioClip>("Energy_sword_melee.wav");
            energySwordScript.SwordHitSfx = assetBundle.LoadAsset<AudioClip>("Energy_sword_hit.wav");
            energySwordScript.SwordHitEnvSfx = assetBundle.LoadAsset<AudioClip>("Energy_sword_hit_env.wav");

            #region Announcer Lines
            energySwordScript.DoubleKill = assetBundle.LoadAsset<AudioClip>("Double_Kill.mp3");
            energySwordScript.TripleKill = assetBundle.LoadAsset<AudioClip>("Triple_Kill.mp3");
            energySwordScript.Overkill = assetBundle.LoadAsset<AudioClip>("Overkill.mp3");
            energySwordScript.Killtacular = assetBundle.LoadAsset<AudioClip>("Killtacular.mp3");
            energySwordScript.Killtrocity = assetBundle.LoadAsset<AudioClip>("Killtrocity.mp3");
            energySwordScript.Killamanjaro = assetBundle.LoadAsset<AudioClip>("Killamanjaro.mp3");
            energySwordScript.Killtastrophe = assetBundle.LoadAsset<AudioClip>("Killtastrophe.mp3");
            energySwordScript.Killpocalypse = assetBundle.LoadAsset<AudioClip>("Killpocalypse.mp3");
            energySwordScript.Killionaire = assetBundle.LoadAsset<AudioClip>("Killionaire.mp3");
            energySwordScript.Betrayal = assetBundle.LoadAsset<AudioClip>("Betrayal.mp3");
            energySwordScript.KillingSpree = assetBundle.LoadAsset<AudioClip>("Killing_Spree.mp3");
            energySwordScript.KillingFrenzy = assetBundle.LoadAsset<AudioClip>("Killing_Frenzy.mp3");
            energySwordScript.RunningRiot = assetBundle.LoadAsset<AudioClip>("Running_Riot.mp3");
            energySwordScript.Rampage = assetBundle.LoadAsset<AudioClip>("Rampage.mp3");
            energySwordScript.Untouchable = assetBundle.LoadAsset<AudioClip>("Untouchable.mp3");
            energySwordScript.Invincible = assetBundle.LoadAsset<AudioClip>("Invincible.mp3");
            energySwordScript.Inconceivable = assetBundle.LoadAsset<AudioClip>("Inconceivable.mp3");
            energySwordScript.Unfrigginbelievable = assetBundle.LoadAsset<AudioClip>("Unfrigginbelievable.mp3");
            #endregion
        }

        public override void Update()
        {
            base.Update();
            PlayFirstAudioInQueue();
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

        public override void OnLostOwnership()
        {
            base.OnLostOwnership();
            ToggleReticle(false);
            _killCount = 0;
        }

        public override void DiscardItem()
        {
            base.DiscardItem();
            ToggleReticle(false);
            _killCount = 0;
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

                    if (hit2 is not null)
                        AudioSource.PlayOneShot(SwordHitEnvSfx);
                }
            }
        }

        private (bool, IHittable?, RaycastHit?) ScanForTarget()
        {
            Vector3 forward = playerHeldBy.gameplayCamera.transform.forward;
            RaycastHit? finalHit = null;

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

                if (hit.transform != playerHeldBy.transform && hit.distance <= LungeMinDist)
                    finalHit = hit;
            }

            return (false, null, finalHit);
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
            EnemyAICollisionDetect? enemyAICollision;
            PlayerControllerB? player;
            float currentTime = Time.time;
            float timeBetweenLastKill = currentTime - _lastKillTimestamp;

            rayHit.transform.TryGetComponent(out enemyAICollision);
            rayHit.transform.TryGetComponent(out player);

            await Task.Delay(200);

            bool isEnemyDead = false;
            bool isAlly = false;

            if (enemyAICollision is not null)
                isEnemyDead = enemyAICollision.mainScript.isEnemyDead;
            else if (player is not null)
            {
                isEnemyDead = player.isPlayerDead;
                isAlly = true;
            }

            if (isEnemyDead)
            {
                _killCount++;
                _tempKillCount = timeBetweenLastKill <= 5 ? _tempKillCount + 1 : 1;
                _lastKillTimestamp = currentTime;
                
                if (isAlly)
                    _announcerQueue.Enqueue(Betrayal);

                switch(_tempKillCount)
                {
                    case 2:
                        _announcerQueue.Enqueue(DoubleKill);
                        break;
                    case 3:
                        _announcerQueue.Enqueue(TripleKill);
                        break;
                    case 4:
                        _announcerQueue.Enqueue(Overkill);
                        break;
                    case 5:
                        _announcerQueue.Enqueue(Killtacular);
                        break;
                    case 6:
                        _announcerQueue.Enqueue(Killtrocity);
                        break;
                    case 7:
                        _announcerQueue.Enqueue(Killamanjaro);
                        break;
                    case 8:
                        _announcerQueue.Enqueue(Killtastrophe);
                        break;
                    case 9:
                        _announcerQueue.Enqueue(Killpocalypse);
                        break;
                    case 10:
                        _announcerQueue.Enqueue(Killionaire);
                        break;
                }

                switch(_killCount)
                {
                    case 3:
                        AnnounceMsg(string.Format("{0} is on a KILLING SPREE", playerHeldBy.name));
                        _announcerQueue.Enqueue(KillingSpree);
                        break;
                    case 5:
                        AnnounceMsg(string.Format("{0} is on a KILLING FRENZY", playerHeldBy.name));
                        _announcerQueue.Enqueue(KillingFrenzy);
                        break;
                    case 7:
                        AnnounceMsg(string.Format("{0} is a RUNNING RIOT", playerHeldBy.name));
                        _announcerQueue.Enqueue(RunningRiot);
                        break;
                    case 9:
                        AnnounceMsg(string.Format("{0} is on a RAMPAGE", playerHeldBy.name));
                        _announcerQueue.Enqueue(Rampage);
                        break;
                    case 11:
                        AnnounceMsg(string.Format("{0} is UNTOUCHABLE", playerHeldBy.name));
                        _announcerQueue.Enqueue(Untouchable);
                        break;
                    case 13:
                        AnnounceMsg(string.Format("{0} is INVINCIBLE", playerHeldBy.name));
                        _announcerQueue.Enqueue(Invincible);
                        break;
                    case 15:
                        AnnounceMsg(string.Format("{0} is INCONCEIVABLE", playerHeldBy.name));
                        _announcerQueue.Enqueue(Inconceivable);
                        break;
                    case 17:
                        AnnounceMsg(string.Format("{0} is UNFRIGGINBELIEVABLE", playerHeldBy.name));
                        _announcerQueue.Enqueue(Unfrigginbelievable);
                        break;
                }
            }
        }

        private async void PlayFirstAudioInQueue()
        {
            if (playerHeldBy is not null && !playerHeldBy.isPlayerDead && isHeld)
            {
                if (!_announcing && _announcerQueue.Count > 0)
                {
                    _announcing = true;
                    AudioClip clipToAnnounce = _announcerQueue.Peek();

                    AnnouncerAudioSource.PlayOneShot(clipToAnnounce);
                    await Task.Delay((int)(clipToAnnounce.length * 1000));

                    _announcerQueue.Dequeue();
                    _announcing = false;
                }
            }
            else
            {
                if (!_announcing && _announcerQueue.Count > 0)
                {
                    for (int i = 0; i < _announcerQueue.Count; i++)
                        _announcerQueue.Dequeue();
                }  
            }
        }

        private void AnnounceMsg(string msg)
        {
            HUDManager.Instance.AddTextToChatOnServer(msg);
        }
    }
}
