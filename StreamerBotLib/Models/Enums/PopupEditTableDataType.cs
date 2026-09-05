namespace StreamerBotLib.Models.Enums
{
    /// <summary>
    /// Determines which UIElement / DataTemplate to use when editing a database row.
    /// </summary>
    public enum PopupEditTableDataType
    {
        /// <summary>Normal text → TextBox + character counter</summary>
        Text,

        /// <summary>DateTime → DatePicker (or validated TextBox)</summary>
        DateTime,

        /// <summary>Boolean → CheckBox</summary>
        Bool,

        /// <summary>Enum → ComboBox filled with Enum.GetValues</summary>
        Enum,

        /// <summary>File path (MediaFile / ImageFile) → TextBox + double-click browser</summary>
        FilePath,

        /// <summary>Category (ICollection&lt;string&gt;) → multi-select ListBox with "All" logic</summary>
        Category,

        /// <summary>Table name → ComboBox of table names (DataBot.GetTableNames)</summary>
        Table,

        /// <summary>KeyField / DataField / CurrencyField → ComboBox that depends on selected Table</summary>
        TableField,

        /// <summary>OverlayAction → ComboBox that depends on OverlayType</summary>
        OverlayAction,

        /// <summary>ModActionType => ModActionType, ModPerformType - fields for ComboBoxes provide which type of action</summary>
        ModActionType,
        /// <summary>ModPerformName => ModActionName, ModPerformAction - fields for ComboBoxes, define the action to perform based on type</summary>
        ModPerformName
    }
}
