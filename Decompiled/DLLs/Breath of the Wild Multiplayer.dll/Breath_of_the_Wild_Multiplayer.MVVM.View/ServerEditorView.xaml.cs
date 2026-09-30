using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Breath_of_the_Wild_Multiplayer.MVVM.ViewModel;

namespace Breath_of_the_Wild_Multiplayer.MVVM.View;

public partial class ServerEditorView : UserControl, IComponentConnector
{
	public ServerEditorView()
	{
		InitializeComponent();
	}

	private void TextBox_ValidateName(object sender, TextCompositionEventArgs e)
	{
		((ServerEditorModel)((FrameworkElement)this).DataContext).ValidateInputs(e.Text);
	}

	private void NameTB_KeyUp(object sender, KeyEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		if ((int)e.Key == 2 || (int)e.Key == 32)
		{
			((ServerEditorModel)((FrameworkElement)this).DataContext).ValidateInputs(((TextBox)((RoutedEventArgs)e).OriginalSource).Text);
		}
	}

	private void TextBox_PasteValidateName(object sender, ExecutedRoutedEventArgs e)
	{
		((ServerEditorModel)((FrameworkElement)this).DataContext).ValidateInputs(Clipboard.GetText());
	}

	private void TextBox_ValidateIP(object sender, TextCompositionEventArgs e)
	{
		((RoutedEventArgs)e).Handled = !ValidateIP(e.Text);
		((ServerEditorModel)((FrameworkElement)this).DataContext).ValidateInputs(null, e.Text);
	}

	private void IPTB_KeyUp(object sender, KeyEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		if ((int)e.Key == 2 || (int)e.Key == 32)
		{
			((ServerEditorModel)((FrameworkElement)this).DataContext).ValidateInputs(null, ((TextBox)((RoutedEventArgs)e).OriginalSource).Text);
		}
	}

	private void TextBox_PasteValidateIP(object sender, ExecutedRoutedEventArgs e)
	{
		if (e.Command == ApplicationCommands.Paste)
		{
			((RoutedEventArgs)e).Handled = !ValidateIP(Clipboard.GetText());
		}
		((ServerEditorModel)((FrameworkElement)this).DataContext).ValidateInputs(null, Clipboard.GetText());
	}

	private void TextBox_ValidatePort(object sender, TextCompositionEventArgs e)
	{
		((RoutedEventArgs)e).Handled = !ValidatePort(e.Text);
		((ServerEditorModel)((FrameworkElement)this).DataContext).ValidateInputs(null, null, e.Text);
	}

	private void PortTB_KeyUp(object sender, KeyEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		if ((int)e.Key == 2 || (int)e.Key == 32)
		{
			((ServerEditorModel)((FrameworkElement)this).DataContext).ValidateInputs(null, null, ((TextBox)((RoutedEventArgs)e).OriginalSource).Text);
		}
	}

	private void TextBox_PasteValidatePort(object sender, ExecutedRoutedEventArgs e)
	{
		if (e.Command == ApplicationCommands.Paste)
		{
			((RoutedEventArgs)e).Handled = !ValidatePort(Clipboard.GetText());
		}
		((ServerEditorModel)((FrameworkElement)this).DataContext).ValidateInputs(null, null, Clipboard.GetText());
	}

	private bool ValidateIP(string text)
	{
		return true;
	}

	private bool ValidatePort(string text)
	{
		return !new Regex("[^0-9]+").IsMatch(text);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "6.0.13.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected Obj, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected Obj, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected Obj, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected Obj, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected Obj, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected Obj, but got Unknown
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected Obj, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected Obj, but got Unknown
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Expected Obj, but got Unknown
		switch (connectionId)
		{
		case 1:
			((UIElement)(TextBox)target).PreviewTextInput += TextBox_ValidateName;
			((UIElement)(TextBox)target).AddHandler(CommandManager.PreviewExecutedEvent, (Delegate)new ExecutedRoutedEventHandler(TextBox_PasteValidateName));
			((UIElement)(TextBox)target).KeyUp += NameTB_KeyUp;
			break;
		case 2:
			((UIElement)(TextBox)target).PreviewTextInput += TextBox_ValidateIP;
			((UIElement)(TextBox)target).AddHandler(CommandManager.PreviewExecutedEvent, (Delegate)new ExecutedRoutedEventHandler(TextBox_PasteValidateIP));
			((UIElement)(TextBox)target).KeyUp += IPTB_KeyUp;
			break;
		case 3:
			((UIElement)(TextBox)target).PreviewTextInput += TextBox_ValidatePort;
			((UIElement)(TextBox)target).AddHandler(CommandManager.PreviewExecutedEvent, (Delegate)new ExecutedRoutedEventHandler(TextBox_PasteValidatePort));
			((UIElement)(TextBox)target).KeyUp += PortTB_KeyUp;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
