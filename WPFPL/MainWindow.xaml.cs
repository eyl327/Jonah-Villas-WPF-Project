﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BE;
using BL;
using WPFPL.Admin;

namespace WPFPL
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> DynamicCityList { get; set; }

        // current visible tab
        public TabItem CurrentTab { get; set; }

        // Host which is currently logged in
        public static Host LoggedInHost { get; set; }

        // Get the instance of the BL
        public static IBL Bl = FactoryBL.GetBL();

        /// <summary>
        /// Startup function
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        /// <summary>
        /// Function to run when window is loaded
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentTab = Tab0;
            DynamicCityList = new ObservableCollection<string> { "Select district first." };
            gPrefCity.ItemsSource = DynamicCityList;
            HostingFrame.Navigate(new HostSignIn());
            AdminFrame.Navigate(new AdminSignIn());
            MyDialog.IsOpen = false;
            this.SizeChanged += ChooseAmenityListBoxStyle;
            this.DataContext = this;

            // start thread for expiring orders daily
            BL.OrderExpiration.StartJob();
        }

        /// <summary>
        /// Open custom dialog box with custom text
        /// </summary>
        /// <param name="text">Text to insert into box</param>
        public static void Dialog(string text, string tag = "", object textBox = null, object combo1 = null, object combo2 = null, object checkbox = null)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

            // hide / show input boxes
            mainWindow.MyDialogTextBox.Height = (textBox == null) ? (0 /* hidden */) : (double.NaN /* Auto */);
            mainWindow.MyDialogComboBox1.Height = (combo1 == null) ? (0 /* hidden */) : (double.NaN /* Auto */);
            mainWindow.MyDialogComboBox2.Height = (combo2 == null) ? (0 /* hidden */) : (double.NaN /* Auto */);
            mainWindow.MyDialogCheckbox.Height = (checkbox == null) ? (0 /* hidden */) : (double.NaN /* Auto */);

            mainWindow.MyDialogTextBox.Margin = (textBox == null) ? new Thickness(0) : new Thickness(0,6,20,6);
            mainWindow.MyDialogComboBox1.Margin = (combo1 == null) ? new Thickness(0) : new Thickness(0, 6, 20, 6);
            mainWindow.MyDialogComboBox2.Margin = (combo2 == null) ? new Thickness(0) : new Thickness(0, 6, 20, 6);
            mainWindow.MyDialogCheckbox.Margin = (checkbox == null) ? new Thickness(0) : new Thickness(0, 10, 0, 0);

            // set text and display
            if (textBox != null) { mainWindow.MyDialogTextBox.Text = textBox.ToString(); }
            if (combo1 != null) { mainWindow.MyDialogComboBox1.SelectedItem = ((string)combo1 != "") ? combo1.ToString() : null; }
            if (combo2 != null) { mainWindow.MyDialogComboBox2.SelectedItem = ((string)combo2 != "") ? combo2.ToString() : null; }
            if (checkbox != null) { mainWindow.MyDialogCheckbox.IsChecked = (bool)checkbox; }
            mainWindow.MyDialog.Tag = tag;
            mainWindow.MyDialogText.Text = text;
            mainWindow.MyDialog.IsOpen = true;
        }

        /// <summary>
        /// Dialog closed handler
        /// </summary>
        private void Dialog_Closed(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            if (MyDialog.Tag != null)
            {
                switch (MyDialog.Tag.ToString())
                {
                    case "HostAddHostingUnit": ((HostHostingUnits)HostingFrame.Content).Add_Hosting_Unit_Named(MyDialogTextBox.Text); break;
                    case "HostDeleteHostingUnit": ((HostHostingUnits)HostingFrame.Content).Confirm_Delete(MyDialogText.Text, MyDialogCheckbox.IsChecked); break;
                    case "HostUpdateHostingUnit": ((HostHostingUnits)HostingFrame.Content).Update_Hosting_Unit_Name(MyDialogText.Text, MyDialogTextBox.Text); break;
                    case "AdminUpdateBankClearance": ((AdminHosts)AdminFrame.Content).FinishUpdateBankClearance(MyDialogText.Text, MyDialogComboBox1.SelectedItem.ToString()); break;
                    case "HostCreateOrder": ((HostRequests)HostingFrame.Content).Finish_Create_Order(MyDialogText.Text, MyDialogComboBox1.SelectedItem); break;
                    case "HostUpdateOrder": ((HostOrders)HostingFrame.Content).Finish_Update_Order(MyDialogText.Text, MyDialogComboBox1.SelectedItem); break;
                    case "AdminUpdateOrder": ((AdminOrders)AdminFrame.Content).Finish_Update_Order(MyDialogText.Text, MyDialogComboBox1.SelectedItem); break;
                    default: break;
                }
            }
        }

        private void MyDialogComboBox1_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (this.MyDialog.Tag != null)
            {
                switch (MyDialog.Tag.ToString())
                {
                    case "HostAddHostingUnit": UpdateCityList(sender, HostHostingUnits.CitiesCollection); break;
                    case "HostUpdateHostingUnit": UpdateCityList(sender, HostHostingUnits.CitiesCollection); break;
                    default: break;
                }
            }
        }

        /// <summary>
        /// Select amenity listbox style based on window width
        /// for responsive experience when the window is resized
        /// </summary>
        private void ChooseAmenityListBoxStyle(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 870)
                gAmenities.Style = (Style)Application.Current.Resources["MyMaterialDesignListBox"];
            else
                gAmenities.Style = (Style)Application.Current.Resources["MyMaterialDesignFilterChipListBox"];
        }

        /// <summary>
        /// Programmatically change to a different tab in Main Tab Control
        /// Modifies the SelectedIndex attribute based on sender's tag
        /// </summary>
        private void ChangeTab(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
                MainTabControl.SelectedIndex =
                    int.Parse(((Button)sender).Tag.ToString()))
            );
        }

        /// <summary>
        /// Based on selected tab, hide borders of input controls from
        /// other tabs that should not be visible and refetch list data
        /// </summary>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { 
            // Home
            if (CurrentTab != Tab0 && Tab0.IsSelected)
            {
                CurrentTab = Tab0;
            }
            // Guest Request
            else if (CurrentTab != Tab1 && Tab1.IsSelected)
            {
                CurrentTab = Tab1;
            }
            // Hosting
            else if (CurrentTab != Tab2 && Tab2.IsSelected)
            {
                CurrentTab = Tab2;
                // refresh lists in case they have changed since last tab visit
                if (HostingFrame.Content is HostRequests hostRequests)
                    hostRequests.Refresh();
                else if (HostingFrame.Content is HostOrders hostOrders)
                    hostOrders.Refresh();
                else if (HostingFrame.Content is HostHostingUnits hostHostingUnits)
                    hostHostingUnits.Refresh();
            }
            // Admin
            else if (CurrentTab != Tab3 && Tab3.IsSelected)
            {
                CurrentTab = Tab3;
                // refresh lists in case they have changed since last tab visit
                if (AdminFrame.Content is AdminRequests adminRequests)
                    adminRequests.Refresh();
                else if (AdminFrame.Content is AdminOrders adminOrders)
                    adminOrders.Refresh();
                else if (AdminFrame.Content is AdminHostingUnits adminHostingUnits)
                    adminHostingUnits.Refresh();
                else if (AdminFrame.Content is AdminHosts adminHosts)
                    adminHosts.Refresh();
            }
        }

        /// <summary>
        /// Update City List when District List changed
        /// </summary>
        private void UpdateCityList(object sender, ObservableCollection<string> cityList)
        {
            // get selected district
            if (((ComboBox)sender).SelectedItem == null) return;
            string selectedDistrict = ((ComboBox)sender).SelectedItem.ToString();
            if (!Enum.TryParse(selectedDistrict.Replace(" ", ""), out District district)) return;
            // get list of cities in district from config
            List<string> update = Config.GetCities[district].ConvertAll(c => PascalCaseToText.Convert(c));
            // clear list
            cityList.Clear();
            // add cities in district
            foreach (string item in update)
            {
                cityList.Add(item);
            }
            // if list is empty, add item that says to select a district first
            if (cityList.Count == 0)
            {
                cityList.Add("Select district first.");
            }
        }

        private void DistrictComboBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            UpdateCityList(sender, DynamicCityList);
        }

        /// <summary>
        /// Create guest request from submitted info
        /// </summary>
        private void Submit_Request_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fname = gFirstName.Text.ToString();
                string lname = gLastName.Text.ToString();
                string email = gEmail.Text.ToString();
                DateTime.TryParse(gEntryDate.SelectedDate.ToString(), out DateTime entry);
                DateTime.TryParse(gReleaseDate.SelectedDate.ToString(), out DateTime release);
                object districtObj = gPrefDistrict.SelectedItem;
                object cityObj = gPrefCity.SelectedItem;
                int numAdults = gNumAdults.SelectedIndex + 1; // No option for 0 adults
                int numChildren = gNumChildren.SelectedIndex;
                object prefTypeObj = gPrefType.SelectedItem;
                IList selectedAmenities = gAmenities.SelectedItems;
                SerializableDictionary<Amenity, PrefLevel> amenities = new SerializableDictionary<Amenity, PrefLevel>();
                foreach (Amenity amenity in Enum.GetValues(typeof(Amenity)))
                {
                    if (selectedAmenities.IndexOf(amenity) > -1)
                        amenities[amenity] = PrefLevel.Required;
                    else
                        amenities[amenity] = PrefLevel.NotInterested;
                }

                MainWindow.Bl.ValidateGuestForm(fname, lname, email, entry.ToString(), release.ToString(), districtObj, cityObj, numAdults, numChildren, prefTypeObj);

                Enum.TryParse(gPrefDistrict.SelectedItem.ToString().Replace(" ", ""), out District district);
                Enum.TryParse(gPrefCity.SelectedItem.ToString().Replace(" ", ""), out City city);
                Enum.TryParse(gPrefType.SelectedItem.ToString().Replace(" ", ""), out TypeOfPlace prefType);

                GuestRequest guest = new GuestRequest(entry, release, fname, lname, email, district, city, prefType, numAdults, numChildren, amenities);

                MainWindow.Bl.CreateGuestRequest(guest);

                Dialog("Success! Your request has been submitted.");
            }
            catch (Exception error)
            {
                Dialog(error.Message);
            }
        }
    }
}
