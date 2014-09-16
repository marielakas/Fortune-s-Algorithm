using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Voronoi.UI
{
    public partial class StartScreen : PhoneApplicationPage
    {
        public StartScreen()
        {
            InitializeComponent();
        }

        private void NavigateToMainPageClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }
    }
}