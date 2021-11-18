﻿using SolastaModApi;
using SolastaModApi.Extensions;
using SolastaCommunityExpansion.CustomFeatureDefinitions;

namespace SolastaCommunityExpansion.Subclasses.Fighter
{
    class Tactician : AbstractSubclass
    {
        private CharacterSubclassDefinition Subclass;
        internal override FeatureDefinitionSubclassChoice GetSubclassChoiceList()
        {
            return DatabaseHelper.FeatureDefinitionSubclassChoices.SubclassChoiceFighterMartialArchetypes;
        }
        internal override CharacterSubclassDefinition GetSubclass()
        {
            if (Subclass == null)
            {
                Subclass = TacticianFighterSubclassBuilder.BuildAndAddSubclass();
            }
            return Subclass;
        }
    }

    internal class KnockDownPowerBuilder
    {
        const string KnockDownPowerName = "KnockDownPower";
        const string KnockDownPowerNameGuid = "90dd5e81-40d7-4824-89b4-45bcf4c05218";

        protected static FeatureDefinitionPower Build(string name, string guid)
        {
            //Create the damage form - TODO make it do the same damage as the wielded weapon?  This doesn't seem possible
            EffectForm damageEffect = new EffectForm();
            damageEffect.DamageForm = new DamageForm();
            damageEffect.DamageForm.DiceNumber = 1;
            damageEffect.DamageForm.DieType = RuleDefinitions.DieType.D6;
            damageEffect.DamageForm.BonusDamage = 2;
            damageEffect.DamageForm.DamageType = "DamageBludgeoning";
            damageEffect.SavingThrowAffinity = RuleDefinitions.EffectSavingThrowType.None;

            //Create the prone effect - Weirdly enough the motion form seems to also automatically apply the prone condition
            EffectForm proneMotionEffect = new EffectForm();
            proneMotionEffect.FormType = EffectForm.EffectFormType.Motion;
            var proneMotion = new MotionForm();
            proneMotion.SetType(MotionForm.MotionType.FallProne);
            proneMotion.SetDistance(1);
            proneMotionEffect.SetMotionForm(proneMotion);
            proneMotionEffect.SavingThrowAffinity = RuleDefinitions.EffectSavingThrowType.Negates;

            //Add to our new effect
            EffectDescription newEffectDescription = new EffectDescription();
            newEffectDescription.Copy(DatabaseHelper.FeatureDefinitionPowers.PowerFighterActionSurge.EffectDescription);
            newEffectDescription.EffectForms.Clear();
            newEffectDescription.EffectForms.Add(damageEffect);
            newEffectDescription.EffectForms.Add(proneMotionEffect);
            newEffectDescription.SetSavingThrowDifficultyAbility("Strength");
            newEffectDescription.SetDifficultyClassComputation(RuleDefinitions.EffectDifficultyClassComputation.AbilityScoreAndProficiency);
            newEffectDescription.SavingThrowAbility = "Strength";
            newEffectDescription.HasSavingThrow = true;
            newEffectDescription.DurationType = RuleDefinitions.DurationType.Turn;

            FeatureDefinitionPowerSharedPoolBuilder builder = new FeatureDefinitionPowerSharedPoolBuilder(name, guid,
                TacticianFighterSubclassBuilder.GambitResourcePool, RuleDefinitions.RechargeRate.ShortRest, RuleDefinitions.ActivationTime.OnAttackHit,
                1, true, true, AttributeDefinitions.Strength, newEffectDescription,
                new GuiPresentationBuilder("Feature/&KnockDownPowerDescription", "Feature/&KnockDownPowerTitle")
                .SetSpriteReference(DatabaseHelper.FeatureDefinitionPowers.PowerFighterActionSurge.GuiPresentation.SpriteReference).Build(), false);

            return builder.AddToDB();
        }

        public static FeatureDefinitionPower CreateAndAddToDB(string name, string guid)
            => Build(name, guid);

        public static FeatureDefinitionPower KnockDownPower = CreateAndAddToDB(KnockDownPowerName, KnockDownPowerNameGuid);
    }


    internal class InspirePowerBuilder 
    {
        const string InspirePowerName = "InspirePower";
        const string InspirePowerNameGuid = "163c28de-48e5-4f75-bdd0-d42374a75ef8";

        protected static FeatureDefinitionPower Build(string name, string guid) 
        {
            //Create the temp hp form
            EffectForm healingEffect = new EffectForm();
            healingEffect.FormType = EffectForm.EffectFormType.TemporaryHitPoints;
            var tempHPForm = new TemporaryHitPointsForm();
            tempHPForm.DiceNumber = 1;
            tempHPForm.DieType = RuleDefinitions.DieType.D6;
            tempHPForm.BonusHitPoints = 2;
            healingEffect.SetTemporaryHitPointsForm(tempHPForm);

            //Create the bless effect - A fun test, unfortunately the two effects can't have varying durations AFAIK so a bless or similar effect might be overpowered (was thinking a bless for 1 round).  Alternatively both could last 1 minute instead and be intended for in battle.
            //EffectForm blessEffect = new EffectForm();
            //blessEffect.ConditionForm = new ConditionForm();
            //blessEffect.FormType = EffectForm.EffectFormType.Condition;
            //blessEffect.ConditionForm.Operation = ConditionForm.ConditionOperation.Add;
            //blessEffect.ConditionForm.ConditionDefinition = DatabaseHelper.ConditionDefinitions.ConditionBlessed;

            //Add to our new effect
            EffectDescription newEffectDescription = new EffectDescription();
            newEffectDescription.Copy(DatabaseHelper.FeatureDefinitionPowers.PowerDomainLifePreserveLife.EffectDescription);
            newEffectDescription.EffectForms.Clear();
            newEffectDescription.EffectForms.Add(healingEffect);
            //newEffectDescription.EffectForms.Add(blessEffect);
            newEffectDescription.HasSavingThrow = false;
            newEffectDescription.DurationType = RuleDefinitions.DurationType.Day;
            newEffectDescription.SetTargetSide(RuleDefinitions.Side.Ally);
            newEffectDescription.SetTargetType(RuleDefinitions.TargetType.Individuals);
            newEffectDescription.SetTargetProximityDistance(12);
            newEffectDescription.SetCanBePlacedOnCharacter(true);
            newEffectDescription.SetRangeType(RuleDefinitions.RangeType.Distance);

            FeatureDefinitionPowerSharedPoolBuilder builder = new FeatureDefinitionPowerSharedPoolBuilder(name, guid,
                TacticianFighterSubclassBuilder.GambitResourcePool, RuleDefinitions.RechargeRate.ShortRest, RuleDefinitions.ActivationTime.BonusAction,
                1, true, true, AttributeDefinitions.Strength, newEffectDescription,
                new GuiPresentationBuilder("Feature/&InspirePowerDescription", "Feature/&InspirePowerTitle")
                .SetSpriteReference(DatabaseHelper.FeatureDefinitionPowers.PowerDomainLifePreserveLife.GuiPresentation.SpriteReference).Build(), false);

            builder.SetShortTitle("Feature/&InspirePowerTitle");

            return builder.AddToDB();
        }

        public static FeatureDefinitionPower CreateAndAddToDB(string name, string guid)
            => Build(name, guid);

        public static FeatureDefinitionPower InspirePower = CreateAndAddToDB(InspirePowerName, InspirePowerNameGuid);
    }


    internal class CounterStrikePowerBuilder 
    {
        const string CounterStrikePowerName = "CounterStrikePower";
        const string CounterStrikePowerNameGuid = "88c294ce-14fa-4f7e-8b81-ea4d289e3d8b";

        protected static FeatureDefinitionPower Build (string name, string guid)
        {
            //Create the damage form - TODO make it do the same damage as the wielded weapon (seems impossible with current tools, would need to use the AdditionalDamage feature but I'm not sure how to combine that with this to make it a reaction ability).
            EffectForm damageEffect = new EffectForm();
            damageEffect.DamageForm = new DamageForm();
            damageEffect.DamageForm.DiceNumber = 1;
            damageEffect.DamageForm.DieType = RuleDefinitions.DieType.D6;
            damageEffect.DamageForm.BonusDamage = 2;
            damageEffect.DamageForm.DamageType = "DamageBludgeoning";
            damageEffect.SavingThrowAffinity = RuleDefinitions.EffectSavingThrowType.None;

            //Add to our new effect
            EffectDescription newEffectDescription = new EffectDescription();
            newEffectDescription.Copy(DatabaseHelper.FeatureDefinitionPowers.PowerDomainLawHolyRetribution.EffectDescription);
            newEffectDescription.EffectForms.Clear();
            newEffectDescription.EffectForms.Add(damageEffect);

            FeatureDefinitionPowerSharedPoolBuilder builder = new FeatureDefinitionPowerSharedPoolBuilder(name, guid,
                TacticianFighterSubclassBuilder.GambitResourcePool, RuleDefinitions.RechargeRate.ShortRest, RuleDefinitions.ActivationTime.Reaction,
                1, true, true, AttributeDefinitions.Strength, newEffectDescription,
                new GuiPresentationBuilder("Feature/&CounterStrikePowerDescription", "Feature/&CounterStrikePowerTitle")
                .SetSpriteReference(DatabaseHelper.FeatureDefinitionPowers.PowerDomainLawHolyRetribution.GuiPresentation.SpriteReference).Build(), false);

            return builder.AddToDB();
        }

        public static FeatureDefinitionPower CreateAndAddToDB(string name, string guid)
            => Build(name, guid);

        public static FeatureDefinitionPower CounterStrikePower = CreateAndAddToDB(CounterStrikePowerName, CounterStrikePowerNameGuid);
    }


    internal class GambitResourcePoolBuilder 
    {
        const string GambitResourcePoolName = "GambitResourcePool";
        const string GambitResourcePoolNameGuid = "00da2b27-139a-4ca0-a285-aaa70d108bc8";

        public static FeatureDefinitionPower CreateAndAddToDB(string name, string guid)
            => new FeatureDefinitionPowerPoolBuilder(name, guid,
                4, RuleDefinitions.UsesDetermination.Fixed, AttributeDefinitions.Dexterity, RuleDefinitions.RechargeRate.ShortRest,
                new GuiPresentationBuilder("Feature/&GambitResourcePoolDescription", "Feature/&GambitResourcePoolTitle").Build()).AddToDB();

        public static FeatureDefinitionPower GambitResourcePool = CreateAndAddToDB(GambitResourcePoolName, GambitResourcePoolNameGuid);
    }

    internal class GambitResourcePoolAddBuilder
    {
        const string GambitResourcePoolAddName = "GambitResourcePoolAdd";
        const string GambitResourcePoolAddNameGuid = "056d786a-2611-4981-a652-704fa5056375";

        public static FeatureDefinitionPower CreateAndAddToDB(string name, string guid)
            => new FeatureDefinitionPowerPoolModifierBuilder(name, guid,
                1, RuleDefinitions.UsesDetermination.Fixed, AttributeDefinitions.Dexterity, TacticianFighterSubclassBuilder.GambitResourcePool,
                new GuiPresentationBuilder("Feature/&GambitResourcePoolAddDescription","Feature/&GambitResourcePoolAddTitle").Build()).AddToDB();

        public static FeatureDefinitionPower GambitResourcePoolAdd = CreateAndAddToDB(GambitResourcePoolAddName, GambitResourcePoolAddNameGuid);
    }

    public static class TacticianFighterSubclassBuilder
    {
        //Unfortunate copy paste error.  Going to leave it incorrect here to not give errors for people that already have a previously release.
        //It doesn't run into issues with the  actual GambitResourcePool since it's in a different DB.
        const string TacticianFighterSubclassName = "GambitResourcePool";
        const string TacticianFighterSubclassNameGuid = "00da2b27-139a-4ca0-a285-aaa70d108bc8";

        public static CharacterSubclassDefinition BuildAndAddSubclass()
        {
            SetupPowers();
            var subclassGuiPresentation = new GuiPresentationBuilder(
                    "Subclass/&TactitionFighterSubclassDescription",
                    "Subclass/&TactitionFighterSubclassTitle")
                    .SetSpriteReference(DatabaseHelper.CharacterSubclassDefinitions.RoguishShadowCaster.GuiPresentation.SpriteReference)
                    .Build();

            var definition = new CharacterSubclassDefinitionBuilder(TacticianFighterSubclassName, TacticianFighterSubclassNameGuid)
                    .SetGuiPresentation(subclassGuiPresentation)
                    .AddFeatureAtLevel(GambitResourcePool, 3)
                    .AddFeatureAtLevel(KnockDownPower, 3)
                    .AddFeatureAtLevel(InspirePower, 3)
                    .AddFeatureAtLevel(CounterStrikePower, 3)
                    .AddFeatureAtLevel(DatabaseHelper.FeatureDefinitionFeatureSets.FeatureSetChampionRemarkableAthlete, 7) //Wasn't sure what to do for level mostly a ribbon feature
                    .AddFeatureAtLevel(GambitResourcePoolAdd, 10)
                    .AddFeatureAtLevel(GambitResourcePoolAdd, 15)
                    .AddFeatureAtLevel(GambitResourcePoolAdd, 18)
                    .AddToDB();

            return definition;
        }

        public static void SetupPowers()
        {
            // Define the pool first because the modifier and powers need to refer to it.
            GambitResourcePool = GambitResourcePoolBuilder.GambitResourcePool;
            GambitResourcePoolAdd = GambitResourcePoolAddBuilder.GambitResourcePoolAdd;
            KnockDownPower = KnockDownPowerBuilder.KnockDownPower;
            InspirePower = InspirePowerBuilder.InspirePower;
            CounterStrikePower = CounterStrikePowerBuilder.CounterStrikePower;
        }

        public static FeatureDefinitionPower KnockDownPower;
        public static FeatureDefinitionPower InspirePower;
        public static FeatureDefinitionPower CounterStrikePower;
        public static FeatureDefinitionPower GambitResourcePool;
        public static FeatureDefinitionPower GambitResourcePoolAdd;
    }
}