﻿using ClassevivaPCTO.Utils;
using Refit;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

// Il modello di elemento Pagina vuota è documentato all'indirizzo https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x410

namespace ClassevivaPCTO
{
    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();


        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {




            try
            {
                buttonLogin.Visibility = Visibility.Collapsed;
                progresslogin.Visibility = Visibility.Visible;

                var api = RestService.For<IClassevivaAPI>("https://web.spaggiari.eu/rest/v1");

                var measurement = new LoginData
                {
                    Uid = edittext_username.Text,
                    Pass = edittext_password.Password,
                };


                var user = await api.LoginAsync(measurement);


                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Login completato";
                dialog.PrimaryButtonText = "OK";
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = "Benvenuto " + user.FirstName + " " + user.LastName;

                var result = await dialog.ShowAsync();


                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(DashboardPage), null, new DrillInNavigationTransitionInfo());

            } catch(Exception ex)
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Errore";
                dialog.PrimaryButtonText = "OK";
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = "Errore durante il login. Controlla il nome utente e la password.";

                var result = await dialog.ShowAsync();


                buttonLogin.Visibility = Visibility.Visible;
                progresslogin.Visibility = Visibility.Collapsed;

            }



        }
    }
}
