using System;

namespace LW.Util.EasyButton
{
    public class EasyButtonAttribute : Attribute
    {
        public string Name { get; }
        
        public EasyButtonAttribute()
        {
        }
        
        public EasyButtonAttribute(string name)
        {
            Name = name;
        }
    }
}