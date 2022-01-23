﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

namespace SolastaCommunityExpansion.Patches.GameUi.AdventureLog
{
    [HarmonyPatch(typeof(GameLocationLabelScreen), "ShowTextFeedback")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class GameLocationLabelScreen_ShowTextFeedback
    {
        internal static void Postfix(string caption)
        {
            if (Main.Settings.EnableAdventureLogTextFeedback)
            {
                Models.AdventureLogContext.LogEntry("Feedback", caption);
            }
        }
    }
}
