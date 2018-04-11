using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MyCircle
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DetailsPage : ContentPage
	{
		public DetailsPage ()
		{
			InitializeComponent ();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			if (Device.RuntimePlatform == Device.UWP)
			{
				messageEntry.Focus();
			}
		}
	}
}