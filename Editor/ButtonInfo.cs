using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using LW.Util.EasyButton.Editor.View;
using UnityEngine;

namespace LW.Util.EasyButton.Editor
{
    public interface IParameterInfo
    {
        public object Value     { get; set; }
        public Type   ValueType { get; }
        public bool   Assign    { get; }
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
        public bool Assign    => true;
    }

    public class FieldParameterInfo<TObj, TVar> : IParameterInfo
    {
        public  TObj             Data         { get; }
        private Func<TObj, TVar> GetValueFunc { get; }

        public FieldParameterInfo(TObj data, string fieldPath)
        {
            Data         = data;
            GetValueFunc = PropertyViewUtils.GetValueFunc<TObj, TVar>(fieldPath);
        }

        public object Value
        {
            get => GetValueFunc.Invoke(Data);
            set => throw new NotSupportedException();
        }

        public Type ValueType { get; } = typeof(TVar);
        public bool Assign    => false;
    }

    public class ButtonInfo
    {
        public Action                   TriggerStaticWithoutParams { get; }
        public Action<object[]>         TriggerStaticWithParams    { get; }
        public Action<object>           TriggerWithoutParams       { get; }
        public Action<object, object[]> TriggerWithParams          { get; }

        public Func<object, IParameterInfo[]> CreateDefaultParams { get; }

        public bool Valid => TriggerWithoutParams != null
                          || TriggerStaticWithoutParams != null
                          || (TriggerStaticWithParams != null && CreateDefaultParams != null)
                          || (TriggerWithParams != null && CreateDefaultParams != null);

        public string     DisplayName   { get; }
        public bool       IsStatic      => Info.IsStatic;
        public MethodInfo Info          { get; }
        public int        Order         { get; }
        public bool       DefaultExpand { get; }

        public EnableMode EnableMode { get; }

        public PrintLevel PrintReturn { get; }

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

            Info          = methodInfo;
            DisplayName   = easyButtonAttribute.Name ?? methodInfo.Name;
            Order         = easyButtonAttribute.Order;
            DefaultExpand = easyButtonAttribute.DefaultExpand;
            EnableMode    = easyButtonAttribute.EnableMode;
            PrintReturn   = easyButtonAttribute.PrintReturn;
            var methodParameters = methodInfo.GetParameters();
            if (methodInfo.IsStatic)
            {
                if (methodParameters.Length == 0)
                {
                    TriggerStaticWithoutParams = CreateStaticWithoutParams();
                }
                else
                {
                    TriggerStaticWithParams = CreateStaticWithParams();
                }
            }
            else
            {
                if (methodParameters.Length == 0)
                {
                    TriggerWithoutParams = CreateWithoutParams();
                }
                else
                {
                    TriggerWithParams = CreateWithParams();
                }
            }

            CreateDefaultParams = CreateDefaultParamsFunc();

            Func<object, IParameterInfo[]> CreateDefaultParamsFunc()
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

                var p = Expression.Parameter(typeof(object), "obj");

                var instance = Expression.TypeAs(p, Info.DeclaringType!);

                var variableList = new List<ParameterExpression>();
                var bodyList     = new List<Expression>();
                for (var index = 0; index < methodParameters.Length; ++index)
                {
                    var methodParameter       = methodParameters[index];
                    var attributeDefaultValue = easyButtonAttribute.DefaultValues?[index];
                    if (attributeDefaultValue == null)
                    {
                        var parameterType    = typeof(ParameterInfo<>).MakeGenericType(methodParameter.ParameterType);
                        var newParameterInfo = Expression.New(parameterType);

                        var variable = Expression.Variable(parameterType);
                        variableList.Add(variable);

                        var assignExpression = Expression.Assign(variable, newParameterInfo);
                        bodyList.Add(assignExpression);

                        if (!methodParameter.HasDefaultValue)
                        {
                            continue;
                        }

                        var valueExpression =
                            Expression.PropertyOrField(variable, nameof(ParameterInfo<int>.InnerValue));
                        var assignDefaultExpression =
                            Expression.Assign(valueExpression,
                                              Expression.Constant(methodParameter.DefaultValue));
                        bodyList.Add(assignDefaultExpression);
                    }
                    else if (attributeDefaultValue is string str && str.StartsWith("."))
                    {
                        var fieldPath = str.Substring(1);
                        var parameterType =
                            typeof(FieldParameterInfo<,>).MakeGenericType(Info.DeclaringType,
                                                                          methodParameter.ParameterType);
                        var constructorInfo =
                            parameterType.GetConstructor(new[] { Info.DeclaringType, typeof(string) });
                        var newParameterInfo =
                            Expression.New(constructorInfo!, instance, Expression.Constant(fieldPath));

                        var variable = Expression.Variable(parameterType);
                        variableList.Add(variable);

                        var assignExpression = Expression.Assign(variable, newParameterInfo);
                        bodyList.Add(assignExpression);
                    }
                    else
                    {
                        var parameterType    = typeof(ParameterInfo<>).MakeGenericType(methodParameter.ParameterType);
                        var newParameterInfo = Expression.New(parameterType);

                        var variable = Expression.Variable(parameterType);
                        variableList.Add(variable);

                        var assignExpression = Expression.Assign(variable, newParameterInfo);
                        bodyList.Add(assignExpression);

                        var valueExpression =
                            Expression.PropertyOrField(variable, nameof(ParameterInfo<int>.InnerValue));
                        var assignDefaultExpression =
                            Expression.Assign(valueExpression,
                                              Expression.Constant(attributeDefaultValue));
                        bodyList.Add(assignDefaultExpression);
                    }
                }

                var createArray = Expression.NewArrayInit(typeof(IParameterInfo), variableList);
                bodyList.Add(createArray);
                return Expression.Lambda<Func<object, IParameterInfo[]>>(Expression.Block(
                                                                              variableList, bodyList), p).Compile();
            }

            Action CreateStaticWithoutParams()
            {
                var body = Expression.Call(methodInfo);
                if (PrintReturn != PrintLevel.None && methodInfo.ReturnType != typeof(void))
                {
                    var printMethodInfo = GetPrintMethodInfo(PrintReturn);
                    body = Expression.Call(printMethodInfo, Expression.Convert(body, typeof(object)));
                }

                return Expression.Lambda<Action>(body).Compile();
            }

            Action<object[]> CreateStaticWithParams()
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
                if (PrintReturn != PrintLevel.None && methodInfo.ReturnType != typeof(void))
                {
                    var printMethodInfo = GetPrintMethodInfo(PrintReturn);
                    invoke = Expression.Call(printMethodInfo, Expression.Convert(invoke, typeof(object)));
                }

                return Expression.Lambda<Action<object[]>>(invoke, parameters).Compile();
            }

            Action<object> CreateWithoutParams()
            {
                var p        = Expression.Parameter(typeof(object), "obj");
                var instance = Expression.TypeAs(p, methodInfo.DeclaringType);
                var invoke   = Expression.Call(instance, methodInfo);
                if (PrintReturn != PrintLevel.None && methodInfo.ReturnType != typeof(void))
                {
                    var printMethodInfo = GetPrintMethodInfo(PrintReturn);
                    invoke = Expression.Call(printMethodInfo, Expression.Convert(invoke, typeof(object)));
                }

                return Expression.Lambda<Action<object>>(invoke, p).Compile();
            }

            Action<object, object[]> CreateWithParams()
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
                if (PrintReturn != PrintLevel.None && methodInfo.ReturnType != typeof(void))
                {
                    var printMethodInfo = GetPrintMethodInfo(PrintReturn);
                    invoke = Expression.Call(printMethodInfo, Expression.Convert(invoke, typeof(object)));
                }

                return Expression.Lambda<Action<object, object[]>>(invoke, p, parameters).Compile();
            }
        }

        private static MethodInfo GetPrintMethodInfo(PrintLevel printLevel)
        {
            switch (printLevel)
            {
                case PrintLevel.None:
                {
                    return null;
                }
                case PrintLevel.Debug:
                {
                    return typeof(Debug).GetMethod(nameof(Debug.Log), BindingFlags.Static | BindingFlags.Public, null,
                                                   new Type[] { typeof(object) }, null);
                }
                case PrintLevel.Warn:
                {
                    return typeof(Debug).GetMethod(nameof(Debug.LogWarning), BindingFlags.Static | BindingFlags.Public,
                                                   null,
                                                   new Type[] { typeof(object) }, null);
                }
                case PrintLevel.Error:
                {
                    return typeof(Debug).GetMethod(nameof(Debug.LogError), BindingFlags.Static | BindingFlags.Public,
                                                   null,
                                                   new Type[] { typeof(object) }, null);
                }
                default:
                {
                    return null;
                }
            }
        }

        public bool CheckEnable()
        {
            return EnableMode switch
            {
                EnableMode.AlwaysEnable   => true,
                EnableMode.EditModeEnable => !Application.isPlaying,
                EnableMode.PlayModeEnable => Application.isPlaying,
                _                         => false,
            };
        }
    }

    public class ButtonsInfo
    {
        public List<ButtonInfo> Infos { get; } = new();
    }
}