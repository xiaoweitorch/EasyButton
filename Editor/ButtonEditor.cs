using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LW.Util.EasyButton.Editor
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class MonoBehaviourEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            InspectorElement.FillDefaultInspector(container, serializedObject, this);
            var buttonsView = CreateButtonsView();
            if (buttonsView != null)
            {
                container.Insert(0, buttonsView);
            }
            return container;
        }

        private ButtonsView CreateButtonsView()
        {
            if (serializedObject.targetObject == null)
            {
                return null;
            }

            var objType     = serializedObject.targetObject.GetType();
            var buttonsInfo = ButtonsInfoProvider.GetButtonsInfo(objType);
            if (buttonsInfo == null || buttonsInfo.Infos == null)
            {
                return null;
            }

            var buttonsView = new ButtonsView();
            buttonsView.Initialize(buttonsInfo, serializedObject.targetObject);
            return buttonsView;
        }
    }
}