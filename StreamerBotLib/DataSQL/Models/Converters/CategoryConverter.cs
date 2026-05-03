using StreamerBotLib.GUI;
using StreamerBotLib.Models.Enums;
using StreamerBotLib.Static;

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace StreamerBotLib.DataSQL.Models.Converters
{
    /* Data Models with Currency fields
    Model:          readonly:   type:     field type:                   display type:       edit type:
    CategoryList    true        single    string                        string (s)          N/A

    Clips           true        ref       (relation on CatID)           N/A (relation - string) N/A      
    
    CommandsBase    false       multi     ICollection<string>           string (s,s,s)      ICollection<string>
    
    Followers       true        single    string, (relation on Name)    string (s)          N/A
    GameDeadCounter true        single    string, (relation)            string (s)          N/A
    
    InRaidData      true        single    string                        string (s)          N/A
    OldFollowUsers  true        single    string                        string (s)          N/A
    
    StreamStats     true        multi     ICollection<string>           ICollection<string> N/A
    */

    // FormatData.AddEscapeFormat((string)value).Split(",").ToList();
    // return FormatData.RemoveEscapeFormat(string.Join(",", output));


    public class CategoryConverter : ValidationRule, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LogWriter.DebugLog("CategoryConverter.Convert", DebugLogTypes.Converters, $"Converting {value}");

            // source data requires SQL-safe escaping, ' , due to SQL queries using source text and mismatching- ' -in the where clause
            List<string> output = [];

            if (value is string)
            {
                LogWriter.DebugLog("CategoryConverter.Convert", DebugLogTypes.Converters, $"Converting {value}");
                output.AddRange((value as string).Split(","));
            }
            else
            {
                LogWriter.DebugLog("CategoryConverter.Convert", DebugLogTypes.Converters, $"Converting {string.Join(", ", value)}");
                output.AddRange(value as List<string>);
            }

            if (output == null || output.Count == 0)
            {
                output.Add("All");
            }
            else if (output[0] == "")
            {
                output[0] = "All";
            }

            output.RemoveAll(s => s is null);

            if (targetType == typeof(string))
            {
                return FormatData.RemoveEscapeFormat(string.Join(",", output));
            }
            else
            {
                return output.Select(c => FormatData.RemoveEscapeFormat(c)).ToList();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LogWriter.DebugLog("CategoryConverter.ConvertBack", DebugLogTypes.Converters, $"Converting {value}");

            return FormatData.AddEscapeFormat((string)value).Split(",").ToList();
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            LogWriter.DebugLog("CategoryConverter.Validate", DebugLogTypes.Converters, $"Converting {value}");

            List<string> Data = value as List<string>;

            if (Data.Count > 1 && Data.Contains("All"))
            {
                return new(false, "All or other categories, not both.");
            }
            else
            {
                return new(true, "Successful.");
            }
        }
    }

    public class EditConvertCategory : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LogWriter.DebugLog("EditConvertCategory.Convert", DebugLogTypes.Converters, $"Converting {value}");

            List<CheckBox> checkBoxes = [];
            List<string> categories = [];

            if (value.GetType() == typeof(string))
            {
                categories.Add((string)value);
            }
            else
            {
                categories.AddRange(value as List<string>);
            }

            categories.RemoveAll(c => string.IsNullOrEmpty(c));

            if (categories.Count == 0)
            {
                categories.Add("All");
            }

            categories.ForEach((c) => c = FormatData.RemoveEscapeFormat(c));

            if (categories.Contains("All"))
            {
                checkBoxes.Add(new() { Content = "All", IsChecked = true });
                checkBoxes.AddRange(from C in GUIDataManagerViews.CurrCategoryList
                                    where C.Category != "All"
                                    orderby C.Category
                                    select new CheckBox() { Content = FormatData.RemoveEscapeFormat(C.Category), IsChecked = false });
            }
            else
            {
                checkBoxes.Add(new() { Content = "All", IsChecked = false }); // add "All" selection first

                checkBoxes.AddRange([.. (from (string, bool) C in
                                         from Cat in GUIDataManagerViews.CurrCategoryList
                                             // ignore "All" category item
                                         where Cat.Category != "All" && categories.Contains(FormatData.RemoveEscapeFormat(Cat.Category))
                                         orderby Cat.Category  // sort by category name for easier search
                                         select (Cat.Category, true)
                                     select new CheckBox() { Content = FormatData.RemoveEscapeFormat(C.Item1), IsChecked = C.Item2 })]);

                checkBoxes.AddRange([.. (from (string, bool) C in
                                         from Cat in GUIDataManagerViews.CurrCategoryList
                                             // ignore "All" category item
                                         where Cat.Category != "All" && !categories.Contains(FormatData.RemoveEscapeFormat(Cat.Category))
                                         orderby Cat.Category  // sort by category name for easier search
                                         select (Cat.Category, false)
                                     select new CheckBox() { Content = FormatData.RemoveEscapeFormat(C.Item1), IsChecked = C.Item2 })]);
            }

            // add Click event handler to each checkbox
            foreach (var check in checkBoxes)
            {
                check.Checked += StreamStats_CategoryCheckBox_Click;
            }

            return checkBoxes;
        }

        private void StreamStats_CategoryCheckBox_Click(object sender, RoutedEventArgs e)
        {
            List<string> Selected = [];

            ListBox categories;
            DependencyObject temp = sender as CheckBox;

            bool AllCategory = (string)((CheckBox)temp).Content == "All" && ((CheckBox)temp).IsChecked == true;

            LogWriter.DebugLog("EditConvertCategory.StreamStats_CategoryCheckBox_Click", DebugLogTypes.Converters, $"Converting {(string)((CheckBox)temp).Content}");

            do
            {
                temp = VisualTreeHelper.GetParent(temp);
            } while (temp.GetType() != typeof(ListBox));

            categories = temp as ListBox;

            var items = categories.Items.Cast<CheckBox>();

            if (!AllCategory && items.Select(c => c.IsChecked).Count() > 1)
            {
                var allItem = items.Where(c => (string)c.Content == "All").Select(c => c).FirstOrDefault();
                if (allItem != default)
                {
                    allItem.IsChecked = false;
                }

                foreach (string selected in items.Where(a => a.IsChecked == true).Select(c => (string)c.Content))
                {
                    Selected.Add(selected);
                }
            }
            else if (AllCategory)
            {
                Selected = ["All"];
                // user clicked the "All" category, uncheck everything else
                foreach (CheckBox cb in items)
                {
                    if ((string)cb.Content != "All")
                    {
                        cb.IsChecked = false;
                    }
                }
            }

            categories.SelectedItem = Selected;
            //categories.SelectedItems.Clear();
            //foreach (var s in Selected)
            //{
            //    categories.SelectedItems.Add(s);
            //}
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LogWriter.DebugLog("EditConvertCategory.ConvertBack", DebugLogTypes.Converters, $"Converting {value}");

            return value as ICollection<string>;
        }
    }

    //public class ConvertCategory : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    { // source data requires SQL-safe escaping, ' , due to SQL queries using source text and mismatching- ' -in the where clause
    //        List<string> output = [];

    //        if (value is string)
    //        {
    //            LogWriter.DebugLog("ConvertCategory.Convert", DebugLogTypes.Converters, $"Converting {value}");
    //            output.AddRange((value as string).Split(","));
    //        }
    //        else
    //        {
    //            LogWriter.DebugLog("ConvertCategory.Convert", DebugLogTypes.Converters, $"Converting {string.Join(", ", value)}");
    //            output.AddRange(value as List<string>);
    //        }

    //        if (output == null || output.Count == 0)
    //        {
    //            output.Add("All");
    //        }
    //        else if (output[0] == "")
    //        {
    //            output[0] = "All";
    //        }

    //        output.RemoveAll(s => s is null);

    //        if (targetType == typeof(string))
    //        {
    //            return FormatData.RemoveEscapeFormat(string.Join(",", output));
    //        }
    //        else
    //        {
    //            return output.Select(c => FormatData.RemoveEscapeFormat(c)).ToList();
    //        }
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    { // source data requires SQL-safe escaping, ' , due to SQL queries using source text and mismatching- ' -in the where clause
    //        LogWriter.DebugLog("ConvertCategory.ConvertBack", DebugLogTypes.Converters, $"Converting {value}");
    //        return FormatData.AddEscapeFormat((string)value).Split(",").ToList();
    //    }
    //}

}
