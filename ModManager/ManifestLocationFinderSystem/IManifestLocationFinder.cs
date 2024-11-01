﻿using System.Collections.Generic;
using ModManager.AddonSystem;

namespace ModManager.ManifestLocationFinderSystem
{
    public interface IManifestLocationFinder
    {
        public IEnumerable<ModManagerManifest> Find();
    }
}