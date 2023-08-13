using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace LW.Util.EasyButton.Editor.View
{
    public class ObjectPropertyView<TObj, TVar> : BasePropertyView<TObj> where TVar : Object
    {
        private static Dictionary<string, Action<TObj, TVar>> SetValueFuncDictionary { get; } = new();
        private static Dictionary<string, Func<TObj, TVar>>   GetValueFuncDictionary { get; } = new();

        private Action<TObj, TVar> _setValueFunc;
        private Func<TObj, TVar>   _getValueFunc;
        private TObj               Data { get; set; }
        
        private ObjectField FieldView { get; set; }

        public ObjectPropertyView()
        {
            FieldView = new ObjectField();
            Add(FieldView);
            FieldView.objectType = typeof(TVar);
            FieldView.RegisterValueChangedCallback(OnValueChanged);
        }

        public override void Initialize(TObj data, string fieldPath)
        {
            base.Initialize(data, fieldPath);

            if (!SetValueFuncDictionary.TryGetValue(fieldPath, out _setValueFunc))
            {
                _setValueFunc = SetValueFuncDictionary[fieldPath] =
                    PropertyViewUtils.SetValueFunc<TObj, TVar>(fieldPath);
            }

            if (!GetValueFuncDictionary.TryGetValue(fieldPath, out _getValueFunc))
            {
                _getValueFunc = GetValueFuncDictionary[fieldPath] =
                    PropertyViewUtils.GetValueFunc<TObj, TVar>(fieldPath);
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

        private void OnValueChanged(ChangeEvent<Object> evt)
        {
            if (Data == null)
            {
                return;
            }

            if (evt.newValue is not TVar newValue)
            {
                return;
            }
            
            _setValueFunc?.Invoke(Data, newValue);
        }
    }
}