using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace SL3Menu
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();            
            base.Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            ObservableCollection<MenuBindingSource> itemsSource = new ObservableCollection<MenuBindingSource>();
            MenuBindingSource menu1 = new MenuBindingSource("Dropdown Menu1");
            MenuBindingSource menu2 = new MenuBindingSource("Dropdown Menu2");
            MenuBindingSource menu3 = new MenuBindingSource("Dropdown Menu3");
            itemsSource.Add(menu1);
            itemsSource.Add(menu2);
            itemsSource.Add(menu3);
            MenuBindingSource submenu1 = new MenuBindingSource("First Level SubMenu1");
            MenuBindingSource submenu2 = new MenuBindingSource("First Level SubMenu2");
            MenuBindingSource submenu3 = new MenuBindingSource("First Level SubMenu3");
            menu1.AddItem(submenu1);
            menu1.AddItem(submenu2);
            menu1.AddItem(submenu3);
            menu2.AddItem(submenu1);
            menu2.AddItem(submenu2);
            menu2.AddItem(submenu3);
            menu3.AddItem(submenu1);
            menu3.AddItem(submenu2);
            menu3.AddItem(submenu3);
            MenuBindingSource subsubmenu1 = new MenuBindingSource("Second Level SubMenu1");
            MenuBindingSource subsubmenu2 = new MenuBindingSource("Second Level SubMenu2");
            submenu1.AddItem(subsubmenu1);
            submenu1.AddItem(subsubmenu2);
            submenu2.AddItem(subsubmenu1);
            submenu2.AddItem(subsubmenu2);
            submenu3.AddItem(subsubmenu1);
            submenu3.AddItem(subsubmenu2);
            this.MainMenu2.ItemsSource = itemsSource;
        }

        private void MainMenu2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e != null && e.AddedItems != null && e.AddedItems.Count != 0)
            {
                MenuBindingSource source = e.AddedItems[0] as MenuBindingSource;
                if (source != null)
                {
                    MessageBox.Show(source.Content, "MainMenu2", MessageBoxButton.OK);
                }
            }
        }

        private void MainMenu1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e != null && e.AddedItems != null && e.AddedItems.Count != 0)
            {
                MenuItem item = e.AddedItems[0] as MenuItem;
                if (item != null && item.Header != null)
                {
                    MessageBox.Show(item.Header.ToString(), "MainMenu1", MessageBoxButton.OK);
                }
            }
        }
    }
}
