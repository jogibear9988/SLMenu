using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace System.Windows.Controls
{
    public static class TransformExtention
    {
        private static readonly Point _zero = new Point(0, 0);
        public static Point TransformFromRootVisual(this UIElement element)
        {
            try
            {
                MatrixTransform globalTransform = (MatrixTransform)element.TransformToVisual(null);
                return globalTransform.Matrix.Transform(_zero);
            }
            catch { }
            return _zero;
        }
    }
}
