using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace LW.Util.EasyButton.Editor.View
{
    public class BooleanPropertyView<TObj> : BasePropertyView<TObj>
    {
        private static Dictionary<string, Action<TObj, bool>> SetValueFuncDictionary { get; } = new();
        private static Dictionary<string, Func<TObj, bool>>   GetValueFuncDictionary { get; } = new();

        private Action<TObj, bool> _setValueFunc;
        private Func<TObj, bool>   _getValueFunc;
        private TObj               Data { get; set; }
        
        private Toggle FieldView { get; set; }

        public BooleanPropertyView()
        {
            FieldView = new Toggle();
            Add(FieldView);
            FieldView.RegisterValueChangedCallback(OnValueChanged);
        }

        public override void Initialize(TObj data, string fieldPath)
        {
            base.Initialize(data, fieldPath);

            if (!SetValueFuncDictionary.TryGetValue(fieldPath, out _setValueFunc))
            {
                _setValueFunc = SetValueFuncDictionary[fieldPath] =
                    PropertyViewUtils.SetValueFunc<TObj, bool>(fieldPath);
            }

            if (!GetValueFuncDictionary.TryGetValue(fieldPath, out _getValueFunc))
            {
                _getValueFunc = GetValueFuncDictionary[fieldPath] =
                    PropertyViewUtils.GetValueFunc<TObj, bool>(fieldPath);
            }

            Data = data;
            FieldView.SetValueWithoutNotify(_getValueFunc?.Invoke(Data) ?? false);
            FieldView.label = fieldPath.Split(".")[^1];
        }

        public override void Reset()
        {
            base.Reset();

            _setValueFunc = null;
            _getValueFunc = null;
        }

        private void OnValueChanged(ChangeEvent<bool> evt)
        {
            if (Data == null)
            {
                return;
            }
            
            _setValueFunc?.Invoke(Data, evt.newValue);
        }
    }
}