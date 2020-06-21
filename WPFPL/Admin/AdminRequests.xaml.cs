﻿using BE;
using BL;
using WPFPL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace WPFPL.Admin
{
    /// <summary>
    /// Interaction logic for AdminRequests.xaml
    /// </summary>
    public partial class AdminRequests : Page
    {
        public MainWindow mainWindow;

        public static ObservableCollection<string> RequestCollection { get; set; }

        public static string Search { get; set; }

        private static int SortIndex { get; set; }

        public AdminRequests()
        {
            InitializeComponent();
            mainWindow = Util.GetMainWindow();
            RequestCollection = new ObservableCollection<string>();
            Requests.ItemsSource = RequestCollection;
            Refresh();
        }

        public static void Refresh(string search = "")
        {
            if (RequestCollection != null)
            {
                try
                {
                    // normalize search
                    if (search != null) { search = Normalize.Convert(search); }
                    else { search = ""; }
                    // clear collection
                    RequestCollection.Clear();
                    // list of requests
                    List<GuestRequest> orderedRequests = new List<GuestRequest>();
                    // get requests and sort
                    switch (SortIndex)
                    {
                        case -1:
                        // Oldest first
                        case 0: orderedRequests = Util.Bl.GetGuestRequests().OrderBy(item => item.GuestRequestKey).ToList(); break;
                        // Newest first
                        case 1: orderedRequests = Util.Bl.GetGuestRequests().OrderByDescending(item => item.GuestRequestKey).ToList(); break;
                        // Last name A-Z
                        case 2: orderedRequests = Util.Bl.GetGuestRequests().OrderBy(item => item.LastName).ToList(); break;
                        // First name A-Z
                        case 3: orderedRequests = Util.Bl.GetGuestRequests().OrderBy(item => item.FirstName).ToList(); break;
                        // Fewest guests first
                        case 4: orderedRequests = Util.Bl.GetGuestRequests().OrderBy(item => item.NumAdults + item.NumChildren).ToList(); break;
                        // Most guests first
                        case 5: orderedRequests = Util.Bl.GetGuestRequests().OrderByDescending(item => item.NumAdults + item.NumChildren).ToList(); break;
                        // Unit Type A-Z
                        case 6: orderedRequests = Util.Bl.GetGuestRequests().OrderBy(item => item.PrefType.ToString()).ToList(); break;
                        // Unit City A-Z
                        case 7: orderedRequests = Util.Bl.GetGuestRequests().OrderBy(item => item.PrefCity.ToString()).ToList(); break;
                        // Unit District A-Z
                        case 8: orderedRequests = Util.Bl.GetGuestRequests().OrderBy(item => item.PrefDistrict.ToString()).ToList(); break;
                        // Entry date soonest first
                        case 9: orderedRequests = Util.Bl.GetGuestRequests().OrderBy(item => item.EntryDate).ToList(); break;
                        // Entry date furthest first
                        case 10: orderedRequests = Util.Bl.GetGuestRequests().OrderByDescending(item => item.EntryDate).ToList(); break;
                        // Request Status A-Z
                        case 11: orderedRequests = Util.Bl.GetGuestRequests().OrderBy(item => item.Status.ToString()).ToList(); break;
                        default: orderedRequests = Util.Bl.GetGuestRequests().OrderBy(item => item.GuestRequestKey).ToList(); break;
                    }
                    // add items to list and filter by search
                    foreach (GuestRequest item in orderedRequests)
                    {
                        // search by all public fields
                        if (Normalize.Convert(item).Contains(search))
                        {
                            RequestCollection.Add(item.ToString());
                        }
                    }
                }
                catch (Exception error)
                {
                    Util.GetMainWindow().MySnackbar.MessageQueue.Enqueue(error.Message);
                }
            }
        }

        private void Return_To_Menu(object sender, RoutedEventArgs e)
        {
            mainWindow.AdminFrame.Navigate(new AdminMenu());
        }

        private void Refresh_Event(object sender, RoutedEventArgs e)
        {
            Search = SearchBox.Text;
            Refresh(Search);
        }

        private void Clear_Search(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            Refresh();
        }
        private void Sort_Selection_Changed(object sender, SelectionChangedEventArgs e)
        {
            SortIndex = sortBy.SelectedIndex;
            Refresh();
        }

    }
}
