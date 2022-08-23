using UWE;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QModManager.API.ModLoading;
using HarmonyLib;
using Logger = QModManager.Utility.Logger;

namespace FightingReapers
{
    public class FightBehavior : MonoBehaviour
    {

        public GameObject holdingObject;
        
        public Transform grabPoint;

        public Vector3 enemyInitialPosition;
        public RaycastHit bitePoint;
        public RaycastHit clawPoint;
        public Collider biteObject;
        public Collider clawObject;
        public GameObject targetObj;
        public GameObject targetReaper;

        public EcoTargetType holdingEcoType;

        public bool targetFound;
        public bool isClawing;
        public bool canPush;
        public bool clawsBusy;
        public bool canClaw;

        public float moveChance;
        public float attackChance;
        public float critChance;

        public float targetDist;
        public float biteDist;
        public float timeEnemyGrabbed;
        
        public float notifRate = 4f;
        public float curiosityUpdateRate = 2.2f;

        public float nextNotif = 0.0f;
        public float nextMove = 0.0f;
        public float nextPush = 0.0f;
        public float nextAttack = 0.0f;
        public float nextTarget = 0.0f;
        public float nextCuriosityUpdate = 0.0f;
        public float randomCooldown;
        public float targetCD = 0.5f;
        public float attackCD;

        public float timeBleedAgain;
        public float bleedInterval = 0.7f;

        public GameObject bloodPrefab;
        internal GameObject CachedBloodPrefab;
        public float lifetimeScale = 12f;
        public float startSizeScale = 8f;
        public float colorScale = 2f;

        public Transform mouth;
        public Transform LTMandible;
        public Transform RTMandible;
        public Transform LBMandible;
        public Transform RBMandible;

        public SphereCollider mouthCol;
        public SphereCollider ltm;
        public SphereCollider rtm;
        public SphereCollider lbm;
        public SphereCollider rbm;

        public Rigidbody mouthRB;
        public Rigidbody ltmRB;
        public Rigidbody rtmRB;
        public Rigidbody lbmRB;
        public Rigidbody rbmRB;

        public Collider clawable;

        public Dictionary<GameObject, Collider> clawObjects = new Dictionary<GameObject, Collider>();

        public GameObject thisReaper;

        public static readonly FMODAsset flinchSound = GetFmodAsset("event:/creature/reaper/attack_player");
        public static readonly FMODAsset painSound = GetFmodAsset("event:/creature/reaper/attack_seamoth");

        public Gradient bloodGradient;

        public void FixedUpdate()

        {
            moveChance = UnityEngine.Random.Range(0.0f, 1.01f);
            attackChance = UnityEngine.Random.Range(0.0f, 1.01f);
            critChance = UnityEngine.Random.Range(0f, 1.0001f);
            randomCooldown = UnityEngine.Random.Range(3f, 6f);
            attackCD = UnityEngine.Random.Range(0.07f, 2f);

        }


        

        public void BloodGen(Collider target, float lifeTime, float lifeTimeScale, float startSizeScale)
        {

            

            if (target == null)
            {
                return;
            }
            LiveMixin lm = target.GetComponentInParent<LiveMixin>();
            if (lm == null)
            {
                return;
            }
            if (lm.data == null)
            {
                return;
            }
            bloodPrefab = lm.data.damageEffect;

            if (bloodPrefab == null)
            {
                return;
            }

            CachedBloodPrefab = Instantiate(bloodPrefab);

            

            Logger.Log(Logger.Level.Debug, "Blood generated!");
            CachedBloodPrefab.SetActive(false);
            foreach (ParticleSystem ps in CachedBloodPrefab.GetComponentsInChildren<ParticleSystem>())
            {
                var main = ps.main;
                var col = ps.colorOverLifetime;
                float alpha = 0.5f;

                var bloodColor = bloodPrefab.GetComponentInChildren<Renderer>().material.color;



                main.startLifetime = new ParticleSystem.MinMaxCurve(main.startLifetime.constant * lifeTimeScale);
                main.startSize = new ParticleSystem.MinMaxCurve(main.startSize.constant * startSizeScale);

                Gradient grad = new Gradient();
                grad.SetKeys(new GradientColorKey[] { new GradientColorKey(bloodColor, 999f), new GradientColorKey(bloodColor, 999f) }, new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(0.0f, 8.0f) });

                col.color = grad;



            }
            VFXDestroyAfterSeconds destroyAfterSeconds = CachedBloodPrefab.GetComponent<VFXDestroyAfterSeconds>();
            destroyAfterSeconds.lifeTime = lifeTime;
            Destroy(destroyAfterSeconds);
        }
        

        public IEnumerator BloodPuff(Collider collider, Vector3 bleedPoint, float lifeTime, float lifeTimeScale, float startSizeScale)

        {
            BloodGen(collider, lifeTime, lifeTimeScale, startSizeScale);

            GameObject blood1 = Instantiate(CachedBloodPrefab, bleedPoint, Quaternion.identity);
            blood1.SetActive(true);

            //yield return new WaitForSeconds(0.5f);

            yield break;

            /*

            BloodGen(collider, lifeTime, lifeTimeScale/2, startSizeScale/2);
            GameObject blood2 = Instantiate(CachedBloodPrefab, bleedPoint, Quaternion.identity);
            blood2.SetActive(true);

            */

            //CoroutineHost.StartCoroutine(BloodFade(blood1));
        }

        public IEnumerator Bleed(Collider collider, Vector3 bleedPoint, float lifeTime, float lifeTimeScale, float startSizeScale)

        {
            BloodGen(collider, lifeTime, lifeTimeScale, startSizeScale);

            var startTime = DateTime.UtcNow;

            

            Vector3 position = collider.transform.InverseTransformPoint(bleedPoint);

            GameObject blood1 = Instantiate(CachedBloodPrefab, position, Quaternion.identity);
            blood1.SetActive(true);

            yield return new WaitForSeconds(0.1f);

            if (Time.time > timeBleedAgain)
            {
                timeBleedAgain = Time.time + bleedInterval;
                Instantiate(CachedBloodPrefab, position, Quaternion.identity);
            }

            yield return new WaitForSeconds(6f);

            yield break;

            
        }





        public static FMODAsset GetFmodAsset(string audioPath)
        {
            FMODAsset asset = ScriptableObject.CreateInstance<FMODAsset>();
            asset.path = audioPath;
            return asset;
        }

    }



    [HarmonyPatch(typeof(Creature))]
    [HarmonyPatch("Start")]

    public class AddAttackReaperBehavior
    {
        [HarmonyPostfix]
        public static void AddBehavior(Creature __instance)
        {
            bool isReaper = __instance.GetComponentInChildren<ReaperLeviathan>();
            bool isGhost = __instance.GetComponentInChildren<GhostLeviathan>();
            bool isDragon = __instance.GetComponentInChildren<SeaDragon>();
            var rm = __instance.GetComponentInChildren<ReaperMeleeAttack>();
            Vector3 baseColPos;
            Vector3 newColDir;
            float baseColRad;

            if (isReaper)
            {
                __instance.gameObject.AddComponent<FightBehavior>();
                __instance.gameObject.AddComponent<AttackReaper>();
                __instance.gameObject.AddComponent<BasicFightingMoves>();                
                __instance.gameObject.EnsureComponent<LiveMixin>();
                __instance.gameObject.EnsureComponent<VFXSurface>();
                __instance.gameObject.EnsureComponent<AggressiveWhenSeeTarget>();
                __instance.gameObject.EnsureComponent<AggressiveOnDamage>();
                //__instance.gameObject.EnsureComponent<NibbleMeat>();
                //__instance.gameObject.AddComponent<SwimToMeat>();
                

                var fb =__instance.gameObject.GetComponent<FightBehavior>();
                

                fb.thisReaper = __instance.gameObject;

                /*

                var baseCol = __instance.GetComponentInChildren<SphereCollider>();
                baseColPos = baseCol.center;
                baseColRad = baseCol.radius;
                newColDir = baseCol.transform.forward * -1;
                UnityEngine.Object.Destroy(__instance.gameObject.GetComponent<SphereCollider>());
                var newCollider = __instance.gameObject.AddComponent<CapsuleCollider>();
                newCollider.center = baseColPos;
                newCollider.radius = 0.95f * baseColRad;
                newCollider.direction = -1;

                baseCol.radius = 0.95f * baseCol.radius;                
                baseCol.center += -0.95f * baseCol.transform.forward;*/



                //fb.mouth = __instance.transform.Find("reaper_leviathan.root/neck/head/mouth_damage_trigger");
                fb.LBMandible = __instance.transform.Find("reaper_leviathan/root/neck/head/LB_mandable");
                fb.RBMandible = __instance.transform.Find("reaper_leviathan/root/neck/head/LB_mandable6");
                fb.LTMandible = __instance.transform.Find("reaper_leviathan/root/neck/head/LT_mandable");
                fb.RTMandible = __instance.transform.Find("reaper_leviathan/root/neck/head/RT_mandable");

                Logger.Log(Logger.Level.Info, "START CHECK 1 PASSED");

                //fb.mouthCol = fb.mouth.gameObject.AddComponent<SphereCollider>();
                //fb.mouthCol.radius = 1f;
                //fb.mouthCol.center = fb.mouth.transform.forward;
                fb.lbm = fb.LBMandible.gameObject.AddComponent<SphereCollider>();
                fb.lbm.radius = 0.5f;
                fb.lbm.center += 1.8f * fb.LBMandible.transform.forward;
                fb.rbm = fb.RBMandible.gameObject.AddComponent<SphereCollider>();
                fb.rbm.radius = 0.5f;
                fb.rbm.center += 1.8f * fb.RBMandible.transform.forward;
                fb.ltm = fb.LTMandible.gameObject.AddComponent<SphereCollider>();
                fb.ltm.radius = 0.5f;
                fb.ltm.center += 1.8f * fb.LTMandible.transform.forward;
                fb.rtm = fb.RTMandible.gameObject.AddComponent<SphereCollider>();
                fb.rtm.radius = 0.5f;
                fb.rtm.center += 1.8f * fb.RTMandible.transform.forward;

                Logger.Log(Logger.Level.Info, "START CHECK 2 PASSED");                

                Logger.Log(Logger.Level.Info, "START CHECK 3 PASSED");

                //fb.mouth.gameObject.AddComponent<MouthTriggerController>();
                fb.LBMandible.gameObject.AddComponent<TriggerController>();
                fb.RBMandible.gameObject.AddComponent<TriggerController>();
                fb.LTMandible.gameObject.AddComponent<TriggerController>();
                fb.RTMandible.gameObject.AddComponent<TriggerController>();

                Logger.Log(Logger.Level.Info, "START CHECK 4 PASSED");
              

                Logger.Log(Logger.Level.Info, "START CHECK 5 PASSED");

                //fb.mouthCol.isTrigger = true;
                fb.lbm.isTrigger = true;
                fb.rbm.isTrigger = true;
                fb.ltm.isTrigger = true;
                fb.rtm.isTrigger = true;

                Logger.Log(Logger.Level.Info, "START CHECK 6 PASSED, REAPER SPAWNED");

                /*
                var liveMixin = __instance.GetComponentInParent<LiveMixin>();

                SafeAnimator.SetBool(__instance.GetAnimator(), "attacking", true);
                global::Utils.PlayEnvSound(rm.playerAttackSound, __instance.transform.forward, 20f);

                UnityEngine.Object.Instantiate(liveMixin.damageEffect, __instance.transform.position, Quaternion.identity);
                */

            }

           

            if (isGhost || isDragon)
            {               

                
            }

        }
    }

    /*

    [HarmonyPatch(typeof(Creature))]
    [HarmonyPatch("OnDestroy")]

    public class Unlister
    {
        [HarmonyPostfix]
        public static void UnlistReaper(Creature __instance)
        {
            bool isReaper = __instance.GetComponentInChildren<ReaperLeviathan>();
            bool isGhost = __instance.GetComponentInChildren<GhostLeviathan>() || __instance.GetComponentInChildren<GhostLeviatanVoid>();
            bool isDragon = __instance.GetComponentInChildren<SeaDragon>();

            if (isReaper || isGhost || isDragon)
            {
                
                ErrorMessage.AddMessage($"REAPER DESTROYED");
                Logger.Log(Logger.Level.Info, "REAPER DESTROYED");
            }
        }
    }

    */

    [HarmonyPatch(typeof(Creature), nameof(Creature.OnKill))]

    public class AnimatorKiller
    {
        

        [HarmonyPostfix]
        public static void StopMoving(Creature __instance)
        {
            bool isReaper = __instance.GetComponentInChildren<ReaperLeviathan>();

            IEnumerator DeathThroes(Creature creature)
            {

                var animator = creature.GetComponentInChildren<Animator>();
                animator.speed = 0.8f;
                yield return new WaitForSeconds(1f);
                animator.speed = 0.6f;
                yield return new WaitForSeconds(1f);
                animator.speed = 0.4f;
                yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 1f));
                animator.speed = 0.1f;
                yield return new WaitForSeconds(1f);
                animator.speed = UnityEngine.Random.Range(0.1f, 0.5f);
                yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 1f));
                animator.speed = 0.2f;
                yield return new WaitForSeconds(1f);
                animator.speed = 0.1f;
                yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 1f));
                animator.speed = 0.2f;
                yield return new WaitForSeconds(1f);
                animator.speed = UnityEngine.Random.Range(0.1f, 0.5f);
                yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 1f));
                animator.speed = 0.2f;
                yield return new WaitForSeconds(1f);
                animator.speed = 0.1f;
                yield return new WaitForSeconds(1f);
                animator.speed = 0.01f;
            }

            if (isReaper)
            {
               
                var rm = __instance.GetComponentInChildren<ReaperMeleeAttack>();
                var fb = __instance.GetComponentInParent<FightBehavior>();
                //SafeAnimator.SetBool(__instance.GetAnimator(), "attacking", false);
                rm.animator.SetBool(MeleeAttack.biteAnimID, false);

                UnityEngine.Object.Destroy(fb.LBMandible.gameObject.GetComponent<TriggerController>());
                UnityEngine.Object.Destroy(fb.RBMandible.gameObject.GetComponent<TriggerController>());
                UnityEngine.Object.Destroy(fb.LTMandible.gameObject.GetComponent<TriggerController>());
                UnityEngine.Object.Destroy(fb.RTMandible.gameObject.GetComponent<TriggerController>());

                CoroutineHost.StartCoroutine(DeathThroes(__instance));

                ErrorMessage.AddMessage($"ANIMATOR DEACTIVATED");
                Logger.Log(Logger.Level.Info, "ANIMATOR DEACTIVATED");
            }
        }
    }

    [HarmonyPatch(typeof(ReaperLeviathan))]
    [HarmonyPatch("OnTakeDamage", new Type[] { typeof(DamageInfo) })]

    public class OnTakeDamagePatch

    {
        private static float timeBleedAgain;
        private static float bleedInterval = 0.7f;
        private static float timeSnapAgain;
        private static float snapInterval = UnityEngine.Random.Range(0.5f, 2.3f);

        public static void Snap(Creature creature, Vector3 position)
        {
            var rb = creature.GetComponentInParent<Rigidbody>();
            var rm = creature.GetComponentInParent<ReaperMeleeAttack>();

            if (Time.time > timeSnapAgain)
            {
                timeSnapAgain = Time.time + snapInterval;
                rb.AddForce(position * 10f, ForceMode.VelocityChange);

                Logger.Log(Logger.Level.Debug, "REFLEXIVE SNAP!");
                ErrorMessage.AddMessage("REFLEXIVE SNAP!");
            }

        }

        [HarmonyPostfix]
        public static void ReactToDamage(ReaperLeviathan __instance, DamageInfo damageInfo)
        {
            var fb = __instance.GetComponentInParent<FightBehavior>();
            var collider = __instance.GetComponentInParent<Collider>();
            var ar = __instance.GetComponentInParent<AttackReaper>();
            var reaperBody = __instance.GetComponentInParent<Rigidbody>();
            var creature = __instance.GetComponentInParent<Creature>();
            var aggro = __instance.GetComponentInParent<AggressiveOnDamage>();
            var swim = __instance.GetComponentInParent<SwimBehaviour>();
            var swimRandom = __instance.GetComponentInParent<SwimRandom>();
            var bm = __instance.GetComponentInParent<BasicFightingMoves>();
            var melee = __instance.GetComponentInParent<MeleeAttack>();
            var aggro2 = __instance.GetComponentInParent<AggressiveWhenSeeTarget>();
            var rm = __instance.GetComponentInChildren<ReaperMeleeAttack>();
            var liveMixin = __instance.GetComponentInParent<LiveMixin>();

            float bloodPuffScale = damageInfo.damage*5f;

            Vector3 bleedPoint = collider.ClosestPointOnBounds(damageInfo.position);



            if (damageInfo.damage >= 20f)
            {
                //Look at damage dealer

                __instance.Aggression.Add(damageInfo.damage * 0.05f);
                __instance.Friendliness.Add(-aggro.friendlinessDecrement);
                __instance.Tired.Add(-aggro.tirednessDecrement);
                __instance.Happy.Add(-aggro.happinessDecrement);
                global::Utils.PlayEnvSound(rm.playerAttackSound, __instance.transform.forward, 40f);
                ar.AssessThreat(damageInfo.dealer.transform);
                Utils.PlayFMODAsset(FightBehavior.flinchSound);
                Logger.Log(Logger.Level.Debug, "NOTICEABLE DAMAGE!");
                ErrorMessage.AddMessage("NOTICEABLE DAMAGE!");

                creature.flinch += 0.04f * damageInfo.damage;
            }


            if (damageInfo.damage >= 25f)
            {
                //Reflexively snap at damage dealer 

                var position = Vector3.Normalize(damageInfo.dealer.transform.position - __instance.transform.position);

                if (Time.time > fb.nextTarget)
                {
                    fb.nextTarget = Time.time + fb.targetCD;
                    ar.DesignateTarget(damageInfo.dealer.transform);
                    
                }

                if (damageInfo.type == DamageType.Drill)

                {

                    fb.BloodPuff(collider, bleedPoint, 11f, 8f, bloodPuffScale*2);
                    fb.Bleed(collider, bleedPoint, 11f, 8f, bloodPuffScale);

                    Logger.Log(Logger.Level.Info, "CLAW PASSED CHECK 3");

                }

                Snap(__instance, position);
                
            }


            if (damageInfo.damage >= 160f)
            {
                //Bleed profusely upon receiving excessive damage

                IEnumerator CritBleeding()
                {
                    var startTime = DateTime.UtcNow;

                    while (DateTime.UtcNow - startTime < TimeSpan.FromMinutes(0.5))
                    {                     
                        if (Time.time > timeBleedAgain)
                        {
                            timeBleedAgain = Time.time + bleedInterval;
                            Vector3 position = __instance.transform.InverseTransformPoint(damageInfo.position);
                            fb.BloodPuff(collider, damageInfo.position, 11f, 8f, 2f);
                            GameObject blood = UnityEngine.Object.Instantiate(fb.CachedBloodPrefab, position, Quaternion.identity);
                            blood.SetActive(true);
                            UnityEngine.Object.Destroy(blood, 10f);
                        }
                        yield return null;
                    }
                }

                CoroutineHost.StartCoroutine(CritBleeding());

                Logger.Log(Logger.Level.Debug, "CRITICAL HIT!");
                ErrorMessage.AddMessage("CRITICAL HIT!");
            }

            //TO DO: WRITE A STAGGERING/WEAKENED STATE 

        }
    }

    
        

    [QModCore]
        public static class FightPatcher
        {         

            [QModPatch]
            public static void Patch()
            {                
                var harmony = new Harmony("com.falselight.fightingreapers");
                harmony.PatchAll();
            }

        }

}





