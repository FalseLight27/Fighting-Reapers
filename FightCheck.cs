using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWE;
using UnityEngine;
using HarmonyLib;
using Logger = QModManager.Utility.Logger;

namespace FightingReapers 
{
    public class ListOfLeviathans : MonoBehaviour
    {
        public static List<Creature> LeviathanList = new List<Creature>();
        public GameObject closestLev;
        public float minDist;
        public float maxDist = 200f;
        public GameObject FindNearbyHostile(Creature me)
        {
            if (closestLev != null)
            {
                minDist = Vector3.Distance(me.transform.position, closestLev.gameObject.transform.position);
            }                       

            foreach (Creature lev in LeviathanList)
            {

                float dist = Vector3.Distance(lev.transform.position, me.transform.position);
                if (((lev.gameObject != me.gameObject &&  dist < maxDist && dist < minDist) || lev.gameObject != me.gameObject && me.GetCanSeeObject(lev.gameObject)) && lev.gameObject != null)
                {
                    closestLev = lev.gameObject;
                    minDist = dist;
                    ErrorMessage.AddMessage("CLOSEST LEVIATHAN ACQUIRED");                                                                                                                          
                }
            }

            return closestLev;
        }

    }


    [HarmonyPatch(typeof(ReaperLeviathan))]
    [HarmonyPatch("Update")]
    internal class FightCheck
    {
        
        [HarmonyPostfix]
        public static void SeekEnemyReaper(ReaperLeviathan __instance)
        {
            

            var fb = __instance.GetComponentInParent<FightBehavior>();
            var ar = __instance.GetComponentInParent<AttackReaper>();
            var bm = __instance.GetComponentInParent<BasicFightingMoves>();            
            var ra = __instance.GetComponentInParent<ReaperMeleeAttack>();
            var at = __instance.GetComponentInParent<AggressiveWhenSeeTarget>();
            var rb = __instance.GetComponentInParent<Rigidbody>();
            var lol = __instance.GetComponentInParent<ListOfLeviathans>();
            var tr = ra.mouth.transform;
            var og = ra.mouth.transform.position;
            var startPosition = tr.forward;
            var endPosition = og + (4f * tr.forward);
            var leftPosition = og + (-2.5f * tr.right) + (2f * tr.forward);
                                    

            GameObject potentialTarget = lol.FindNearbyHostile(__instance);

            

            //If the Reaper can see the enemy leviathan, or if the enemy leviathan is within 100m of the Reaper, it will become the designated target.           

            if (potentialTarget != null)

            {
                bool isReaper = potentialTarget.GetComponentInChildren<ReaperLeviathan>();
                bool isGhost = potentialTarget.GetComponentInChildren<GhostLeviathan>() || potentialTarget.GetComponentInChildren<GhostLeviatanVoid>();                
                bool isDragon = potentialTarget.GetComponentInChildren<SeaDragon>();

                if (isReaper || isGhost || isDragon)
                {
                    fb.targetReaper = potentialTarget;
                    
                    if (isReaper)
                    {
                        ErrorMessage.AddMessage("TARGET FOUND: REAPER");
                    }
                    if (isGhost)
                    {
                        ErrorMessage.AddMessage("TARGET FOUND: GHOST");
                    }
                    if (isDragon)
                    {
                        ErrorMessage.AddMessage("TARGET FOUND: DRAGON");
                    }
                }

            }

            fb.targetDist = Vector3.Distance(__instance.transform.position, fb.targetReaper.transform.position);

            // Spherecast to detect biteable objects in front of the reaper's mouth

            Physics.SphereCast(ra.mouth.transform.position, 2.5f, ra.mouth.transform.forward, out fb.clawPoint, 5f);

            // Regulate reaper aggression to prevent too much rapid-fire clawing

            if (__instance.Aggression.Value >= 0.80 && __instance.Aggression.Value > 0.15)
            {
                __instance.Aggression.Add(Time.deltaTime * -0.25f);
            }



            Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 1");            
                                                         
            //bm.OnTouch(biteObject);

            Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 2");

            // 50 percent of the time, if the enemy is within twice the bite distance, the Reaper will do a pincer attack with its claws
            if (fb.clawPoint.collider != null)
            {                
                bm.Claw();                                
                Logger.Log(Logger.Level.Info, "CLAW CHECK PASSED");

            }
            
            else if (fb.clawPoint.collider == null)
            {
                bm.StopClaw();
                Logger.Log(Logger.Level.Info, "STOPCLAW CHECK PASSED");
            }
            
            
            Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 3");

            if (fb.targetReaper != null)

            {
                
                fb.targetFound = true;
                __instance.Aggression.Add(UnityEngine.Random.Range(0.15f, 0.31f));
                ar.DesignateTarget(fb.targetReaper.transform);
                ar.StartPerform(__instance);
                ar.UpdateAttackPoint();


                if (fb.targetFound == true)
                {
                    ar.Approach();

                    if (__instance.Aggression.Value <= 0.80)
                    {
                        __instance.Aggression.Add(Time.deltaTime * 0.09f);
                    }                                                          

                    
                    if(__instance.Aggression.Value >= 0.30f && __instance.Tired.Value < 0.30f && Time.time > fb.nextMove && fb.moveChance > 0.50f)
                    {
                        fb.nextMove = Time.time + fb.randomCooldown;
                        ar.Charge(__instance);
                        ErrorMessage.AddMessage("RAMMING SPEED");
                    }

                    

                    Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 4");

                    

                    if (fb.targetDist <= 25f)
                    {                                                

                        // 50 percent of the time, a Reaper will twist its body in order to fit its claws around the enemy Reaper's body
                        if (fb.moveChance <= 0.80f && Time.time > fb.nextMove)
                        {
                            fb.nextMove = Time.time + fb.randomCooldown;
                            bm.Twist();                            

                        }

                        // 50 percent of the time, a Reaper will reel back to prepare for a lunge

                        if (fb.moveChance < 0.80f && Time.time > fb.nextMove)
                        {
                            fb.nextMove = Time.time + fb.randomCooldown;
                            bm.Reel();                            
                        }
                        Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 5");

                    }

                    

                    // If the Reaper is within 30m of the enemy, it will lunge at it.

                    if (fb.targetDist < 50f && fb.moveChance >= 0.9f && __instance.Tired.Value < 0.60f && Time.time > fb.nextMove)
                    {
                        fb.nextMove = Time.time + fb.randomCooldown;
                        bm.Lunge(__instance);                        
                        Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 6");

                    }

                    if (Time.time > fb.nextNotif)
                    {
                        fb.nextNotif = Time.time + fb.notifRate;
                        Logger.Log(Logger.Level.Debug, $"ENEMY REAPER IS : {fb.targetDist} AWAY FROM ME");
                    }
                }
            }
            else if (fb.targetReaper == null)
            {
                                
                if (Time.time > fb.nextNotif)
                {
                    fb.nextNotif = Time.time + fb.notifRate;
                    Logger.Log(Logger.Level.Debug, $"NO ENEMY REAPERS IN VICINITY");
                    ErrorMessage.AddMessage($"NO ENEMY REAPERS IN VICINITY");
                }
                ar.StopPerform(__instance);
                Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 7");
            }

            

            //bm.Tackle();

            Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 8");
        }

        


    }

    /*
    [HarmonyPatch(typeof(AggressiveWhenSeeTarget))]
    [HarmonyPatch("IsTargetValid", new Type[] { typeof(GameObject) })]

    internal class ValidTargetPatch
    {
        [HarmonyPrefix]
        public static void Prefix(AggressiveWhenSeeTarget __instance, GameObject target, ref bool __result)
        {

            bool isReaper = __instance.gameObject.GetComponentInParent<ReaperLeviathan>();

            if (isReaper)
            {

                __instance.ignoreSameKind = false;

                if (CraftData.GetTechType(target) == TechType.Reefback)
                {

                    __result = false;
                }
            }

            else
            {
                __instance.ignoreSameKind = true;
            }


        }

    } */

}


