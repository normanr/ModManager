using System;
using Modio.Models;
using ModManager.VersionSystem;
using UnityEngine.UIElements;

namespace ModManagerUI.UIComponents.ModManagerPanel
{
    public class VersionStatusRadioButtonGroup : CustomRadioButtonGroup
    {
        public VersionStatusRadioButtonGroup(VisualElement root, TagOption tagOption) : base(root, tagOption)
        {
        }

        protected override void OnValueChanged()
        {
            if (!HasTagSelected() || !Enum.TryParse(GetActiveTag(), out VersionStatus versionStatus))
            {
                FilterController.VersionStatusFilter = null;
                base.OnValueChanged();
                return;
            }
            FilterController.VersionStatusFilter = versionStatus;
            base.OnValueChanged();
        }
    }
}