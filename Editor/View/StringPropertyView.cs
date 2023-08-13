using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace LW.Util.EasyButton.Editor.View
{
    public class StringPropertyView<TObj> : BasePropertyView<TObj>
    {
        private static Dictionary<string, Action<TObj, string>> SetValueFuncDictionary { get; } = new();
        private static Dictionary<string, Func<TObj, string>>   GetValueFuncDictionary { get; } = new();

        private Action<TObj, string> _setValueFunc;
        private Func<TObj, string>   _getValueFunc;
        private TObj                 Data { get; set; }
        
        private TextField FieldView { get; set; }

        public StringPropertyView()
        {
            FieldView = new TextField();
            Add(FieldView);
            FieldView.RegisterValueChangedCallback(OnValueChanged);
        }

        public override void Initialize(TObj data, string fieldPath)
        {
            base.Initialize(data, fieldPath);

            if (!SetValueFuncDictionary.TryGetValue(fieldPath, out _setValueFunc))
            {
                _setValueFunc = SetValueFuncDictionary[fieldPath] =
                    PropertyViewUtils.SetValueFunc<TObj, string>(fieldPath);
            }

            if (!GetValueFuncDictionary.TryGetValue(fieldPath, out _getValueFunc))
            {
                _getValueFunc = GetValueFuncDictionary[fieldPath] =
                    PropertyViewUtils.GetValueFunc<TObj, string>(fieldPath);
            }

            Data = data;
            FieldView.SetValueWithoutNotify(_getValueFunc?.Invoke(Data));
            FieldView.label = fieldPath.Split(".")[^1];
        }

        public override void Reset()
        {
            base.Reset();

            _setValueFunc = null;
            _getValueFunc = null;
        }

        private void OnValueChanged(ChangeEvent<string> evt)
        {
            if (Data == null)
            {
                return;
            }
            
            _setValueFunc?.Invoke(Data, evt.newValue);
        }
    }
}