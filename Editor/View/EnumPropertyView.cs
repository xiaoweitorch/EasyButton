using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace LW.Util.EasyButton.Editor.View
{
    public class EnumPropertyView<TObj, TVar> : BasePropertyView<TObj> where TVar : Enum
    {
        private static Dictionary<string, Action<TObj, TVar>> SetValueFuncDictionary { get; } = new();
        private static Dictionary<string, Func<TObj, TVar>>   GetValueFuncDictionary { get; } = new();

        private Action<TObj, TVar> _setValueFunc;
        private Func<TObj, TVar>   _getValueFunc;
        private TObj               Data { get; set; }
        
        private PopupField<Enum>   EnumView      { get; }
        private MaskField EnumFlagsView { get; }

        public EnumPropertyView()
        {
            EnumView = new PopupField<Enum>();
            EnumView.RegisterValueChangedCallback(OnValueChanged);

            EnumFlagsView = new MaskField();
            EnumFlagsView.RegisterValueChangedCallback(OnFlagsValueChanged);
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
            
            if (typeof(TVar).GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
            {
                EnumFlagsView.label   = fieldPath.Split(".")[^1];
                EnumFlagsView.choices = Enum.GetValues(typeof(TVar)).Cast<TVar>().Select(t => t.ToString()).ToList();
                EnumFlagsView.choicesMasks = Enum.GetValues(typeof(TVar)).Cast<int>().ToList();
                EnumFlagsView.SetValueWithoutNotify(Convert.ToInt32(_getValueFunc.Invoke(Data)));
                Add(EnumFlagsView);
            }
            else
            {
                EnumView.label   = fieldPath.Split(".")[^1];
                EnumView.choices = Enum.GetValues(typeof(TVar)).Cast<Enum>().ToList();
                EnumView.SetValueWithoutNotify(_getValueFunc.Invoke(Data));
                Add(EnumView);
            }
        }

        public override void Reset()
        {
            base.Reset();

            _setValueFunc = null;
            _getValueFunc = null;
            // TODO: quning 应该把flags和普通的拆开
            Remove(EnumView);
            Remove(EnumFlagsView);
        }

        private void OnFlagsValueChanged(ChangeEvent<int> evt)
        {
            if (Data == null)
            {
                return;
            }

            var newValue = (TVar)Enum.ToObject(typeof(TVar), evt.newValue);
            _setValueFunc?.Invoke(Data, newValue);
        }

        private void OnValueChanged(ChangeEvent<Enum> evt)
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