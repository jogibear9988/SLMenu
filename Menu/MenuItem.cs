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
using System.Windows.Threading;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Windows.Controls
{
    [TemplatePart(Name = "ElementContent", Type = typeof(ContentControl))]
    [TemplatePart(Name = "Popup", Type = typeof(Popup))]
    [TemplatePart(Name = "ElementIcon", Type = typeof(Image))]  
    [TemplateVisualState(GroupName = "CheckStates", Name = "Checked")]
    [TemplateVisualState(GroupName = "CheckStates", Name = "UnChecked")]
    [TemplateVisualState(GroupName = "HasItemsStates", Name = "HasItems")]
    [TemplateVisualState(GroupName = "HasItemsStates", Name = "NoItems")]
    public class MenuItem : HeaderedItemsControl
    {
        #region Constant Members
        private const double ShowSubMenuDelay = 300.0;
        private const double CloseSubMenuDelay = 300.0;
        #endregion

        #region DependencyProperty Members
        public static readonly DependencyProperty HasItemsProperty = DependencyProperty.Register("HasItems",
            typeof(bool), typeof(MenuItem), new PropertyMetadata(false, new PropertyChangedCallback(OnHasItemsPropertyChanged)));
        public static readonly DependencyProperty IsHighlightedProperty = DependencyProperty.Register("IsHighlighted",
            typeof(bool), typeof(MenuItem), new PropertyMetadata(false, new PropertyChangedCallback(OnIsHighlightedPropertyChanged)));
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
            typeof(ICommand), typeof(MenuItem), new PropertyMetadata(null));
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter",
            typeof(object), typeof(MenuItem), new PropertyMetadata(null));
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon",
            typeof(ImageSource), typeof(MenuItem), new PropertyMetadata(null, new PropertyChangedCallback(OnIconPropertyChanged)));
        public static readonly DependencyProperty IsSubMenuOpenProperty = DependencyProperty.Register("IsSubMenuOpen",
            typeof(bool), typeof(MenuItem), new PropertyMetadata(false, new PropertyChangedCallback(OnIsSubMenuOpenPropertyChanged)));
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
            typeof(bool), typeof(MenuItem), new PropertyMetadata(false, new PropertyChangedCallback(OnIsSelectedPropertyChanged)));
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked",
            typeof(Nullable<bool>), typeof(MenuItem), new PropertyMetadata(null, new PropertyChangedCallback(OnIsCheckedPropertyChanged)));
        #endregion

        #region Parent Members
        private MenuItem m_ParentMenuItem;
        internal MenuItem ParentMenuItem
        {
            get { return m_ParentMenuItem; }
            set
            {
                if (m_ParentMenuItem != null)
                    m_ParentMenuItem.OnSubMenuOpened -= OnMenuOpened;
                m_ParentMenuItem = value;
                if (m_ParentMenuItem != null)
                    m_ParentMenuItem.OnSubMenuOpened += OnMenuOpened;
            }
        }

        void OnMenuOpened(object sender, RoutedEventArgs e)
        {
            if (Command != null)
            {
                base.IsEnabled = Command.CanExecute(CommandParameter);
            }
        }

        private MenuBase m_ParentMenu;
        internal MenuBase ParentMenu
        {
            get { return m_ParentMenu; }
            set
            {
                ContextMenu contextMenu = m_ParentMenu as ContextMenu;
                if (contextMenu != null)
                    contextMenu.Opened -= OnParentMenuOpened;
                m_ParentMenu = value;
                contextMenu = m_ParentMenu as ContextMenu;
                if (contextMenu != null)
                    contextMenu.Opened += OnParentMenuOpened;
            }
        }

        void OnParentMenuOpened(object sender, RoutedEventArgs e)
        {
            if (Command != null)
            {
                base.IsEnabled = Command.CanExecute(CommandParameter);
            }
        }

        protected internal MenuBase RootMenu
        {
            get
            {
                if (m_ParentMenu == null)
                {
                    if (m_ParentMenuItem != null)
                        return m_ParentMenuItem.RootMenu;
                }
                return m_ParentMenu;
            }
        }

        private MenuItem m_SelectedItem;
        internal virtual MenuItem SelectedItem
        {
            get { return m_SelectedItem; }
            set
            {
                if (m_SelectedItem != value)
                {
                    if (m_SelectedItem != null)
                        m_SelectedItem.IsSelected = false;
                    m_SelectedItem = value;
                }
            }
        }
        #endregion

        #region Event Members
        public event RoutedEventHandler OnSubMenuOpened;
        public event RoutedEventHandler OnSubMenuClosed;
        public event RoutedEventHandler Checked;
        public event RoutedEventHandler Click;
        #endregion

        #region Constructor Members
        public MenuItem()
        {
            base.DefaultStyleKey = typeof(MenuItem);
            m_ItemsCache = new Dictionary<object, DependencyObject>();
            this.ApplyTemplate();
        }
        #endregion

        #region UIField Members
        private Popup _Popup;
        private Image _Icon;
        private ContentControl _CommandContent;  
        #endregion

        #region Template Members
        protected override void OnHeaderChanged(object oldHeader, object newHeader)
        {
            base.OnHeaderChanged(oldHeader, newHeader);
            if (_CommandContent != null)
                _CommandContent.Content = newHeader;
        }

        protected override void OnHeaderTemplateChanged(DataTemplate oldHeaderTemplate, DataTemplate newHeaderTemplate)
        {
            base.OnHeaderTemplateChanged(oldHeaderTemplate, newHeaderTemplate);
            if (_CommandContent != null)
                _CommandContent.ContentTemplate = newHeaderTemplate;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this._Popup = (Popup)GetTemplateChild("Popup");
            this._Icon = (Image)GetTemplateChild("ElementIcon");
            this._CommandContent = (ContentControl)GetTemplateChild("ElementContent");           

            if (_CommandContent != null)
            {
                if (Header != null)
                    _CommandContent.Content = Header;
                if (HeaderTemplate != null)
                    _CommandContent.ContentTemplate = HeaderTemplate;
            }

            if (this._Icon != null && this.Icon != null)
                _Icon.Source = Icon;

            if (this._Popup != null)
            {
                this._Popup.Opened += (o, e) =>
                    {
                        if (this.OnSubMenuOpened != null)
                            OnSubMenuOpened(this, new RoutedEventArgs());
                    };
                this._Popup.Closed += (o, e) =>
                    {
                        if (this.OnSubMenuClosed != null)
                            OnSubMenuClosed(this, new RoutedEventArgs());
                    };
                this._Popup.Child.MouseEnter += new MouseEventHandler(_ItemsHost_MouseEnter);
                this._Popup.Child.MouseLeave += new MouseEventHandler(_ItemsHost_MouseLeave);
            }
            ChangeVisualState(null);
        }

        void _ItemsHost_MouseLeave(object sender, MouseEventArgs e)
        {
            //SetCloseHierarchyTimer(ref this._closeHierarchyTimer);
        }

        void _ItemsHost_MouseEnter(object sender, MouseEventArgs e)
        {
            StopTimer(ref this._closeHierarchyTimer);
        }
        #endregion

        #region Property Members
        internal protected bool IsHighlighted
        {
            get { return (bool)GetValue(IsHighlightedProperty); }
            set { SetValue(IsHighlightedProperty, value); }
        }

        [TypeConverter(typeof(NullableBoolConverter))]
        public bool? IsChecked
        {
            get { return (bool?)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public bool IsSubMenuOpen
        {
            get { return (bool)GetValue(IsSubMenuOpenProperty); }
            protected set { SetValue(IsSubMenuOpenProperty, value); }
        }

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public bool HasItems
        {
            get { return (bool)GetValue(HasItemsProperty); }
            private set { SetValue(HasItemsProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        internal object Item
        {
            get;
            set;
        }
        #endregion

        #region MouseEvents Members
        private bool _MouseOverNow = false;
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            Debug.WriteLine("Header is {0}", Header);
            base.OnMouseEnter(e);
            _MouseOverNow = true;
            IsHighlighted = true;
            if (ParentMenuItem != null || !(ParentMenu is Menu) || (ParentMenu is Menu && ParentMenu.SelectedMenuItem != null))
                IsSelected = true;
            StopTimer(ref _closeHierarchyTimer);
            if (HasItems && !IsSubMenuOpen && (ParentMenuItem != null || !(ParentMenu is Menu)))
                SetOpenHierarchyTimer(ref _openHierarchyTimer);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            _MouseOverNow = false;
            IsHighlighted = false;
            StopTimer(ref _openHierarchyTimer);
            if (HasItems && IsSubMenuOpen && (ParentMenuItem != null || !(ParentMenu is Menu)))
                SetCloseHierarchyTimer(ref _closeHierarchyTimer);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (ParentMenu is Menu)
                IsSelected = !IsSelected;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (Click != null)
                Click(this, new RoutedEventArgs());
            if (Command != null)
                Command.Execute(CommandParameter);
            if (!HasItems && RootMenu != null)
            {
                RootMenu.SelectedItem = this.Item ?? this;
            }
        }
        #endregion
        
        public bool IsSeperator
        {
            get { return (bool)GetValue(IsSeperatorProperty); }
            set { SetValue(IsSeperatorProperty, value); }
        }

        public static readonly DependencyProperty IsSeperatorProperty =
            DependencyProperty.Register("IsSeperator", typeof(bool), typeof(MenuItem), new PropertyMetadata(false));
        
        #region ItemsControl Members
        /// <summary>
        /// Creates a new MenuItem to use to display the object.
        /// </summary>
        /// <returns>A new MenuItem.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MenuItem();
        }

        /// <summary>
        /// Determines whether an object is a MenuItem.
        /// </summary>
        /// <param name="item">The object to evaluate.</param>
        /// <returns>true if item is a MenuItem; otherwise, false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MenuItem;
        }

        /// <summary>
        /// Prepares the specified element to display the specified item.
        /// </summary>
        /// <param name="element">
        /// Element used to display the specified item.
        /// </param>
        /// <param name="item">Specified item.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            m_ItemsCache[item] = element;
            MenuItem node = element as MenuItem;
            if (node != null)
            {
                // Associate the Parent ItemsControl
                node.Item = item;
                node.ParentMenuItem = this;
            }
            base.PrepareContainerForItemOverride(element, item);
        }

        /// <summary>
        /// Undoes the effects of PrepareContainerForItemOverride.
        /// </summary>
        /// <param name="element">The container element.</param>
        /// <param name="item">The contained item.</param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            m_ItemsCache.Remove(item);
            MenuItem node = element as MenuItem;
            if (node != null)
            {
                node.ParentMenuItem = null;
            }
            base.ClearContainerForItemOverride(element, item);
        }

        /// <summary>
        /// Provides handling for the ItemsChanged event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            base.OnItemsChanged(e);
            HasItems = Items.Count > 0;

            // Associate any MenuItems with their parent
            if (e.NewItems != null)
            {
                foreach (MenuItem item in e.NewItems.OfType<MenuItem>())
                {
                    item.ParentMenuItem = this;
                }
            }

            // Remove the association with the Parent ItemsControl
            if (e.OldItems != null)
            {
                foreach (MenuItem item in e.OldItems.OfType<MenuItem>())
                {
                    item.ParentMenuItem = null;
                }
            }
        }
        #endregion ItemsControl

        #region VisualState Members
        private const string MouseOverState = "MouseOver";
        private const string CheckedState = "Checked";
        private const string UnCheckedState = "UnChecked";
        private const string HasItemsState = "HasItems";
        private const string NoItemsState = "NoItems";
        private const string NormalState = "Normal";
        private const string SubMenuOpendState = "SubMenuOpend";
        internal virtual void ChangeVisualState(string state)
        {
            if (HasItems && this.ParentMenuItem != null)
                VisualStateManager.GoToState(this, HasItemsState, true);
            else
                VisualStateManager.GoToState(this, NoItemsState, true);
            if (IsChecked == true)
                VisualStateManager.GoToState(this, CheckedState, true);
            else
                VisualStateManager.GoToState(this, UnCheckedState, true);
            if (!IsSelected && !IsHighlighted)
                VisualStateManager.GoToState(this, NormalState, true);
            else
                VisualStateManager.GoToState(this, MouseOverState, true);

            if (!HasItems && state == SubMenuOpendState)
                 VisualStateManager.GoToState(this, SubMenuOpendState, true);            
        }
        #endregion

        #region PropertyChanged Members
        private void OnHasItemsPropertyChanged(bool newValue)
        {
            if (newValue)
                ChangeVisualState(HasItemsState);
            else
                ChangeVisualState(NoItemsState);
        }

        private void OnIsHighlightedPropertyChanged(bool newValue)
        {
            if (newValue)
                ChangeVisualState(MouseOverState);
            else
                ChangeVisualState(NormalState);
        }

        private void OnIconPropertyChanged(ImageSource newValue)
        {
            if (newValue != null && this._Icon != null)
            {
                _Icon.Source = newValue;
            }
        }

        private void OnIsSelectedPropertyChanged(bool newValue)
        {
            if (newValue)
            {
                if (ParentMenuItem != null)
                    ParentMenuItem.SelectedItem = this;
                else if (ParentMenu != null)
                    ParentMenu.SelectedMenuItem = this;
                if (ParentMenu is Menu)
                    OpenSubMenu();
            }
            else
            {
                if (HasItems && IsSubMenuOpen)
                {
                    if (ParentMenu is Menu)
                        CloseSubMenu();
                    else
                        SetCloseHierarchyTimer(ref this._closeHierarchyTimer);
                }
                if (ParentMenu != null && ParentMenu.SelectedMenuItem == this)
                    ParentMenu.SelectedMenuItem = null;
                this.SelectedItem = null;
            }
            ChangeVisualState(null);
        }

        private void OnIsCheckedPropertyChanged(bool? newValue)
        {
            if (newValue == true)
            {
                ChangeVisualState(CheckedState);
                if (Checked != null)
                    Checked(this, new RoutedEventArgs());
            }
            else
                ChangeVisualState(UnCheckedState);
        }

        private void OnIsSubMenuPropertyChanged(bool newValue)
        {
            foreach (var item in m_ItemsCache.Values.OfType<MenuItem>())
                item.IsHighlighted = false;
            if (_Popup != null)
            {
                _Popup.IsOpen = newValue;
            }
            if (newValue)
            {
                IsHighlighted = true;
            }
            else
            {
                if (!_MouseOverNow)
                    IsHighlighted = false;
            }
        }

        private static void OnHasItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MenuItem)d).OnHasItemsPropertyChanged((bool)e.NewValue);
        }

        private static void OnIsHighlightedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MenuItem)d).OnIsHighlightedPropertyChanged((bool)e.NewValue);
        }

        private static void OnIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MenuItem)d).OnIconPropertyChanged((ImageSource)e.NewValue);
        }

        private static void OnIsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MenuItem)d).OnIsSelectedPropertyChanged((bool)e.NewValue);
        }

        private static void OnIsCheckedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MenuItem)d).OnIsCheckedPropertyChanged((bool?)e.NewValue);
        }

        private static void OnIsSubMenuOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MenuItem)d).OnIsSubMenuPropertyChanged((bool)e.NewValue);
        }
        #endregion

        #region Timer Members
        private DispatcherTimer _closeHierarchyTimer;
        private DispatcherTimer _openHierarchyTimer;

        private void StopTimer(ref DispatcherTimer timer)
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        private void SetCloseHierarchyTimer(ref DispatcherTimer timer)
        {
            if (timer == null)
                timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(CloseSubMenuDelay);
            timer.Tick += (o, e) => { CloseSubMenu(); };
            timer.Start();
        }

        private void SetOpenHierarchyTimer(ref DispatcherTimer timer)
        {
            if (timer == null)
                timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(ShowSubMenuDelay);
            timer.Tick += (o, e) => { OpenSubMenu(); };
            timer.Start();
        }
        #endregion

        #region SubMenu Members
        private IDictionary<object, DependencyObject> m_ItemsCache;
        protected virtual void OpenSubMenu()
        {
            ChangeVisualState(SubMenuOpendState);

            if (HasItems)
            {
                IsSubMenuOpen = true;
                ShowSubMenu();
            }
        }

        private void ShowSubMenu()
        {
            if (!HasItems)
                return;

            if (this.ParentMenuItem == null && this.ParentMenu is Menu)
            {
                double height = _CommandContent != null ? _CommandContent.ActualHeight : 20;
                if (_Popup != null)
                    _Popup.VerticalOffset = height;
            }
            else
            {
                GeneralTransform gt = TransformToVisual(Application.Current.RootVisual);
                Point p = gt.Transform(new Point(0, 0));

                //Point p = this.TransformFromRootVisual();
                double width = double.IsNaN(Width) ? ActualWidth : Width;
                double height = double.IsNaN(Height) ? ActualHeight : Height;
                double absoluteX = p.X + width;
                double absoluteY = p.Y;
                double hostWidth = Application.Current.Host.Content.ActualWidth;
                double hostHeight = Application.Current.Host.Content.ActualHeight;
                double desiredX = 200;
                double desiredY = 20 * Items.Count;
                var mode = DisplayMode.None;
                if (absoluteX + desiredX <= hostWidth && absoluteY + desiredY < hostHeight)
                    mode = DisplayMode.ParentRightDown;
                else if (absoluteX + desiredX > hostWidth && absoluteY + desiredY < hostHeight)
                    mode = DisplayMode.ParentLeftDown;
                else if (absoluteX + desiredX <= hostWidth && absoluteY + desiredY > hostHeight)
                    mode = DisplayMode.ParentRightUp;
                else if (absoluteX + desiredX > hostWidth && absoluteY + desiredY > hostHeight)
                    mode = DisplayMode.ParentLeftUp;
                double X = 0;
                double Y = 0;
                switch (mode)
                {
                    case DisplayMode.ParentRightDown:
                        X = absoluteX;
                        Y = absoluteY;
                        break;
                    case DisplayMode.ParentRightUp:
                        X = absoluteX;
                        Y = absoluteY - desiredY;
                        break;
                    case DisplayMode.ParentLeftUp:
                        X = absoluteX - width - desiredX;
                        Y = absoluteY - desiredY;
                        break;
                    case DisplayMode.ParentLeftDown:
                        X = absoluteX - width - desiredX;
                        Y = absoluteY;
                        break;
                }
                //_Popup.HorizontalOffset = X; //- p.X;
                //_Popup.VerticalOffset = Y - height * (ParentMenuItem.Items.IndexOf(this.Item ?? this) + 1);// -p.Y;

                if (System.Environment.Version.Major == 5)
                {
                    _Popup.HorizontalOffset = X - p.X;
                    // _popup.VerticalOffset = Y - height * (ParentMenuItem.Items.IndexOf(this.Item ?? this) + 1) - p.Y;
                }
                else
                {
                    _Popup.HorizontalOffset = X; // -p.X;
                    _Popup.VerticalOffset = Y - height * (this.ParentMenuItem.Items.IndexOf(this.Item ?? this) + 1); // - p.Y;
                }
            }
        }

        protected internal virtual void CloseSubMenu()
        {
            IsSubMenuOpen = false;
        }
        #endregion

        #region NestedType
        private enum DisplayMode
        {
            None,
            ParentLeftUp,
            ParentRightUp,
            ParentLeftDown,
            ParentRightDown
        }
        #endregion
    }
}
