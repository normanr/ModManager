using System;
using System.Collections.Generic;
using System.Linq;
using Modio.Models;
using ModManager.VersionSystem;
using Timberborn.Common;
using Timberborn.DropdownSystem;
using UnityEngine;

namespace ModManagerUI.UIComponents.ModFullInfo
{
    public class VersionDropdownProvider : IExtendedDropdownProvider
    {
        private readonly ModFullInfoController _infoController;
        private readonly List<File> _versions;
        private readonly Dictionary<uint, File> _versionsById;
        
        public VersionDropdownProvider(
            ModFullInfoController infoController,
            List<File> versions)
        {
            _infoController = infoController;
            _versions = versions;
            _versionsById = versions.ToDictionary(x => x.Id);
        }

        public IReadOnlyList<string> Items => _versions.Select(x => x.Id + ":" + x.Version ?? "").ToList();

        public string GetValue()
        {
            return _infoController.CurrentFile!.Id + ":" + _infoController.CurrentFile!.Version ?? "";
        }

        public void SetValue(string value)
        {
            uint.TryParse(value.Split(":", 2)[0], out var id);
            var currFile = _versionsById.GetOrDefault(id);
            _infoController.CurrentFile = currFile;
            _infoController.Refresh();
        }

        public string FormatDisplayText(string value)
        {
            return value.Split(":", 2)[1];
        }

        public Sprite GetIcon(string value)
        {
            uint.TryParse(value.Split(":", 2)[0], out var id);
            var versionStatus = VersionStatusService.GetVersionStatus(_versionsById.GetOrDefault(id));
            switch (versionStatus)
            {
                case VersionStatus.Unknown:
                    return StatusIconLoader.Instance.UnknownSprite;
                case VersionStatus.Compatible:
                    return StatusIconLoader.Instance.CompatibleSprite;
                case VersionStatus.Incompatible:
                    return StatusIconLoader.Instance.IncompatibleSprite;
                default:
                    throw new ArgumentOutOfRangeException(versionStatus.ToString());
            }
        }
    }
}
