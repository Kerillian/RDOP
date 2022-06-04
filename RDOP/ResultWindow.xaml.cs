using System.Windows;

namespace RDOP
{
	public partial class ResultWindow : Window
	{
		public ResultWindow()
		{
			InitializeComponent();
		}

		private void OkButtonClicked(object sender, RoutedEventArgs e)
		{
			Close();
		}

		public static void Show(string title, string message, Window? owner = null)
		{
			ResultWindow win = new ResultWindow
			{
				Title = title,
				MainTextBlock =
				{
					Text = message
				},
				Owner = owner
			};
			
			win.Show();
		}
	}
}