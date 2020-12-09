using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace HuntingRestricted
{
	// Token: 0x02000004 RID: 4
	public class RangeSelection : JobDriver_Hunt
	{
		// Token: 0x0600000A RID: 10 RVA: 0x000029D9 File Offset: 0x00000BD9
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(delegate()
			{
                if (!job.ignoreDesignations)
				{
                    Pawn victim = Victim;
                    if (victim != null && !victim.Dead && Map.designationManager.DesignationOn(victim, DesignationDefOf.Hunt) == null)
					{
						return true;
					}
				}
				return false;
			});
			yield return new Toil
			{
				initAction = delegate()
				{
					jobStartTick = Find.TickManager.TicksGame;
				}
			};
			yield return Toils_Combat.TrySetJobToUseAttackVerb(TargetIndex.A);
			Toil startCollectCorpse = StartCollectCorpseToil();
			Toil gotoCastPos = MarvsGotoCastPosition(TargetIndex.A, true).JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpse).FailOn(() => Find.TickManager.TicksGame > jobStartTick + 5000);
			yield return gotoCastPos;
			Toil moveIfCannotHit = MarvsJumpIfTargetNotHittable(TargetIndex.A, gotoCastPos);
			yield return moveIfCannotHit;
			yield return MarvsJumpIfTargetDownedDistant(TargetIndex.A, gotoCastPos);
			yield return Toils_Combat.CastVerb(TargetIndex.A, false).JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpse).FailOn(() => Find.TickManager.TicksGame > jobStartTick + 5000);
			yield return Toils_Jump.JumpIfTargetDespawnedOrNull(TargetIndex.A, startCollectCorpse);
			yield return Toils_Jump.Jump(moveIfCannotHit);
			yield return startCollectCorpse;
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false);
			Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);
			yield break;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000029EC File Offset: 0x00000BEC
		private Toil StartCollectCorpseToil()
		{
			var toil = new Toil();
			toil.initAction = delegate()
			{
                if (Victim == null)
				{
					toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
				}
				else
				{
                    TaleRecorder.RecordTale(TaleDefOf.Hunted, new object[]
					{
                        pawn,
                        Victim
                    });
                    Corpse corpse = Victim.Corpse;
                    if (corpse == null || !pawn.CanReserveAndReach(corpse, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, false))
					{
                        pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
					}
					else
					{
						corpse.SetForbidden(false, true);
                        if (corpse.InnerPawn.RaceProps.deathActionWorkerClass != null && !Hunting_Loader.settings.shouldCollectExplodables)
						{
                            pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
						}
						else
						{
                            if (StoreUtility.TryFindBestBetterStoreCellFor(corpse, pawn, Map, StoragePriority.Unstored, pawn.Faction, out IntVec3 c, true))
							{
                                ReservationUtility.Reserve(pawn, corpse, job, 1, -1, null);
                                ReservationUtility.Reserve(pawn, c, job, 1, -1, null);
                                job.SetTarget(TargetIndex.B, c);
                                job.SetTarget(TargetIndex.A, corpse);
                                job.count = 1;
                                job.haulMode = HaulMode.ToCellStorage;
							}
							else
							{
                                pawn.jobs.EndCurrentJob(JobCondition.Succeeded, true);
							}
						}
					}
				}
			};
			return toil;
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002A98 File Offset: 0x00000C98
		private static bool IsPawnTriggerHappy(Pawn p)
		{
			Trait namedTrait = MarvsHuntWhenSane.GetNamedTrait(p, shootingAccuracy);
            if (namedTrait != null)
			{
                if (namedTrait.Degree == -1)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002AD4 File Offset: 0x00000CD4
		private static float GetGoodWeaponHuntingRange(ThingDef weapon)
		{
			var isMeleeWeapon = weapon.IsMeleeWeapon;
			float result;
			if (isMeleeWeapon)
			{
				result = 0f;
			}
			else
			{
                if (weapon == null)
				{
					result = 0f;
				}
				else
				{
					var num = 13f;
					var statFactorFromList = weapon.statBases.GetStatFactorFromList(StatDefOf.AccuracyTouch);
					var statFactorFromList2 = weapon.statBases.GetStatFactorFromList(StatDefOf.AccuracyShort);
					var statFactorFromList3 = weapon.statBases.GetStatFactorFromList(StatDefOf.AccuracyMedium);
					var statFactorFromList4 = weapon.statBases.GetStatFactorFromList(StatDefOf.AccuracyLong);
					var num2 = 0f;
					foreach (VerbProperties verbProperties in weapon.Verbs)
					{
                        if (verbProperties.range > 0f)
						{
							num2 = verbProperties.range;
						}
					}
                    if (statFactorFromList > statFactorFromList2)
					{
						num = 8f;
					}
                    if (statFactorFromList2 > statFactorFromList3 && statFactorFromList2 > statFactorFromList)
					{
                        if (num2 >= 19f)
						{
							num = 19f;
						}
						else
						{
							num = num2;
						}
					}
                    if (statFactorFromList3 > statFactorFromList4 && statFactorFromList3 > statFactorFromList2 && statFactorFromList3 > statFactorFromList)
					{
                        if (num2 >= 34f)
						{
							num = 34f;
						}
						else
						{
							num = num2;
						}
					}
                    if (statFactorFromList4 > statFactorFromList3 && statFactorFromList4 > statFactorFromList2 && statFactorFromList4 > statFactorFromList)
					{
                        if (num2 >= 54f)
						{
							num = 54f;
						}
						else
						{
							num = num2;
						}
					}
					result = num;
				}
			}
			return result;
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002C78 File Offset: 0x00000E78
		private static float GetSafeHuntingDistance(Pawn huntingTarget)
		{
            float result;
			if (huntingTarget == null)
			{
				result = -1f;
			}
			else
			{
				var num = (float)huntingTarget.RaceProps.executionRange;
                if (huntingTarget.RaceProps.deathActionWorkerClass != null)
				{
					num += 4f;
				}
				result = num;
			}
			return result;
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002CC4 File Offset: 0x00000EC4
		private static Toil MarvsJumpIfTargetDownedDistant(TargetIndex ind, Toil jumpToil)
		{
			var toil = new Toil();
			toil.initAction = delegate()
			{
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
				var pawn = curJob.GetTarget(ind).Thing as Pawn;
				var executionRange = pawn.RaceProps.executionRange;
                if (pawn != null && (pawn.Downed || (!pawn.Awake() && !pawn.def.race.predator)) && (actor.Position - pawn.Position).LengthHorizontalSquared > executionRange * executionRange)
				{
					actor.jobs.curDriver.JumpToToil(jumpToil);
				}
			};
			return toil;
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002D14 File Offset: 0x00000F14
		private static Toil MarvsGotoCastPosition(TargetIndex targetInd, bool closeIfDowned = false)
		{
			var toil = new Toil();
			toil.initAction = delegate()
			{
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                Thing thing = curJob.GetTarget(targetInd).Thing;
				var pawn = thing as Pawn;
				var curWeatherAccuracyMultiplier = pawn.Map.weatherManager.CurWeatherAccuracyMultiplier;
				var newReq = new CastPositionRequest
                {
					caster = toil.actor,
					target = thing,
					verb = curJob.verbToUse,
					wantCoverFromTarget = false
				};
                if (pawn == null)
				{
					newReq.maxRangeFromTarget = curJob.verbToUse.verbProps.range;
				}
				else
				{
                    if (!pawn.Downed && !pawn.Awake() && pawn.def.race.predator)
					{
						toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
						return;
					}
                    if (closeIfDowned && (pawn.Downed || !pawn.Awake()))
					{
						newReq.maxRangeFromTarget = Mathf.Min(curJob.verbToUse.verbProps.range, pawn.RaceProps.executionRange);
					}
					else
					{
                        ThingDef def = actor.equipment.Primary.def;
						var num = GetGoodWeaponHuntingRange(def);
                        if (IsPawnTriggerHappy(actor) && !def.IsMeleeWeapon)
						{
							num += TriggerHappyRangeReduction;
						}
                        if (!def.IsMeleeWeapon)
						{
							num *= curWeatherAccuracyMultiplier;
						}
						var safeHuntingDistance = GetSafeHuntingDistance(pawn);
						newReq.maxRangeFromTarget = Mathf.Max(num, safeHuntingDistance);
					}
				}
                if (!CastPositionFinder.TryFindCastPosition(newReq, out IntVec3 intVec))
				{
					toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
				}
				else
				{
					toil.actor.pather.StartPath(intVec, PathEndMode.OnCell);
					actor.Map.pawnDestinationReservationManager.Reserve(actor, curJob, intVec);
				}
			};
			toil.FailOnDespawnedOrNull(targetInd);
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			return toil;
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002D80 File Offset: 0x00000F80
		private static Toil MarvsJumpIfTargetNotHittable(TargetIndex ind, Toil jumpToil)
		{
			var toil = new Toil();
			toil.initAction = delegate()
			{
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
				var curWeatherAccuracyMultiplier = actor.Map.weatherManager.CurWeatherAccuracyMultiplier;
                LocalTargetInfo target = curJob.GetTarget(ind);
				var num = 0f;
                if (!(target.Thing is Pawn) && (curJob.verbToUse == null || !curJob.verbToUse.IsStillUsableBy(actor) || !curJob.verbToUse.CanHitTarget(target)))
				{
					actor.jobs.curDriver.JumpToToil(jumpToil);
				}
				else
                {
                    ThingDef def = actor.equipment.Primary.def;
                    if (curJob.verbToUse != null)
                    {
                        if (!def.IsMeleeWeapon)
                        {
                            num = GetGoodWeaponHuntingRange(def);
                        }
                        else
                        {
                            num = 0f;
                        }
                    }
                    if (IsPawnTriggerHappy(actor) && !def.IsMeleeWeapon)
                    {
                        num += TriggerHappyRangeReduction;
                    }
                    if (!def.IsMeleeWeapon)
                    {
                        num *= curWeatherAccuracyMultiplier;
                    }
                    var huntingTarget = target.Thing as Pawn;
                    var safeHuntingDistance = GetSafeHuntingDistance(huntingTarget);
                    if (!def.IsMeleeWeapon)
                    {
                        num = Mathf.Max(safeHuntingDistance, num);
                    }
                    if ((actor.Position - target.Cell).LengthHorizontal <= safeHuntingDistance)
                    {
                        return;
                    }
                    if ((curJob.verbToUse != null && !curJob.verbToUse.IsStillUsableBy(actor)) || (curJob.verbToUse != null && !curJob.verbToUse.CanHitTarget(target)) || (curJob.verbToUse != null && (actor.Position - target.Cell).LengthHorizontalSquared > num * num))
                    {
                        actor.jobs.curDriver.JumpToToil(jumpToil);
                    }
                }
            };
			return toil;
		}

		// Token: 0x0400001E RID: 30
		private int jobStartTick = -1;

		// Token: 0x0400001F RID: 31
		private const float TriggerHappyRangeReduction = -9f;

		// Token: 0x04000020 RID: 32
		private const string shootingAccuracy = "ShootingAccuracy";
	}
}
