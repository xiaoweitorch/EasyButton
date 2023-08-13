using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace LW.Util.EasyButton.Editor.View
{
    public class UIntegerPropertyView<TObj> : BasePropertyView<TObj>
    {
        private static Dictionary<string, Action<TObj, uint>> SetValueFuncDictionary { get; } = new();
        private static Dictionary<string, Func<TObj, uint>>   GetValueFuncDictionary { get; } = new();

        private Action<TObj, uint> _setValueFunc;
        private Func<TObj, uint>   _getValueFunc;
        private TObj              Data { get; set; }
        
        private UnsignedIntegerField FieldView { get; set; }

        public UIntegerPropertyView()
        {
            FieldView = new UnsignedIntegerField();
            Add(FieldView);
            FieldView.RegisterValueChangedCallback(OnValueChanged);
        }

        public override void Initialize(TObj data, string fieldPath)
        {
            base.Initialize(data, fieldPath);

            if (!SetValueFuncDictionary.TryGetValue(fieldPath, out _setValueFunc))
            {
                _setValueFunc = SetValueFuncDictionary[fieldPath] =
                    PropertyViewUtils.SetValueFunc<TObj, uint>(fieldPath);
            }

            if (!GetValueFuncDictionary.TryGetValue(fieldPath, out _getValueFunc))
            {
                _getValueFunc = GetValueFuncDictionary[fieldPath] =
                    PropertyViewUtils.GetValueFunc<TObj, uint>(fieldPath);
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

        private void OnValueChanged(ChangeEvent<uint> evt)
        {
            if (Data == null)
            {
                return;
            }
            
            _setValueFunc?.Invoke(Data, evt.newValue);
        }
    }
}