using System;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls.Data.PropertyGrid;

namespace ExtensibleData
{
    public class ExtensibleDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var propDef = item as PropertyDefinition;

            if (propDef == null)
                return null;

            if (propDef.SourceProperty.PropertyType == typeof(DateTime) || propDef.SourceProperty.PropertyType == typeof(DateTime?))                
                return DatePickerTemplate;
            

            return null;
        }

        public DataTemplate DatePickerTemplate { get; set; }
    }
}
