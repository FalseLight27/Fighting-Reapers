using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWE;
using UnityEngine;
using HarmonyLib;
using Logger = QModManager.Utility.Logger;

namespace FightingReapers
{
    public class AttackReaper : CreatureAction
    {
		public LastTarget lastTarget;
		private float maxDistToLeash = 100f;
		public float swimVelocity = 10f;		
		private float swimInterval = 0.3f;
		public float timeLastAttack;
		public CreatureTrait aggressiveToNoise;
		private bool isActive;
		internal GameObject currentTarget;		
		private bool currentTargetIsDecoy;
		private Vector3 targetAttackPoint;
		public float scratchTimer = 0f;
		public bool startTimer = false;
		public Vector3 dir;
		public Vector3 og;
		public GameObject susObj;
		public GameObject searchObj;
		public GameObject targetObj;

		public Collider[] hearObj;

		public IEcoTarget target1;
		public IEcoTarget target2;
		public IEcoTarget target3;

		

		public static List<Creature> LeviathanList = new List<Creature>();
		public GameObject closestLev;
		public GameObject thisReaper;
		public float minDist;
		public float maxDist = 150f;
		public EcoRegion.TargetFilter isTargetValidFilter;



		public static AttackReaper main;
		
		public override void Awake()
        {
			base.Awake();
			main = this;

        }

		public void Start()
		{
			thisReaper = this.gameObject;

			isTargetValidFilter = new EcoRegion.TargetFilter(IsNotSelf);
		}

		public bool IsNotSelf(IEcoTarget target)
		{
			if (target.GetGameObject() != thisReaper)
			{
				return true;
			}

			return false;
		}

		public IEnumerator AssessThreat(Transform transform)
		{
			
			var swim = creature.GetComponentInParent<SwimBehaviour>();
			var fb = this.gameObject.GetComponentInParent<FightBehavior>();

			swim.LookAt(transform);
			yield return new WaitForSeconds(10f);
			yield break;

		}

		public GameObject SeeEnemy(GameObject go)
		{
			float dist;
			bool isVisible = creature.IsInFieldOfView(go, out dist);
			
			bool hasLiveMixin;
			bool isBase;
			bool isLeviathan;

			if (isVisible)
			{
				ErrorMessage.AddMessage("REAPER HAS SIGHT");
				GameObject seeObject = go;
				
				var rootObject = UWE.Utils.GetEntityRoot(seeObject) ?? seeObject;


				hasLiveMixin = rootObject.GetComponent<LiveMixin>();
				isBase = rootObject.GetComponent<Base>();
				isLeviathan = rootObject.GetComponent<GhostLeviathan>() ||
							  rootObject.GetComponent<GhostLeviatanVoid>() ||
							  rootObject.GetComponent<ReaperLeviathan>() ||
							  rootObject.GetComponent<SeaDragon>();

				if (isLeviathan)
				{
					ErrorMessage.AddMessage("ENEMY VISIBLE");
					return seeObject;
				}
				else if (isBase)
				{
					ErrorMessage.AddMessage("BASE VISIBLE");
					return null;
				}

			}
			ErrorMessage.AddMessage("NO SIGHT");
			return null;

		}

		public bool CheckDistance (IEcoTarget target)

        {
			if (Vector3.Distance(target.GetGameObject().transform.position, this.gameObject.transform.position) < 7f)
            {
				return true;
            }
			ErrorMessage.AddMessage("CLAWABLE TOO FAR AWAY");
			return false;
        }

		public bool SenseClawable()
		{			
						

			target1 = EcoRegionManager.main.FindNearestTarget(EcoTargetType.Leviathan, this.gameObject.transform.position, isTargetValidFilter);
			target2 = EcoRegionManager.main.FindNearestTarget(EcoTargetType.Tech, this.gameObject.transform.position, isTargetValidFilter);
			target3 = EcoRegionManager.main.FindNearestTarget(EcoTargetType.Shark, this.gameObject.transform.position, isTargetValidFilter);

			Logger.Log(Logger.Level.Info, $"SENSE CLAWABLE PASSED CHECK 1");

			if (target1 != null)
			{
				

				if (CheckDistance(target1))
                {
					ErrorMessage.AddMessage("LEVIATHAN WITHIN CLAWABLE RANGE");
					return true;
				}
				
			}

			else if (target2 != null)
			{
				

				if (CheckDistance(target2))
				{
					ErrorMessage.AddMessage("TECH WITHIN CLAWABLE RANGE");
					return true;
				}

			}

			else if (target3 != null)
			{
				

				if (CheckDistance(target3))
				{
					ErrorMessage.AddMessage("SHARK WITHIN CLAWABLE RANGE");
					return true;
				}

			}

			ErrorMessage.AddMessage("NO CLAWABLE WITHIN RANGE");
			return false;

		}

		public GameObject Search()
        {
			var fb = creature.GetComponent<FightBehavior>();
			var swim = creature.GetComponent<SwimBehaviour>();
			var ra = creature.GetComponentInChildren<ReaperMeleeAttack>();
			var loco = creature.GetComponent<Locomotion>();
			var thisReaper = creature;

			float detectRange = 300f;
			float distFromTarget;
			//If the Reaper can see the enemy leviathan, or if the enemy leviathan is within 100m of the Reaper, it will become the designated target.  

			Logger.Log(Logger.Level.Info, "SEARCH PASSED CHECK 1");

			IEcoTarget potentialtarget = EcoRegionManager.main.FindNearestTarget(EcoTargetType.Leviathan, this.gameObject.transform.position, isTargetValidFilter);
			var potentialObj = potentialtarget.GetGameObject();
			var potentialLiveMixin = potentialObj.GetComponent<LiveMixin>();
			var potentialRigid = potentialObj.GetComponent<Rigidbody>();



			if (potentialtarget != null && potentialLiveMixin.health > 0)
			{
				currentTarget = SeeEnemy(potentialObj);

				distFromTarget = Vector3.Distance(potentialObj.transform.position, creature.transform.position);

				if (distFromTarget < detectRange)
				{
					if (Time.time > fb.nextCuriosityUpdate)
					{
						fb.nextCuriosityUpdate = Time.time + fb.curiosityUpdateRate;
						creature.Curiosity.Add(((potentialRigid.velocity.magnitude / distFromTarget) * 100f) + (10f * (1/distFromTarget)));
					}

								if (10f < creature.Curiosity.Value  && creature.Curiosity.Value <= 20f)
								{
									AssessThreat(potentialObj.transform);
									ErrorMessage.AddMessage($"ASSESSING THREAT. CURIOSITY IS AT {creature.Curiosity.Value}");
								}

								else if (20f < creature.Curiosity.Value && creature.Curiosity.Value <= 30f)
								{
									//Reaper will try to approach curious object and investigate
									swim.SwimTo(potentialObj.transform.position, this.swimVelocity);
								}

								else if (30f < creature.Curiosity.Value && creature.Curiosity.Value <= 40f)
								{
									//Reaper will try to pursue curious object further
									swim.SwimTo(potentialObj.transform.position, this.swimVelocity*3);
								}

				}
				
				
				ErrorMessage.AddMessage("CLOSEST LEVIATHAN ACQUIRED");
			}

			
			

			Logger.Log(Logger.Level.Info, "SEARCH PASSED CHECK 2");

			if (currentTarget != null)

            {				
				Logger.Log(Logger.Level.Info, "TARGET FOUND. SEARCH PASSED CHECK 3");
				return currentTarget;
			}

			
			Logger.Log(Logger.Level.Info, "SEARCH PASSED CHECK 4");
			return null;			
		}

		public void DesignateTarget(Transform transform)

        {
			var fb = creature.GetComponent<FightBehavior>();
			var swim = creature.GetComponent<SwimBehaviour>();
			
			this.currentTarget = transform.gameObject;
			swim.LookAt(transform);
            		

			Logger.Log(Logger.Level.Debug, "Hostile detected");

        }

		public override void StartPerform(Creature creature)
		{
			var fb = creature.GetComponent<FightBehavior>();
			var aws = creature.GetComponent<AggressiveWhenSeeTarget>();

			SafeAnimator.SetBool(creature.GetAnimator(), "attacking", true);
			
			//this.lastTarget.SetLockedTarget(this.currentTarget);
			//this.isActive = true;

			Logger.Log(Logger.Level.Debug, "Acquiring!");

			if (aws.sightedSound != null && !aws.sightedSound.GetIsPlaying())
			{
				Debug.Log("Not playing sighted sound, starting " + Time.time);
				aws.sightedSound.StartEvent();
			}

		}

		public override void StopPerform(Creature creature)
		{
			SafeAnimator.SetBool(creature.GetAnimator(), "attacking", false);
			this.currentTarget = null;
			/*this.lastTarget.UnlockTarget();
			this.lastTarget.target = null;
			this.isActive = false;
			this.StopAttack();
			*/
		}

		protected void StopAttack()
		{
			this.aggressiveToNoise.Value = 0f;
			this.creature.Aggression.Value = 0f;
			this.timeLastAttack = Time.time;
		}		

		public void Approach()
		{			
			Vector3 targetPosition = this.currentTargetIsDecoy ? this.currentTarget.transform.position : this.currentTarget.transform.TransformPoint(this.targetAttackPoint);
			base.swimBehaviour.SwimTo(targetAttackPoint, this.swimVelocity * 2f);

			if (creature.Aggression.Value <= 0.80)
			{
				creature.Aggression.Add(Time.deltaTime * 0.09f);
			}
		}

		public void Charge(Creature creature)
		{			
			
			IEnumerator AddSpeed()
            {
				creature.Tired.Add(0.1f);
				if (creature.Tired.Value < 0.50)
                {
					this.swimVelocity = 50f;
				}

				else if (creature.Tired.Value >= 0.50)
                {
					this.swimVelocity = 35f;
                }
				
				yield return new WaitForSeconds(5);
				this.swimVelocity = 10f;
			}

			StartCoroutine(AddSpeed());

		}		

		public void UpdateAttackPoint()
		{
			var fb = this.GetComponentInParent<FightBehavior>();
			var rm = this.GetComponentInParent<ReaperMeleeAttack>();
			var targetCollider = fb.targetReaper.GetComponentInParent<Collider>();
			
				
				//Transform attackTransform = fb.eyeHit.transform;
				var thisReaper = GetComponentInParent<ReaperLeviathan>();

				if (!this.currentTargetIsDecoy && this.currentTarget != null)
				{
					this.targetAttackPoint = targetCollider.ClosestPointOnBounds(rm.mouth.transform.position);
					Vector3 vector = this.currentTarget.transform.InverseTransformPoint(thisReaper.transform.position);
					this.targetAttackPoint.z = Mathf.Clamp(vector.z, -2.5f, 2.5f);
					this.targetAttackPoint.y = Mathf.Clamp(vector.y, -2.5f, 2.5f);
				//base.swimBehaviour.LookAt(attackTransform);

					if (fb.canPush)
					{
						this.targetAttackPoint = targetCollider.gameObject.transform.position;
					}
			}

			

			Logger.Log(Logger.Level.Debug, "UPDATING ATTACK POINT!");
		}
				

	}
}
