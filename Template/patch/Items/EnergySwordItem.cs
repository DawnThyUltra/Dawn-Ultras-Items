using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace YourThunderstoreTeam.patch.Items
{
    public class EnergySwordItem : GrabbableObject
    {
        private const int PRICE = 0;//100;
        private const string DESC = "Energy Sword.";

        private int _layerMask = 1084754248;
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
        private RuntimeAnimatorController _animController;
        //private AnimatorOverrideController _overrideController;

        #region Sound Effects

        #endregion


        public static float LungeMinDist
        {
            get { return 4f; }
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
        }

        public override void Start()
        {
            base.Start();
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
                    if (Vector3.Distance(playerHeldBy.transform.position, _lungeOrigin) >= _lungeRayHit.distance - 2f)
                    {
                        _lunging = false;
                        _lungeHittable.Hit(4, playerHeldBy.transform.forward, playerHeldBy, false, 2);

                        playerHeldBy.playerBodyAnimator.SetTrigger("UseHeldItem1");
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

            if (buttonDown && !_lunging)
            {
                (bool success, IHittable? hit, RaycastHit? hit2) = ScanForTarget();

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
                    Console.WriteLine("No targets scanned");
                }   
            }
        }

        private (bool, IHittable?, RaycastHit?) ScanForTarget()
        {
            Vector3 forward = playerHeldBy.gameplayCamera.transform.forward;

            _scannedObjects = Physics.RaycastAll(
                        playerHeldBy.gameplayCamera.transform.position,
                        forward,
                        15f,
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
    }
}
