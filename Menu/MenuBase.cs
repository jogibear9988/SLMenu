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
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;
using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Controls
{
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(MenuItem))]
    public abstract class MenuBase : ItemsControl
    {
        #region Constructor Members
        protected MenuBase()
        {
            m_ItemsCache = new Dictionary<object, DependencyObject>();
        }
        #endregion

        #region DependencyProperty Members
        public static DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register("ItemContainerStyle",
            typeof(Style), typeof(MenuBase), new PropertyMetadata(null, new PropertyChangedCallback(OnItemContainerStyleChanged)));
        public static DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem",
            typeof(object), typeof(MenuBase), new PropertyMetadata(null, new PropertyChangedCallback(OnSelectedItemPropertyChanged)));
        #endregion

        #region ItemsControl Members
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MenuItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MenuItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            ItemsCache.Add(item, element);
            MenuItem node = element as MenuItem;
            if (node != null)
            {
                // Associate the Parent ItemsControl
                node.ParentMenu = this;
                if (node.Style == null && ItemContainerStyle != null)
                    node.Style = ItemContainerStyle;
                if (this.ItemTemplate != null)
                {
                    node.SetValue(HeaderedItemsControl.ItemTemplateProperty, this.ItemTemplate);
                    node.SetValue(HeaderedItemsControl.HeaderTemplateProperty, this.ItemTemplate);
                }
                node.Item = item;
                if (!item.Equals(element))
                    node.Header = item;
                HierarchicalDataTemplate headerTemplate = ItemTemplate as HierarchicalDataTemplate;
                if (headerTemplate != null)
                {
                    if (headerTemplate.ItemsSource != null)
                    {
                        node.SetBinding(
                            HeaderedItemsControl.ItemsSourceProperty,
                            new Binding
                            {
                                Converter = headerTemplate.ItemsSource.Converter,
                                ConverterCulture = headerTemplate.ItemsSource.ConverterCulture,
                                ConverterParameter = headerTemplate.ItemsSource.ConverterParameter,
                                Mode = headerTemplate.ItemsSource.Mode,
                                NotifyOnValidationError = headerTemplate.ItemsSource.NotifyOnValidationError,
                                Path = headerTemplate.ItemsSource.Path,
                                Source = node.Header,
                                ValidatesOnExceptions = headerTemplate.ItemsSource.ValidatesOnExceptions
                            });
                    }
                }
            }
            base.PrepareContainerForItemOverride(element, item);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            ItemsCache.Remove(item);
            MenuItem node = element as MenuItem;
            if (node != null)
            {
                node.ParentMenu = null;
                node.Header = null;
            }
            base.ClearContainerForItemOverride(element, item);
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            base.OnItemsChanged(e);

            if (e.NewItems != null)
            {
                foreach (MenuItem item in e.NewItems.OfType<MenuItem>())
                {
                    item.ParentMenu = this;
                    if (item.Style == null && ItemContainerStyle != null)
                        item.Style = ItemContainerStyle;
                }
            }

            if (e.OldItems != null)
            {
                foreach (MenuItem item in e.OldItems.OfType<MenuItem>())
                {
                    item.ParentMenuItem = null;
                }
            }
        }
        #endregion

        #region Property Members
        public Style ItemContainerStyle
        {
            get { return (Style)GetValue(ItemContainerStyleProperty); }
            set { SetValue(ItemContainerStyleProperty, value); }
        }
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            protected internal set
            {
                CloseMenu();
                SetValue(SelectedItemProperty, value);
            }
        }
        #endregion

        #region PropertyChanged Members
        private bool _SetSelectedItemInternal = false;
        protected virtual void OnItemContainerStyleChanged(Style newValue)
        {
            foreach (MenuItem item in ItemsCache.Values.OfType<MenuItem>())
            {
                item.Style = newValue;
                item.ItemContainerStyle = newValue;
            }
        }
        private void OnSelectedItemChanged(object oldValue, object newValue)
        {
            if (_SetSelectedItemInternal)
            {
                _SetSelectedItemInternal = false;
                return;
            }
            object[] oldItems = new object[1] { oldValue };
            object[] newItems = new object[1] { newValue };
            OnSelectionChanged(oldItems, newItems);
            if (newValue != null)
            {
                _SetSelectedItemInternal = true;
                SelectedItem = null;
            }
        }

        private static void OnItemContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MenuBase)d).OnItemContainerStyleChanged((Style)e.NewValue);
        }
        private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MenuBase)d).OnSelectedItemChanged(e.OldValue, e.NewValue);
        }
        #endregion

        #region MenuBase Members
        public virtual void CloseMenu()
        {
            SelectedMenuItem = null;
        }

        private MenuItem m_SelectedMenuItem;
        internal protected virtual MenuItem SelectedMenuItem
        {
            get { return m_SelectedMenuItem; }
            set
            {
                if (m_SelectedMenuItem != value)
                {
                    if (m_SelectedMenuItem != null)
                        m_SelectedMenuItem.IsSelected = false;
                    m_SelectedMenuItem = value;
                }
            }
        }

        private Dictionary<object, DependencyObject> m_ItemsCache;
        protected IDictionary<object, DependencyObject> ItemsCache
        {
            get { return m_ItemsCache; }
        }
        #endregion

        #region Event Members
        public event SelectionChangedEventHandler SelectionChanged;
        private void OnSelectionChanged(IList removedItems, IList addedItems)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, new SelectionChangedEventArgs(removedItems, addedItems));
        }
        #endregion
    }
}
