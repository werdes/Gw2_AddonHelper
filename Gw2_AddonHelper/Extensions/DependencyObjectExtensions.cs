using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Gw2_AddonHelper.Extensions
{
    public static class DependencyObjectExtensions
    {
        public static List<T> GetChildrenOfType<T>(this DependencyObject depdendencyObject) where T : DependencyObject
        {
            if (depdendencyObject == null) return null;

            List<T> result = new List<T>();
            
            Queue<DependencyObject> dependencyObjects = new Queue<DependencyObject>();
            dependencyObjects.Enqueue(depdendencyObject);
            while (dependencyObjects.Count > 0)
            {
                var currentElement = dependencyObjects.Dequeue();
                var childrenCount = VisualTreeHelper.GetChildrenCount(currentElement);
                for (var i = 0; i < childrenCount; i++)
                {
                    var childObject = VisualTreeHelper.GetChild(currentElement, i);
                    if (childObject is T)
                        result.Add(childObject as T);
                    dependencyObjects.Enqueue(childObject);
                }
            }

            return result;
        }
    }
}
