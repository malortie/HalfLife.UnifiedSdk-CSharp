﻿using HalfLife.UnifiedSdk.Utilities.Entities;
using HalfLife.UnifiedSdk.Utilities.Games;
using HalfLife.UnifiedSdk.Utilities.Tools.UpgradeTool;
using System.Collections.Immutable;

namespace HalfLife.UnifiedSdk.MapUpgrader.Upgrades.BlueShift
{
    /// <summary>
    /// Updates references to specific sentences to use the correct vanilla Half-Life sentence.
    /// </summary>
    internal sealed class ChangeBlueShiftSentencesUpgrade : IMapUpgradeAction
    {
        private const string MessageKey = "message";

        private static readonly ImmutableDictionary<string, string> SentenceMap = new Dictionary<string, string>
        {
            // The NA group is for No Access, EA is for Enable Access.
            // BS incorrectly adds an access granted sentence to the NA group.
            { "!NA1", "!EA0" },
        }.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);

        public void Apply(MapUpgradeContext context)
        {
            if (context.GameInfo != ValveGames.BlueShift)
            {
                return;
            }

            foreach (var entity in context.Map.Entities.OfClass("ambient_generic"))
            {
                if (entity.TryGetValue(MessageKey, out var value) && SentenceMap.TryGetValue(value, out var replacement))
                {
                    entity.SetString(MessageKey, replacement);
                }
            }
        }
    }
}