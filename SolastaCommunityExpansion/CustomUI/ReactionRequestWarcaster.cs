﻿using System.Collections.Generic;
using System.Linq;
using SolastaCommunityExpansion.Builders;
using SolastaModApi;
using SolastaModApi.Infrastructure;

namespace SolastaCommunityExpansion.CustomUI
{
    public class ReactionRequestWarcaster : ReactionRequest
    {
        public const string Name = "WarcasterReaction";
        public static ReactionDefinition ReactWarcasterDefinition;

        public static void Initialize()
        {
            ReactWarcasterDefinition = ReactionDefinitionBuilder
                .Create(DatabaseHelper.ReactionDefinitions.OpportunityAttack, ReactionRequestWarcaster.Name,
                    DefinitionBuilder.CENamespaceGuid)
                .SetGuiPresentation(Category.Reaction)
                .AddToDB();
        }

        public ReactionRequestWarcaster(CharacterActionParams reactionParams)
            : base(Name, reactionParams)
        {
            BuildSuboptions();
            this.ReactionParams.StringParameter2 = "Warcaster";
        }

        void BuildSuboptions()
        {
            this.SubOptionsAvailability.Clear();
            this.SubOptionsAvailability.Add(0, true);

            var battleManager = ServiceRepository.GetService<IGameLocationBattleService>() as GameLocationBattleManager;
            if (battleManager == null)
            {
                this.SelectSubOption(0);
                return;
            }

            var cantrips = new List<SpellDefinition>();
            var reactionParams = this.ReactionParams;
            var actingCharacter = reactionParams.ActingCharacter;

            actingCharacter.RulesetCharacter.EnumerateReadyAttackCantrips(cantrips);

            cantrips.RemoveAll(cantrip =>
            {
                if (cantrip.ActivationTime != RuleDefinitions.ActivationTime.Action
                    && cantrip.ActivationTime != RuleDefinitions.ActivationTime.BonusAction)
                {
                    return true;
                }

                var attackParams = new BattleDefinitions.AttackEvaluationParams();
                ActionModifier actionModifier = new ActionModifier();
                var targetCharacters = reactionParams.TargetCharacters;

                attackParams.FillForMagic(actingCharacter,
                    actingCharacter.LocationPosition,
                    cantrip.EffectDescription,
                    cantrip.Name,
                    targetCharacters[0],
                    targetCharacters[0].LocationPosition,
                    actionModifier);

                return !battleManager.InvokeMethodBool("IsValidAttackForReadiedAction", attackParams, false);
            });

            reactionParams.SpellRepertoire = new RulesetSpellRepertoire();

            var i = 1;
            foreach (var c in cantrips)
            {
                reactionParams.SpellRepertoire.KnownSpells.Add(c);
                this.SubOptionsAvailability.Add(i, true);
                i++;
            }

            this.SelectSubOption(0);
        }

        public override int SelectedSubOption
        {
            get
            {
                var spell = (ReactionParams.RulesetEffect as RulesetEffectSpell)?.SpellDefinition;
                if (spell == null)
                {
                    return 0;
                }

                return ReactionParams.SpellRepertoire.KnownSpells.FindIndex(s => s == spell) + 1;
            }
        }


        public override void SelectSubOption(int option)
        {
            this.ReactionParams.RulesetEffect?.Terminate(false);
            var reactionParams = this.ReactionParams;

            var targetCharacters = reactionParams.TargetCharacters;

            while (targetCharacters.Count > 1)
            {
                reactionParams.TargetCharacters.RemoveAt(targetCharacters.Count - 1);
                reactionParams.ActionModifiers.RemoveAt(reactionParams.ActionModifiers.Count - 1);
            }

            var actingCharacter = reactionParams.ActingCharacter;
            if (option == 0)
            {
                reactionParams.ActionDefinition = ServiceRepository.GetService<IGameLocationActionService>()
                    .AllActionDefinitions[ActionDefinitions.Id.AttackOpportunity];
                reactionParams.RulesetEffect = null;
                var attackParams = new BattleDefinitions.AttackEvaluationParams();
                var actionModifier = new ActionModifier();
                attackParams.FillForPhysicalReachAttack(actingCharacter,
                    actingCharacter.LocationPosition,
                    reactionParams.AttackMode,
                    reactionParams.TargetCharacters[0],
                    reactionParams.TargetCharacters[0].LocationPosition, actionModifier);
                reactionParams.ActionModifiers[0] = actionModifier;
            }
            else
            {
                reactionParams.ActionDefinition = ServiceRepository.GetService<IGameLocationActionService>()
                    .AllActionDefinitions[ActionDefinitions.Id.CastReaction];
                var spell = reactionParams.SpellRepertoire.KnownSpells[option - 1];
                IRulesetImplementationService rulesService =
                    ServiceRepository.GetService<IRulesetImplementationService>();
                var rulesetCharacter = actingCharacter.RulesetCharacter;
                rulesetCharacter.CanCastSpell(spell, true, out var spellRepertoire);
                var spellEffect = rulesService.InstantiateEffectSpell(rulesetCharacter, spellRepertoire,
                    spell, spell.SpellLevel, false);
                ReactionParams.RulesetEffect = spellEffect;

                var spelltargets = spellEffect.ComputeTargetParameter();
                if (reactionParams.RulesetEffect.EffectDescription.IsSingleTarget && spelltargets > 0)
                {
                    var target = reactionParams.TargetCharacters.FirstOrDefault();
                    var mod = reactionParams.ActionModifiers.FirstOrDefault();

                    while (target != null && mod != null && reactionParams.TargetCharacters.Count < spelltargets)
                    {
                        reactionParams.TargetCharacters.Add(target);
                        // Technically casts after first might need to have different mods, but not by much since we attacking same target.
                        reactionParams.ActionModifiers.Add(mod);
                    }
                }

                // for (int i = 0; i < spelltargets; i++)
                // {
                //     var attackParams = new BattleDefinitions.AttackEvaluationParams();
                //     var actionModifier = new ActionModifier();
                //     attackParams.FillForMagic(actingCharacter,
                //         actingCharacter.LocationPosition,
                //         this.ReactionParams.RulesetEffect.EffectDescription, spell.Name,
                //         reactionParams.TargetCharacters[0],
                //         reactionParams.TargetCharacters[0].LocationPosition, actionModifier);
                //     reactionParams.ActionModifiers[i] = actionModifier;
                // }
            }
        }


        public override string SuboptionTag => "Warcaster";

        public override bool IsStillValid
        {
            get
            {
                GameLocationCharacter targetCharacter = this.ReactionParams.TargetCharacters[0];
                return ServiceRepository.GetService<IGameLocationCharacterService>().ValidCharacters
                    .Contains(targetCharacter) && !targetCharacter.RulesetCharacter.IsDeadOrDyingOrUnconscious;
            }
        }

        public override string FormatDescription()
        {
            GuiCharacter caster = new GuiCharacter(this.Character);
            GuiCharacter target = new GuiCharacter(this.ReactionParams.TargetCharacters[0]);
            return Gui.Format(base.FormatDescription(), caster.Name, target.Name, "");
        }

        public override string FormatReactDescription() => Gui.Format(base.FormatReactDescription(), "");

        public override void OnSetInvalid()
        {
            base.OnSetInvalid();
            this.ReactionParams.RulesetEffect?.Terminate(false);
        }
    }
}