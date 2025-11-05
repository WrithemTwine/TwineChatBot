using System.Windows.Input;

namespace StreamerBotLib.Models.Events
{
    public class PreviewKeyDownDeleteRowsEventArgs(object dataGridSender, KeyEventArgs e) : EventArgs
    {
        public object DataGridSender { get; set; } = dataGridSender;
        public KeyEventArgs e { get; set; } = e;

    }
}
