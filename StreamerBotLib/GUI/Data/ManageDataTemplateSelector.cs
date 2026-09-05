using StreamerBotLib.DataSQL.TableMeta;
using StreamerBotLib.Models.Enums;

using System.Windows;
using System.Windows.Controls;

namespace StreamerBotLib.GUI.Data
{
    internal class ManageDataTemplateSelector : DataTemplateSelector
    {
        internal DataTemplate StringTemplate { get; set; }
        internal DataTemplate FilePathTemplate { get; set; }
        internal DataTemplate BoolTemplate { get; set; }
        internal DataTemplate DateTimeTemplate { get; set; }
        internal DataTemplate EnumTemplate { get; set; }

        internal DataTemplate TableTemplate { get; set; }
        internal DataTemplate TableKeyFieldTemplate { get; set; }
        internal DataTemplate TableCurrencyFieldTemplate { get; set; }
        internal DataTemplate TableDataFieldTemplate { get; set; }
        internal DataTemplate OverlayActionTemplate { get; set; }
        internal DataTemplate ModActionTypeTemplate { get; set; }
        internal DataTemplate ModPerformNameTemplate { get; set; }
        internal DataTemplate CategoryTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is TableMeta.EntityData data)
            {
                if (data.Name == "KeyField")
                {
                    return TableKeyFieldTemplate;
                }
                else if (data.Name == "CurrencyField")
                {
                    return TableCurrencyFieldTemplate;
                }
                else if (data.Name == "DataField")
                {
                    return TableDataFieldTemplate;
                }

                return data.TableDataType switch
                {
                    PopupEditTableDataType.Category => CategoryTemplate,
                    PopupEditTableDataType.FilePath => FilePathTemplate,
                    PopupEditTableDataType.Bool => BoolTemplate,
                    PopupEditTableDataType.DateTime => DateTimeTemplate,
                    PopupEditTableDataType.Enum => EnumTemplate,
                    PopupEditTableDataType.Table => TableTemplate,
                    PopupEditTableDataType.Text => StringTemplate,
                    PopupEditTableDataType.OverlayAction => OverlayActionTemplate,
                    PopupEditTableDataType.ModActionType => ModActionTypeTemplate,
                    PopupEditTableDataType.ModPerformName => ModPerformNameTemplate,
                    _ => StringTemplate
                };
            }

            return base.SelectTemplate(item, container);
        }
    }
}
