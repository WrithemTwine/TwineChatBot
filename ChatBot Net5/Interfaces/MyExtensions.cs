
using ChatBot_Net5.Models;

using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;

namespace ChatBot_Net5.Interfaces
{
    internal static class MyExtensions
    {
        internal static IEnumerable<XElement> ConvertXElement(this BindingList<string> bl )
        {
            foreach(object a in bl)
            {
                yield return new XElement(a.ToString());
            }
        }

        //internal static IEnumerable<XElement> ConvertXElement(this BindingList<CommandRight> bl)
        //{
        //    foreach (object a in bl)
        //    {
        //        yield return new XElement(a.ToString());
        //    }
        //}

        internal static IEnumerable<XElement> ConvertXElement(this BindingList<CommandAction> bl)
        {
            foreach (object a in bl)
            {
                yield return new XElement(a.ToString());
            }
        }
    }
}
