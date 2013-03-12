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
using System.Windows.Browser;

namespace System.Windows.Controls
{
    public class ContextMenuService
    {
        public static readonly DependencyProperty ContextMenuProperty=DependencyProperty.RegisterAttached("ContextMenu",
            typeof(ContextMenu), typeof(UIElement), new PropertyMetadata(null, new PropertyChangedCallback(OnContextMenuPropertyChanged)));

        private static Grid m_Host;        
        static ContextMenuService()
        {
            m_Host = new Grid();
            m_Host.Background = new SolidColorBrush(Colors.Transparent);
            m_Host.MouseLeftButtonDown += OnHostClick;
            m_Host.Visibility = Visibility.Collapsed; 
            HtmlPage.Document.AttachEvent("oncontextmenu", OnContextMenu);
        }

        static void OnContextMenu(object sender, HtmlEventArgs e)
        {
            RemoveMenu(m_CurrentMenu);
        }

        static void OnHostClick(object sender, MouseButtonEventArgs e)
        {
            RemoveMenu(m_CurrentMenu);
        }

        internal static ContextMenu m_CurrentMenu;
        internal static Panel m_ContextMenuHost;
        public static void ShowMenu(ContextMenu contextMenu)
        {
            if (m_CurrentMenu != contextMenu)
            {
                RemoveMenu(m_CurrentMenu);
                AddMenuToHost(contextMenu);
                m_CurrentMenu = contextMenu;
            }
        }

        internal static void RemoveMenu(ContextMenu contextMenu)
        {
            if (contextMenu != null 
                && m_ContextMenuHost != null
                && m_Host.Children.Contains(contextMenu))
            {
                m_CurrentMenu.CloseMenu();
                m_Host.Children.Remove(contextMenu);
                m_Host.Visibility = Visibility.Collapsed;
            }
        }

        internal static void AddMenuToHost(ContextMenu contextMenu)
        {
            if (contextMenu != null && m_ContextMenuHost != null
                && !m_Host.Children.Contains(contextMenu))
            {
                m_Host.Visibility = Visibility.Visible;
                m_Host.Children.Add(contextMenu);                
            }
        }

        public static void RegisterMenuHost(Panel hostPanel)
        {
            if (hostPanel != m_ContextMenuHost)
            {
                if (m_ContextMenuHost != null && m_ContextMenuHost.Children.Contains(m_Host))
                    m_ContextMenuHost.Children.Remove(m_Host);
                m_ContextMenuHost = hostPanel;
                if (m_ContextMenuHost != null)
                    m_ContextMenuHost.Children.Add(m_Host);
            }
        }

        private static void OnContextMenuPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = (UIElement)d;
        }
    }
}
