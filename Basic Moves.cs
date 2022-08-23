using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UWE;
using HarmonyLib;
using Logger = QModManager.Utility.Logger;


namespace FightingReapers
{
    public class BasicFightingMoves : ReaperMeleeAttack
    {

        public static BasicFightingMoves main;
        internal Collider collider2;
        public Vector3 reaperMouth;
        public new float biteDamage = 100f;
        private BehaviourType bt;
        public LiveMixin enemyLiveMixin;
        public float timeToUnfreeze = 0f;
        public float freezeCD = 2f;
        public Animation anim;
        public GameObject prefab;
        public GameObject prefab2;

        public float timeObjGrabbed;
        public float timeObjReleased;
        public Quaternion ObjInitialRotation;
        public Vector3 ObjInitialPosition;

        public float creatureDPS = 44f;



        public override float GetBiteDamage(GameObject target)
        {
            var fb = GetComponentInParent<FightBehavior>();

            if (target.GetComponent<SubControl>() != null)
            {
                ErrorMessage.AddMessage($"BITE ATTACK!");
                return this.cyclopsDamage;
            }

            if (fb.critChance <= 0.40f)
            {
                ErrorMessage.AddMessage($"CRITICAL BITE!");
                return 200f;
            }
            ErrorMessage.AddMessage($"BITE ATTACK!");
            return base.GetBiteDamage(target);
        }



        /*
        private void StartSetAnimParam(string paramName, float duration)
        {
            base.StartCoroutine(this.SetAnimParamAsync(paramName, false, duration));
        }

        private IEnumerator SetAnimParamAsync(string paramName, bool value, float duration)
        {
            yield return new WaitForSeconds(duration);
            this.animator.SetBool(paramName, value);
            yield break;
        }

        */



        public void Claw()
        {
            var fb = this.GetComponentInParent<FightBehavior>();

            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();


            SafeAnimator.SetBool(thisReaper.GetAnimator(), "attacking", true);
            thisReaper.GetAnimator().speed = 1.8f;

            /*

            if (thisReaper.Aggression.Value < 0.5f)
            {
                thisReaper.Aggression.Value += 5f;
            }

            */

            if (fb.targetDist < 10f && fb.targetDist >= 0)

            {
                thisReaper.GetAnimator().speed *= 2f;
            }

            fb.isClawing = true;
            //thisReaper.Tired.Add(0.035f);

            Logger.Log(Logger.Level.Info, "CLAW PASSED CHECK 1");

            /*

            foreach (KeyValuePair<GameObject, Collider> pair in fb.clawObjects)

            {
                GameObject mandible = pair.Key;
                Collider clawed = pair.Value;

                LiveMixin enemyLiveMixin = clawed.GetComponentInParent<LiveMixin>();
                BreakableResource breakable = clawed.GetComponentInParent<BreakableResource>();

                Logger.Log(Logger.Level.Info, "CLAW PASSED CHECK 1.1");
                              
                Vector3 position = clawed.ClosestPointOnBounds(mandible.transform.position);
                Vector3 bleedPoint = clawed.transform.InverseTransformPoint(position);

                Logger.Log(Logger.Level.Info, "CLAW PASSED CHECK 1.2");                

                Logger.Log(Logger.Level.Info, "CLAW PASSED CHECK 2");

                    if (enemyLiveMixin)
                    {
                        enemyLiveMixin.TakeDamage(80f, position, DamageType.Normal, thisReaper.gameObject);
                        

                        fb.BloodGen(clawed);

                        GameObject blood1 = Instantiate(fb.CachedBloodPrefab, bleedPoint, Quaternion.identity);
                        
                        blood1.SetActive(true);
                        
                        Destroy(blood1, 4f);
                        

                        Logger.Log(Logger.Level.Info, "CLAW PASSED CHECK 3");

                    }

                    if (breakable)
                    {
                        breakable.HitResource();
                        breakable.HitResource();
                        breakable.HitResource();
                    }

                    VFXSurface component1 = clawed.GetComponentInParent<VFXSurface>();
                    
                    VFXSurfaceTypeManager.main.Play(component1, VFXEventTypes.impact, position, Quaternion.identity, thisReaper.transform);
                    

                    Logger.Log(Logger.Level.Info, "CLAW PASSED CHECK 4");
                
            }

            */




            ErrorMessage.AddMessage($"CLAW ATTACK!");
        }

        public void StopClaw()
        {
            var fb = this.GetComponentInParent<FightBehavior>();
            var rm = this.GetComponentInParent<ReaperMeleeAttack>();
            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();


            SafeAnimator.SetBool(thisReaper.GetAnimator(), "attacking", false);
            thisReaper.GetAnimator().speed = 1f;
            thisReaper.Aggression.Value *= 0.5f;
            fb.isClawing = false;


            ErrorMessage.AddMessage($"NOT CLAWING");


        }

        internal void FreezeRotation()
        {
            IEnumerator FreezeForSeconds()
            {
                var thisReaperBody = this.GetComponentInParent<Rigidbody>();
                thisReaperBody.constraints = RigidbodyConstraints.FreezeRotationX;
                yield return new WaitForSeconds(5);
                thisReaperBody.constraints = RigidbodyConstraints.None;

            }

            StartCoroutine(FreezeForSeconds());
        }

        public void Twist()

        {
            var fb = this.GetComponent<FightBehavior>();
            var thisReaperBody = this.GetComponentInParent<Rigidbody>();
            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();

            int directionChoose = UnityEngine.Random.Range(1, 3);


            switch (directionChoose)
            {
                //Roll right
                case 1:
                    thisReaperBody.AddTorque(thisReaperBody.transform.forward * 3.5f, ForceMode.VelocityChange);
                    Invoke("FreezeRotation", 2.5f);
                    break;

                //Roll left
                case 2:
                    thisReaperBody.AddTorque(thisReaperBody.transform.forward * -3.5f, ForceMode.VelocityChange);
                    Invoke("FreezeRotation", 2.5f);
                    break;
            }

            Logger.Log(Logger.Level.Debug, $"TWIST ROTATION: {thisReaperBody.rotation}");
        }

        public void Lunge(Creature creature)
        {
            var fb = this.GetComponent<FightBehavior>();
            var ar = this.GetComponentInParent<AttackReaper>();
            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();
            var rm = this.GetComponentInParent<ReaperMeleeAttack>();
            var thisReaperBody = this.GetComponentInParent<Rigidbody>();

            if (creature.Tired.Value < 0.90f)
            {
                thisReaperBody.AddForce(Vector3.Normalize(rm.mouth.transform.position - fb.targetReaper.transform.position) * 45f, ForceMode.VelocityChange);
            }
            else if (creature.Tired.Value >= 0.90f)
            {
                thisReaperBody.AddForce(Vector3.Normalize(rm.mouth.transform.position - fb.targetReaper.transform.position) * 30f, ForceMode.VelocityChange);
            }

            Logger.Log(Logger.Level.Debug, $"LUNGING AT {thisReaperBody.velocity.magnitude} M/S");

        }

        public void Push(Creature creature, GameObject go)
        {
            var fb = this.GetComponent<FightBehavior>();
            var ar = this.GetComponentInParent<AttackReaper>();
            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();
            var rm = this.GetComponentInParent<ReaperMeleeAttack>();
            var thisReaperBody = this.GetComponentInParent<Rigidbody>();

            //var swim = creature.gameObject.EnsureComponent<SwimBehaviour>();

            Vector3 position = go.transform.forward * -2;

            Logger.Log(Logger.Level.Debug, $"PUSH PASSED CHECK 1");

            if (creature.Tired.Value < 0.90f)
            {
                //swim.SwimTo(position, 25f);
                thisReaperBody.AddForce((rm.mouth.transform.forward * 40f) + (rm.mouth.transform.right * UnityEngine.Random.Range(-20f, 20f)), ForceMode.VelocityChange);
                Logger.Log(Logger.Level.Debug, $"PUSH PASSED CHECK 2");
            }
            else if (creature.Tired.Value >= 0.90f)
            {
                //swim.SwimTo(position, 15f);
                thisReaperBody.AddForce((rm.mouth.transform.forward * 30f) + (rm.mouth.transform.right * UnityEngine.Random.Range(-12f, 12f)), ForceMode.VelocityChange);
                Logger.Log(Logger.Level.Debug, $"PUSH PASSED CHECK 3");
            }

            Logger.Log(Logger.Level.Debug, $"PUSHING AT {thisReaperBody.velocity.magnitude} M/S");

        }


        public IEnumerator Reel()
        {
            var fb = this.GetComponent<FightBehavior>();
            var ar = this.GetComponentInParent<AttackReaper>();
            var thisCreature = this.GetComponentInParent<Creature>();
            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();
            var thisReaperBody = this.GetComponentInParent<Rigidbody>();
            var rm = this.GetComponentInParent<ReaperMeleeAttack>();
            var tr = rm.mouth.transform;
            var og = rm.mouth.transform.forward;

            thisReaperBody.AddForce(og + (9 * tr.up) + -(7 * tr.forward), ForceMode.VelocityChange);
            
            if (ar.currentTarget != null)
            {
                yield return new WaitForSeconds(1f);
                Lunge(thisCreature);
            }
            Logger.Log(Logger.Level.Debug, $"REELING!");
            yield break;
        }


        public void Tackle()
        {

            //(NON-FUNCTIONAL. This method is meant to make the reaper do a ramming move with its horn)

            var fb = this.GetComponentInParent<FightBehavior>();

            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();

            var thisReaperBody = this.GetComponentInParent<Rigidbody>();

            var velocity = thisReaperBody.velocity.magnitude;

            RaycastHit hit;

            Physics.SphereCast(mouth.transform.position, 1f, mouth.transform.up, out hit, 2.5f);

            if (hit.collider != null)

            {
                Vector3 position = hit.collider.ClosestPointOnBounds(this.mouth.transform.up);
                Rigidbody hitBody = hit.collider.GetComponentInParent<Rigidbody>();

                LiveMixin enemyLiveMixin = hit.collider.GetComponentInParent<LiveMixin>();

                if (velocity > 70f)

                {
                    if (hitBody)
                    {
                        hitBody.AddForceAtPosition(thisReaperBody.transform.forward * velocity, this.mouth.transform.position, ForceMode.Impulse);
                    }

                    if (enemyLiveMixin)
                    {
                        enemyLiveMixin.TakeDamage(velocity / 2, position, DamageType.Collide, thisReaper.gameObject);
                        UnityEngine.Object.Instantiate(enemyLiveMixin.damageEffect, position, Quaternion.identity);
                    }

                    VFXSurface component = hit.collider.GetComponent<VFXSurface>();
                    VFXSurfaceTypeManager.main.Play(component, VFXEventTypes.impact, mouth.transform.position, Quaternion.identity, thisReaper.transform);

                }

            }

        }

        // --------------------------------------PREY GRABBING BEHAVIOR (WIP!)-------------------------------------------\\

        public void ReleaseObject()
        {
            var fb = this.GetComponent<FightBehavior>();

            if (fb.holdingObject != null)
            {

                fb.holdingObject.GetComponent<Rigidbody>().isKinematic = false;
                //fb.holdingObject.collisionModel.SetActive(true);
                fb.holdingObject = null;
                timeObjReleased = Time.time;
            }
            fb.holdingEcoType = EcoTargetType.None;
            base.CancelInvoke("DamageVehicle");
            //this.seamothGrabSound.Stop();
        }

        // Token: 0x06000944 RID: 2372 RVA: 0x00039814 File Offset: 0x00037A14
        private void DamageObject()
        {
            var fb = this.GetComponent<FightBehavior>();

            if (fb.holdingObject != null)
            {
                var liveMixin = fb.holdingObject.GetComponent<LiveMixin>();

                liveMixin.TakeDamage(creatureDPS, default(Vector3), DamageType.Drill, null);
            }
        }

        public void GrabObject(GameObject grabobj)

        {
            var fb = this.GetComponent<FightBehavior>();
            var eco = grabobj.GetComponent<EcoTarget>();


            bool isReefBack = grabobj.GetComponent<Reefback>();

            grabobj.GetComponent<Rigidbody>().isKinematic = true;

            //vehicle.collisionModel.SetActive(false);

            fb.holdingObject = grabobj;
            fb.holdingEcoType = eco.GetTargetType();

            //this.Aggression.Value = 0f;



            this.timeObjGrabbed = Time.time;
            this.ObjInitialRotation = grabobj.transform.rotation;
            this.ObjInitialPosition = grabobj.transform.position;

            /*

            this.seamothGrabSound.Play();

            
            base.Invoke("ReleaseVehicle", 8f + UnityEngine.Random.value * 5f);

            */

            if ((fb.holdingEcoType == EcoTargetType.Shark || fb.holdingEcoType == EcoTargetType.Whale) && !isReefBack)

            {
                Reap(grabobj);
            }


        }

        public bool IsHoldingObj()
        {
            var fb = this.GetComponent<FightBehavior>();

            return fb.holdingObject != null;
        }

        public void Reap(GameObject prey)
        {

            var fb = this.GetComponent<FightBehavior>();
            var rm = this.GetComponentInParent<ReaperMeleeAttack>();
            var rl = this.GetComponentInParent<ReaperLeviathan>();

            var creature = this.GetComponentInParent<Creature>();
            var preyCreature = prey.GetComponent<Creature>();

            /*

            if (prey.EcoTargetType && this.holdingVehicle == null)
            {
                this.ReleaseVehicle();
            }

            */



            //SafeAnimator.SetBool(base.GetAnimator(), "exo_attack", this.IsHoldingExosuit());

            InvokeRepeating("DamageObject", 1f, 1f);

        }

        

        [HarmonyPatch(typeof(ReaperLeviathan))]
        [HarmonyPatch("OnTakeDamage", new Type[] { typeof(DamageInfo) })]

        public class ReleaseObjectPatch
        {
            [HarmonyPostfix]

            public static void ReleaseObjPatch(ReaperLeviathan __instance, DamageInfo damageInfo)
            {
                var fb = __instance.GetComponentInParent<FightBehavior>();
                var bm = __instance.GetComponentInParent<BasicFightingMoves>();

                if ((damageInfo.type == DamageType.Electrical || damageInfo.type == DamageType.Poison) && fb.holdingObject != null)
                {
                    bm.ReleaseObject();
                }
            }
        }

        [HarmonyPatch(typeof(ReaperLeviathan))]
        [HarmonyPatch("Update")]
        public class HoldObjUpdate
        {
            [HarmonyPostfix]

            public static void UpdateHoldObj(ReaperLeviathan __instance)
            {
                var fb = __instance.GetComponentInParent<FightBehavior>();
                var bm = __instance.GetComponentInParent<BasicFightingMoves>();
                var al = __instance.GetComponentInParent<AttackLastTarget>();
                
                

                if (al.currentTarget != null)

                {

                    
                    var eco = al.currentTarget.GetComponent<EcoTarget>();
                    var ecoType = eco.GetTargetType();

                    bool isReefBack = al.currentTarget.GetComponent<Reefback>();

                    bool isPrey = ecoType == EcoTargetType.Shark || ecoType == EcoTargetType.Whale;

                    //vehicle.collisionModel.SetActive(false);

                    //fb.holdingObject = grabobj;
                  

                    float distToHoldTarget = Vector3.Distance(__instance.transform.position, al.currentTarget.transform.position);

                    

                    if (distToHoldTarget < 5f && isPrey && !isReefBack)
                    {
                        bm.GrabObject(al.currentTarget);
                    }

                }

                

                if (fb.holdingObject != null)
                {
                    bm.StopClaw();
                    fb.clawsBusy = true;

                    SafeAnimator.SetBool(__instance.GetAnimator(), "seamoth_attack", bm.IsHoldingObj());

                    float num = Mathf.Clamp01(Time.time - bm.timeObjGrabbed);
                    if (num >= 1f)
                    {
                        fb.holdingObject.transform.position = __instance.seamothAttachPoint.position;
                        fb.holdingObject.transform.rotation = __instance.seamothAttachPoint.transform.rotation;
                        return;
                    }
                    fb.holdingObject.transform.position = (__instance.seamothAttachPoint.position - bm.ObjInitialPosition) * num + __instance.vehicleInitialPosition;
                    fb.holdingObject.transform.rotation = Quaternion.Lerp(bm.ObjInitialRotation, __instance.seamothAttachPoint.transform.rotation, num);
                }
            }

        }

}
}
