using System;

namespace LW.Util.EasyButton
{
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

        #endregion

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