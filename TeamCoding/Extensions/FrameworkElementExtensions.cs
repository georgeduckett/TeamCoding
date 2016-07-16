using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TeamCoding.Extensions
{
    public static class FrameworkElementExtensions
    {
        public static void Remove(this FrameworkElement element)
        {
            element.Parent.RemoveChild(element);
        }
    }
}
