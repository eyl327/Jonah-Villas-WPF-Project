﻿using WPFPL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BL;

namespace WPFPL
{
    /// <summary>
    /// Interaction logic for HostOrders.xaml
    /// </summary>
    public partial class HostOrders : Page
    {
        public MainWindow mainWindow;

        public static ObservableCollection<string> OrdersCollection { get; set; }
        public HostOrders()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            OrdersCollection = new ObservableCollection<string>();
            Orders.ItemsSource = OrdersCollection;
            Refresh();
        }

        public static void Refresh()
        {
            if (OrdersCollection != null)
            {
                OrdersCollection.Clear();
                foreach (BE.Order item in Util.Bl.GetHostOrders(Util.MyHost.HostKey))
                {
                    OrdersCollection.Add(item.ToString());
                }
            }
        }

        private void Return_To_Options(object sender, RoutedEventArgs e)
        {
            mainWindow.HostingFrame.Navigate(new HostMenu());
        }

        private void Update_Order(object sender, RoutedEventArgs e)
        {

        }
    }
}