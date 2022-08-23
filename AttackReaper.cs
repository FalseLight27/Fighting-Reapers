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
	public class AttackReaper : AttackCyclops
	{
		//public LastTarget lastTarget;
		//private float maxDistToLeash = 100f;
		//public float swimVelocity = 10f;		
		//private float swimInterval = 0.3f;
		//public float timeLastAttack;
		//public CreatureTrait aggressiveToNoise;
		//private bool isActive;
		//internal GameObject currentTarget;		
		//private bool currentTargetIsDecoy;
		//private Vector3 targetAttackPoint;
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
		public IEcoTarget potentialTarget;
		public Transform initialTarget;



		public static List<Creature> LeviathanList = new List<Creature>();
		public GameObject closestLev;
		public GameObject thisReaper;
		public float minDist;
		public float maxDist = 150f;
		public EcoRegion.TargetFilter isTargetValidFilter2;



		public static AttackReaper main;

		public override void Awake()
		{
			base.Awake();
			main = this;

		}

		public void Start()
		{
			thisReaper = this.gameObject;

			isTargetValidFilter2 = new EcoRegion.TargetFilter(IsNotSelf);
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
			var loco = creature.GetComponentInParent<Locomotion>();
			var fb = this.gameObject.GetComponentInParent<FightBehavior>();

			initialTarget = loco.lookTarget; 

			swim.LookAt(transform);
			yield return new WaitForSeconds(10f);

			if (currentTarget == null)
            {
				swim.LookAt(null);
			}
			

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

				if (isLeviathan && dist < 301)
				{
					ErrorMessage.AddMessage("ENEMY VISIBLE");
					return seeObject;
				}
				else if (isBase && dist < 301)
				{
					ErrorMessage.AddMessage("BASE VISIBLE");
					return null;
				}

			}
			ErrorMessage.AddMessage("NO SIGHT");
			return null;

		}

		public bool CheckDistance(IEcoTarget target)

		{
			if (Vector3.Distance(target.GetGameObject().transform.position, creature.transform.position) < 21f)
			{
				return true;
			}
			ErrorMessage.AddMessage("CLAWABLE TOO FAR AWAY");
			return false;
		}

		public bool SenseClawable()
		{


			target1 = EcoRegionManager.main.FindNearestTarget(EcoTargetType.Leviathan, this.gameObject.transform.position, isTargetValidFilter2);
			target2 = EcoRegionManager.main.FindNearestTarget(EcoTargetType.Tech, this.gameObject.transform.position, isTargetValidFilter2);
			target3 = EcoRegionManager.main.FindNearestTarget(EcoTargetType.Shark, this.gameObject.transform.position, isTargetValidFilter2);

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

			float detectRange = 350f;
			float potentialTargetDist;

			//If the Reaper can see the enemy leviathan, or if the enemy leviathan is within 100m of the Reaper, it will become the designated target.  

			Logger.Log(Logger.Level.Info, "SEARCH PASSED CHECK 1");

			potentialTarget = EcoRegionManager.main.FindNearestTarget(EcoTargetType.Leviathan, creature.transform.position, isTargetValidFilter2, 6);


			Logger.Log(Logger.Level.Info, "SEARCH PASSED CHECK 1.1");

			if (potentialTarget != null && fb.targetReaper == null)
			{
				var potentialObj = potentialTarget.GetGameObject();
				var potentialLiveMixin = potentialObj.GetComponentInChildren<LiveMixin>();
				var potentialRigid = potentialObj.GetComponentInChildren<Rigidbody>();
				potentialTargetDist = Vector3.Distance(potentialObj.transform.position, creature.transform.position);

				//ErrorMessage.AddMessage("NEAREST TARGET OBTAINED");

				if (potentialLiveMixin.health > 0)

				{
					currentTarget = SeeEnemy(potentialObj);

					if (currentTarget == null)

                    {
						if (potentialTargetDist < detectRange)
						{
							if (Time.time > fb.nextCuriosityUpdate)
							{
								fb.nextCuriosityUpdate = Time.time + fb.curiosityUpdateRate;
								creature.Curiosity.Add((((potentialRigid.velocity.magnitude * potentialRigid.mass) / (potentialTargetDist * 5000))) + ((0.1f / potentialTargetDist)));
							}

							Logger.Log(Logger.Level.Info, "SEARCH PASSED CHECK 1.2");

							/*
							if (creature.Curiosity.Value > 0.10f)
							{
								AssessThreat(potentialObj.transform);
								ErrorMessage.AddMessage($"ASSESSING THREAT. CURIOSITY IS AT {creature.Curiosity.Value}");
							}
							
							if (creature.Curiosity.Value > 0.20f && creature.Curiosity.Value <= 0.30f)
							{
								//Reaper will try to approach curious object and investigate
								swim.SwimTo(potentialObj.transform.position, this.swimVelocity);
							}

							else if (creature.Curiosity.Value > 0.30f)
							{
								//Reaper will try to pursue curious object further
								swim.SwimTo(potentialObj.transform.position, this.swimVelocity * 3);
							}
							*/
						}
					}

					
				}

				Logger.Log(Logger.Level.Info, "SEARCH PASSED CHECK 1.3");

				if (potentialTargetDist > detectRange)

				{
					if (Time.time > fb.nextCuriosityUpdate)
					{
						fb.nextCuriosityUpdate = Time.time + fb.curiosityUpdateRate;
						creature.Curiosity.Add(-0.05f);
					}
				}


				ErrorMessage.AddMessage("CLOSEST LEVIATHAN ACQUIRED");
			}

			else if (potentialTarget == null)

			{
				return null;
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

		public IEnumerator DesignateTarget(Transform transform)

		{
			var fb = creature.GetComponent<FightBehavior>();
			var swim = creature.GetComponent<SwimBehaviour>();
			var loco = creature.GetComponent<Locomotion>();
			float dist;

			

			this.currentTarget = transform.gameObject;
			swim.LookAt(transform);

			yield return new WaitForSeconds(8f);

			if (!creature.IsInFieldOfView(transform.gameObject, out dist))

            {
				swim.LookAt(null);
            }


			Logger.Log(Logger.Level.Debug, "Hostile detected");

		}

		public override void StartPerform(Creature creature)
		{
			var fb = creature.GetComponent<FightBehavior>();
			var aws = creature.GetComponentInChildren<AggressiveWhenSeeTarget>();
			var last = creature.GetComponentInChildren<LastTarget>();
			var ls = creature.GetComponentInChildren<LastScarePosition>();

			SafeAnimator.SetBool(creature.GetAnimator(), "attacking", true);

			//this.lastTarget.SetLockedTarget(this.currentTarget);
			//this.isActive = true;

			Logger.Log(Logger.Level.Debug, "Acquiring!");

			ls.lastScarePosition = fb.targetReaper.transform.position;
			last.target = fb.targetReaper;

			if (aws.sightedSound != null && !aws.sightedSound.GetIsPlaying())
			{
				Debug.Log("Not playing sighted sound, starting " + Time.time);
				aws.sightedSound.StartEvent();
			}

		}

		public override void StopPerform(Creature creature)
		{
			SafeAnimator.SetBool(creature.GetAnimator(), "attacking", false);
			//this.currentTarget = null;
			/*this.lastTarget.UnlockTarget();
			this.lastTarget.target = null;
			this.isActive = false;
			this.StopAttack();
			*/
		}

		/*

		protected void StopAttack()
		{
			this.aggressiveToNoise.Value = 0f;
			this.creature.Aggression.Value = 0f;
			this.timeLastAttack = Time.time;
		}

		*/

		public void Approach()
		{
			Vector3 targetPosition = this.currentTargetIsDecoy ? this.currentTarget.transform.position : this.currentTarget.transform.TransformPoint(this.targetAttackPoint);
			base.swimBehaviour.SwimTo(targetAttackPoint, this.swimVelocity * 2f);

			if (creature.Aggression.Value <= 0.80)
			{
				creature.Aggression.Add(Time.deltaTime * 0.09f);
			}
		}

		public IEnumerator Charge(Creature creature)
		{

			
			if (creature.Tired.Value < 0.50)
			{
				this.swimVelocity = 50f;
			}

			else if (creature.Tired.Value >= 0.50)
			{
				this.swimVelocity = 35f;
			}
			creature.Tired.Add(0.1f);
			yield return new WaitForSeconds(5);
			this.swimVelocity = 10f;

		}

		public void UpdateAttackPoint2()
		{
			var fb = this.GetComponentInParent<FightBehavior>();
			var rm = this.GetComponentInParent<ReaperMeleeAttack>();
			var targetCollider = fb.targetReaper.GetComponentInParent<Collider>();


			//Transform attackTransform = fb.eyeHit.transform;
			var thisReaper = GetComponentInParent<ReaperLeviathan>();
			float dist;

			bool targetVisible = creature.IsInFieldOfView(currentTarget, out dist);

			if (!this.currentTargetIsDecoy && this.currentTarget != null && targetVisible)
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

				Logger.Log(Logger.Level.Debug, "UPDATING ATTACK POINT!");
			}

			else if (currentTargetIsDecoy || currentTarget == null || !targetVisible)

            {
				fb.targetReaper = null;
				Logger.Log(Logger.Level.Debug, $"LOST SIGHT OF TARGET! NEW TARGET IS NOW {currentTarget}");

			}



			
		}


		
	}
}
