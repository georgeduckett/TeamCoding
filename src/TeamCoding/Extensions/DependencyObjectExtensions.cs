using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TeamCoding.Extensions
{
    public static class DependencyObjectExtensions
    {
        public static IEnumerable<DependencyObject> Children(this DependencyObject obj)
        {
            (obj as FrameworkElement)?.ApplyTemplate();

            int childrenCount = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                (child as FrameworkElement)?.ApplyTemplate();
                yield return child;
            }
        }
        public static IEnumerable<DependencyObject> FindChildren(this DependencyObject parent, string name)
        { // http://stackoverflow.com/a/19539095
            // confirm parent and name are valid.
            if (parent == null || string.IsNullOrEmpty(name)) yield break;

            if (parent is FrameworkElement && (parent as FrameworkElement).Name == name) yield return parent;

            if (parent is FrameworkElement) (parent as FrameworkElement).ApplyTemplate();

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                foreach (var c in FindChildren(child, name))
                {
                    yield return c;
                }
            }
        }

        public static DependencyObject FindChild(this DependencyObject parent, string name)
        { // http://stackoverflow.com/a/19539095
            // confirm parent and name are valid.
            if (parent == null || string.IsNullOrEmpty(name)) return null;

            if (parent is FrameworkElement && (parent as FrameworkElement).Name == name) return parent;

            DependencyObject result = null;

            if (parent is FrameworkElement) (parent as FrameworkElement).ApplyTemplate();

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                result = FindChild(child, name);
                if (result != null) break;
            }

            return result;
        }
    }
}
