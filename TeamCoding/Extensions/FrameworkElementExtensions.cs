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
