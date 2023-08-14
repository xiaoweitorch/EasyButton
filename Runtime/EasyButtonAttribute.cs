using System;

namespace LW.Util.EasyButton
{
    public class EasyButtonAttribute : Attribute
    {
        public string   Name          { get; set; }
        public object[] DefaultValues { get; }
        
        public EasyButtonAttribute()
        {
        }

        public EasyButtonAttribute(params object[] defaultValues)
        {
            DefaultValues = defaultValues;
        }
    }
}