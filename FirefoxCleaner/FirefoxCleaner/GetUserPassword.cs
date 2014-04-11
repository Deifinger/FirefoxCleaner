using System;
using Gtk;

namespace FirefoxCleaner
{
	public partial class GetUserPassword : Gtk.Dialog
	{
		string userPassword;
		bool isPushedOK;

		Entry entry_passwd;

		public string UserPassword { 
			get { 
				return userPassword; 
			}
		}
		public bool IsPushedOK { 
			get { 
				return isPushedOK; 
			}
		}

		public GetUserPassword ()
		{
			userPassword = null;
			isPushedOK = false;

			this.Build ();

			entry_passwd = new Entry();
			entry_passwd.WidthRequest = 157;
			entry_passwd.Visible = true;
			entry_passwd.CanFocus = true;
			fixed1.Add(entry_passwd);
			fixed1.Move(entry_passwd, 165, 41);
			entry_passwd.GrabFocus();

			this.Run();
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			this.Hide();
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			isPushedOK = true;
			userPassword = entry_passwd.Text;
			this.Hide();
		}

		protected void OnClose (object sender, EventArgs e)
		{
			Hide();
		}
	}
}

