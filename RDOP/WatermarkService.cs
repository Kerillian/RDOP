using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

namespace RDOP
{
	public static class WatermarkService
	{
		public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached(
			"Watermark",
			typeof(object),
			typeof(WatermarkService),
			new FrameworkPropertyMetadata(null, OnWatermarkChanged)
		);
		
		public static object GetWatamark(DependencyObject d)
		{
			return d.GetValue(WatermarkProperty);
		}

		public static void SetWatermark(DependencyObject d, object value)
		{
			d.SetValue(WatermarkProperty, value);
		}

		private static void RemoveWatermark(UIElement control)
		{
			if (AdornerLayer.GetAdornerLayer(control) is { } layer)
			{
				if (layer.GetAdorners(control) is { Length: > 0 } adorners)
				{
					foreach (Adorner adorner in adorners)
					{
						if (adorner is WatermarkAdorner)
						{
							adorner.Visibility = Visibility.Hidden;
							layer.Remove(adorner);
						}
					}
				}
			}
		}

		private static void ShowWatermark(Control control)
		{
			if (AdornerLayer.GetAdornerLayer(control) is { } layer)
			{
				layer.Add(new WatermarkAdorner(control, GetWatamark(control)));
			}
		}

		private static bool ShouldShowWatermark(Control control)
		{
			return control switch
			{
				ComboBox comboBox => comboBox.Text == string.Empty,
				TextBox textBox   => textBox.Text == string.Empty && !textBox.IsKeyboardFocused,
				_                 => false
			};
		}

		private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Control control = (Control)d;
			control.Loaded += ControlOnLoaded;

			switch (d)
			{
				case ComboBox:
					control.GotKeyboardFocus += ControlOnGotKeyboardFocus;
					control.LostKeyboardFocus += ControlOnLoaded;
					break;
				case TextBox:
					control.GotKeyboardFocus += ControlOnGotKeyboardFocus;
					control.LostKeyboardFocus += ControlOnLoaded;
					((TextBox)control).TextChanged += ControlOnGotKeyboardFocus;
					break;
			}
		}

		private static void ControlOnGotKeyboardFocus(object sender, RoutedEventArgs e)
		{
			Control c = (Control)sender;

			if (ShouldShowWatermark(c))
			{
				ShowWatermark(c);
			}
			else
			{
				RemoveWatermark(c);
			}
		}

		private static void ControlOnLoaded(object sender, RoutedEventArgs e)
		{
			Control c = (Control)sender;

			if (ShouldShowWatermark(c))
			{
				ShowWatermark(c);
			}
		}
	}
}