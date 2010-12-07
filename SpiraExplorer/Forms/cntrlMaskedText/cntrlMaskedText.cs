using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Controls
{

	class cntrlMaskedTextBox : TextBox
	{
		#region Private Vars
		private MaskedTextProvider _mprovider;
		private bool _InsertIsON;
		#endregion

		#region Public Properties
		/// <summary>Whether or not the user can leave the control with invalid text in it.</summary>
		public bool StayInFocusUntilValid
		{
			get;
			set;
		}

		/// <summary>Whether or not user-entered text is proper for the mask.</summary>
		public bool NewTextIsOk
		{
			get;
			set;
		}

		/// <summary>Whether or not to ignore spaces or not.</summary>
		public bool IgnoreSpace
		{
			get;
			set;
		}

		/// <summary>The mask to display for input.</summary>
		public string Mask
		{
			get
			{
				if (_mprovider != null)
					return _mprovider.Mask;
				else
					return "";
			}
			set
			{
				_mprovider = new MaskedTextProvider(value);
				this.Text = _mprovider.ToDisplayString();
			}
		}
		#endregion

		#region Control Events
		/// <summary>Hit when a key is pressed.</summary>
		/// <param name="e">KeyEventArgs</param>
		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			if (this.SelectionLength > 1)
			{
				this.SelectionLength = 0;
				e.Handled = true;
			}
			if (e.Key == Key.Insert ||
			    e.Key == Key.Delete ||
			    e.Key == Key.Back ||
			   (e.Key == Key.Space && this.IgnoreSpace))
			{
				e.Handled = true;
			}
			base.OnPreviewKeyDown(e);
		}

		/// <summary>Hit when the control receives focus.</summary>
		/// <param name="e">RoutedEventArgs</param>
		protected override void OnGotFocus(RoutedEventArgs e)
		{
			base.OnGotFocus(e);
			if (!_InsertIsON)
			{
				PressKey(Key.Insert);
				_InsertIsON = true;
			}
		}

		/// <summary>Hit when the user enters a key.</summary>
		/// <param name="e">TextCompositionEventArgs</param>
		protected override void OnPreviewTextInput(TextCompositionEventArgs e)
		{
			System.ComponentModel.MaskedTextResultHint hint;
			int TestPosition;

			if (e.Text.Length == 1)
				this.NewTextIsOk = _mprovider.VerifyChar(e.Text[0], this.CaretIndex, out hint);
			else
				this.NewTextIsOk = _mprovider.VerifyString(e.Text, out TestPosition, out hint);

			base.OnPreviewTextInput(e);
		}

		/// <summary>Hit when Text is inserted into the control.</summary>
		/// <param name="e">TextCompositionEventArgs</param>
		protected override void OnTextInput(TextCompositionEventArgs e)
		{
			string PreviousText = this.Text;
			if (this.NewTextIsOk)
			{
				base.OnTextInput(e);
				if (_mprovider.VerifyString(this.Text) == false) this.Text = PreviousText;
				while (!_mprovider.IsEditPosition(this.CaretIndex) && _mprovider.Length > this.CaretIndex) this.CaretIndex++;

			}
			else
				e.Handled = true;
		}

		/// <summary>Hit when the control loses focus from the keyboard.</summary>
		/// <param name="e">KeyboardFocusChangedEventArgs</param>
		protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			if (StayInFocusUntilValid)
			{
				_mprovider.Clear();
				_mprovider.Add(this.Text);
				if (!_mprovider.MaskFull)
				{
					e.Handled = true;
					this.Text = this._mprovider.ToDisplayString();
				}
			}

			base.OnPreviewLostKeyboardFocus(e);
		}
		#endregion

		private void PressKey(Key key)
		{
			KeyEventArgs eInsertBack = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, key);
			eInsertBack.RoutedEvent = KeyDownEvent;
			InputManager.Current.ProcessInput(eInsertBack);
		}
	}
}