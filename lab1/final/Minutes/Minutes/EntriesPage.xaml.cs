using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Minutes
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EntriesPage : ContentPage
	{
		public EntriesPage ()
		{
			InitializeComponent ();

			entries.ItemTapped += OnItemTapped;
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			entries.ItemsSource = await App.Entries.GetAll();
		}

		private async void OnItemTapped(object sender, ItemTappedEventArgs e)
		{
			var item = e.Item as NoteEntry;
			await Navigation.PushAsync(new NoteEntryEditPage(item));
		}

		private async void OnAddEntry(object sender, EventArgs e)
		{
			string text = newEntry.Text;
			if (!string.IsNullOrWhiteSpace(text))
			{
				var item = new NoteEntry { Title = text };
				await Navigation.PushAsync(new NoteEntryEditPage(item));

				await App.Entries.AddAsync (item);
				newEntry.Text = string.Empty;
			}
		}
	}
}