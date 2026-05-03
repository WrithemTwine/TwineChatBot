using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace ImplementCustomTitleBar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isMaximized = false;
        private double _restoreWidth;
        private double _restoreHeight;
        private double _restoreLeft;
        private double _restoreTop;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnMaximizeRestore_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void ToggleMaximize()
        {
            if (_isMaximized)
            {
                // Restore
                _isMaximized = false;
                WindowState = WindowState.Normal;
                Left = _restoreLeft;
                Top = _restoreTop;
                Width = _restoreWidth;
                Height = _restoreHeight;
            }
            else
            {
                // Maximize to working area
                _isMaximized = true;

                // Store current state
                _restoreWidth = Width;
                _restoreHeight = Height;
                _restoreLeft = Left;
                _restoreTop = Top;

                // Get working area
                var hwnd = new WindowInteropHelper(this).Handle;
                var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTOPRIMARY);
                var mi = new MONITORINFO { cbSize = Marshal.SizeOf(typeof(MONITORINFO)) };
                if (GetMonitorInfo(monitor, ref mi))
                {
                    Left = mi.rcWork.Left;
                    Top = mi.rcWork.Top;
                    Width = mi.rcWork.Right - mi.rcWork.Left;
                    Height = mi.rcWork.Bottom - mi.rcWork.Top;
                }

                // Force normal state but sized to working area
                WindowState = WindowState.Normal;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Hover effects
        private void WindowButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#33FFFFFF")); // light hover
            }
        }

        private void WindowButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.Background = Brushes.Transparent;
            }
        }

        private void btnClose_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E81123")); // red hover
            }
        }

        private void btnMinimize_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Preview click detected on minimize");
        }

        //protected override void OnStateChanged(EventArgs e)
        //{
        //    base.OnStateChanged(e);

        //    var hwnd = new WindowInteropHelper(this).Handle;

        //    if (WindowState == WindowState.Maximized)
        //    {
        //        // Store current (pre-maximized) client size/position
        //        _restoreWidth = Width;
        //        _restoreHeight = Height;
        //        _restoreLeft = Left;
        //        _restoreTop = Top;

        //        // Get the working area (excludes taskbar)
        //        var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTOPRIMARY);
        //        var mi = new MONITORINFO { cbSize = Marshal.SizeOf(typeof(MONITORINFO)) };
        //        if (GetMonitorInfo(monitor, ref mi))
        //        {
        //            // Set window to exactly the working area
        //            Left = mi.rcWork.Left;
        //            Top = mi.rcWork.Top;
        //            Width = mi.rcWork.Right - mi.rcWork.Left;
        //            Height = mi.rcWork.Bottom - mi.rcWork.Top;
        //        }
        //    }
        //    else if (WindowState == WindowState.Normal)
        //    {
        //        // Restore exact previous client size/position
        //        if (_restoreWidth > 0 && _restoreHeight > 0) // safety
        //        {
        //            Left = _restoreLeft;
        //            Top = _restoreTop;
        //            Width = _restoreWidth;
        //            Height = _restoreHeight;
        //        }
        //    }
        //}

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        private const uint MONITOR_DEFAULTTOPRIMARY = 0x00000001;

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }


        private void DockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Double-click only on the title bar area (top CaptionHeight pixels)
            if (e.ClickCount == 2) // 36 = CaptionHeight
            {
                ToggleMaximize();
                e.Handled = true; // Block default WPF maximize
            }
            else if (e.ClickCount == 1 && e.ButtonState == MouseButtonState.Pressed)
            {
                // Manual drag if needed (only if default fails)
                DragMove();
            }
        }
    }
}