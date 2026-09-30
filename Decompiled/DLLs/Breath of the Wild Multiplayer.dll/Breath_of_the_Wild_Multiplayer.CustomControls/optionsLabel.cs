using System.Windows;
using System.Windows.Controls;
using Breath_of_the_Wild_Multiplayer.Source_files;

namespace Breath_of_the_Wild_Multiplayer.CustomControls;

public class optionsLabel : Label
{
	public static readonly DependencyProperty leftButtonProperty = DependencyProperty.Register("leftButton", typeof(RelayCommand), typeof(optionsLabel));

	public static readonly DependencyProperty rightButtonProperty = DependencyProperty.Register("rightButton", typeof(RelayCommand), typeof(optionsLabel));

	public static readonly DependencyProperty movingLeftProperty = DependencyProperty.Register("movingLeft", typeof(bool), typeof(optionsLabel));

	public static readonly DependencyProperty movingRightProperty = DependencyProperty.Register("movingRight", typeof(bool), typeof(optionsLabel));

	public RelayCommand leftButton
	{
		get
		{
			return (RelayCommand)((DependencyObject)this).GetValue(leftButtonProperty);
		}
		set
		{
			((DependencyObject)this).SetValue(leftButtonProperty, (object)value);
		}
	}

	public RelayCommand rightButton
	{
		get
		{
			return (RelayCommand)((DependencyObject)this).GetValue(rightButtonProperty);
		}
		set
		{
			((DependencyObject)this).SetValue(rightButtonProperty, (object)value);
		}
	}

	public bool movingLeft
	{
		get
		{
			return (bool)((DependencyObject)this).GetValue(movingLeftProperty);
		}
		set
		{
			((DependencyObject)this).SetValue(movingLeftProperty, (object)value);
		}
	}

	public bool movingRight
	{
		get
		{
			return (bool)((DependencyObject)this).GetValue(movingRightProperty);
		}
		set
		{
			((DependencyObject)this).SetValue(movingRightProperty, (object)value);
		}
	}
}
