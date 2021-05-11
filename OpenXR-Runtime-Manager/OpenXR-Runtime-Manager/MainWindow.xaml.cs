using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenXR_Runtime_Manager
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private RuntimeManager runtimeManager = new RuntimeManager();

		public MainWindow()
		{
			InitializeComponent();
			UpdateActiveRuntimeDisplay();
			UpdateAvailableRuntimeList();
		}

		private void UpdateAvailableRuntimeList()
		{
			var availableRuntimeList = runtimeManager.AvailableRuntimeNames;
			RuntimeList.Items.Clear();
			foreach (string name in availableRuntimeList)
			{
				RuntimeList.Items.Add(name);
				if (runtimeManager.HasActiveRuntime && name == runtimeManager.ActiveRuntime.Name)
					RuntimeList.SelectedItem = name;
			}
		}

		private void UpdateActiveRuntimeDisplay()
		{
			if (runtimeManager.HasActiveRuntime)
			{
				var mainRuntime = runtimeManager.ActiveRuntime;
				Debug.Print($"The system's main OpenXR runtime is {mainRuntime.Name}");

				RuntimeNameLabel.Text = mainRuntime.Name;
				ManifestPathLabel.Text = Environment.ExpandEnvironmentVariables(mainRuntime.ManifestFilePath);
				LibraryPathLabel.Text = mainRuntime.LibraryDLLPath;
				VersionLabel.Text = mainRuntime.Version.ShortName;
			}
		}

		private void ChangeSystemRuntime_OnClick(object sender, RoutedEventArgs e)
		{
			if (runtimeManager.SetRuntimeAsSystem(RuntimeList.SelectionBoxItem.ToString()))
			{
				UpdateActiveRuntimeDisplay();
			}
		}
	}
}
