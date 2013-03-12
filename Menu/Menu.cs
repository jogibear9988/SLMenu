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
using System.Windows.Controls.Primitives;

namespace System.Windows.Controls
{
    [TemplatePart(Name = "Popup", Type = typeof(Popup))]
    public class Menu : MenuBase
    {
        #region Constructor Members
        public Menu()
        {
            base.DefaultStyleKey = typeof(Menu);
            Application.Current.Host.Content.Resized += OnApplicationResized;
            this.ApplyTemplate();
        }
        #endregion

        #region Template Members
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this._Popup = (Popup)GetTemplateChild("Popup");            

            if (this._Popup != null)
            {
                _ElementOutsidePopup = new Canvas() { Visibility = Visibility.Collapsed };
                _Popup.Child = _ElementOutsidePopup;
                _TopCanvas = new Canvas() { Background = new SolidColorBrush(Colors.Transparent) };
                _TopCanvas.MouseLeftButtonDown += OnElementOutsideLeftMouseButtonDown;
                _LeftCanvas = new Canvas() { Background = new SolidColorBrush(Colors.Transparent) };
                _LeftCanvas.MouseLeftButtonDown += OnElementOutsideLeftMouseButtonDown;
                _RightCanvas = new Canvas() { Background = new SolidColorBrush(Colors.Transparent) };
                _RightCanvas.MouseLeftButtonDown += OnElementOutsideLeftMouseButtonDown;
                _BottomCanvas = new Canvas() { Background = new SolidColorBrush(Colors.Transparent) };
                _BottomCanvas.MouseLeftButtonDown += OnElementOutsideLeftMouseButtonDown;
                _ElementOutsidePopup.Children.Add(_TopCanvas);
                _ElementOutsidePopup.Children.Add(_LeftCanvas);
                _ElementOutsidePopup.Children.Add(_RightCanvas);
                _ElementOutsidePopup.Children.Add(_BottomCanvas);
            }
            ArrangePopup();
        }

        private void OnElementOutsideLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            CloseMenu();
        }

        private void OnApplicationResized(object sender, EventArgs e)
        {
            ArrangePopup();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            ArrangePopup();
            return base.ArrangeOverride(finalSize);
        }
        #endregion

        #region ItemsControl Members
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            MenuItem menuItem = element as MenuItem;
            if (menuItem != null)
            {
                menuItem.OnSubMenuOpened += OnSubMenuOpened;
                menuItem.OnSubMenuClosed += OnSubMenuClosed;
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);
            MenuItem menuItem = element as MenuItem;
            if (menuItem != null)
            {
                menuItem.OnSubMenuOpened -= OnSubMenuOpened;
                menuItem.OnSubMenuClosed -= OnSubMenuClosed;
            }
        }
        #endregion

        #region SubMenu Members
        private void OnSubMenuOpened(object sender, RoutedEventArgs e)
        {
            _ElementOutsidePopup.Visibility = Visibility.Visible;
            ArrangePopup();
        }
        private void OnSubMenuClosed(object sender, RoutedEventArgs e)
        {
            if (_ElementOutsidePopup != null)
            {
                _ElementOutsidePopup.Visibility = Visibility.Collapsed;
            }
        }

        private void ArrangePopup()
        {
            if (_ElementOutsidePopup != null)
            {
                double wholeWidth = Application.Current.Host.Content.ActualWidth;
                double wholeHeight = Application.Current.Host.Content.ActualHeight;
                double actualHeight = this.ActualHeight;
                double actualWidth = this.ActualWidth;
                Point p = this.TransformFromRootVisual();

                _TopCanvas.Width = wholeWidth;
                _TopCanvas.Height = p.Y;
                Canvas.SetLeft(_TopCanvas, -p.X);
                Canvas.SetTop(_TopCanvas, -p.Y);

                _LeftCanvas.Width = p.X;
                _LeftCanvas.Height = actualHeight;
                Canvas.SetLeft(_LeftCanvas, -p.X);
                Canvas.SetTop(_LeftCanvas, 0);

                _RightCanvas.Width = Math.Max(wholeWidth - p.X - actualWidth, 0);
                _RightCanvas.Height = actualHeight;
                Canvas.SetLeft(_RightCanvas, actualWidth);
                Canvas.SetTop(_RightCanvas, 0);

                _BottomCanvas.Width = wholeWidth;
                _BottomCanvas.Height = Math.Max(wholeHeight - p.Y - actualHeight, 0);
                Canvas.SetLeft(_BottomCanvas, -p.X);
                Canvas.SetTop(_BottomCanvas, actualHeight);
            }
        }
        #endregion

        #region Field Members
        private Popup _Popup;
        private Canvas _ElementOutsidePopup;        
        private Canvas _TopCanvas;
        private Canvas _LeftCanvas;
        private Canvas _RightCanvas;
        private Canvas _BottomCanvas;
        #endregion
    }
}
