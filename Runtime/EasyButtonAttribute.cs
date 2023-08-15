using System;

namespace LW.Util.EasyButton
{
    public enum EnableMode
    {
        AlwaysEnable,
        PlayModeEnable,
        EditModeEnable,
    }
    
    public class EasyButtonAttribute : Attribute
    {
        #region optional params

        public string Name  { get; set; }
        public int    Order { get; set; }
        
        /// <summary>
        /// control parameter foldout default expandable
        /// only work when have params
        /// </summary>
        public bool DefaultExpand { get; set; }
        
        public EnableMode EnableMode { get; set; }

        #endregion

        public object[] DefaultValues { get; }

        public EasyButtonAttribute()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="defaultValues">
        /// must write match type value
        /// when using str startWith ".", treat as bind with instance fieldOrProperty
        /// </param>
        public EasyButtonAttribute(params object[] defaultValues)
        {
            DefaultValues = defaultValues;
        }
    }
}