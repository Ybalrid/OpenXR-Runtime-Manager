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

			if(runtimeManager.HasActiveRuntime)
			{
				var mainRuntime = runtimeManager.ActiveRuntime;
				Debug.Print($"The system's main OpenXR runtime is {mainRuntime.Name}");
			}
		}

		private void TestMe_Click(object sender, RoutedEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}
