using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Telerik.Windows.Controls.GridView;

namespace ExtensibleData
{
    public class ModifiedRowStyleSelector : StyleSelector
    {   
        public override Style SelectStyle(object item, DependencyObject container)
        {
            var style = new Style(typeof(GridViewRow), (Style) Application.Current.FindResource("GridViewRowStyle"));            
            var items = ((GridViewRow)container).GridViewDataControl.Items;
                                 
            
            if (item is IChangeTracking)
            {
                var newItem = item as IChangeTracking;
                if (newItem.IsChanged)
                {
                    style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(Colors.Green)));
                    style.Setters.Add(new Setter(Control.FontStyleProperty, FontStyles.Italic));
                }                    
            }            

            if (items.IndexOf(item) % 2 == 1)
            {                
                style.Setters.Add(new Setter(Control.BackgroundProperty, new BrushConverter().ConvertFrom("#FFCAF9FF")));
            }

            return style;
        }
    }
}
