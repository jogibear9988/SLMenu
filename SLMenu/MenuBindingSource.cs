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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace SL3Menu
{
    public class MenuBindingSource : INotifyPropertyChanged
    {
        public MenuBindingSource() { }
        public MenuBindingSource(string content)
        {
            m_Content = content;
        }

        private ObservableCollection<MenuBindingSource> m_Items;
        public IEnumerable<MenuBindingSource> Items
        {
            get { return m_Items; }
        }

        private string m_Content;
        public string Content
        {
            get { return m_Content; }
            set 
            {
                if (m_Content != value)
                {
                    m_Content = value;
                    OnPropertyChanged("Content");
                }
            }
        }

        public void AddItem(MenuBindingSource source)
        {
            if (m_Items == null)
                m_Items = new ObservableCollection<MenuBindingSource>();
            m_Items.Add(source);
        }

        public bool RemoveItem(MenuBindingSource source)
        {
            if (m_Items != null)
            {
                return m_Items.Remove(source);
            }
            return false;
        }

        public void Clear()
        {
            if (m_Items != null)
            {
                m_Items.Clear();
            }
        }

        public void Insert(int index, MenuBindingSource item)
        {
            if (m_Items == null)
                m_Items = new ObservableCollection<MenuBindingSource>();
            m_Items.Insert(index, item);
        }

        public override string ToString()
        {
            return m_Content;
        }


        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
