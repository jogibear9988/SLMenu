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
    [TemplatePart(Name = "HostRoot", Type = typeof(Border))]
    public class ContextMenu : MenuBase
    {
        #region DependencyProperty Members
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen",
            typeof(bool), typeof(ContextMenu), new PropertyMetadata(false, new PropertyChangedCallback(OnIsOpenPropertyChanged)));
        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset",
            typeof(double), typeof(ContextMenu), new PropertyMetadata(0.0, new PropertyChangedCallback(OnHorizontalOffsetPropertyChanged)));
        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset",
            typeof(double), typeof(ContextMenu), new PropertyMetadata(0.0, new PropertyChangedCallback(OnVerticalOffsetPropertyChanged)));
        #endregion

        #region Event Members
        public event RoutedEventHandler Opened;
        public event RoutedEventHandler Closed;
        #endregion

        #region Property Members
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }

        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }
        #endregion

        #region Constructor Members

        public ContextMenu()
        {
            base.DefaultStyleKey = typeof(ContextMenu);
            this.ApplyTemplate();
        }
        #endregion

        #region UIField Members
        private Popup _Popup;
        private Border _HostRoot;
        #endregion

        #region Template Members
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this._Popup = (Popup)GetTemplateChild("Popup");
            this._HostRoot = (Border)GetTemplateChild("HostRoot");

            ShowMenuByCachePoint();
        }
        #endregion

        #region PropertyChanged Members
        private void OnIsOpenPropertyChanged(bool newValue)
        {
            if (_Popup != null && _Popup.IsOpen != newValue)
                _Popup.IsOpen = newValue;
            if (newValue)
            {
                RoutedEventHandler handler = Opened;
                if (handler != null)
                    handler(this, new RoutedEventArgs());
            }
            else
            {
                RoutedEventHandler handler = Closed;
                if (handler != null)
                    handler(this, new RoutedEventArgs());
            }
        }

        private void OnHorizontalOffsetPropertyChanged(double newValue)
        {
            if (_Popup != null)
                _Popup.HorizontalOffset = newValue;
        }

        private void OnVerticalOffsetPropertyChanged(double newValue)
        {
            if (_Popup != null)
                _Popup.VerticalOffset = newValue;
        }

        private static void OnIsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContextMenu)d).OnIsOpenPropertyChanged((bool)e.NewValue);
        }

        private static void OnHorizontalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContextMenu)d).OnHorizontalOffsetPropertyChanged((double)e.NewValue);
        }

        private static void OnVerticalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContextMenu)d).OnVerticalOffsetPropertyChanged((double)e.NewValue);
        }
        #endregion

        #region Function Members
        private Point? m_CachePoint = null;
        private void ShowMenuByCachePoint()
        {
            if (_Popup != null && m_CachePoint != null)
            {
                double absoluteX = m_CachePoint.Value.X;
                double absoluteY = m_CachePoint.Value.Y;
                double width = double.IsNaN(_HostRoot.Width) ? _HostRoot.ActualWidth : _HostRoot.Width;
                double height = double.IsNaN(_HostRoot.Height) ? _HostRoot.ActualHeight : _HostRoot.Height;
                double hostWidth = Application.Current.Host.Content.ActualWidth;
                double hostHeight = Application.Current.Host.Content.ActualHeight;
                DisplayMode mode = DisplayMode.None;
                if (absoluteX + width <= hostWidth && absoluteY + height < hostHeight)
                    mode = DisplayMode.RightDown;
                else if (absoluteX + width > hostWidth && absoluteY + height < hostHeight)
                    mode = DisplayMode.LeftDown;
                else if (absoluteX + width <= hostWidth && absoluteY + height > hostHeight)
                    mode = DisplayMode.RightUp;
                else if (absoluteX + width > hostWidth && absoluteY + height > hostHeight)
                    mode = DisplayMode.LeftUp;
                IsOpen = true;
                double X = 0;
                double Y = 0;
                switch (mode)
                {
                    case DisplayMode.RightDown:
                        X = absoluteX;
                        Y = absoluteY;
                        break;
                    case DisplayMode.RightUp:
                        X = absoluteX;
                        Y = absoluteY - height;
                        break;
                    case DisplayMode.LeftUp:
                        X = absoluteX - width;
                        Y = absoluteY - height;
                        break;
                    case DisplayMode.LeftDown:
                        X = absoluteX - width;
                        Y = absoluteY;
                        break;
                }
                HorizontalOffset = X;
                VerticalOffset = Y;
                m_CachePoint = null;
            }
        }

        public void ShowMenu(Point absolutePosition)
        {
            if (absolutePosition == null)
                throw new ArgumentNullException("Absolute Position");
            if (Application.Current.RootVisual == null)
                throw new InvalidOperationException("Application RootVisual");
            m_CachePoint = absolutePosition;
            ContextMenuService.ShowMenu(this);
            ShowMenuByCachePoint();
        }

        public override void CloseMenu()
        {
            IsOpen = false;
        }
        #endregion

        #region ItemsControl Members
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            MenuItem menuItem = element as MenuItem;
            if (menuItem != null && ItemContainerStyle != null)
            {
                menuItem.SetValue(HeaderedItemsControl.ItemContainerStyleProperty, ItemContainerStyle);
            }
        }
        #endregion

        #region NestedType Members
        private enum DisplayMode
        {
            None,
            LeftUp,
            LeftDown,
            RightUp,
            RightDown,
            AlongLeft,
            AlongTop,
            AlongRight,
            AlongBottom
        }
        #endregion
    }
}
