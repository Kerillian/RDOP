using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace RDOP
{
	internal class WatermarkAdorner : Adorner
	{
		private readonly ContentPresenter contentPresenter;

		public WatermarkAdorner(UIElement adornedElement, object watermark) : base(adornedElement)
		{
			IsHitTestVisible = false;
			Control control = (Control)adornedElement;
			
			contentPresenter = new ContentPresenter
			{
				Content = watermark,
				Opacity = 0.5,
				Margin = new Thickness(control.Padding.Left, control.Padding.Top + 1, control.Padding.Right, control.Padding.Bottom)
			};

			if (control is ItemsControl and not ComboBox)
			{
				contentPresenter.VerticalAlignment = VerticalAlignment.Center;
				contentPresenter.HorizontalAlignment = HorizontalAlignment.Center;
			}

			Binding binding = new Binding("IsVisible")
			{
				Source = adornedElement,
				Converter = new BooleanToVisibilityConverter()
			};
			
			SetBinding(VisibilityProperty, binding);
		}

		protected override int VisualChildrenCount => 1;

		protected override Visual GetVisualChild(int index)
		{
			return contentPresenter;
		}

		protected override Size MeasureOverride(Size constraint)
		{
			Control control = (Control)AdornedElement;
			
			contentPresenter.Measure(control.RenderSize);
			return control.RenderSize;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			contentPresenter.Arrange(new Rect(finalSize));
			return finalSize;
		}
	}
}