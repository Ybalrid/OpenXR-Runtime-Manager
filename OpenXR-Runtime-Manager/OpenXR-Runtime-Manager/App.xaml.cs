using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Media;

namespace OpenXR_Runtime_Manager
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			ApplyThemeFromWindows();

			// Optional but nice: react if user changes theme while app is running
			SystemEvents.UserPreferenceChanged += (_, __) => ApplyThemeFromWindows();
		}

		private void ApplyThemeFromWindows()
		{
			bool dark = IsWindowsDarkMode();
			ApplyTheme(dark);
		}

		private void ApplyTheme(bool dark)
		{
			if (dark)
			{
				SetBrush("AppBg", ColorFrom(0x20, 0x20, 0x20));
				SetBrush("AppPanelBg", ColorFrom(0x2A, 0x2A, 0x2A));
				SetBrush("AppFg", ColorFrom(0xF0, 0xF0, 0xF0));
				SetBrush("AppBorder", ColorFrom(0x4A, 0x4A, 0x4A));
				SetBrush("AppAccent", ColorFrom(0x5A, 0xA0, 0xFF));
				SetBrush("AppSelectionBg", ColorFrom(0x3A, 0x6F, 0xB0));

				// WPF system brushes used by default templates
				SetSysBrush(SystemColors.WindowBrushKey, ColorFrom(0x20, 0x20, 0x20));
				SetSysBrush(SystemColors.WindowTextBrushKey, ColorFrom(0xF0, 0xF0, 0xF0));
				SetSysBrush(SystemColors.ControlBrushKey, ColorFrom(0x2A, 0x2A, 0x2A));
				SetSysBrush(SystemColors.ControlTextBrushKey, ColorFrom(0xF0, 0xF0, 0xF0));
				SetSysBrush(SystemColors.HighlightBrushKey, ColorFrom(0x5A, 0xA0, 0xFF));
				SetSysBrush(SystemColors.HighlightTextBrushKey, ColorFrom(0x00, 0x00, 0x00));
			}
			else
			{
				// LIGHT MODE: do NOT leave any dark brush behind
				SetBrush("AppBg", Colors.White);
				SetBrush("AppPanelBg", ColorFrom(0xF3, 0xF3, 0xF3));
				SetBrush("AppFg", Colors.Black);
				SetBrush("AppBorder", ColorFrom(0xB8, 0xB8, 0xB8));
				SetBrush("AppAccent", ColorFrom(0x00, 0x78, 0xD4));   // Win-ish blue
				SetBrush("AppSelectionBg", ColorFrom(0xC7, 0xE0, 0xF4));   // pale selection

				SetSysBrush(SystemColors.WindowBrushKey, Colors.White);
				SetSysBrush(SystemColors.WindowTextBrushKey, Colors.Black);
				SetSysBrush(SystemColors.ControlBrushKey, ColorFrom(0xF0, 0xF0, 0xF0));
				SetSysBrush(SystemColors.ControlTextBrushKey, Colors.Black);
				SetSysBrush(SystemColors.HighlightBrushKey, ColorFrom(0x00, 0x78, 0xD4));
				SetSysBrush(SystemColors.HighlightTextBrushKey, Colors.White);
			}
		}

		private void SetBrush(string key, Color c)
			=> Resources[key] = new SolidColorBrush(c);

		private void SetSysBrush(ResourceKey key, Color c)
			=> Resources[key] = new SolidColorBrush(c);

		private static Color ColorFrom(byte r, byte g, byte b)
			=> Color.FromRgb(r, g, b);


		private static bool IsWindowsDarkMode()
		{
			using (var key = Registry.CurrentUser.OpenSubKey(
				@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
			{
				// AppsUseLightTheme: 0 = dark, 1 = light
				object value = key?.GetValue("AppsUseLightTheme");
				return value is int v && v == 0;
			}
		}
	}
}
