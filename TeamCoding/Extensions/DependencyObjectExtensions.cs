using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TeamCoding.Extensions
{
    public static class DependencyObjectExtensions
    {
        public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);

            if (parent == null)
            {
                return null;
            }
            
            var typedParent = parent as T;

            if (typedParent != null)
            {
                return typedParent;
            }
            else
            {
                return FindParent<T>(parent);
            }
        }
        public static void RemoveChild(this DependencyObject parent, UIElement child)
        { // http://stackoverflow.com/a/19318405
            var panel = parent as Panel;
            if (panel != null)
            {
                panel.Children.Remove(child);
                return;
            }

            var decorator = parent as Decorator;
            if (decorator != null)
            {
                if (decorator.Child == child)
                {
                    decorator.Child = null;
                }
                return;
            }

            var contentPresenter = parent as ContentPresenter;
            if (contentPresenter != null)
            {
                if (contentPresenter.Content == child)
                {
                    contentPresenter.Content = null;
                }
                return;
            }

            var contentControl = parent as ContentControl;
            if (contentControl != null)
            {
                if (contentControl.Content == child)
                {
                    contentControl.Content = null;
                }
                return;
            }
        }

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
        public static IEnumerable<DependencyObject> FindChildren(this DependencyObject parent, string name) => FindChildren(parent, (d => (d as FrameworkElement)?.Name == name));
        public static IEnumerable<DependencyObject> FindChildren(this DependencyObject parent, Func<DependencyObject, bool> predicate, bool returnFirst = false)
        {
            var toProcess = new Stack<DependencyObject>();
            var result = new List<DependencyObject>();

            toProcess.Push(parent);

            while (toProcess.Count != 0)
            {
                var nextControl = toProcess.Pop();

                if (predicate(nextControl))
                {
                    result.Add(nextControl);
                    if (returnFirst)
                    {
                        return result;
                    }
                }

                (nextControl as FrameworkElement)?.ApplyTemplate();
                int childrenCount = VisualTreeHelper.GetChildrenCount(nextControl);
                for (int i = 0; i < childrenCount; i++)
                {
                    toProcess.Push(VisualTreeHelper.GetChild(nextControl, i));
                }
            }

            return result;
        }
        public static IEnumerable<T> FindChildren<T>(this DependencyObject parent) where T : DependencyObject => FindChildren(parent, p => p is T).Cast<T>();
        public static T FindChild<T>(this DependencyObject parent, Func<T, bool> predicate = null) where T : DependencyObject => (T)FindChild(parent, p => p is T && (predicate == null || predicate(p as T)));
        public static DependencyObject FindChild(this DependencyObject parent, Func<DependencyObject, bool> predicate) => FindChildren(parent, predicate, true).FirstOrDefault();
    }
}
