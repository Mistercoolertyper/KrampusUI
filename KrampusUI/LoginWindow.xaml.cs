using Newtonsoft.Json;
using System.IO;
using System.Windows;

namespace KrampusUI
{
	public partial class LoginWindow : Window
	{
		public LoginWindow()
		{
			InitializeComponent();
			if (!Directory.Exists("Data"))
			{
				Directory.CreateDirectory("Data");
			}

			if (!File.Exists("Data/Settings.json"))
			{
				File.WriteAllText("Data/Settings.json", JsonConvert.SerializeObject(new
				{
					TopMost = true,
					AutoAttach = true,
					AutoLaunch = false
				}));
			}

			Settings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("Data/Settings.json"));
			Globals.Settings = settings;

			if (File.Exists("Data/LoginToken.txt"))
			{
				MainWindow main = new MainWindow();
				main.Show();
				Visibility = Visibility.Hidden;
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			File.WriteAllText("Data/LoginToken.txt", LoginTokenBox.Text);
			MainWindow main = new MainWindow();
			main.Show();
			Visibility = Visibility.Hidden;
		}
	}
}
