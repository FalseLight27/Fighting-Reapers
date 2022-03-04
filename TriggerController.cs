using System;
using System.Collections.Generic;
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
        public GameObject thisReaper;
        public void Awake()
        {
            thisMandible = this.gameObject;
            thisReaper = this.GetComponentInParent<ReaperLeviathan>().gameObject;
            Logger.Log(Logger.Level.Debug, "This mandible established");

        }

        public void OnTriggerEnter(Collider other)
        {
            var fb = thisMandible.GetComponentInParent<FightBehavior>();
            var thisLiveMixin = thisMandible.GetComponentInParent<LiveMixin>();
            LiveMixin enemyLiveMixin = other.GetComponentInParent<LiveMixin>();
            BreakableResource breakable = other.GetComponentInParent<BreakableResource>();
            VFXSurface component1 = other.GetComponentInParent<VFXSurface>();
            Vector3 position = other.ClosestPointOnBounds(thisMandible.transform.position);
            Vector3 bleedPoint = other.transform.InverseTransformPoint(position);

            if (fb.isClawing && (other != thisMandible.GetComponentInParent<Collider>()) && (thisLiveMixin.health != 0))
            {

                VFXSurfaceTypeManager.main.Play(component1, VFXEventTypes.impact, position, Quaternion.identity, thisReaper.transform);

                if (enemyLiveMixin)
                {
                    enemyLiveMixin.TakeDamage(80f, position, DamageType.Normal, thisReaper.gameObject);


                    fb.BloodGen(other);

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

                //fb.clawObjects.Add(thisMandible, other);                        

            }
        }

    }
}
