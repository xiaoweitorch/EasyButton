using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace LW.Util.EasyButton.Editor.View
{
    public class DoublePropertyView<TObj> : BasePropertyView<TObj>
    {
        private static Dictionary<string, Action<TObj, double>> SetValueFuncDictionary { get; } = new();
        private static Dictionary<string, Func<TObj, double>>   GetValueFuncDictionary { get; } = new();

        private Action<TObj, double> _setValueFunc;
        private Func<TObj, double>   _getValueFunc;
        private TObj                 Data { get; set; }
        
        private DoubleField FieldView { get; set; }

        public DoublePropertyView()
        {
            FieldView = new DoubleField();
            Add(FieldView);
            FieldView.RegisterValueChangedCallback(OnValueChanged);
        }

        public override void Initialize(TObj data, string fieldPath)
        {
            base.Initialize(data, fieldPath);

            if (!SetValueFuncDictionary.TryGetValue(fieldPath, out _setValueFunc))
            {
                _setValueFunc = SetValueFuncDictionary[fieldPath] =
                    PropertyViewUtils.SetValueFunc<TObj, double>(fieldPath);
            }

            if (!GetValueFuncDictionary.TryGetValue(fieldPath, out _getValueFunc))
            {
                _getValueFunc = GetValueFuncDictionary[fieldPath] =
                    PropertyViewUtils.GetValueFunc<TObj, double>(fieldPath);
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

        private void OnValueChanged(ChangeEvent<double> evt)
        {
            if (Data == null)
            {
                return;
            }
            
            _setValueFunc?.Invoke(Data, evt.newValue);
        }
    }
}