﻿using SolastaModApi;
using SolastaModApi.Extensions;

namespace SolastaCommunityExpansion.Models
{
    public static class HouseFeatureContext
    {
        public static void LateLoad()
        {
            FixDivineSmiteRestrictions();
        }

        /**
         * Makes Divine Smite trigger only from melee attacks.
         * This wasn't relevant until we changed how SpendSpellSlot trigger works.
         */
        private static void FixDivineSmiteRestrictions()
        {
            DatabaseHelper.FeatureDefinitionAdditionalDamages.AdditionalDamagePaladinDivineSmite
                .SetAttackModeOnly(true)
                .SetRequiredProperty(RuleDefinitions.AdditionalDamageRequiredProperty.MeleeWeapon);
        }
    }
}