using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Woz1.NET
{
    internal class TermTextBox : TextBox
    {
        public TermTextBox()
        {
            this.ReadOnly = true;
            this.BackColor = Color.White;
            this.GotFocus += TextBoxGotFocus;
            this.Cursor = Cursors.Arrow; // mouse cursor like in other controls
        }
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x0201://WM_LBUTTONDOWN
                {
                    ResetTextBox();
                    return;
                }

                case 0x0202://WM_LBUTTONUP
                {
                    ResetTextBox();
                    return;
                }

                case 0x0203://WM_LBUTTONDBLCLK
                {
                    ResetTextBox();
                    return;
                }

                case 0x0204://WM_RBUTTONDOWN
                {
                    ResetTextBox();
                    return;
                }

                case 0x0205://WM_RBUTTONUP
                {
                    ResetTextBox();
                    return;
                }

                case 0x0206://WM_RBUTTONDBLCLK
                {
                    ResetTextBox();
                    return;
                }
            }

            base.WndProc(ref m);
        }

        private void ResetTextBox()
        {
            this.SelectionLength = 0;
            if (Text.Length > 0) this.SelectionStart = Text.Length - 1;
            return;
        }

        [DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr hWnd);
        private void TextBoxGotFocus(object sender, EventArgs args)
        {
            HideCaret(this.Handle);
        }
    }
}