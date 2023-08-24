using System;
using System.Collections.Generic;
using System.Windows;

namespace SimpleTestFeature
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TestText LogTest { get; set; }

        private List<string> RandomStrings = new() { 
            "This hurts, but is fun.", 
            "Oh what a wonderful day.",
            "Is this working well?",
            "Haha, update it.",
            "More random phrases.",
            "Is this random enough?"
        };
        private Random random = new();

        public MainWindow()
        {
            InitializeComponent();

            LogTest = Resources["TestLog"] as TestText ?? new();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LogTest.AddLog(RandomStrings[random.Next(RandomStrings.Count)]);
        }

        private void Frame_TestObjectBinding_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            (Frame_TestObjectBinding.Content as FramePage)?.SetDataContext(LogTest);
        }

    }
}
