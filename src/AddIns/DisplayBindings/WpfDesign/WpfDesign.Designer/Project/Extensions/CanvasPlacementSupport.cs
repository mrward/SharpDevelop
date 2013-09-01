﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using ICSharpCode.WpfDesign.Designer.Controls;
using ICSharpCode.WpfDesign.Extensions;

namespace ICSharpCode.WpfDesign.Designer.Extensions
{
	/// <summary>
	/// Provides <see cref="IPlacementBehavior"/> behavior for <see cref="Canvas"/>.
	/// </summary>
	[ExtensionFor(typeof(Canvas), OverrideExtension=typeof(DefaultPlacementBehavior))]
	public sealed class CanvasPlacementSupport : SnaplinePlacementBehavior
	{
		GrayOutDesignerExceptActiveArea grayOut;
		
		static double GetCanvasProperty(UIElement element, DependencyProperty d)
		{
			double v = (double)element.GetValue(d);
			if (double.IsNaN(v))
				return 0;
			else
				return v;
		}
		
		static bool IsPropertySet(UIElement element, DependencyProperty d)
		{
			return element.ReadLocalValue(d) != DependencyProperty.UnsetValue;
		}
		
		public override void SetPosition(PlacementInformation info)
		{
			base.SetPosition(info);
			info.Item.Properties[FrameworkElement.MarginProperty].Reset();

			UIElement child = info.Item.View;
			Rect newPosition = info.Bounds;

			if (IsPropertySet(child, Canvas.RightProperty))
			{
				var newR = ((Canvas) ExtendedItem.Component).ActualWidth - newPosition.Right;
				if (newR != GetCanvasProperty(child, Canvas.RightProperty))
					info.Item.Properties.GetAttachedProperty(Canvas.RightProperty).SetValue(newR);
			}
			else if (newPosition.Left != GetCanvasProperty(child, Canvas.LeftProperty))
			{
				info.Item.Properties.GetAttachedProperty(Canvas.LeftProperty).SetValue(newPosition.Left);
			}


			if (IsPropertySet(child, Canvas.BottomProperty))
			{
				var newB = ((Canvas)ExtendedItem.Component).ActualHeight - newPosition.Bottom;
				if (newB != GetCanvasProperty(child, Canvas.BottomProperty))
					info.Item.Properties.GetAttachedProperty(Canvas.BottomProperty).SetValue(newB);
			}
			else if (newPosition.Top != GetCanvasProperty(child, Canvas.TopProperty))
			{
				info.Item.Properties.GetAttachedProperty(Canvas.TopProperty).SetValue(newPosition.Top);
			}

			if (info.Item == Services.Selection.PrimarySelection)
			{
				var cv = this.ExtendedItem.View as Canvas;
				var b = new Rect(0, 0, cv.ActualWidth, cv.ActualHeight);
				// only for primary selection:
				if (grayOut != null)
				{
					grayOut.AnimateActiveAreaRectTo(b);
				}
				else
				{
					GrayOutDesignerExceptActiveArea.Start(ref grayOut, this.Services, this.ExtendedItem.View, b);
				}
			}
		}
		
		public override void LeaveContainer(PlacementOperation operation)
		{
			GrayOutDesignerExceptActiveArea.Stop(ref grayOut);
			base.LeaveContainer(operation);
			foreach (PlacementInformation info in operation.PlacedItems) {
				info.Item.Properties.GetAttachedProperty(Canvas.LeftProperty).Reset();
				info.Item.Properties.GetAttachedProperty(Canvas.TopProperty).Reset();
			}
		}
		
		public override void EnterContainer(PlacementOperation operation)
		{
			base.EnterContainer(operation);
			foreach (PlacementInformation info in operation.PlacedItems) {
				info.Item.Properties[FrameworkElement.HorizontalAlignmentProperty].Reset();
				info.Item.Properties[FrameworkElement.VerticalAlignmentProperty].Reset();
				info.Item.Properties[FrameworkElement.MarginProperty].Reset();
			}
		}

		public override void EndPlacement(PlacementOperation operation)
		{
			GrayOutDesignerExceptActiveArea.Stop(ref grayOut);
			base.EndPlacement(operation);
		}
	}
}
