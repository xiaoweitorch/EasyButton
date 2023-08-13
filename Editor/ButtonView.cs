using System.Collections.Generic;
using System.Linq;
using LW.Util.EasyButton.Editor.View;
using UnityEngine.UIElements;

namespace LW.Util.EasyButton.Editor
{
    public class ButtonStaticWithoutParamsView : VisualElement
    {
        public ButtonInfo Info         { get; private set; }
        public Button     InvokeButton { get; }

        public ButtonStaticWithoutParamsView()
        {
            InvokeButton         =  new Button();
            InvokeButton.clicked += OnButtonClick;
            Add(InvokeButton);
        }

        public void Initialize(ButtonInfo info)
        {
            Info = info;

            if (Info == null || !info.Valid || !info.IsStatic)
            {
                Reset();
                return;
            }

            InvokeButton.text = Info.DisplayName;
        }

        public void Reset()
        {
            Info              = null;
            InvokeButton.text = null;
        }

        private void OnButtonClick()
        {
            if (Info == null || !Info.Valid || !Info.IsStatic)
            {
                return;
            }

            Info.TriggerStaticWithoutParams.Invoke();
        }
    }

    public class ButtonStaticWithParamsView : VisualElement
    {
        public ButtonInfo           Info      { get; private set; }
        public Foldout              Container { get; }
        public List<IParameterInfo> Params    { get; } = new();

        public ButtonStaticWithParamsView()
        {
            Container = new Foldout();
            Add(Container);

            var toggle = Container.Q<Toggle>();
            if (toggle != null)
            {
                var invokeButton = new Button();
                invokeButton.clicked += OnButtonClick;
                invokeButton.text    =  "Invoke";
                toggle.Add(invokeButton);
            }
        }

        public void Initialize(ButtonInfo info)
        {
            Info = info;

            if (Info == null || !info.Valid || !info.IsStatic)
            {
                Reset();
                return;
            }

            Container.text = Info.DisplayName;
            Params.AddRange(Info.CreateDefaultParams.Invoke());
            foreach (var parameterInfo in Params)
            {
                var propertyView = PropertyViewProvider<IParameterInfo>.GetPropertyView(parameterInfo.ValueType);
                propertyView.Initialize(parameterInfo, nameof(IParameterInfo.Value));
                Container.Add(propertyView);
            }
        }

        public void Reset()
        {
            Info           = null;
            Container.text = null;
            Container.Clear();
            Params.Clear();
        }

        private void OnButtonClick()
        {
            if (Info == null || !Info.Valid || !Info.IsStatic)
            {
                return;
            }

            Info.TriggerStaticWithParams.Invoke(Params.Select(p => p.Value).ToArray());
        }
    }

    public class ButtonWithoutParamsView : VisualElement
    {
        public ButtonInfo Info     { get; private set; }
        public object     Instance { get; private set; }

        public Button InvokeButton { get; }

        public ButtonWithoutParamsView()
        {
            InvokeButton         =  new Button();
            InvokeButton.clicked += OnButtonClick;
            Add(InvokeButton);
        }

        public void Initialize(ButtonInfo info, object instance)
        {
            Info     = info;
            Instance = instance;

            if (Info == null || !info.Valid)
            {
                Reset();
                return;
            }

            InvokeButton.text = Info.DisplayName;
        }

        public void Reset()
        {
            Info              = null;
            Instance          = null;
            InvokeButton.text = null;
        }

        private void OnButtonClick()
        {
            if (Info == null || !Info.Valid)
            {
                return;
            }

            if (Instance == null)
            {
                return;
            }

            Info.TriggerWithoutParams.Invoke(Instance);
        }
    }

    public class ButtonWithParamsView : VisualElement
    {
        public ButtonInfo           Info      { get; private set; }
        public Foldout              Container { get; }
        public List<IParameterInfo> Params    { get; } = new();
        public object               Instance  { get; private set; }

        public ButtonWithParamsView()
        {
            Container = new Foldout();
            Add(Container);

            var toggle = Container.Q<Toggle>();
            if (toggle != null)
            {
                var invokeButton = new Button();
                invokeButton.clicked += OnButtonClick;
                invokeButton.text    =  "Invoke";
                toggle.Add(invokeButton);
            }
        }

        public void Initialize(ButtonInfo info, object instance)
        {
            Info     = info;
            Instance = instance;

            if (Info == null || !info.Valid)
            {
                Reset();
                return;
            }

            Container.text = Info.DisplayName;
            Params.AddRange(Info.CreateDefaultParams.Invoke());
            foreach (var parameterInfo in Params)
            {
                var propertyView = PropertyViewProvider<IParameterInfo>.GetPropertyView(parameterInfo.ValueType);
                propertyView.Initialize(parameterInfo, nameof(IParameterInfo.Value));
                Container.Add(propertyView);
            }
        }

        public void Reset()
        {
            Info           = null;
            Instance       = null;
            Container.text = null;
            Container.Clear();
            Params.Clear();
        }

        private void OnButtonClick()
        {
            if (Info == null || !Info.Valid || Instance == null)
            {
                return;
            }

            Info.TriggerWithParams.Invoke(Instance, Params.Select(p => p.Value).ToArray());
        }
    }

    public class ButtonsView : VisualElement
    {
        public ButtonsInfo Info     { get; private set; }
        public object      Instance { get; private set; }

        public ButtonsView()
        {
        }

        public void Initialize(ButtonsInfo info, object instance)
        {
            if (info == null || info.Infos.Count == 0)
            {
                Reset();
                return;
            }

            Info     = info;
            Instance = instance;
            foreach (var buttonInfo in Info.Infos)
            {
                if (buttonInfo.IsStatic)
                {
                    if (buttonInfo.Info.GetParameters().Length > 0)
                    {
                        var buttonView = new ButtonStaticWithParamsView();
                        buttonView.Initialize(buttonInfo);
                        Add(buttonView);
                    }
                    else
                    {
                        var buttonView = new ButtonStaticWithoutParamsView();
                        buttonView.Initialize(buttonInfo);
                        Add(buttonView);
                    }
                }
                else
                {
                    if (buttonInfo.Info.GetParameters().Length > 0)
                    {
                        var buttonView = new ButtonWithParamsView();
                        buttonView.Initialize(buttonInfo, instance);
                        Add(buttonView);
                    }
                    else
                    {
                        var buttonView = new ButtonWithoutParamsView();
                        buttonView.Initialize(buttonInfo, instance);
                        Add(buttonView);
                    }
                }
            }
        }

        public void Reset()
        {
            Info     = null;
            Instance = null;
            Clear();
        }
    }
}