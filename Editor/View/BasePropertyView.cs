using System;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LW.Util.EasyButton.Editor.View
{
    public abstract class BasePropertyView<TObj> : VisualElement
    {
        private const string StyleSheetPath = "Packages/com.leadingwerido.easybutton/Editor/View/PropertyView.uss";
        public        bool   Valid { get; set; }

        protected BasePropertyView()
        {
            var styleSheet =
                AssetDatabase
                    .LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
            }
        }

        public virtual void Initialize(TObj data, string fieldPath)
        {
            Debug.Assert(!Valid, $"already initialize type={GetType()}");
            Valid = true;
        }

        public virtual void Reset()
        {
            Valid = false;
        }
    }

    public static class PropertyViewUtils
    {
        private static readonly StyleEnum<DisplayStyle> ShowStyle = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        private static readonly StyleEnum<DisplayStyle> HideStyle = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        public static void SetDisplay(this VisualElement element, bool show)
        {
            element.style.display = show ? ShowStyle : HideStyle;
        }
        
        public static Func<TObj, TVar> GetValueFunc<TObj, TVar>(string fieldPath)
        {
            if (fieldPath == null)
            {
                return null;
            }

            var elements = fieldPath.Split('.');
            if (elements.Length == 0)
            {
                return null;
            }

            var        rootParameter = Expression.Parameter(typeof(TObj), "obj");
            Expression objVariable   = rootParameter;
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "")
                                                              .Replace("]", ""));
                    objVariable = GetValueExpression(objVariable, elementName, index);
                }
                else
                {
                    objVariable = GetValueExpression(objVariable, element);
                }
            }

            var lambda =
                Expression.Lambda<Func<TObj, TVar>>(Expression.Convert(objVariable, typeof(TVar)), rootParameter);
            return lambda.Compile();
        }

        private static Expression GetValueExpression(Expression objVariable, string fieldName)
        {
            return Expression.PropertyOrField(objVariable, fieldName);
        }

        private static Expression GetValueExpression(Expression objVariable, string fieldName, int index)
        {
            var field = Expression.PropertyOrField(objVariable, fieldName);
            // return Expression.ArrayIndex(field, Expression.Constant(index, typeof(int)));\
            return Expression.Property(field, "Item", Expression.Constant(index));
        }

        public static Action<TObj, TVar> SetValueFunc<TObj, TVar>(string fieldPath)
        {
            if (fieldPath == null)
            {
                return null;
            }

            var elements = fieldPath.Split('.');
            if (elements.Length == 0)
            {
                return null;
            }

            var        rootParameter  = Expression.Parameter(typeof(TObj), "obj");
            var        valueParameter = Expression.Parameter(typeof(TVar), "value");
            Expression objVariable    = rootParameter;
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "")
                                                              .Replace("]", ""));
                    objVariable = GetValueExpression(objVariable, elementName, index);
                }
                else
                {
                    objVariable = GetValueExpression(objVariable, element);
                }
            }

            var lambda =
                Expression
                    .Lambda<
                        Action<TObj, TVar>>(Expression.Assign(objVariable, Expression.Convert(valueParameter, objVariable.Type)),
                                            rootParameter,
                                            valueParameter);
            return lambda.Compile();
        }
    }
}