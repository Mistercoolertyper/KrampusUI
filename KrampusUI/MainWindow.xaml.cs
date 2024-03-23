using System.Windows.Input;
using WebView2.DevTools.Dom;
using Window = System.Windows.Window;
using System.IO;
using System.Diagnostics;
using File = System.IO.File;
using Microsoft.Win32;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace KrampusUI
{
	public partial class MainWindow : Window
	{
		private KrampusWebSocket ws;
		public bool SettingsOpen;

		public MainWindow()
		{
			InitializeComponent();
			SettingsOpen = false;
			webView.Source = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"MonacoEditor\index.html"));
			ws = new(File.ReadAllText("Data/LoginToken.txt"));
			Topmost = Globals.Settings.TopMost;
			if (Globals.Settings.AutoAttach) StartAutoAttachLoop();
			webView.Loaded += (obj, args) =>
			{
				if (File.Exists("Data/SavedContent.txt"))
				{
					SetContent(File.ReadAllText("Data/SavedContent.txt"));
				}
			};
		}

		private async Task<string> GetContent()
		{
			await using var devToolsContext = await webView.CoreWebView2.CreateDevToolsContextAsync();
			var result = await devToolsContext.EvaluateFunctionAsync<string>("() => window.editor.getValue()");
			return result;
		}

		private void SetContent(string content)
		{
			Dispatcher.Invoke(async () =>
			{
				await webView.EnsureCoreWebView2Async();
				await using var devToolsContext = await webView.CoreWebView2.CreateDevToolsContextAsync();
				var result = await devToolsContext.EvaluateFunctionAsync<string>($"() => window.editor.setValue(`{content}`)");
			});
		}

		private async void ExecuteButtonImage_MouseDown(object sender, MouseButtonEventArgs e)
		{
			ws.Execute(await GetContent());
		}

		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left && e.ButtonState != MouseButtonState.Released)
			{
				DragMove();
			}
		}

		private bool injecting = false;

		private void SetTitle(string title = "")
		{
			Dispatcher.Invoke(() =>
			{
				TitleBox.Content = title;
			});
		}

		int lastPid = 0;

		private bool WaitForRobloxWindow(Process robloxProcess)
		{
			while (robloxProcess.MainWindowTitle == "")
			{
				robloxProcess.Refresh();
				if (robloxProcess.HasExited)
				{
					return false;
				}
				Thread.Sleep(1000);
			}
			return true;
		}

		private Process? CheckForRoblox()
		{
			Process[] processes = Process.GetProcessesByName("RobloxPlayerBeta");
			if (processes.Length == 0)
			{
				return null;
			}
			return processes[0];
		}

		public void StartAutoAttachLoop()
		{
			Task.Run(() =>
			{
				while (Globals.Settings.AutoAttach)
				{
					Process? process = CheckForRoblox();
					if (process != null && process.Id != lastPid)
					{
						lastPid = process.Id;
						bool res = WaitForRobloxWindow(process);
						if (!res) continue;
						Thread.Sleep(5000);
						Inject();
					}
					Thread.Sleep(1000);
				}
			});
		}

		private void Inject()
		{
			if (injecting) return;
			Task.Run(() =>
			{
				injecting = true;
				string? krampusExe = null;
				if (!File.Exists("Data/KrampusExe.txt") || (File.Exists("Data/KrampusExe.txt") && !File.Exists(File.ReadAllText("Data/KrampusExe.txt"))))
				{
					var dialog = new OpenFileDialog();
					dialog.Filter = "Executable Files (.exe)|*.exe";
					dialog.Title = "Select Krampus EXE";
					var result = dialog.ShowDialog().Value;
					if (result)
					{
						krampusExe = dialog.FileName;
						File.WriteAllText("Data/KrampusExe.txt", dialog.FileName);
					} else
					{
						return;
					}
				}
				if (krampusExe == null)
				{
					krampusExe = File.ReadAllText("Data/KrampusExe.txt");
				}
				if (CheckForRoblox() == null)
				{
					Task.Run(() =>
					{
						SetTitle("Krampus - Failed to find Roblox");
						Thread.Sleep(5000);
						SetTitle("Krampus");
					});
					return;
				}
				SetTitle("Krampus - Injecting");
				Process process = new()
				{
					StartInfo = new()
					{
						FileName = krampusExe,
						RedirectStandardOutput = true,
						UseShellExecute = false,
						CreateNoWindow = true,
						WindowStyle = ProcessWindowStyle.Hidden,
						WorkingDirectory = new FileInfo(krampusExe).DirectoryName
					}
				};
				process.Start();
				string stdout = process.StandardOutput.ReadToEnd();
				if (stdout.Contains("Success!"))
				{
					SetTitle("Krampus - Injected");
					Thread.Sleep(5000);
					SetTitle("Krampus");
				}
                injecting = false;
            });
		}

		private void InjectButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Inject();
		}

		private void SettingsButtonImage_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (SettingsOpen) return;
			SettingsOpen = true;
			new SettingsWindow(this).Show();
		}

		private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			File.WriteAllText("Data/SavedContent.txt", await GetContent());
			Environment.Exit(0);
		}
	}
}
