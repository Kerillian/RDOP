using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace RDOP
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private static Config? config;
		private static readonly Regex PassphrasePattern = new Regex(@"\<\!\-\- (.+) \-\-\>", RegexOptions.Compiled);

		private static bool IsValidGamePath(string path)
		{
			return File.Exists(Path.Combine(path, "RDR2.exe")) &&
			       Directory.Exists(Path.Combine(path, "x64")) &&
			       Directory.Exists(Path.Combine(path, "x64/data"));
		}

		private async Task<bool> TryUpdatePassphrase()
		{
			if (IsValidGamePath(Rdr2Box.Text))
			{
				string ymt = Path.Combine(Rdr2Box.Text, "x64/boot_launcher_flow.ymt");

				if (File.Exists(ymt) && (await File.ReadAllLinesAsync(ymt)).Skip(1).FirstOrDefault() is {Length: > 0} comment)
				{
					if (PassphrasePattern.Match(comment) is {Groups.Count: > 1} match)
					{
						PassphraseBox.Text = match.Groups[1].Value;
						return true;
					}
				}
			}

			return false;
		}
		
		private async void OnMainWindowLoaded(object sender, RoutedEventArgs e)
		{
			if (File.Exists("config.json"))
			{
				config = JsonSerializer.Deserialize<Config>(await File.ReadAllTextAsync("config.json"));
				
				if (config?.Path is { Length: > 0 } && IsValidGamePath(config.Path))
				{
					Rdr2Box.Text = config.Path;

					if (!await TryUpdatePassphrase() && config.Passphrase is {Length: > 0})
					{
						PassphraseBox.Text = config.Passphrase;
					}
				}
			}
		}
		
		private async void OnMainWindowClosed(object? sender, EventArgs e)
		{
			await File.WriteAllTextAsync("config.json", JsonSerializer.Serialize(new Config()
			{
				Passphrase = PassphraseBox.Text,
				Path = Rdr2Box.Text
			}));
		}

		private async void Rdr2BoxDoubleClicked(object sender, MouseButtonEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog()
			{
				Multiselect = false,
				Filter = "RDR2 Executable|RDR2.exe"
			};

			if (dialog.ShowDialog() == true)
			{
				string? path = Path.GetDirectoryName(dialog.FileName);
				
				if (path is not null && IsValidGamePath(path))
				{
					Rdr2Box.Text = path;
					await TryUpdatePassphrase();
				}
				else
				{
					ResultWindow.Show("Error", "Invalid game path.", this);
				}
			}
		}

		private void RevertButtonClicked(object sender, RoutedEventArgs e)
		{
			if (IsValidGamePath(Rdr2Box.Text))
			{
				string ymt = Path.Combine(Rdr2Box.Text, "x64/boot_launcher_flow.ymt");
				string meta = Path.Combine(Rdr2Box.Text, "x64/data/startup.meta");
				
				try
				{
					if (File.Exists(ymt))
					{
						File.Delete(ymt);
					}

					if (File.Exists(meta))
					{
						File.Delete(meta);
					}
					
					ResultWindow.Show("Success", "You are now able to join public sessions.", this);
				}
				catch (Exception ex)
				{
					ResultWindow.Show("Error", "An error has occurred whilst trying to undo modifications.\n\n" + ex.Message, this);
				}
			}
			else
			{
				ResultWindow.Show("Error", "Invalid game path.", this);
			}
		}

		private async void SaveButtonClicked(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(PassphraseBox.Text))
			{
				ResultWindow.Show("Error", "No passphrase set.", this);
				return;
			}
			
			if (IsValidGamePath(Rdr2Box.Text))
			{
				string ymtPath = Path.Combine(Rdr2Box.Text, "x64/boot_launcher_flow.ymt");
				string metaPath = Path.Combine(Rdr2Box.Text, "x64/data/startup.meta");

				List<string>? ymtLines = await ResourceHelper.Read("boot_launcher_flow.ymt");
				List<string>? metaLines = await ResourceHelper.Read("startup.meta");

				if (ymtLines is { Count: > 0 } && metaLines is { Count: > 0 })
				{
					ymtLines.Insert(1, $"<!-- {PassphraseBox.Text} -->");
					metaLines.Insert(1, $"<!-- {PassphraseBox.Text} -->");

					try
					{
						await File.WriteAllLinesAsync(ymtPath, ymtLines);
						await File.WriteAllLinesAsync(metaPath, metaLines);
						
						ResultWindow.Show("Success", "Your sessions should now be private.", this);
					}
					catch (Exception ex)
					{
						ResultWindow.Show("Error", "An error has occurred whilst trying to make modifications.\n\n" + ex.Message, this);
					}
				}
			}
			else
			{
				ResultWindow.Show("Error", "Invalid game path.", this);
			}
		}
	}
}