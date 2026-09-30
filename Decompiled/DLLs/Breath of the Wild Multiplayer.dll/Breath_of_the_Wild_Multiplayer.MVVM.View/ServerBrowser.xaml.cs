using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Breath_of_the_Wild_Multiplayer.MVVM.ViewModel;

namespace Breath_of_the_Wild_Multiplayer.MVVM.View;

public partial class ServerBrowser : UserControl, IComponentConnector, IStyleConnector
{
	internal ItemsControl serverList;

	public ServerBrowser()
	{
		InitializeComponent();
	}

	private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected Obj, but got Unknown
		ScrollViewer val = (ScrollViewer)sender;
		if (val.VerticalOffset == 0.0 && val.VerticalOffset == val.ScrollableHeight)
		{
			((FrameworkElement)val).Tag = 0;
		}
		else if (val.VerticalOffset == 0.0 && val.VerticalOffset != val.ScrollableHeight)
		{
			((FrameworkElement)val).Tag = 1;
		}
		else if (val.VerticalOffset != 0.0 && val.VerticalOffset != val.ScrollableHeight)
		{
			((FrameworkElement)val).Tag = 2;
		}
		else if (val.VerticalOffset != 0.0 && val.VerticalOffset == val.ScrollableHeight)
		{
			((FrameworkElement)val).Tag = 3;
		}
	}

	private void EditClick(object sender, RoutedEventArgs e)
	{
		((ServerBrowserModel)((FrameworkElement)this).DataContext).EditServer();
	}

	private void RemoveClick(object sender, RoutedEventArgs e)
	{
		((ServerBrowserModel)((FrameworkElement)this).DataContext).RemoveServer();
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "6.0.13.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected Obj, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected Obj, but got Unknown
		switch (connectionId)
		{
		case 1:
			((ScrollViewer)target).ScrollChanged += ScrollViewer_ScrollChanged;
			break;
		case 2:
			serverList = (ItemsControl)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "6.0.13.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IStyleConnector.Connect(int connectionId, object target)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected Obj, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected Obj, but got Unknown
		switch (connectionId)
		{
		case 3:
			((MenuItem)target).Click += EditClick;
			break;
		case 4:
			((MenuItem)target).Click += RemoveClick;
			break;
		}
	}
}
