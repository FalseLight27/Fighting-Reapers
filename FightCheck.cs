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
            var collider = __instance.GetComponentInParent<Collider>();

            Logger.Log(Logger.Level.Info, "SEEK INITIAL CHECK 1 PASSED");

            var rb = __instance.GetComponentInParent<Rigidbody>();
           
            var thisReaper = __instance.GetComponentInParent<ReaperLeviathan>();
            var tr = ra.mouth.transform;
            var og = ra.mouth.transform.position;
            var startPosition = tr.forward;
            var endPosition = og + (4f * tr.forward);
            var leftPosition = og + (-2.5f * tr.right) + (2f * tr.forward);

            Logger.Log(Logger.Level.Info, "SEEK INITIAL CHECK 2 PASSED");

            

            Logger.Log(Logger.Level.Info, "SEEK INITIAL CHECK 3 PASSED");

            

            if (__instance.IsHoldingVehicle())

            {
                fb.clawsBusy = true;
            }

            else 
            {
                fb.clawsBusy = false;
            }

            //fb.canClaw = ar.SenseClawable();



            Logger.Log(Logger.Level.Info, "SEEK INITIAL CHECK 4 PASSED");

            


            // Spherecast to detect biteable objects in front of the reaper's mouth
            //clawables = Physics.OverlapSphere(__instance.transform.forward * 2, 10f);


            Logger.Log(Logger.Level.Info, "SEEK INITIAL CHECK 5 PASSED");



           

            // Regulate reaper aggression to prevent too much rapid-fire clawing

            if (__instance.Aggression.Value > 0.30)
            {
                __instance.Aggression.Add(Time.deltaTime * -0.01f);
            }

            Logger.Log(Logger.Level.Info, "SEEK INITIAL CHECK 6 PASSED");


            //clawableObj = ar.SenseClawable(clawables);

                     

            Logger.Log(Logger.Level.Info, "SEEK INITIAL CHECK 7 PASSED");

            
            
            
                    
                
                
                

                Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 1");

            

            

                                 
                                   
                                                         
            //bm.OnTouch(biteObject);                                                         

            if (fb.targetReaper != null && fb.targetReaper != thisReaper)

            {
                
                fb.targetFound = true;
                fb.targetDist = Vector3.Distance(__instance.transform.position, fb.targetReaper.transform.position);
                __instance.Aggression.Add(UnityEngine.Random.Range(0.31f, 0.51f));

                if (Time.time > fb.nextTarget)
                {
                    fb.nextTarget = Time.time + fb.targetCD;
                    ar.DesignateTarget(fb.targetReaper.transform);

                }

                
                ar.StartPerform(__instance);
                ar.UpdateAttackPoint();
                Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 2");

                if (fb.targetFound == true)
                {
                    ar.Approach();

                    Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 3");

                    if (__instance.Aggression.Value >= 400f && __instance.Tired.Value < 0.30f && Time.time > fb.nextMove && fb.moveChance > 0.50f)
                    {
                        fb.nextMove = Time.time + fb.randomCooldown;
                        ar.Charge(__instance);
                        ErrorMessage.AddMessage("RAMMING SPEED");
                    }

                    

                    Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 4");

                    

                    if (fb.targetDist <= 80f)
                    {                                                

                        // 80 percent of the time, a Reaper will twist its body in order to fit its claws around the enemy Reaper's body
                        if (fb.moveChance <= 0.80f && Time.time > fb.nextMove && rb.velocity.magnitude > 4f)
                        {
                            fb.nextMove = Time.time + fb.randomCooldown;
                            bm.Twist();                            

                        }

                        // 80 percent of the time, a Reaper will reel back to prepare for a lunge

                        if (fb.moveChance < 0.80f && Time.time > fb.nextMove)
                        {
                            fb.nextMove = Time.time + fb.randomCooldown;
                            bm.Reel();                            
                        }
                        Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 5");

                    }

                    if (fb.targetDist <= 15f && Time.time > fb.nextPush)
                    {
                        fb.nextPush = Time.time + fb.randomCooldown;
                        fb.canPush = true;
                        bm.Push(__instance, fb.targetReaper);

                        ErrorMessage.AddMessage($"PUSHING");
                        Logger.Log(Logger.Level.Info, "PUSH CHECK PASSED");
                    }
                    



                        // If the Reaper is within 50m of the enemy, the Reaper will lunge at it.

                    if (fb.targetDist < 70f && fb.targetDist > 10f && fb.moveChance >= 0.10f && __instance.Tired.Value < 0.90f && Time.time > fb.nextMove)
                    {
                        fb.nextMove = Time.time + fb.randomCooldown;
                        bm.Lunge(__instance);                        
                        Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 6");

                    }

                    if (Time.time > fb.nextNotif)
                    {
                        fb.nextNotif = Time.time + fb.notifRate;
                        //Logger.Log(Logger.Level.Debug, $"ENEMY REAPER IS : {fb.targetDist} AWAY FROM ME");
                    }
                }
            }

            else if (fb.targetReaper == null)
            {
                                
                if (Time.time > fb.nextNotif)
                {
                    fb.targetReaper = ar.Search();
                    fb.nextNotif = Time.time + fb.notifRate;
                    Logger.Log(Logger.Level.Debug, $"NO ENEMY REAPERS IN VICINITY");
                    ErrorMessage.AddMessage($"NO ENEMY REAPERS IN VICINITY");
                }
                ar.StopPerform(__instance);
                Logger.Log(Logger.Level.Info, "SEEK_ENEMY_REAPERS PASSED CHECK 7");
            }
                
            if (fb.targetDist < 20f)
            {
                ErrorMessage.AddMessage("CLAWS READY");

                // If the enemy within range, the Reaper will do a pincer attack with its claws
                
                    
                    bm.Claw();
                    Logger.Log(Logger.Level.Info, "CLAW CHECK PASSED");

                
               
                /*
                if ((fb.clawPoint.collider != null) && (fb.clawPoint.distance < 2f) && (fb.clawPoint.collider != thisReaper.GetComponentInChildren<Collider>()))
                {
                    bm.Bite(fb.clawPoint.collider);
                    Logger.Log(Logger.Level.Info, "BITE CHECK PASSED");

                }
                */


                //bm.Tackle();

            }

            else if (fb.targetDist > 20f)
            {
                ErrorMessage.AddMessage("CANNOT CLAW");
                Logger.Log(Logger.Level.Info, "CANNOT CLAW");

                if (fb.clawsBusy)

                {
                    ErrorMessage.AddMessage("CLAWS BUSY");
                    Logger.Log(Logger.Level.Info, "CLAWS BUSY");
                }

                if (fb.isClawing)
                {
                    
                    bm.StopClaw();
                    Logger.Log(Logger.Level.Info, "STOPCLAW CHECK PASSED");
                }
            }



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


