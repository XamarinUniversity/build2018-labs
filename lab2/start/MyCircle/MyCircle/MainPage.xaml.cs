using MyCircle.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MyCircle
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
        }

        private async void OnMessageSelected(object sender, ItemTappedEventArgs e)
        {
            var item = e.Item as CircleMessageViewModel;
            await Navigation.PushAsync(new DetailsPage() { BindingContext = new DetailsViewModel(item) });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            BindingContext = new MainViewModel();

            if (Device.RuntimePlatform == Device.UWP
                || Device.RuntimePlatform == Device.WPF 
                || Device.RuntimePlatform == Device.macOS)
            {
                // On desktop apps, shift focus to the entry.
                // Don't do this on mobile devices as onscreen keyboard obscures data.
                messageEntry.Focus();
            }
        }
    }
}
