using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace LW.Util.EasyButton.Editor.View
{
    public class ClassPropertyView<TObj, TVar> : BasePropertyView<TObj> where TVar : class, new()
    {
        public const   string BaseClassName = "class-property-view";
        public const   string NoneViewClassName = "class-property-view_none-view";
        public const   string NoneViewLabelClassName = "class-property-view_none-view-label";
        public const   string FieldViewClassName = "class-property-view_field-view";
        private static Dictionary<string, Action<TObj, TVar>> SetValueFuncDictionary { get; } = new();
        private static Dictionary<string, Func<TObj, TVar>> GetValueFuncDictionary { get; } = new();

        private Action<TObj, TVar> _setValueFunc;
        private Func<TObj, TVar>   _getValueFunc;
        private TObj               Data      { get; set; }
        private string             FieldPath { get; set; }

        private VisualElement NoneView  { get; }
        private Label         NoneLabel { get; }
        private Foldout       FieldView { get; }

        public ClassPropertyView()
        {
            NoneView = new VisualElement();
            NoneView.AddToClassList(NoneViewClassName);

            NoneLabel = new Label();
            NoneView.Add(NoneLabel);

            var noneLabel = new Label($"None {typeof(TVar)}");
            noneLabel.AddToClassList(NoneViewLabelClassName);
            NoneView.Add(noneLabel);
            NoneView.Add(new Button(CreateNewInstance) { text = "创建" });

            Add(NoneView);

            FieldView = new Foldout();
            var toggle = FieldView.Q<Toggle>();
            toggle.Add(new Button(SetNull) { text = "删除" });
            Add(FieldView);
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

            Data      = data;
            FieldPath = fieldPath;

            var fieldData = _getValueFunc?.Invoke(Data);
            if (fieldData == null)
            {
                NoneView.SetDisplay(true);
                FieldView.SetDisplay(false);
                
                NoneLabel.text = FieldPath.Split(".")[^1];
            }
            else
            {
                NoneView.SetDisplay(false);
                FieldView.SetDisplay(true);
                InitializeFieldView();
            }
        }

        private void InitializeFieldView()
        {
            FieldView.text = FieldPath.Split(".")[^1];
            foreach (var fieldInfo in typeof(TVar).GetFields())
            {
                var propertyView = PropertyViewProvider<TObj>.GetPropertyView(fieldInfo.FieldType);
                propertyView.Initialize(Data, $"{FieldPath}.{fieldInfo.Name}");
                FieldView.Add(propertyView);
            }
        }

        public override void Reset()
        {
            base.Reset();

            _setValueFunc = null;
            _getValueFunc = null;

            FieldView.Clear();
        }

        private void CreateNewInstance()
        {
            _setValueFunc(Data, new TVar());

            NoneView.SetDisplay(false);
            FieldView.SetDisplay(true);
            InitializeFieldView();
        }

        private void SetNull()
        {
            FieldView.Clear();
            NoneView.SetDisplay(true);
            FieldView.SetDisplay(false);
            NoneLabel.text = FieldPath.Split(".")[^1];
            
            _setValueFunc(Data, null);
        }
    }
}