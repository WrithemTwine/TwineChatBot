using System.Diagnostics;

namespace ChatBot_Net5.Models
{
    [DebuggerDisplay("Parameter={Parameter}, Value={Value}")]
    public class Command
    {
        public string Parameter { get; set; }
        public string Value { get; set; }
    }
}
