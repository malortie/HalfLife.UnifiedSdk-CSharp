﻿using HalfLife.UnifiedSdk.Utilities.Logging;
using Sledge.Formats.Bsp;
using System.CommandLine;
using System.Diagnostics;

namespace HalfLife.UnifiedSdk.Bsp2Obj
{
    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var bspFileNameOption = new Option<FileInfo>("--filename", description: "Path to the BSP filename")
            {
                IsRequired = true,
            };

            var destinationDirectoryOption = new Option<DirectoryInfo?>("--destination",
                description: "Directory to save the OBJ file to."
                + "If not provided the file will be saved to the directory that the source file is located in");

            var rootCommand = new RootCommand("Half-Life game content installer")
            {
                bspFileNameOption,
                destinationDirectoryOption
            };

            rootCommand.Description = "Half-Life Unified SDK BSP to OBJ converter";

            rootCommand.SetHandler((bspFileName, destinationDirectory, logger) =>
            {
                if (!bspFileName.Exists)
                {
                    logger.Error("The given bsp file \"{BspFileName}\" does not exist", bspFileName);
                    return;
                }

                destinationDirectory ??= bspFileName.Directory;

                if (destinationDirectory is null)
                {
                    logger.Error("Destination directory is invalid");
                    return;
                }

                BspFile bspFile;

                try
                {
                    logger.Information("Reading BSP file");
                    using var stream = bspFileName.OpenRead();
                    bspFile = new(stream);
                }
                catch (Exception e)
                {
                    logger.Error(e, "An error occurred while reading the BSP file");
                    return;
                }

                logger.Information("Converting BSP data to OBJ");
                var objFile = BspToObjConverter.Convert(bspFile);

                try
                {
                    destinationDirectory.Create();
                }
                catch (Exception e)
                {
                    logger.Error(e, "An error occurred while creating the destination directory");
                    return;
                }

                try
                {
                    var baseName = Path.GetFileNameWithoutExtension(bspFileName.FullName);
                    var destinationFileName = Path.Combine(destinationDirectory.FullName, baseName + ".obj");

                    logger.Information("Writing OBJ file {FileName}", destinationFileName);

                    using var destinationFile = File.Open(destinationFileName, FileMode.Create);

                    var version = FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).FileVersion ?? "Unknown version";

                    objFile.HeaderText = $" Generated by Half-Life Unified SDK Bsp2Obj {version}\n{objFile.HeaderText}";

                    objFile.WriteTo(destinationFile);
                }
                catch (Exception e)
                {
                    logger.Error(e, "An error occurred while writing the obj file");
                    return;
                }
            }, bspFileNameOption, destinationDirectoryOption, LoggerBinder.Instance);

            return await rootCommand.InvokeAsync(args);
        }
    }
}