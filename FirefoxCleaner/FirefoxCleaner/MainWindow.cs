using System;
using Gtk;
using System.Diagnostics;

namespace FirefoxCleaner
{
	public partial class MainWindow: Gtk.Window
	{
		Cleaner cleaner = new Cleaner ();
		public MainWindow () : base (Gtk.WindowType.Toplevel)
		{
			Build ();

			if (cleaner.FirefoxAppPath == string.Empty) {
				Additional.GetMessageBox(this,
				                         "FirefoxClearing error",
				                         "Mozilla is not installed in your computer",
				                         "",
				                         DialogFlags.Modal,
				                         MessageType.Warning,
				                         ButtonsType.Ok);
				MainPanel.Sensitive = false;
				return;
			}

			if (Cleaner.osPlatform == PlatformID.Unix) {
				FFUpdateLogs.Sensitive = false;
				MZUpdates.Sensitive = false;
				MaintenServLogs.Sensitive = false;

				GetUserPassword usPassDlg = new GetUserPassword();
				if(usPassDlg.IsPushedOK)
				{
					Cleaner.UserPasswd = usPassDlg.UserPassword;
				}
				else
				{
					this.Sensitive = false;
				}
				usPassDlg.Destroy();
			}
			
			ClearBtn.IsFocus = true;
		}

		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Application.Quit ();
			a.RetVal = true;
		}

		protected void OnClearBtnClicked (object sender, EventArgs e)
		{
			try
			{
				// Check is Firefox running
				Additional.IsFirefoxRunning ();

				// Run cleaning
				cleaner.Run ();
			}
			catch(ApplicationException appex) {
				Additional.GetMessageBox (null, 
				                          "FirefoxClearing error", 
				                          appex.Message, 
				                          "",
				               			  DialogFlags.Modal, 
				                          MessageType.Error, 
				                          ButtonsType.Ok);
			}
			catch(Exception ex) {
				Additional.GetMessageBox (null, 
				                          "FirefoxClearing error", 
				                          "ERROR MESSAGE: " + ex.Message, 
				                          "",
				               			  DialogFlags.Modal, 
				                          MessageType.Error, 
				                          ButtonsType.Ok);
			}
		}

        #region checkboxes

        protected void OnInetHistoryClicked (object sender, EventArgs e)
		{
			if (InetHistory.Active == true)
				cleaner.SelectInternetHistory (true);
			else
				cleaner.SelectInternetHistory();
		}

		protected void OnInetCacheClicked (object sender, EventArgs e)
		{
			if (InetCache.Active == true)
				cleaner.SelectInternetCache (true);
			else
				cleaner.SelectInternetCache();
		}

		protected void OnCookiesClicked (object sender, EventArgs e)
		{
			if (Cookies.Active == true)
				cleaner.SelectCookies (true);
			else
				cleaner.SelectCookies();
		}

		protected void OnAdblockBackupsClicked (object sender, EventArgs e)
		{
			if (AdblockBackups.Active == true)
				cleaner.SelectAdblockBackups (true);
			else
				cleaner.SelectAdblockBackups();
		}

		protected void OnBookmarkBackupsClicked (object sender, EventArgs e)
		{
			if (BookmarkBackups.Active == true)
				cleaner.SelectBookmarkBackups (true);
			else
				cleaner.SelectBookmarkBackups();
		}

		protected void OnCrashReportsClicked (object sender, EventArgs e)
		{
			if (CrashReports.Active == true)
				cleaner.SelectCrashReports (true);
			else
				cleaner.SelectCrashReports();
		}

		protected void OnDownloadHistoryClicked (object sender, EventArgs e)
		{
			if (DownloadHistory.Active == true)
				cleaner.SelectDownloadHistory (true);
			else
				cleaner.SelectDownloadHistory();
		}

		protected void OnFFCorruptSQLClicked (object sender, EventArgs e)
		{
			if (FFCorruptSQL.Active == true)
				cleaner.SelectFirefoxCorruptSQLites (true);
			else
				cleaner.SelectFirefoxCorruptSQLites();
		}

		protected void OnFFExtLogClicked (object sender, EventArgs e)
		{
			if (FFExtLog.Active == true)
				cleaner.SelectFirefoxExtensionsLogs (true);
			else
				cleaner.SelectFirefoxExtensionsLogs();
		}

		protected void OnFFLogsClicked (object sender, EventArgs e)
		{
			if (FFLogs.Active == true)
				cleaner.SelectFirefoxLogs (true);
			else
				cleaner.SelectFirefoxLogs();
		}

		protected void OnFFMinidumpsClicked (object sender, EventArgs e)
		{
			if (FFMinidumps.Active == true)
				cleaner.SelectFirefoxMinidumps (true);
			else
				cleaner.SelectFirefoxMinidumps();
		}

		protected void OnFFStartupCacheClicked (object sender, EventArgs e)
		{
			if (FFStartupCache.Active == true)
				cleaner.SelectFirefoxStartupCache (true);
			else
				cleaner.SelectFirefoxStartupCache();
		}

		protected void OnFFTelemetryClicked (object sender, EventArgs e)
		{
			if (FFTelemetry.Active == true)
				cleaner.SelectFirefoxTelemetry (true);
			else
				cleaner.SelectFirefoxTelemetry();
		}

		protected void OnFFTestPilotErrorLogsClicked (object sender, EventArgs e)
		{
			if (FFTestPilotErrorLogs.Active == true)
				cleaner.SelectFirefoxTestPilotErrorLogs (true);
			else
				cleaner.SelectFirefoxTestPilotErrorLogs();
		}

		protected void OnFFUpdateLogsClicked (object sender, EventArgs e)
		{
			if (FFUpdateLogs.Active == true)
				cleaner.SelectFirefoxUpdateLogs (true);
			else
				cleaner.SelectFirefoxUpdateLogs();
		}

		protected void OnFFurlclassifier3Clicked (object sender, EventArgs e)
		{
			if (FFurlclassifier3.Active == true)
				cleaner.SelectFirefoxUrlclassifier (true);
			else
				cleaner.SelectFirefoxUrlclassifier();
		}

		protected void OnFFwebappsstoreClicked (object sender, EventArgs e)
		{
			if (FFwebappsstore.Active == true)
				cleaner.SelectFirefoxWebappstore (true);
			else
				cleaner.SelectFirefoxWebappstore();
		}

		protected void OnFlashGotClicked (object sender, EventArgs e)
		{
			if (FlashGot.Active == true)
				cleaner.SelectFlashGot (true);
			else
				cleaner.SelectFlashGot();
		}

		protected void OnLockFilesClicked (object sender, EventArgs e)
		{
			if (LockFiles.Active == true)
				cleaner.SelectLockFiles (true);
			else
				cleaner.SelectLockFiles();
		}

		protected void OnMaintenServLogsClicked (object sender, EventArgs e)
		{
			if (MaintenServLogs.Active == true)
				cleaner.SelectMaintananceServiceLogs (true);
			else
				cleaner.SelectMaintananceServiceLogs();
		}

		protected void OnMZUpdatesClicked (object sender, EventArgs e)
		{
			if (MZUpdates.Active == true)
				cleaner.SelectMozillaUpdates (true);
			else
				cleaner.SelectMozillaUpdates();
		}

		protected void OnRecovFileFragmentsClicked (object sender, EventArgs e)
		{
			if (RecovFileFragments.Active == true)
				cleaner.SelectRecoveredFileFragments (true);
			else
				cleaner.SelectRecoveredFileFragments();
		}

		protected void OnStylishSyncBackupsClicked (object sender, EventArgs e)
		{
			if (StylishSyncBackups.Active == true)
				cleaner.SelectStylishSyncBackups (true);
			else
				cleaner.SelectStylishSyncBackups();
		}

		protected void OnSyncLogsClicked (object sender, EventArgs e)
		{
			if (SyncLogs.Active == true)
				cleaner.SelectSyncLogs (true);
			else
				cleaner.SelectSyncLogs();
		}

		protected void OnThumbnailsClicked (object sender, EventArgs e)
		{
			if (Thumbnails.Active == true)
				cleaner.SelectThumbnails (true);
			else
				cleaner.SelectThumbnails();
		}

		protected void OnCompactDBClicked (object sender, EventArgs e)
		{
			if (CompactDB.Active == true)
				cleaner.SelectCompactDatabases (true);
			else
				cleaner.SelectCompactDatabases();
		}

		protected void OnDumpFilesClicked (object sender, EventArgs e)
		{
			if (CompactDB.Active == true)
				cleaner.SelectDumpFiles (true);
			else
				cleaner.SelectDumpFiles();
		}

        #endregion

        protected void OnUninstallFirefoxBtnClicked (object sender, EventArgs e)
		{
			int response = Additional.GetMessageBox(this, 
							                        "Uninstalling Mozilla Firefox",
							                        "If you continue Mozilla Firefox will removed from your computer",
							                        "",
							                        DialogFlags.Modal, 
							                        MessageType.Warning, 
							                        ButtonsType.OkCancel);

			if (response != -5)
				return;

			try
			{
				bool isFirefoxWasDefaultBrowser = cleaner.UninstallFirefox ();
				if(Cleaner.osPlatform == PlatformID.Unix)
				{
					Additional.GetMessageBox(this, 
					                         "Uninstalling Mozilla Firefox", 
					                         "Mosilla Firefox uninstalled successfully",
					                         "",
					                         DialogFlags.Modal, 
					                         MessageType.Info, 
					                         ButtonsType.Ok);
				}
				else
				{
		            string secondaryText = string.Empty;
		            if (isFirefoxWasDefaultBrowser) secondaryText = "Internet Explorer is default browser now.\n\n";
		            secondaryText += "For correct unistalling need reboot computer.\nReboot computer now?";
					response = Additional.GetMessageBox(this, 
								                        "Uninstalling Mozilla Firefox", 
								                        "Mosilla Firefox uninstalled successfully",
								                        secondaryText,
								                        DialogFlags.Modal, 
								                        MessageType.Info, 
								                        ButtonsType.YesNo);
		            if (response == -8)
					{
		                Process.Start("shutdown", "-r -f");
					}
				}
				
				MainPanel.Sensitive = false;
			}
			catch(Exception ex) {
				Additional.GetMessageBox (null, 
				                          "FirefoxCleaner exception", 
				                          "Uninstalling error", 
				                          ex.Message,
				               			  DialogFlags.Modal, 
				                          MessageType.Error, 
				                          ButtonsType.Ok);
			}
		}

	}
}
