using StreamerBotLib.GUI;
using StreamerBotLib.Static;

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace StreamerBotLib.Models.Converters
{
    public class EditConvertCategory : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<CheckBox> checkBoxes = [];
            List<string> categories = value as List<string>;

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

                checkBoxes.AddRange((from (string, bool) C in
                                         from Cat in GUIDataManagerViews.CurrCategoryList
                                             // ignore "All" category item
                                         where Cat.Category != "All" && categories.Contains(FormatData.RemoveEscapeFormat(Cat.Category))
                                         orderby Cat.Category  // sort by category name for easier search
                                         select (Cat.Category, true)
                                     select new CheckBox() { Content = FormatData.RemoveEscapeFormat(C.Item1), IsChecked = C.Item2 }).ToList());

                checkBoxes.AddRange((from (string, bool) C in
                                         from Cat in GUIDataManagerViews.CurrCategoryList
                                             // ignore "All" category item
                                         where Cat.Category != "All" && !categories.Contains(FormatData.RemoveEscapeFormat(Cat.Category))
                                         orderby Cat.Category  // sort by category name for easier search
                                         select (Cat.Category, false)
                                     select new CheckBox() { Content = FormatData.RemoveEscapeFormat(C.Item1), IsChecked = C.Item2 }).ToList());
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
            ListBox categories;
            DependencyObject temp = sender as CheckBox;

            bool AllCategory = (string)((CheckBox)temp).Content == "All" && ((CheckBox)temp).IsChecked == true;

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
            }
            else if (AllCategory)
            {
                // user clicked the "All" category, uncheck everything else
                foreach (CheckBox cb in items)
                {
                    if ((string)cb.Content != "All")
                    {
                        cb.IsChecked = false;
                    }
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ICollection<CheckBox> categories = value as ICollection<CheckBox>;

            return
                (from C in categories
                 where (string)C.Content == "All"
                 select C.IsChecked).FirstOrDefault() == true
                ? ["All"] : new List<string>(from C in categories
                                             where C.IsChecked == true
                                             select FormatData.AddEscapeFormat((string)C.Content));
        }
    }
}
