﻿using HalfLife.UnifiedSdk.MapUpgrader.Upgrades;
using HalfLife.UnifiedSdk.Utilities.Games;
using HalfLife.UnifiedSdk.Utilities.Tools;
using HalfLife.UnifiedSdk.Utilities.Tools.UpgradeTool;
using System.CommandLine;
using System.CommandLine.IO;

namespace HalfLife.UnifiedSdk.Installer
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            var games = ValveGames.GoldSourceGames;

            var defaultGame = ValveGames.HalfLife1;

            var gameOption = new Option<string>("--game", description: "The name of a game's mod directory to apply upgrades for that game."
                + $"If not specified, uses \"{defaultGame.ModDirectory}\"");

            gameOption.AddCompletions(games.Select(g => g.ModDirectory).ToArray());

            gameOption.AddValidator((result) =>
            {
                var game = result.GetValueForOption(gameOption)!;

                if (!games.Any(g => g.ModDirectory == game))
                {
                    result.ErrorMessage = $"Invalid game \"{game}\" specified.";
                }
            });

            var mapsOption = new Option<IEnumerable<FileInfo>>("--maps", description: "List of maps to upgrade");

            var rootCommand = new RootCommand("Half-Life Unified SDK map upgrader")
            {
                gameOption,
                mapsOption
            };

            rootCommand.SetHandler((string? game, IEnumerable<FileInfo> maps, IConsole console) =>
            {
                game ??= defaultGame.ModDirectory;

                var gameInfo = games.SingleOrDefault(g => g.ModDirectory == game);

                if (gameInfo is null)
                {
                    //Should never get here.
                    return;
                }

                if (!maps.Any())
                {
                    console.WriteLine("No maps to upgrade");
                    return;
                }

                var upgradeTool = MapUpgradeToolFactory.Create();

                console.WriteLine($"Upgrading maps for game {gameInfo.Name} ({gameInfo.ModDirectory}) to version {upgradeTool.LatestVersion}");

                foreach (var map in maps)
                {
                    var mapData = MapFormats.Deserialize(map.FullName);

                    var currentVersion = upgradeTool.GetVersion(mapData);

                    console.Out.Write($"Upgrading \"{map.FullName}\" from version {currentVersion}");

                    upgradeTool.Upgrade(new MapUpgradeCommand(mapData, gameInfo));

                    using var stream = File.Open(map.FullName, FileMode.Create, FileAccess.Write);

                    mapData.Serialize(stream);
                }
            }, gameOption, mapsOption);

            return rootCommand.Invoke(args);
        }
    }
}