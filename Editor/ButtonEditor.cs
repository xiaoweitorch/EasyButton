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
            var buttonsView = ButtonViewUtil.CreateButtonsView(serializedObject.targetObject);
            if (buttonsView != null)
            {
                container.Insert(0, buttonsView);
            }
            return container;
        }
    }
}