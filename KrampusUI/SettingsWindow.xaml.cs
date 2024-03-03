using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace KrampusUI
{
	public partial class SettingsWindow : Window
	{
		private MainWindow _main;

		public SettingsWindow(MainWindow main)
		{
			InitializeComponent();
			_main = main;
			TopMostBox.IsChecked = Globals.Settings.TopMost;
			AutoAttachBox.IsChecked = Globals.Settings.AutoAttach;
		}

		private void SaveSettings()
		{
			File.WriteAllText("Data/Settings.json", JsonConvert.SerializeObject(Globals.Settings));
		}

		private void TopMostBox_Click(object sender, RoutedEventArgs e)
		{
			Globals.Settings.TopMost = TopMostBox.IsChecked.Value;
			SaveSettings();
			_main.Topmost = TopMostBox.IsChecked.Value;
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			_main.SettingsOpen = false;
			Close();
		}

		private void AutoAttachBox_Click(object sender, RoutedEventArgs e)
		{
			Globals.Settings.AutoAttach = AutoAttachBox.IsChecked.Value;
			SaveSettings();
			if (AutoAttachBox.IsChecked.Value)
			{
				_main.StartAutoAttachLoop();
			}
		}
	}
}
