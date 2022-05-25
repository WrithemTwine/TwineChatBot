namespace StreamerBotLib.Enums
{
    /// <summary>
    /// List of Enums for editing database rows, to determine the type of UIElement to present the user.
    /// </summary>
    public enum PopupEditTableDataType
    {
        /// <summary>
        /// Specifies text field, TextBox
        /// </summary>
        text,
        /// <summary>
        /// Specifies the field is a datetime
        /// </summary>
        datestring,
        /// <summary>
        /// Specifies a combo box for table names
        /// </summary>
        combotable,
        /// <summary>
        /// Specifies a combo box for a data list
        /// </summary>
        combolist,
        /// <summary>
        /// Specifies a combo box for enum data
        /// </summary>
        comboenum,
        /// <summary>
        /// Specifies the kind of action in a MediaOverlay server type
        /// </summary>
        combooverlayaction,
        /// <summary>
        /// Specifies a bool element checkbox
        /// </summary>
        databool,
        /// <summary>
        /// Specifies this field is a file path
        /// </summary>
        filebrowse
    }
}
