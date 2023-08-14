using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace LW.Util.EasyButton.Editor
{
    public interface IParameterInfo
    {
        public object Value     { get; set; }
        public Type   ValueType { get; }
    }

    public class ParameterInfo<T> : IParameterInfo
    {
        public T InnerValue;

        public object Value
        {
            get => InnerValue;
            set => InnerValue = (T)value;
        }

        public Type ValueType { get; } = typeof(T);
    }

    public class ButtonInfo
    {
        public Action                   TriggerStaticWithoutParams { get; }
        public Action<object[]>         TriggerStaticWithParams    { get; }
        public Action<object>           TriggerWithoutParams       { get; }
        public Action<object, object[]> TriggerWithParams          { get; }

        public Func<IParameterInfo[]> CreateDefaultParams { get; }

        public bool Valid => TriggerWithoutParams != null
                          || TriggerStaticWithoutParams != null
                          || (TriggerStaticWithParams != null && CreateDefaultParams != null)
                          || (TriggerWithParams != null && CreateDefaultParams != null);

        public string     DisplayName { get; }
        public bool       IsStatic    => Info.IsStatic;
        public MethodInfo Info        { get; }

        public ButtonInfo(MethodInfo methodInfo)
        {
            var easyButtonAttribute = methodInfo.GetCustomAttribute<EasyButtonAttribute>();
            if (easyButtonAttribute == null)
            {
                Debug.LogError($"[EasyButton] without easyButtonAttribute type={methodInfo.DeclaringType} method={methodInfo.Name}");
                return;
            }

            if (methodInfo.DeclaringType == null)
            {
                Debug.LogError($"[EasyButton] declaringType is null type={methodInfo.DeclaringType} method={methodInfo.Name}");
                return;
            }

            // HACK: lw 暂时处理无参、无返回值情况
            Info        = methodInfo;
            DisplayName = easyButtonAttribute.Name ?? methodInfo.Name;
            var methodParameters = methodInfo.GetParameters();
            if (methodInfo.IsStatic)
            {
                if (methodParameters.Length == 0)
                {
                    var invoke = Expression.Call(methodInfo);
                    TriggerStaticWithoutParams = Expression.Lambda<Action>(invoke).Compile();
                }
                else
                {
                    var parameters            = Expression.Parameter(typeof(object[]), "parameters");
                    var parametersConvertList = new List<Expression>();
                    for (var index = 0; index < methodParameters.Length; ++index)
                    {
                        var methodParameter = methodParameters[index];
                        var parameter       = Expression.ArrayIndex(parameters, Expression.Constant(index));
                        var convertParam    = Expression.Convert(parameter, methodParameter.ParameterType);
                        parametersConvertList.Add(convertParam);
                    }

                    var invoke = Expression.Call(methodInfo, parametersConvertList);
                    TriggerStaticWithParams = Expression.Lambda<Action<object[]>>(invoke, parameters).Compile();
                }
            }
            else
            {
                if (methodParameters.Length == 0)
                {
                    var p        = Expression.Parameter(typeof(object), "obj");
                    var instance = Expression.TypeAs(p, methodInfo.DeclaringType);
                    var invoke   = Expression.Call(instance, methodInfo);
                    TriggerWithoutParams = Expression.Lambda<Action<object>>(invoke, p).Compile();
                }
                else
                {
                    var p                     = Expression.Parameter(typeof(object), "obj");
                    var instance              = Expression.TypeAs(p, methodInfo.DeclaringType);
                    var parameters            = Expression.Parameter(typeof(object[]), "parameters");
                    var parametersConvertList = new List<Expression>();
                    for (var index = 0; index < methodParameters.Length; ++index)
                    {
                        var methodParameter = methodParameters[index];
                        var parameter       = Expression.ArrayIndex(parameters, Expression.Constant(index));
                        var convertParam    = Expression.Convert(parameter, methodParameter.ParameterType);
                        parametersConvertList.Add(convertParam);
                    }

                    var invoke = Expression.Call(instance, methodInfo, parametersConvertList);
                    TriggerWithParams = Expression.Lambda<Action<object, object[]>>(invoke, p, parameters).Compile();
                }
            }

            CreateDefaultParams =  CreateDefaultParamsFunc();
            
            Func<IParameterInfo[]> CreateDefaultParamsFunc()
            {
                if (methodParameters.Length == 0)
                {
                    return null;
                }

                if (easyButtonAttribute.DefaultValues != null &&
                    easyButtonAttribute.DefaultValues.Length != methodParameters.Length)
                {
                    return null;
                }
            
                var variableList = new List<ParameterExpression>();
                var bodyList     = new List<Expression>();
                for (var index = 0; index < methodParameters.Length; ++index)
                {
                    var methodParameter = methodParameters[index];

                    var parameterType    = typeof(ParameterInfo<>).MakeGenericType(methodParameter.ParameterType);
                    var newParameterInfo = Expression.New(parameterType);
                        
                    var variable = Expression.Variable(parameterType);
                    variableList.Add(variable);
                        
                    var assignExpression = Expression.Assign(variable, newParameterInfo);
                    bodyList.Add(assignExpression);

                    var attributeDefaultValue = easyButtonAttribute.DefaultValues?[index];
                    if (!methodParameter.HasDefaultValue && attributeDefaultValue == null)
                    {
                        continue;
                    }
                    
                    var valueExpression = Expression.PropertyOrField(variable, nameof(ParameterInfo<int>.InnerValue));
                    var assignDefaultExpression =
                        Expression.Assign(valueExpression, Expression.Constant(attributeDefaultValue ?? methodParameter.DefaultValue));
                    bodyList.Add(assignDefaultExpression);
                }

                var createArray = Expression.NewArrayInit(typeof(IParameterInfo), variableList);
                bodyList.Add(createArray);
                return Expression.Lambda<Func<IParameterInfo[]>>(Expression.Block(
                                                                                 variableList, bodyList)).Compile();
            }
        }
    }

    public class ButtonsInfo
    {
        public List<ButtonInfo> Infos { get; } = new();
    }
}