﻿using ClassevivaPCTO.Converters;
using ClassevivaPCTO.Services;
using ClassevivaPCTO.Utils;
using ClassevivaPCTO.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ClassevivaPCTO.Views
{
    public sealed partial class DashboardPage : Page
    {
        private readonly IClassevivaAPI apiClient;

        private readonly IClassevivaAPI apiWrapper;

        public DashboardPageViewModel DashboardPageViewModel { get; } =
            new DashboardPageViewModel();

        public DashboardPage()
        {
            this.InitializeComponent();

            App app = (App)App.Current;
            apiClient = app.Container.GetService<IClassevivaAPI>();

            apiWrapper = PoliciesDispatchProxy<IClassevivaAPI>.CreateProxy(apiClient);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Card cardResult = ViewModelHolder.getViewModel().CardsResult.Cards[0];

            TextBenvenuto.Text =
                "Dashboard di " + VariousUtils.UppercaseFirst(cardResult.firstName);

            /*

            JsonConvert.DefaultSettings =
                       () => new JsonSerializerSettings()
                       {
                           Converters = { new CustomIntConverter() }
                       };

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new CustomIntConverter());

            */

            await Task.Run(async () =>
            {
                await LoadAgendaCard();
            });

            //run in a background thread otherwise the UI thread gets stuck when displaying a dialog
            await Task.Run(async () =>
            {
                await CaricaMediaCard();
            });
        }

        public async Task LoadAgendaCard()
        {
            LoginResult loginResult = ViewModelHolder.getViewModel().LoginResult;
            Card cardResult = ViewModelHolder.getViewModel().CardsResult.Cards[0];

            var result1 = await apiWrapper
                .GetGrades(cardResult.usrId.ToString(), loginResult.Token.ToString())
                .ConfigureAwait(false);

            var fiveMostRecent = result1.Grades.OrderByDescending(x => x.evtDate).Take(5);

            //update UI on UI thread
            await CoreApplication.MainView.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                async () =>
                {
                    ListRecentGrades.ItemsSource = fiveMostRecent;

                    //todoo
                    ListViewAbsencesDate.ItemsSource = result1.Grades;
                    ListViewVotiDate.ItemsSource = result1.Grades;
                    ListViewAgendaDate.ItemsSource = result1.Grades;
                }
            );

        }

        public async Task CaricaMediaCard()
        {
            DashboardPageViewModel.IsLoadingMedia = true;

            LoginResult loginResult = ViewModelHolder.getViewModel().LoginResult;
            Card cardResult = ViewModelHolder.getViewModel().CardsResult.Cards[0];

            var result1 = await apiWrapper
                .GetGrades(cardResult.usrId.ToString(), "asd")
                .ConfigureAwait(false);

            // Calcoliamo la media dei voti
            float media = VariousUtils.CalcolaMedia(result1.Grades);

            TextBlockMedia.Foreground = (Brush)
                new GradeToColorConverter().Convert(media, null, null, null);

            // Stampiamo la media dei voti
            TextBlockMedia.Text = media.ToString("0.00");
            TextBlockMedia.Visibility = Visibility.Visible;

            DashboardPageViewModel.IsLoadingMedia = false;
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            //NavigationService.Navigate(typeof(Views.DettaglioVoti), null);
            NavigationService.Navigate(typeof(Views.DettaglioVoti));
        }

        private async void HyperlinkButton_Click_1(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(Views.Agenda));
        }
    }
}
