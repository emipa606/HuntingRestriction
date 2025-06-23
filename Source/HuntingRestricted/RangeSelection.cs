using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace HuntingRestricted;

public class RangeSelection : JobDriver_Hunt
{
    private const float TriggerHappyRangeReduction = -9f;

    private const string ShootingAccuracy = "ShootingAccuracy";

    private int jobStartTick = -1;

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOn(delegate
        {
            if (job.ignoreDesignations)
            {
                return false;
            }

            var victim = Victim;
            return victim is { Dead: false } &&
                   Map.designationManager.DesignationOn(victim, DesignationDefOf.Hunt) == null;
        });
        yield return new Toil
        {
            initAction = delegate { jobStartTick = Find.TickManager.TicksGame; }
        };
        yield return Toils_Combat.TrySetJobToUseAttackVerb(TargetIndex.A);
        var startCollectCorpse = startCollectCorpseToil();
        var gotoCastPos = marvsGotoCastPosition(TargetIndex.A, true)
            .JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpse)
            .FailOn(() => Find.TickManager.TicksGame > jobStartTick + 5000);
        yield return gotoCastPos;
        var moveIfCannotHit = marvsJumpIfTargetNotHittable(TargetIndex.A, gotoCastPos);
        yield return moveIfCannotHit;
        yield return marvsJumpIfTargetDownedDistant(TargetIndex.A, gotoCastPos);
        yield return Toils_Combat.CastVerb(TargetIndex.A, false)
            .JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpse)
            .FailOn(() => Find.TickManager.TicksGame > jobStartTick + 5000);
        yield return Toils_Jump.JumpIfTargetDespawnedOrNull(TargetIndex.A, startCollectCorpse);
        yield return Toils_Jump.Jump(moveIfCannotHit);
        yield return startCollectCorpse;
        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
        yield return Toils_Haul.StartCarryThing(TargetIndex.A);
        var carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
        yield return carryToCell;
        yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);
    }

    private Toil startCollectCorpseToil()
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            if (Victim == null)
            {
                toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
            }
            else
            {
                TaleRecorder.RecordTale(TaleDefOf.Hunted, pawn, Victim);
                var corpse = Victim.Corpse;
                if (corpse == null || !pawn.CanReserveAndReach(corpse, PathEndMode.ClosestTouch, Danger.Deadly))
                {
                    pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
                else
                {
                    corpse.SetForbidden(false);
                    if (corpse.InnerPawn.RaceProps.DeathActionWorker != null &&
                        !Hunting_Loader.Settings.ShouldCollectExplodables)
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                    }
                    else
                    {
                        if (StoreUtility.TryFindBestBetterStoreCellFor(corpse, pawn, Map, StoragePriority.Unstored,
                                pawn.Faction, out var c))
                        {
                            pawn.Reserve(corpse, job);
                            pawn.Reserve(c, job);
                            job.SetTarget(TargetIndex.B, c);
                            job.SetTarget(TargetIndex.A, corpse);
                            job.count = 1;
                            job.haulMode = HaulMode.ToCellStorage;
                        }
                        else
                        {
                            pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
                        }
                    }
                }
            }
        };
        return toil;
    }

    private static bool isPawnTriggerHappy(Pawn p)
    {
        var namedTrait = MarvsHuntWhenSane.GetNamedTrait(p, ShootingAccuracy);
        if (namedTrait == null)
        {
            return false;
        }

        return namedTrait.Degree == -1;
    }

    private static float getGoodWeaponHuntingRange(ThingDef weapon)
    {
        var isMeleeWeapon = weapon.IsMeleeWeapon;
        float result;
        if (isMeleeWeapon)
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
            foreach (var verbProperties in weapon.Verbs)
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
                num = num2 >= 19f ? 19f : num2;
            }

            if (statFactorFromList3 > statFactorFromList4 && statFactorFromList3 > statFactorFromList2 &&
                statFactorFromList3 > statFactorFromList)
            {
                num = num2 >= 34f ? 34f : num2;
            }

            if (statFactorFromList4 > statFactorFromList3 && statFactorFromList4 > statFactorFromList2 &&
                statFactorFromList4 > statFactorFromList)
            {
                num = num2 >= 54f ? 54f : num2;
            }

            result = num;
        }

        return result;
    }

    private static float getSafeHuntingDistance(Pawn huntingTarget)
    {
        float result;
        if (huntingTarget == null)
        {
            result = -1f;
        }
        else
        {
            var num = (float)huntingTarget.RaceProps.executionRange;
            if (huntingTarget.RaceProps.DeathActionWorker != null)
            {
                num += 4f;
            }

            result = num;
        }

        return result;
    }

    private static Toil marvsJumpIfTargetDownedDistant(TargetIndex ind, Toil jumpToil)
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            var actor = toil.actor;
            var curJob = actor.jobs.curJob;
            if (curJob.GetTarget(ind).Thing is not Pawn pawn)
            {
                return;
            }

            var executionRange = pawn.RaceProps.executionRange;
            if ((pawn.Downed || !pawn.Awake() && !pawn.def.race.predator &&
                    Hunting_Loader.Settings.ShouldApproachSleepers) &&
                (actor.Position - pawn.Position).LengthHorizontalSquared > executionRange * executionRange)
            {
                actor.jobs.curDriver.JumpToToil(jumpToil);
            }
        };
        return toil;
    }

    private static Toil marvsGotoCastPosition(TargetIndex targetInd, bool closeIfDowned = false)
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            var actor = toil.actor;
            var curJob = actor.jobs.curJob;
            var thing = curJob.GetTarget(targetInd).Thing;
            if (thing is not Pawn pawn)
            {
                return;
            }

            var curWeatherAccuracyMultiplier = pawn.Map.weatherManager.CurWeatherAccuracyMultiplier;
            var newReq = new CastPositionRequest
            {
                caster = toil.actor,
                target = thing,
                verb = curJob.verbToUse,
                wantCoverFromTarget = false
            };
            if (!pawn.Downed && !pawn.Awake() && pawn.def.race.predator)
            {
                toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                return;
            }

            if (closeIfDowned &&
                (pawn.Downed || Hunting_Loader.Settings.ShouldApproachSleepers && !pawn.Awake()))
            {
                newReq.maxRangeFromTarget =
                    Mathf.Min(curJob.verbToUse.verbProps.range, pawn.RaceProps.executionRange);
            }
            else
            {
                var def = actor.equipment.Primary.def;
                var num = getGoodWeaponHuntingRange(def);
                if (isPawnTriggerHappy(actor) && !def.IsMeleeWeapon)
                {
                    num += TriggerHappyRangeReduction;
                }

                if (!def.IsMeleeWeapon)
                {
                    num *= curWeatherAccuracyMultiplier;
                }

                var safeHuntingDistance = getSafeHuntingDistance(pawn);
                newReq.maxRangeFromTarget = Mathf.Max(num, safeHuntingDistance);
            }

            if (!CastPositionFinder.TryFindCastPosition(newReq, out var intVec))
            {
                toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
            }
            else
            {
                if (intVec != toil.actor.Position)
                {
                    toil.actor.pather.StartPath(intVec, PathEndMode.OnCell);
                    actor.Map.pawnDestinationReservationManager.Reserve(actor, curJob, intVec);
                }
                else
                {
                    toil.actor.pather.StopDead();
                    toil.actor.jobs.curDriver.ReadyForNextToil();
                }
            }
        };
        toil.FailOnDespawnedOrNull(targetInd);
        toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
        return toil;
    }

    private static Toil marvsJumpIfTargetNotHittable(TargetIndex ind, Toil jumpToil)
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            var actor = toil.actor;
            var curJob = actor.jobs.curJob;
            var curWeatherAccuracyMultiplier = actor.Map.weatherManager.CurWeatherAccuracyMultiplier;
            var target = curJob.GetTarget(ind);
            var num = 0f;
            if (target.Thing is not Pawn && (curJob.verbToUse == null || !curJob.verbToUse.IsStillUsableBy(actor) ||
                                             !curJob.verbToUse.CanHitTarget(target)))
            {
                actor.jobs.curDriver.JumpToToil(jumpToil);
            }
            else
            {
                var def = actor.equipment.Primary.def;
                if (curJob.verbToUse != null)
                {
                    num = !def.IsMeleeWeapon ? getGoodWeaponHuntingRange(def) : 0f;
                }

                if (isPawnTriggerHappy(actor) && !def.IsMeleeWeapon)
                {
                    num += TriggerHappyRangeReduction;
                }

                if (!def.IsMeleeWeapon)
                {
                    num *= curWeatherAccuracyMultiplier;
                }

                var huntingTarget = target.Thing as Pawn;
                var safeHuntingDistance = getSafeHuntingDistance(huntingTarget);
                if (!def.IsMeleeWeapon)
                {
                    num = Mathf.Max(safeHuntingDistance, num);
                }

                if ((actor.Position - target.Cell).LengthHorizontal <= safeHuntingDistance)
                {
                    return;
                }

                if (curJob.verbToUse != null && !curJob.verbToUse.IsStillUsableBy(actor) ||
                    curJob.verbToUse != null && !curJob.verbToUse.CanHitTarget(target) ||
                    curJob.verbToUse != null && (actor.Position - target.Cell).LengthHorizontalSquared > num * num)
                {
                    actor.jobs.curDriver.JumpToToil(jumpToil);
                }
            }
        };
        return toil;
    }
}