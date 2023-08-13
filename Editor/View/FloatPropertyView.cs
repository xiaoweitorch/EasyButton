using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace LW.Util.EasyButton.Editor.View
{
    public class FloatPropertyView<TObj> : BasePropertyView<TObj>
    {
        private static Dictionary<string, Action<TObj, float>> SetValueFuncDictionary { get; } = new();
        private static Dictionary<string, Func<TObj, float>>   GetValueFuncDictionary { get; } = new();

        private Action<TObj, float> _setValueFunc;
        private Func<TObj, float>   _getValueFunc;
        private TObj              Data { get; set; }
        
        private FloatField FieldView { get; set; }

        public FloatPropertyView()
        {
            FieldView = new FloatField();
            Add(FieldView);
            FieldView.RegisterValueChangedCallback(OnValueChanged);
        }

        public override void Initialize(TObj data, string fieldPath)
        {
            base.Initialize(data, fieldPath);

            if (!SetValueFuncDictionary.TryGetValue(fieldPath, out _setValueFunc))
            {
                _setValueFunc = SetValueFuncDictionary[fieldPath] =
                    PropertyViewUtils.SetValueFunc<TObj, float>(fieldPath);
            }

            if (!GetValueFuncDictionary.TryGetValue(fieldPath, out _getValueFunc))
            {
                _getValueFunc = GetValueFuncDictionary[fieldPath] =
                    PropertyViewUtils.GetValueFunc<TObj, float>(fieldPath);
            }

            Data = data;
            FieldView.SetValueWithoutNotify(_getValueFunc?.Invoke(Data) ?? 0);
            FieldView.label = fieldPath.Split(".")[^1];
        }

        public override void Reset()
        {
            base.Reset();

            _setValueFunc = null;
            _getValueFunc = null;
        }

        private void OnValueChanged(ChangeEvent<float> evt)
        {
            if (Data == null)
            {
                return;
            }
            
            _setValueFunc?.Invoke(Data, evt.newValue);
        }
    }
}