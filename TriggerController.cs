using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWE;
using UnityEngine;
using Logger = QModManager.Utility.Logger;

namespace FightingReapers
{
    
    class TriggerController : MonoBehaviour
    {
        public GameObject thisMandible; //this trigger controller is for mandibles
        
        private static readonly FMODAsset clawSound = FightBehavior.GetFmodAsset("event:/creature/reaper/attack_player_claw");

        public void Awake()
        {
            thisMandible = this.gameObject;
            
            Logger.Log(Logger.Level.Debug, "This mandible established");

        }

        public void OnTriggerEnter(Collider other)
        {
            var thisReaper = this.GetComponentInParent<ReaperLeviathan>();
            var fb = thisMandible.GetComponentInParent<FightBehavior>();
            var thisLiveMixin = thisMandible.GetComponentInParent<LiveMixin>();
            LiveMixin enemyLiveMixin = other.GetComponentInParent<LiveMixin>();
            bool isCreature = other.GetComponentInParent<Creature>();
            BreakableResource breakable = other.GetComponentInParent<BreakableResource>();
            VFXSurface component1 = other.GetComponentInParent<VFXSurface>();

            Vector3 dist = (other.ClosestPointOnBounds(thisMandible.transform.forward) - other.transform.position);
            Vector3 position = (other.ClosestPointOnBounds(thisMandible.transform.position));
            //Vector3 vector = position - other.transform.position;
            Vector3 bleedPoint = position + (dist/5);

            var rootObject = UWE.Utils.GetEntityRoot(other.gameObject) ?? other.gameObject;
            var rootEco = BehaviourData.GetEcoTargetType(rootObject);


            if (fb.isClawing && (other != thisMandible.GetComponentInParent<Collider>()) && (thisLiveMixin.health != 0))
            {

                VFXSurfaceTypeManager.main.Play(component1, VFXEventTypes.impact, position, Quaternion.identity, thisReaper.transform);

                if (enemyLiveMixin)
                {
                    enemyLiveMixin.TakeDamage((UnityEngine.Random.Range(25f, 40f)), position, DamageType.Drill, thisReaper.gameObject);
                    Utils.PlayFMODAsset(clawSound, thisReaper.transform.position);


                    //To make blood persist even after LiveMixin is at 0 hp

                    if (enemyLiveMixin.health == 0 && rootEco == EcoTargetType.Leviathan)
                    {
                        fb.Bleed(other, bleedPoint, 11f, 8f, 2f);
                    }

                }

                

                if (breakable)
                {
                    breakable.HitResource();
                    breakable.HitResource();
                    breakable.HitResource();
                }

                //fb.clawObjects.Add(thisMandible, other);                        

            }
        }

    }
}
