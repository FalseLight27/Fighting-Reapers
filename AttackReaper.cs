﻿using System;
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
		public GameObject targetObj;



		public static AttackReaper main;
		
		public override void Awake()
        {
			base.Awake();
			main = this;

        }

		public GameObject SeeEnemy(RaycastHit[] array)
		{
			bool obstructed;
			bool hasLiveMixin;
			bool isBase;
			bool isLeviathan;
			for (int i = 0; i < array.Length; i++)
			{
				hasLiveMixin = array[i].transform.gameObject.GetComponent<LiveMixin>();
				isBase = array[i].transform.gameObject.GetComponent<Base>();
				isLeviathan = array[i].transform.GetComponentInChildren<GhostLeviathan>() || array[i].transform.GetComponentInChildren<GhostLeviatanVoid>() || array[i].transform.GetComponentInChildren<ReaperLeviathan>() || array[i].transform.GetComponentInChildren<SeaDragon>();
				if (isLeviathan)
				{
					ErrorMessage.AddMessage("ENEMY VISIBLE");
					return array[i].transform.gameObject;
				}
					
				else if (!hasLiveMixin || isBase)
                {
					return null;
				}
					
			}

			return null;
			
		}

		public GameObject HearEnemy(Collider[] array)
		{
			
			bool hasLiveMixin;
			bool isBase;
			bool isLeviathan;
			for (int i = 0; i < array.Length; i++)
			{
				hasLiveMixin = array[i].transform.gameObject.GetComponent<LiveMixin>();
				isBase = array[i].transform.gameObject.GetComponent<Base>();
				isLeviathan = array[i].transform.GetComponentInChildren<GhostLeviathan>() || array[i].transform.GetComponentInChildren<GhostLeviatanVoid>() || array[i].transform.GetComponentInChildren<ReaperLeviathan>() || array[i].transform.GetComponentInChildren<SeaDragon>();
				if (isLeviathan)
                {
					return array[i].transform.gameObject;
					ErrorMessage.AddMessage("REAPER HEARS SOMETHING");
				}									
			}

			return null;

		}

		public GameObject Search()
        {
			var fb = creature.GetComponent<FightBehavior>();
			var swim = creature.GetComponent<SwimBehaviour>();
			var ra = creature.GetComponent<ReaperMeleeAttack>();
			var loco = creature.GetComponent<Locomotion>();
			var thisReaper = creature;
			bool isReaper;
			bool isGhost;
			bool isDragon;			

						
			RaycastHit[] searchPoints;
			Collider[] hearObj;

			//loco.targetForward.x = 20f;

			dir = ra.mouth.transform.forward * 2;
			og = ra.mouth.transform.up + new Vector3(0, 0, 1);
			searchPoints = Physics.SphereCastAll(og, 5f, dir, 150f);
			hearObj = Physics.OverlapSphere(ra.mouth.transform.up, 60f);
			



			IEnumerator ScanSurroundings()
			{
				loco.targetForward.x = UnityEngine.Random.Range(0, 999f);
				loco.targetForward.y = UnityEngine.Random.Range(-20f, -100f);
				loco.targetForward.z = UnityEngine.Random.Range(0, 999f);
				yield return new WaitForSeconds(UnityEngine.Random.Range(1, 6f));
				loco.targetForward.x = UnityEngine.Random.Range(0, 999f);
				loco.targetForward.y = UnityEngine.Random.Range(-20f, -100f);
				loco.targetForward.z = UnityEngine.Random.Range(0, 999f);
				yield return new WaitForSeconds(UnityEngine.Random.Range(1, 6f));
				loco.targetForward.x = UnityEngine.Random.Range(0, 999f);
				loco.targetForward.y = UnityEngine.Random.Range(-20f, -100f);
				loco.targetForward.z = UnityEngine.Random.Range(0, 999f);
				yield return new WaitForSeconds(UnityEngine.Random.Range(1, 6f));
				loco.targetForward.x = UnityEngine.Random.Range(0, 999f);
				loco.targetForward.y = UnityEngine.Random.Range(-20f, -100f);
				loco.targetForward.z = UnityEngine.Random.Range(0, 999f);
				yield return new WaitForSeconds(UnityEngine.Random.Range(1, 6f));

			}

			CoroutineHost.StartCoroutine(ScanSurroundings());

			targetObj = SeeEnemy(searchPoints);

			if (targetObj != null)

            {
				isReaper = targetObj.GetComponentInChildren<ReaperLeviathan>();
				isGhost = targetObj.transform.GetComponentInChildren<GhostLeviathan>() || targetObj.transform.GetComponentInChildren<GhostLeviatanVoid>();
				isDragon = targetObj.GetComponentInChildren<SeaDragon>();

				if (isReaper)
                {
					ErrorMessage.AddMessage("REAPER ACQUIRED");
                }

				if (isGhost)
				{
					ErrorMessage.AddMessage("GHOST ACQUIRED");
				}

				if (isDragon)
				{
					ErrorMessage.AddMessage("SEADRAGON ACQUIRED");
				}
			}

			else if (targetObj == null)

            {
				susObj = HearEnemy(hearObj);				

				swim.LookAt(susObj.transform);
				
			}

			if (susObj != null)
			{
				if (Vector3.Distance(susObj.transform.position, thisReaper.transform.position) < 35f)
				{
					susObj = targetObj;
				}
			}



			return targetObj;			
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

			SafeAnimator.SetBool(creature.GetAnimator(), "attacking", true);
			
			//this.lastTarget.SetLockedTarget(this.currentTarget);
			//this.isActive = true;

			Logger.Log(Logger.Level.Debug, "Acquiring!");
										
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
		}

		public void Charge(Creature creature)
		{			
			
			IEnumerator AddSpeed()
            {
				creature.Tired.Add(0.1f);
				if (creature.Tired.Value < 0.50)
                {
					this.swimVelocity = 40f;
				}

				else if (creature.Tired.Value >= 0.50)
                {
					this.swimVelocity = 25;
                }
				
				yield return new WaitForSeconds(3);
				this.swimVelocity = 10f;
			}

			StartCoroutine(AddSpeed());

		}


		public void OnCollisionEnter(Collision collision)
		{
			var fb = creature.GetComponent<FightBehavior>();
			var rb = collision.gameObject.GetComponent<Rigidbody>();
			var velocity = rb.velocity.magnitude;
			var thisReaper = creature.GetComponent<ReaperLeviathan>();

			if (velocity >= 80f)
			{			
				
				this.currentTarget = collision.gameObject;
				this.aggressiveToNoise.Value = 15f;

				base.swimBehaviour.SwimTo(thisReaper.gameObject.transform.forward + new Vector3(0, 0, 50), this.swimVelocity * 4f);
			}
		}

		public void UpdateAttackPoint()
		{
			var fb = this.GetComponentInParent<FightBehavior>();
			var rm = this.GetComponentInParent<ReaperMeleeAttack>();
			var targetCollider = fb.targetReaper.GetComponentInParent<Collider>();
			
				this.targetAttackPoint = targetCollider.ClosestPointOnBounds(rm.mouth.transform.position);
				//Transform attackTransform = fb.eyeHit.transform;
				var thisReaper = GetComponentInParent<ReaperLeviathan>();

				if (!this.currentTargetIsDecoy && this.currentTarget != null)
				{
					Vector3 vector = this.currentTarget.transform.InverseTransformPoint(thisReaper.transform.position);
					this.targetAttackPoint.z = Mathf.Clamp(vector.z, -2.5f, 2.5f);
					this.targetAttackPoint.y = Mathf.Clamp(vector.y, -2.5f, 2.5f);
					//base.swimBehaviour.LookAt(attackTransform);
				}

			

			Logger.Log(Logger.Level.Debug, "UPDATING ATTACK POINT!");
		}
				

	}
}
