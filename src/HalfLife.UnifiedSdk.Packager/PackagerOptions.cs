﻿namespace HalfLife.UnifiedSdk.Packager
{
    internal sealed record PackagerOptions(string PackageName, string RootDirectory, IEnumerable<PackageDirectory> Directories)
    {
        public bool Verbose { get; init; }

        public bool ListOmittedFiles { get; init; }
    }
}
