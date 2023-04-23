﻿using ClassevivaPCTO.Adapters;
using ClassevivaPCTO.Utils;
using ClassevivaPCTO.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Il modello di elemento Pagina vuota è documentato all'indirizzo https://go.microsoft.com/fwlink/?LinkId=234238

namespace ClassevivaPCTO.Views
{
    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class Agenda : Page
    {
        public AgendaViewModel AgendaViewModel { get; } = new AgendaViewModel();

        private readonly IClassevivaAPI apiWrapper;

        public Agenda()
        {
            this.InitializeComponent();

            App app = (App)App.Current;
            var apiClient = app.Container.GetService<IClassevivaAPI>();

            apiWrapper = PoliciesDispatchProxy<IClassevivaAPI>.CreateProxy(apiClient);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            AgendaViewModel.IsLoadingAgenda = true;


            //listender for calendaragenda date change
            CalendarAgenda.DateChanged += CalendarAgenda_DateChanged;

            //imposto la data di oggi del picker
            CalendarAgenda.Date = DateTime.Now;

            LoginResultComplete loginResult = ViewModelHolder.getViewModel().LoginResult;
            Card cardResult = ViewModelHolder.getViewModel().CardsResult.Cards[0];

            var api = RestService.For<IClassevivaAPI>(Endpoint.CurrentEndpoint);

            //string fixedId = new CvUtils().GetCode(loginResult.Ident);

            string caldate = VariousUtils.ToApiDateTime(CalendarAgenda.Date.Value.DateTime);

            OverviewResult overviewResult = await api.GetOverview(
                cardResult.usrId.ToString(),
                caldate,
                caldate,
                loginResult.token.ToString()
            );

            AgendaViewModel.IsLoadingAgenda = false;


        }

        private async void CalendarAgenda_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            //if the date is today, then the button to go to today is disabled
            if (CalendarAgenda.Date.Value.Date == DateTime.Now.Date)
            {
                ButtonToday.IsChecked = true;
                //ButtonToday.IsEnabled = false;
            }
            else
            {
                ButtonToday.IsChecked = false;
                //ButtonToday.IsEnabled = true;
            }

            string apiDate = VariousUtils.ToApiDateTime(CalendarAgenda.Date.Value.Date);

            await Task.Run(async () =>
            {
                await LoadData(apiDate);
            });

        }


        private async Task LoadData(string apiDateToLoad)
        {

            LoginResultComplete loginResult = ViewModelHolder.getViewModel().LoginResult;
            Card cardResult = ViewModelHolder.getViewModel().CardsResult.Cards[0];

            
            OverviewResult overviewResult = await apiWrapper.GetOverview(
                cardResult.usrId.ToString(),
                apiDateToLoad,
                apiDateToLoad,
                loginResult.token.ToString()
            );

            //update UI on UI thread
            await CoreApplication.MainView.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                async () =>
                {
                    //ListViewAbsencesDate.ItemsSource = overviewResult.Grades;
                    ListViewVotiDate.ItemsSource = overviewResult.Grades;

                    //order lessons by evtHPos
                    var orderedlessons = overviewResult.Lessons.OrderBy(x => x.evtHPos).ToList();

                    //remove duplicates based on lessonArg and authorname and increment evtDuration it it is a duplicate
                    foreach (var lesson in orderedlessons.ToList())
                    {
                        var duplicates = orderedlessons
                            .Where(
                                x =>
                                    x.lessonArg == lesson.lessonArg
                                    && x.authorName == lesson.authorName
                            )
                            .ToList();
                        if (duplicates.Count > 1)
                        {
                            lesson.evtDuration += duplicates[1].evtDuration;
                            orderedlessons.Remove(duplicates[1]);
                        }
                    }

                    //orderedlessons = orderedlessons.GroupBy(x => x.lessonArg).Select(x => x.First()).ToList();

                    ListViewLezioniDate.ItemsSource = orderedlessons
                        .Select(les => new LessonAdapter(les))
                        .ToList();

                    // Wrap each AgendaEvent object in an instance of AgendaEventAdapter and handle null case
                    var eventAdapters = overviewResult.AgendaEvents
                        ?.Select(evt => new AgendaEventAdapter(evt))
                        .ToList();

                    ListViewAgendaDate.ItemsSource = eventAdapters;

                }
            );
        }

        private void ButtonToday_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            CalendarAgenda.Date = DateTime.Now;
        }

        private void ButtonYesterday_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            CalendarAgenda.Date = CalendarAgenda.Date.Value.AddDays(-1);
        }

        private void ButtonTomorrow_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //add one day to the calendaragenda date
            CalendarAgenda.Date = CalendarAgenda.Date.Value.AddDays(1);
        }
    }
}
