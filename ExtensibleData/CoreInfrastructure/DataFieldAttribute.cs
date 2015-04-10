using System;

namespace Pocos
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DataFieldAttribute: Attribute
    {
        public string FieldName { get; set; }        
    }
}
