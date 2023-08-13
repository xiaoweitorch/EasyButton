using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace LW.Util.EasyButton.Editor.View
{
    public static class PropertyViewProvider<TObj>
    {
        public static BasePropertyView<TObj> GetPropertyView<TVar>()
        {
            return GetPropertyView(typeof(TVar));
        }

        public static BasePropertyView<TObj> GetPropertyView(Type varType)
        {
            if (varType == typeof(int))
            {
                return new IntegerPropertyView<TObj>();
            }

            if (varType == typeof(uint))
            {
                return new UIntegerPropertyView<TObj>();
            }

            if (varType == typeof(float))
            {
                return new FloatPropertyView<TObj>();
            }

            if (varType == typeof(double))
            {
                return new DoublePropertyView<TObj>();
            }

            if (varType == typeof(bool))
            {
                return new BooleanPropertyView<TObj>();
            }

            if (varType == typeof(string))
            {
                return new StringPropertyView<TObj>();
            }

            if (varType.IsEnum)
            {
                var type = typeof(EnumPropertyView<,>).MakeGenericType(typeof(TObj), varType);
                return Activator.CreateInstance(type) as BasePropertyView<TObj>;
            }

            if (typeof(Object).IsAssignableFrom(varType))
            {
                var type = typeof(ObjectPropertyView<,>).MakeGenericType(typeof(TObj), varType);
                return Activator.CreateInstance(type) as BasePropertyView<TObj>;
            }

            if (varType.IsGenericType)
            {
                if (varType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var parameterType = varType.GetGenericArguments()[0];
                    var type          = typeof(ListPropertyView<,>).MakeGenericType(typeof(TObj), parameterType);
                    return Activator.CreateInstance(type) as BasePropertyView<TObj>;
                }
            }

            if (varType.IsClass)
            {
                var type = typeof(ClassPropertyView<,>).MakeGenericType(typeof(TObj), varType);
                return Activator.CreateInstance(type) as BasePropertyView<TObj>;
            }

            return null;
        }
    }
}