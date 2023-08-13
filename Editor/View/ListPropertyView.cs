using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace LW.Util.EasyButton.Editor.View
{
    public class ListPropertyView<TObj, TVar> : BasePropertyView<TObj>
    {
        public const string BaseClassName              = "list-property-view";
        public const string NoneViewClassName          = "list-property-view_none-view";
        public const string NoneViewNoneLabelClassName = "list-property-view_none-view_none-label";
        public const string FieldViewClassName         = "list-property-view_field-view";

        private static Dictionary<string, Action<TObj, List<TVar>>> SetValueFuncDictionary { get; } = new();
        private static Dictionary<string, Func<TObj, List<TVar>>>   GetValueFuncDictionary { get; } = new();

        private Action<TObj, List<TVar>> _setValueFunc;
        private Func<TObj, List<TVar>>   _getValueFunc;
        private TObj                     Data      { get; set; }
        private List<TVar>               FieldData { get; set; }
        private string                   FieldPath { get; set; }

        private VisualElement NoneView  { get; }
        private ListView      FieldView { get; }

        public ListPropertyView()
        {
            AddToClassList(BaseClassName);

            NoneView = new VisualElement();
            NoneView.AddToClassList(NoneViewClassName);

            var noneLabel = new Label($"None ({typeof(List<TVar>)})");
            noneLabel.AddToClassList(NoneViewNoneLabelClassName);
            NoneView.Add(noneLabel);

            NoneView.Add(new Button(CreateNewInstance) { text = "创建" });
            Add(NoneView);

            FieldView                         = new ListView();
            FieldView.showBorder              = true;
            FieldView.showFoldoutHeader       = true;
            FieldView.showBoundCollectionSize = false;
            FieldView.virtualizationMethod    = CollectionVirtualizationMethod.DynamicHeight;
            FieldView.showAddRemoveFooter     = true;
            FieldView.makeItem                = MakeItem;
            FieldView.bindItem                = BindItem;
            FieldView.unbindItem              = UnbindItem;
            FieldView.AddToClassList(FieldViewClassName);

            var toggle = FieldView.Q<Toggle>();
            toggle?.Add(new Button(SetNull) { text = "删除" });
            Add(FieldView);
        }

        private static VisualElement MakeItem()
        {
            return PropertyViewProvider<TObj>.GetPropertyView<TVar>();
        }

        private void BindItem(VisualElement element, int index)
        {
            var view = (BasePropertyView<TObj>)element;
            view.Initialize(Data, $"{FieldPath}[{index}]");
        }

        private static void UnbindItem(VisualElement element, int index)
        {
            var view = (BasePropertyView<TObj>)element;
            view.Reset();
        }

        public override void Initialize(TObj data, string fieldPath)
        {
            base.Initialize(data, fieldPath);

            if (!SetValueFuncDictionary.TryGetValue(fieldPath, out _setValueFunc))
            {
                _setValueFunc = SetValueFuncDictionary[fieldPath] =
                    PropertyViewUtils.SetValueFunc<TObj, List<TVar>>(fieldPath);
            }

            if (!GetValueFuncDictionary.TryGetValue(fieldPath, out _getValueFunc))
            {
                _getValueFunc = GetValueFuncDictionary[fieldPath] =
                    PropertyViewUtils.GetValueFunc<TObj, List<TVar>>(fieldPath);
            }

            Data      = data;
            FieldPath = fieldPath;
            FieldData = _getValueFunc?.Invoke(Data);

            if (FieldData == null)
            {
                NoneView.SetDisplay(true);
                FieldView.SetDisplay(false);

                ResetFieldView();
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
            FieldView.headerTitle = FieldPath.Split(".")[^1];
            FieldView.itemsSource = FieldData;
        }

        private void ResetFieldView()
        {
            FieldView.itemsSource = null;
        }

        public override void Reset()
        {
            base.Reset();

            _setValueFunc         = null;
            _getValueFunc         = null;
            FieldView.itemsSource = null;
        }

        private void CreateNewInstance()
        {
            _setValueFunc?.Invoke(Data, new List<TVar>());

            FieldData = _getValueFunc?.Invoke(Data);
            NoneView.SetDisplay(false);
            FieldView.SetDisplay(true);

            InitializeFieldView();
        }


        private void SetNull()
        {
            _setValueFunc?.Invoke(Data, null);

            FieldData = null;
            NoneView.SetDisplay(true);
            FieldView.SetDisplay(false);

            ResetFieldView();
        }
    }
}