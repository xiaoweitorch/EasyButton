using System;
using System.Collections.Generic;
using System.Reflection;

namespace LW.Util.EasyButton.Editor
{
    public static class ButtonsInfoProvider
    {
        private const BindingFlags ButtonMethodFlags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static Dictionary<Type, ButtonsInfo> ButtonsInfoDict { get; } = new();

        public static ButtonsInfo GetButtonsInfo(Type type)
        {
            if (ButtonsInfoDict.TryGetValue(type, out var info))
                return info;

            ButtonsInfoDict[type] = info = new ButtonsInfo();
            
            var methods = type.GetMethods(ButtonMethodFlags);
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<EasyButtonAttribute>();
                if (attr == null)
                    continue;

                var buttonInfo = new ButtonInfo(method);
                if (!buttonInfo.Valid)
                {
                    continue;
                }
                
                info.Infos.Add(buttonInfo);
            }
            return info;
        }
        
        public static ButtonsInfo GetButtonsInfo<T>()
        {
            return GetButtonsInfo(typeof(T));
        }
    }
}