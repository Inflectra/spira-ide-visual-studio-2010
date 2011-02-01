using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Controls
{

	class NumericTextBox : TextBox
	{
		/// <summary>Hit when the control has keyboard focus.</summary>
		/// <param name="e">KeyboardFocusChangedEventArgs</param>
		protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			//Check to make sure that no non-numeric values were entered.
			if (this.Text.Length > 0)
			{
				if (!this.Text.All<char>(char.IsNumber))
				{
					this.Text = "";
				}
				else
				{
					//Highlight the full number..
					this.CaretIndex = 0;
					this.SelectAll();
				}
			}

			base.OnGotKeyboardFocus(e);
		}

		/// <summary>Hit when the user enters a digit.</summary>
		/// <param name="e">KeyEventArgs</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if ((e.Key >= Key.D0 && e.Key <= Key.D9) ||									//Numeric Keys
				(e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||						//Number Pad Keys
				(e.Key == Key.Up || e.Key == Key.Left) ||								//Left/Right keys.
				(e.Key == Key.Delete || e.Key == Key.Insert || e.Key == Key.Back) ||	//Delete, Inisert, Backspace
				(e.Key == Key.Home || e.Key == Key.End) ||								// Home / End
				(e.Key == Key.Tab || e.Key == Key.Enter))								// Tab / Enter
			{
				//Valid key, let them enter it.
				base.OnKeyDown(e);
			}
			else
			{
				//Invalid key, ignore it.
				e.Handled = true;
			}
		}

		/// <summary>Hit when the text changes.</summary>
		/// <param name="e">TextCompositionEventArgs</param>
		protected override void OnPreviewTextInput(TextCompositionEventArgs e)
		{
			//Verify the new text is still a number.
			string origText = ((NumericTextBox)e.OriginalSource).Text;

			if (!e.Text.All<char>(char.IsNumber))
			{
				this.Text = origText;
				e.Handled = true;
			}
			else
			{
				base.OnPreviewTextInput(e);
			}
		}

		/// <summary>Hit when the user leaves the testbox.</summary>
		/// <param name="e">RoutedEventArgs</param>
		protected override void OnLostFocus(RoutedEventArgs e)
		{
			//Transform he entered number into a real number..
			if (!string.IsNullOrWhiteSpace(this.Text) && this.Text.All<char>(char.IsNumber))
			{
				this.Text = int.Parse(this.Text).ToString();
			}
			else
			{
				this.Text = "";
			}

			base.OnLostFocus(e);
		}
	}
}