using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modio.Models;
using ModManagerUI.EventSystem;
using UnityEngine.UIElements;

namespace ModManagerUI.UIComponents.ModManagerPanel
{
    public abstract class CustomRadioButtonGroup
    {
        private readonly VisualElement _root;
        private readonly TagOption _tagOption;
        
        private int _tagsLastValue = -1;

        public RadioButtonGroup? RadioButtonGroup { get; private set; }
        
        public List<string> TagOptions => _tagOption.Tags;
        
        public CustomRadioButtonGroup(VisualElement root, TagOption tagOption)
        {
            _root = root;
            _tagOption = tagOption;
        }
        
        public void Initialize(bool formatTags = false)
        {
            RadioButtonGroupRegistry.RadioButtonGroups.Add(this);
            
            var header = new Label();
            header.name = $"{_tagOption.Name}Header";
            header.text = _tagOption.Name;
            header.AddToClassList("text--default");
            header.AddToClassList("mods-box__tags-label");
            _root.Add(header);

            var radioButtonGroup = new RadioButtonGroup();
            radioButtonGroup.name = $"{_tagOption.Name}TagRadioButtonGroup";
            radioButtonGroup.AddToClassList("mods-box__tags");
            
            radioButtonGroup.choices = formatTags ? _tagOption.Tags.Select(FormatTag) : _tagOption.Tags;
            radioButtonGroup.RegisterValueChangedCallback(_ => OnValueChanged());

            radioButtonGroup.Query<RadioButton>().ForEach(radioButton =>
            {
                radioButton.RegisterCallback((ClickEvent @event) => ClickTagRadioButton(@event));
            });

            RadioButtonGroup = radioButtonGroup;
            
            _root.Add(radioButtonGroup);
        }

        public bool HasTagSelected()
        {
            return _tagsLastValue != -1;
        }

        public string GetActiveTag()
        {
            return TagOptions[_tagsLastValue];
        }

        private static string FormatTag(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            var newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);

            for (var i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                    newText.Append(' ');

                newText.Append(text[i]);
            }

            return newText.ToString();
        }
        
        protected virtual void OnValueChanged()
        {
            _tagsLastValue = RadioButtonGroup!.value;
            EventBus.Instance.PostEvent(new ModManagerPanelRefreshEvent());
        }

        protected virtual void ClickTagRadioButton(ClickEvent clickEvent)
        {
            if (_tagsLastValue == RadioButtonGroup!.value)
            {
                RadioButtonGroup.value = -1;
            }
        }
    }
}