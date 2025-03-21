using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace YourThunderstoreTeam.patch.Other
{
    public class FireSerpent: MonoBehaviour
    {
        private const int collisionLayerMask = 526592;
        private const int visibleThreatsMask = 524296;

        private float despawnTimer = 0f;
        private float startupTimer = 0f;
        private bool hitWall = true;
        private float currentSpeed = StartingSpeed;
        private EnemyAI targetEnemy = null;

        public static float BaseSpeed = 30f;
        public static float StartingSpeed = 0.60f;
        public static float SpeedIncrement = 0.2f;
        public static float MaxSpeed = 3f;
        public static float StartupDelay = 1f;
        public static float TimeUntilDespawn = 5f;

        public PlayerControllerB player;

        private void Start()
        {
            hitWall = false;
        }

        private void FixedUpdate()
        {
            if (startupTimer < StartupDelay)
            {
                startupTimer += Time.deltaTime;
                return;
            }

            if (hitWall)
                return;

            if (despawnTimer < TimeUntilDespawn && player is not null)
            {
                despawnTimer += Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward * 2f, BaseSpeed * Time.deltaTime);
                //transform.position += transform.forward * BaseSpeed * currentSpeed;
                //currentSpeed = Mathf.Clamp(currentSpeed + (SpeedIncrement * Time.deltaTime), BaseSpeed, MaxSpeed);

                if (targetEnemy is not null)
                {
                    transform.LookAt(targetEnemy.transform.position);
                    //transform.rotation.SetLookRotation(Vector3.RotateTowards(transform.position, targetEnemy.transform.position, 1f * Time.deltaTime, 0.0f).normalized);
                    Console.WriteLine(targetEnemy.name);

                    if (Vector3.Distance(transform.position, targetEnemy.transform.position) <= 1f)
                    {
                        hitWall = true;
                        UnityEngine.Object.Destroy(gameObject);
                    }

                    if (!hitWall)
                    {
                        RaycastHit[] raycastHits = Physics.RaycastAll(transform.position, transform.forward, );
                    }
                }
                else
                    targetEnemy = GetClosestEnemy();
            }
            else
            {
                UnityEngine.Object.Destroy(gameObject);
            }
        }

        private EnemyAI? GetClosestEnemy()
        {
            EnemyAI result = null;
            float closestDist = 2000f;

            Collider[] colliders = RoundManager.Instance.tempColliderResults;
            int colliderCount = Physics.OverlapSphereNonAlloc(gameObject.transform.position, 200f, colliders, visibleThreatsMask, QueryTriggerInteraction.Collide);

            for (int i = 0; i < colliderCount; i++)
            {
                Collider currentCollider = colliders[i];
                EnemyAICollisionDetect enemyAICollision = currentCollider.transform.GetComponent<EnemyAICollisionDetect>();
                EnemyAI enemy = enemyAICollision is not null ? enemyAICollision.mainScript : currentCollider.transform.GetComponent<EnemyAI>();

                if (enemy is not null)
                {
                    float distFromEnemy = Vector3.Distance(gameObject.transform.position, enemy.transform.position);
                    bool l = Physics.Linecast(gameObject.transform.position, enemy.transform.position, 256);
                    
                    if (!l && distFromEnemy < closestDist)
                    {
                        closestDist = distFromEnemy;
                        result = enemy;
                    }
                }
            }

            return result;
        }
    }
}
