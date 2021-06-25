using Gw2_AddonHelper.Model.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Gw2_AddonHelper.UI.Controls
{
    public class PanePanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            Size size = new Size();

            if (double.IsInfinity(availableSize.Width))
            {
                availableSize.Width = RenderSize.Width;
                availableSize.Height = RenderSize.Height;
            }

            if (Children.Count > 0)
            {
                foreach (UIElement child in Children)
                {
                    child.Measure(availableSize);
                }

                size.Width = availableSize.Width;
                size.Height = availableSize.Height;
            }

            return size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size size = new Size();

            if (Children.Count > 0)
            {
                Rect childRect = new Rect(0, 0, finalSize.Width, finalSize.Height);

                foreach (UIElement child in Children)
                {
                    child.Arrange(childRect);
                }

                size.Width = finalSize.Width;
                size.Height = finalSize.Height;
            }

            return size;
        }
    }
}
