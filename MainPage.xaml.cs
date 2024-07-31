using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ElectricityCostCalculatorUWP
{
    public class Item
    {
        public string ItemSpan { get; set; }
        public string ItemKWH { get; set; }
        public string ItemCost { get; set; }
    }

    public sealed partial class MainPage : Page
    {
        public ObservableCollection<Item> ResultList { get; set; } = new ObservableCollection<Item>();
        private static readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
        private static readonly string[] span = { "", resourceLoader.GetString("timeDay"), resourceLoader.GetString("timeWeek"), resourceLoader.GetString("timeMonth"), resourceLoader.GetString("timeYear"), resourceLoader.GetString("time5Years"), resourceLoader.GetString("time10Years") };
        private static decimal[] days = { 0, 1, 7, 30, 365, 1825, 3650 };
        private readonly string dot = resourceLoader.GetString("dot");

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(500, 380));
            ApplicationView.PreferredLaunchViewSize = new Size(500, 380);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
#if DEBUG
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en";
#endif
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Application.Current.Suspending += OnSuspending;
        }

        private void Calc()
        {
            ResultList.Clear();

            decimal ParseDecimal(TextBox textBox, string dot)
            {
                return textBox.Text != string.Empty && textBox.Text != dot ? decimal.Parse(textBox.Text) : 0;
            }

            decimal hour = ParseDecimal(TextBox_Hour, dot);
            decimal watt = ParseDecimal(TextBox_Watt, dot);
            decimal cost = ParseDecimal(TextBox_Cost, dot);
            decimal time = ParseDecimal(TextBox_Time, dot);

            switch (ComboBox_Time.SelectedIndex)
            {
                case 1:
                    time *= 7;
                    break;
                case 2:
                    time *= 30;
                    break;
                case 3:
                    time *= 365;
                    break;
            }

            days[0] = time;
            watt = hour * watt / 1000;
            cost *= watt / 100;

            for (int i = 0; i < span.Length; i++)
            {
                ResultList.Add(new Item
                {
                    ItemSpan = span[i],
                    ItemKWH = $"{days[i] * watt} kWh",
                    ItemCost = $"{days[i] * cost:C}"
                });
            }
        }

        private void TextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c) && c != dot[0]);
            if (args.NewText.Where(c => c == dot[0]).Count() > 1)
                args.Cancel = true;
        }

        private void Textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Calc();
        }

        private void ComboBox_Time_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Calc();
        }

        private async void AppBarButton_Reset_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = resourceLoader.GetString("ContentDialog_ResetTitle"),
                Content = resourceLoader.GetString("ContentDialog_ResetContent"),
                PrimaryButtonText = "OK",
                CloseButtonText = resourceLoader.GetString("ContentDialog_ResetClose")
            };

            ContentDialogResult cdResult = await contentDialog.ShowAsync();
            if (cdResult == ContentDialogResult.Primary)
            {
                await ApplicationData.Current.ClearAsync();
                Frame.Navigate(this.GetType());
            }
        }

        private void AppBarButton_Feedback_Click(object sender, RoutedEventArgs e)
        {
            _ = Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9N3H7CGRR7LJ"));
        }

        private void AppBarButton_GitHub_Click(object sender, RoutedEventArgs e)
        {
            _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/wagneradrian/Electricity-Cost-Calculator-Ultimate"));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            TextBox_Hour.Text = localSettings.Values["Hour"] as string ?? "8";
            TextBox_Watt.Text = localSettings.Values["Watt"] as string ?? "500";
            TextBox_Time.Text = localSettings.Values["Time"] as string ?? "5";
            TextBox_Cost.Text = localSettings.Values["Cost"] as string ?? "28";
            ComboBox_Time.SelectedIndex = int.Parse(localSettings.Values["Combo"] as string ?? "0");

            Calc();
        }

        private void OnSuspending(Object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["Hour"] = TextBox_Hour.Text;
            localSettings.Values["Watt"] = TextBox_Watt.Text;
            localSettings.Values["Time"] = TextBox_Time.Text;
            localSettings.Values["Cost"] = TextBox_Cost.Text;
            localSettings.Values["Combo"] = ComboBox_Time.SelectedIndex.ToString();
            deferral.Complete();
        }
    }
}
