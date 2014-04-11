using Gtk;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Security;
using Microsoft.Win32;
using System.Collections;

namespace FirefoxCleaner
{
	public class Cleaner
	{
		delegate void actions();
		actions acts;

		// User password - used in Unix systems
		public static string UserPasswd { get; set; }

        // Info about OS Platform
        public static PlatformID osPlatform { get; set; }
        // Current Program Files path
		public string FirefoxAppPath { get; set; }
		// Current Program Data path
		public string ProgramDataPath { get; set; }
		// Current Roaming path
		public string RoamingPath { get; set; }
		// Current Local path
		public string LocalPath { get; set; }
		// Current Roaming profilies path
		public string RoamingProfilesPath { 
			get { 
				if(Cleaner.osPlatform == PlatformID.Unix)
					return RoamingPath + "/firefox";
				else
					return RoamingPath + "/Firefox/Profiles"; 
			}
		}
		// Current Local profilies path
		public string LocalProfilesPath { 
			get { 
				if(Cleaner.osPlatform == PlatformID.Unix)
					return LocalPath + "/firefox";
				else
					return LocalPath + "/Firefox/Profiles"; 
			}
		}

        // System folders
        string sys_ProgramData;
        string sys_AppData;
        string sys_LocalData;
        string sys_UserProfile;
        string sys_OSDir;
        string sys_PublicDesktop;
        string sys_StartMenuPrograms;
        string sys_UserDesktop;

		static Process term;

		public Cleaner ()
		{
            Cleaner.osPlatform = Environment.OSVersion.Platform;

			acts = delegate {};
			//firefoxVersion = FileVersionInfo.GetVersionInfo(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\firefox.exe").FileVersion;

			FirefoxAppPath = string.Empty;
			UserPasswd = string.Empty;

			if(osPlatform == PlatformID.Unix)
			{
				sys_ProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, // /usr/share
                	Environment.SpecialFolderOption.Create);
	            sys_AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, // /home/z/.config
	                Environment.SpecialFolderOption.Create);
	            sys_LocalData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, // /home/z/.local/share
	                Environment.SpecialFolderOption.Create);
				sys_UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.Personal, // /home/z
	                Environment.SpecialFolderOption.Create);
				sys_UserDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory, // /home/z/Desktop
	                Environment.SpecialFolderOption.Create);

				// Start the child process.
				Process proc = new Process();
				// Redirect the output stream of the child process.
				proc.StartInfo.UseShellExecute = false;
				proc.StartInfo.RedirectStandardOutput = true;
				proc.StartInfo.FileName = "which";
				proc.StartInfo.Arguments = "firefox";
				proc.Start();
				// Read the output stream first and then wait.
				string firefoxBinPath = proc.StandardOutput.ReadLine();
				proc.WaitForExit();

				if(firefoxBinPath != null)
				{
					// Read firefox bin file
					string firefoxBinText = File.ReadAllText(firefoxBinPath);

					string binFileEntry = "MOZ_LIBDIR=";
					int startIndex = firefoxBinText.IndexOf(binFileEntry) + binFileEntry.Length;
					int endIndex = firefoxBinText.IndexOf("\n", startIndex);

	 				FirefoxAppPath = firefoxBinText.Substring(startIndex, endIndex - startIndex);
				}

				ProgramDataPath = sys_ProgramData + @"/Mozilla";
	            RoamingPath = sys_UserProfile + @"/.mozilla";
				if(Directory.Exists(sys_UserProfile + "/.cache/mozilla"))
					LocalPath = sys_UserProfile + "/.cache/mozilla";
				else
					LocalPath = RoamingPath;

			}
			else
			{
				sys_ProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData,
                	Environment.SpecialFolderOption.Create);
	            sys_AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData,
	                Environment.SpecialFolderOption.Create);
	            sys_LocalData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
	                Environment.SpecialFolderOption.Create);
	            sys_UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile,
	                Environment.SpecialFolderOption.Create);
	            sys_OSDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows,
	                Environment.SpecialFolderOption.Create);
	            sys_PublicDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory,
	                Environment.SpecialFolderOption.Create);
	            sys_StartMenuPrograms = Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms,
	                Environment.SpecialFolderOption.Create);
	            sys_UserDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory,
	                Environment.SpecialFolderOption.Create);

				RegistryKey rk = Registry.LocalMachine.OpenSubKey (@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\firefox.exe");
				if (rk != null)
				{
					FirefoxAppPath = (string)rk.GetValue ("Path");
					rk.Dispose();
				}

				ProgramDataPath = sys_ProgramData + @"/Mozilla";
	            RoamingPath = sys_AppData + @"/Mozilla";
	            LocalPath = sys_LocalData + @"/Mozilla";
			}
            
		}

		#region actions

		void InternetHistory()
		{
             Additional.DeleteFromTableInRoamingUserProfiles("places.sqlite", new string[] { "moz_places", "moz_historyvisits" }, RoamingProfilesPath);
        }

		void InternetCache()
		{
			// Clear Cache
			Additional.ClearAllFoldersInDirectory (LocalProfilesPath, new string[] { "/Cache" });
		}

		void Cookies()
		{
			// Clear Cookies for firefox less 3 version
			Additional.DeleteFilesFromAllProfiles (RoamingProfilesPath, new string[] { "/cookies.txt" });

			// Clear Cookies for firefox >= 3 version
			Additional.DeleteFromTableInRoamingUserProfiles ("cookies.sqlite", new string[] { "moz_cookies" }, RoamingProfilesPath);
		}

		void AdblockBackups()
		{
			// Clear Adblock Backups
			Additional.ClearAllFoldersInDirectory (RoamingProfilesPath, new string[] { "/adblockplus" }, new string[] { ".ini" });
		}

		void BookmarkBackups()
		{
			// Clear Bookmark Backups
			Additional.ClearAllFoldersInDirectory (RoamingProfilesPath, new string[] { "/bookmarkbackups" });
		}

		void CrashReports()
		{
			// Clear Crash Reports
			Additional.ClearAllFoldersInDirectory (RoamingPath + "/Firefox/Crash Reports", new string[] { "/pending", "/submitted" });
		}

		void DownloadHistory()
		{
			// Clear Download History for firefox less 3 version
			Additional.DeleteFilesFromAllProfiles (RoamingProfilesPath, new string[] { "/downloads.rdf" });

			// Clear Download History v. 3 - 18
			Additional.DeleteFilesFromAllProfiles (RoamingPath, new string[] { "/downloads.sqlite" });

			// Clear Download History v. above 18
			Additional.DeleteFromTableInRoamingUserProfiles ("places.sqlite", new string[] { "moz_annos" }, RoamingProfilesPath, 
				"where anno_attribute_id IN (SELECT id FROM moz_anno_attributes WHERE name LIKE \"downloads/%\")");
		}

		void CompactDatabases()
		{
			// Get SQLite databases
			string[] databases = Additional.GetFilesFromDirectories (new string[] { RoamingProfilesPath, LocalProfilesPath }, new string[] { "*.sqlite" }, SearchOption.AllDirectories);

			// Compact all databases
			foreach(string db in databases)
				Additional.SetDBCommand ("Data Source=" + db + "; Version=3;", "vacuum;");
		}

		void FirefoxCorruptSQLites()
		{
			// Remove all corrupted SQLite databases
			Additional.SearchFilesAndRemoveIt (new string[] { RoamingProfilesPath, LocalProfilesPath }, new string[] { "*.corrupt" }, 
				SearchOption.AllDirectories);
		}

		void FirefoxExtensionsLog()
		{
			// Clear file
			Additional.CreateEmptyFileInAllProfiles (RoamingProfilesPath, "extensions.log");
		}

		void FirefoxLogs()
		{
			// Get all log files
			string[] logs = Additional.GetFilesFromDirectories (new string[] { RoamingProfilesPath, LocalProfilesPath },
				new string[] { "*.log" }, SearchOption.AllDirectories);

			// Clear all logs
			foreach(string log in logs)
				using (File.Create (log)){};
		}

		void FirefoxMinidumps()
		{
			Additional.ClearAllFoldersInDirectory (RoamingProfilesPath, new string[] { "/minidumps" });
		}

		void FirefoxStartupCache()
		{
			Additional.ClearAllFoldersInDirectory (LocalProfilesPath, new string[] { "/startupCache" }, new string[] { ".little" });
		}

		void FirefoxTelemetry()
		{
			Additional.ClearAllFoldersInDirectory (RoamingProfilesPath, new string[] { "/saved-telemetry-pings" });

			//Clear file
			Additional.CreateEmptyFileInAllProfiles (RoamingProfilesPath, "Telemetry.ShutdownTime.txt");
		}

		void FirefoxTestPilotErrorLogs()
		{
			//Clear file
			Additional.CreateEmptyFileInAllProfiles (RoamingProfilesPath, "TestPilotErrorLog.log");
		}

		void FirefoxUpdateLogs()
		{
			Additional.ClearAllFoldersInDirectory (LocalPath + "/updates/updates", new string[] { "" }, new string[] { ".log" });
		}

		void FirefoxUrlclassifier()
		{
			Additional.DeleteFilesFromAllProfiles (LocalProfilesPath, new string[] { "/urlclassifier3.sqlite"});
		}

		void FirefoxWebappsstore()
		{
			Additional.DeleteFilesFromAllProfiles (RoamingProfilesPath, new string[] { "/webappsstore.sqlite" });
		}

		void FlashGot()
		{
			Additional.DeleteFilesFromAllProfiles (RoamingProfilesPath, new string[] { "/flashgot.log", "/flashgot.log.bak" });
		}

		void LockFiles()
		{
			// Delete all lock files
			Additional.SearchFilesAndRemoveIt (new string[] { RoamingProfilesPath, LocalProfilesPath }, new string[] { "*.lock" }, SearchOption.AllDirectories);
		}

		void MaintananceServiceLogs()
		{
			// Delete all maintananceservice logs
			Additional.SearchFilesAndRemoveIt (new string[] { ProgramDataPath }, 
				new string[] { "maintenanceservice*.log" }, SearchOption.AllDirectories);
		}

		void MozillaUpdates()
		{
			Additional.ClearAllFoldersInDirectory (LocalPath + "/updates", new string[] { "" });
		}

		void RecoveredFileFragments()
		{
			// Delete all RecoveredFileFragments (*.chk)
			Additional.SearchFilesAndRemoveIt (new string[] { FirefoxAppPath }, 
				new string[] { "*.chk" }, SearchOption.AllDirectories);
		}

		void StylishSyncBackups()
		{
			Additional.ClearAllFoldersInDirectory(RoamingProfilesPath, new string[] { "/stylishsync" }, new string[] { ".sqlite" });
		}

		void SyncLogs()
		{
			Additional.ClearAllFoldersInDirectory (RoamingProfilesPath, new string[] { "/weave/logs" }); 
		}

		void Thumbnails()
		{
			Additional.ClearAllFoldersInDirectory (LocalProfilesPath, new string[] { "/thumbnails" });
		}

		void DumpFiles()
		{
			// Delete all dump files (*.dmp)
			Additional.SearchFilesAndRemoveIt (new string[] { LocalProfilesPath, RoamingProfilesPath },
				new string[] { "*.dmp" }, SearchOption.AllDirectories);
		}

		#endregion
		
		#region add to delegate

		public void SelectInternetHistory(bool isChecked = false)
		{
			if (isChecked)
				acts += InternetHistory;
			else
				acts -= InternetHistory;
		}

		public void SelectInternetCache(bool isChecked = false)
		{
			if (isChecked)
				acts += InternetCache;
			else
				acts -= InternetCache;
		}

		public void SelectCookies(bool isChecked = false)
		{
			if (isChecked)
				acts += Cookies;
			else
				acts -= Cookies;
		}

		public void SelectAdblockBackups(bool isChecked = false)
		{
			if (isChecked)
				acts += AdblockBackups;
			else
				acts -= AdblockBackups;
		}

		public void SelectBookmarkBackups(bool isChecked = false)
		{
			if (isChecked)
				acts += BookmarkBackups;
			else
				acts -= BookmarkBackups;
		}

		public void SelectCrashReports(bool isChecked = false)
		{
			if (isChecked)
				acts += CrashReports;
			else
				acts -= CrashReports;
		}

		public void SelectDownloadHistory(bool isChecked = false)
		{
			if (isChecked)
				acts += DownloadHistory;
			else
				acts -= DownloadHistory;
		}

		public void SelectCompactDatabases(bool isChecked = false)
		{
			if (isChecked)
				acts += CompactDatabases;
			else
				acts -= CompactDatabases;
		}

		public void SelectFirefoxCorruptSQLites(bool isChecked = false)
		{
			if (isChecked)
				acts += FirefoxCorruptSQLites;
			else
				acts -= FirefoxCorruptSQLites;
		}

		public void SelectFirefoxExtensionsLogs(bool isChecked = false)
		{
			if (isChecked)
				acts += FirefoxExtensionsLog;
			else
				acts -= FirefoxExtensionsLog;
		}

		public void SelectFirefoxLogs(bool isChecked = false)
		{
			if (isChecked)
				acts += FirefoxLogs;
			else
				acts -= FirefoxLogs;
		}

		public void SelectFirefoxMinidumps(bool isChecked = false)
		{
			if (isChecked)
				acts += FirefoxMinidumps;
			else
				acts -= FirefoxMinidumps;
		}

		public void SelectFirefoxStartupCache(bool isChecked = false)
		{
			if (isChecked)
				acts += FirefoxStartupCache;
			else
				acts -= FirefoxStartupCache;
		}

		public void SelectFirefoxTelemetry(bool isChecked = false)
		{
			if (isChecked)
				acts += FirefoxTelemetry;
			else
				acts -= FirefoxTelemetry;
		}

		public void SelectFirefoxTestPilotErrorLogs(bool isChecked = false)
		{
			if (isChecked)
				acts += FirefoxTestPilotErrorLogs;
			else
				acts -= FirefoxTestPilotErrorLogs;
		}

		public void SelectFirefoxUpdateLogs(bool isChecked = false)
		{
			if (isChecked)
				acts += FirefoxUpdateLogs;
			else
				acts -= FirefoxUpdateLogs;
		}

		public void SelectFirefoxUrlclassifier(bool isChecked = false)
		{
			if (isChecked)
				acts += FirefoxUrlclassifier;
			else
				acts -= FirefoxUrlclassifier;
		}

		public void SelectFirefoxWebappstore(bool isChecked = false)
		{
			if (isChecked)
				acts += FirefoxWebappsstore;
			else
				acts -= FirefoxWebappsstore;
		}

		public void SelectFlashGot(bool isChecked = false)
		{
			if (isChecked)
				acts += FlashGot;
			else
				acts -= FlashGot;
		}

		public void SelectLockFiles(bool isChecked = false)
		{
			if (isChecked)
				acts += LockFiles;
			else
				acts -= LockFiles;
		}

		public void SelectMaintananceServiceLogs(bool isChecked = false)
		{
			if (isChecked)
				acts += MaintananceServiceLogs;
			else
				acts -= MaintananceServiceLogs;
		}

		public void SelectMozillaUpdates(bool isChecked = false)
		{
			if (isChecked)
				acts += MozillaUpdates;
			else
				acts -= MozillaUpdates;
		}

		public void SelectRecoveredFileFragments(bool isChecked = false)
		{
			if (isChecked)
				acts += RecoveredFileFragments;
			else
				acts -= RecoveredFileFragments;
		}

		public void SelectStylishSyncBackups(bool isChecked = false)
		{
			if (isChecked)
				acts += StylishSyncBackups;
			else
				acts -= StylishSyncBackups;
		}

		public void SelectSyncLogs(bool isChecked = false)
		{
			if (isChecked)
				acts += SyncLogs;
			else
				acts -= SyncLogs;
		}

		public void SelectThumbnails(bool isChecked = false)
		{
			if (isChecked)
				acts += Thumbnails;
			else
				acts -= Thumbnails;
		}

		public void SelectDumpFiles(bool isChecked = false)
		{
			if (isChecked)
				acts += DumpFiles;
			else
				acts -= DumpFiles;
		}

		#endregion

		public void Run()
		{
			acts();
		}

        public bool UninstallFirefox ()
		{
			bool isFirefoxWasDefaultBrowser = false;

			if (osPlatform == PlatformID.Unix) {

				/*Process term = new Process();
				term.StartInfo.FileName = "sudo";
				term.StartInfo.Arguments = "apt-get remove firefox";
				term.StartInfo.UseShellExecute = false;
				term.StartInfo.RedirectStandardInput = true;
				term.StartInfo.RedirectStandardOutput = true;
				term.StartInfo.RedirectStandardError = true;
				term.Start ();

				StreamWriter swTerm = term.StandardInput;
				StreamReader srTerm = term.StandardOutput;



				string sr_str = srTerm.ReadToEnd();
				// Loop for set commands into terminal
				while(sr_str != null)
				{
					if(sr_str.Contains("[y/n]"))
					{
						swTerm.WriteLine("y");
						break;
					}
					else if(sr_str.Contains("[sudo] password for"))
						swTerm.WriteLine(UserPasswd);
					else if(sr_str.Contains("Package 'firefox' is not installed"))
						return false;

					sr_str = srTerm.ReadToEnd();
				}

				srTerm.Close();
				swTerm.Close();
				term.WaitForExit();*/

				term = new Process();
				term.StartInfo.FileName = "sudo";
				term.StartInfo.Arguments = "apt-get remove firefox";
				term.StartInfo.UseShellExecute = false;
				term.OutputDataReceived += new DataReceivedEventHandler(SortOutputHandler);
				term.StartInfo.RedirectStandardInput = true;
				term.StartInfo.RedirectStandardOutput = true;
				term.Start ();
				term.BeginOutputReadLine();

				/*// Save all folders for remove
				string[] pathsForRemove = { FirefoxAppPath,
					"/usr/lib/firefox-addons",
					"/etc/firefox",
					RoamingProfilesPath,
					LocalProfilesPath
				};

				string paths = string.Empty;

				// Remove all folders
				foreach (string path in pathsForRemove) {
					if (Directory.Exists (path)) {
						try
						{
							Directory.Delete (path);
						}
						catch
						{
							paths += " '" + path + "'";
						}
					}
				}*/

				/*Process term = new Process();
				term.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				term.StartInfo.FileName = "sudo";
				term.StartInfo.Arguments = "rm -r" + paths;
				term.Start ();

				Additional.GetMessageBox(null,
				                         "Uninstalling Firefox",
				                         "Please, enter root's password in Terminal for uninstalling Firefox",
				                         "Program will be sleep",
				                         DialogFlags.Modal,
				                         MessageType.Warning,
				                         ButtonsType.Ok);

				term.WaitForExit();
				
				Additional.SearchFilesAndRemoveIt (new string[] { 
                "/var/lib/dpkg/info" },
				new string[] { "*firefox*" },
				SearchOption.TopDirectoryOnly);*/
			}
			else {

				// Save all folders for remove
				string[] pathsForRemove = { FirefoxAppPath,
					sys_LocalData + "/VirtualStore/Program Files (x86)/Mozilla Firefox",
					LocalPath + "/Firefox",
					RoamingPath + "/Firefox"
				};

				// Remove all folders
				foreach (string path in pathsForRemove) {
					if (Directory.Exists (path))
						Directory.Delete (path, true);
				}

				// Remove all firefox files in Prefetch
				Additional.SearchFilesAndRemoveIt (new string[] { sys_OSDir + "/Prefetch" },
					new string[] { "FIREFOX*" },
					SearchOption.TopDirectoryOnly);
				Additional.SearchFilesAndRemoveIt (new string[] { 
	                sys_AppData + @"/Microsoft/nternet Explorer/Quick Launch/User Pinned/TaskBar",
	                sys_AppData + @"/Microsoft/Internet Explorer/Quick Launch" },
					new string[] { "*Firefox*.lnk" },
					SearchOption.TopDirectoryOnly);
				// Remove links
				Additional.SearchFilesAndRemoveIt (new string[] { sys_PublicDesktop,	sys_StartMenuPrograms },
					new string[] { "Mozilla Firefox.lnk" },
					SearchOption.TopDirectoryOnly);
				Additional.SearchFilesAndRemoveIt (new string[] { 
	                sys_AppData + @"/Microsoft/nternet Explorer/Quick Launch/User Pinned/TaskBar",
	                sys_AppData + @"/Microsoft/Internet Explorer/Quick Launch" },
					new string[] { "*Firefox*.lnk" },
					SearchOption.TopDirectoryOnly);

				// Values for setting default browser
				string defaultIconValue = string.Empty;
				string shellOpenCommandValue = string.Empty;

				// Fill the RegistryPathsFoldersHKEY_LCL_MCHN list
				List<string> RegistryPathsFoldersHKEY_LCL_MCHN = new List<string> ();
				using (RegistryKey localMachineKey = Registry.LocalMachine) {
					// Get value of DefaultIcon of IEXPLORE
					RegistryKey defIconSubKey = 
                    localMachineKey.OpenSubKey (@"SOFTWARE\Clients\StartMenuInternet\IEXPLORE.EXE\DefaultIcon");
					if (defIconSubKey != null) {
						defaultIconValue = (string)defIconSubKey.GetValue ("");
						defIconSubKey.Dispose ();
					}
					// Get value of shell\open\command of IEXPLORE
					RegistryKey shellOpenCommandSubKey = 
                    localMachineKey.OpenSubKey (@"SOFTWARE\Clients\StartMenuInternet\IEXPLORE.EXE\shell\open\command");
					if (shellOpenCommandSubKey != null) {
						shellOpenCommandValue = (string)shellOpenCommandSubKey.GetValue ("");
						shellOpenCommandSubKey.Dispose ();
					}

					Additional.CheckAndAddRegPaths (localMachineKey, RegistryPathsFoldersHKEY_LCL_MCHN, new string[]{
				    @"SOFTWARE\Microsoft\ESENT\Process\firefox", // XP key
                    @"SOFTWARE\Microsoft\MediaPlayer\ShimInclusionList\FIREFOX.EXE",
				    @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\firefox.exe",
				    @"SOFTWARE\Microsoft\RADAR\HeapLeakDetection\DiagnosedApplications\firefox.exe",
				    @"SOFTWARE\Microsoft\Windows Search\CrawlScopeManager\Windows\SystemIndex\WorkingSetRules\1",
				    @"SOFTWARE\Classes\FirefoxHTML",
				    @"SOFTWARE\Classes\FirefoxURL",
				    @"SOFTWARE\Clients\StartMenuInternet\FIREFOX.EXE",
				    @"SOFTWARE\Classes\Wow6432Node\CLSID\{0D68D6D0-D93D-4D08-A30D-F00DD1F45B24}",
				    @"SOFTWARE\Classes\Wow6432Node\Interface\{0D68D6D0-D93D-4D08-A30D-F00DD1F45B24}",
				    @"SOFTWARE\Classes\Wow6432Node\Interface\{1814CEEB-49E2-407F-AF99-FA755A7D2607}",
				    @"SOFTWARE\Classes\Wow6432Node\Interface\{4E747BE5-2052-4265-8AF0-8ECAD7AAD1C0}",
				    @"SOFTWARE\Wow6432Node\Classes\CLSID\{0D68D6D0-D93D-4D08-A30D-F00DD1F45B24}",
				    @"SOFTWARE\Wow6432Node\Classes\Interface\{0D68D6D0-D93D-4D08-A30D-F00DD1F45B24}",
				    @"SOFTWARE\Wow6432Node\Classes\Interface\{1814CEEB-49E2-407F-AF99-FA755A7D2607}",
				    @"SOFTWARE\Wow6432Node\Classes\Interface\{4E747BE5-2052-4265-8AF0-8ECAD7AAD1C0}",
			    }
					);
					/**
                 * getSubKeys_values[some index][0] - path of subkey
                 * getSubKeys_values[some index][0] - search pattern for Additional.GetSubKeys()
                 **/
					string[][] getSubKeys_values = new string[4][] {
                    new string[] { @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", "Mozilla Firefox" },
                    new string[] { @"SOFTWARE\Mozilla", "Mozilla Firefox" },
                    new string[] {
						@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
						"Mozilla Firefox"
					},
                    new string[] { @"SOFTWARE\Wow6432Node\Mozilla", "Firefox" }
                };
					foreach (string[] pathSearchvalue in getSubKeys_values) {
						using (RegistryKey openedSubKey = localMachineKey.OpenSubKey(pathSearchvalue[0])) {
							RegistryPathsFoldersHKEY_LCL_MCHN.AddRange (Additional.GetSubKeys (
				            openedSubKey, pathSearchvalue [1])
							);
						}
					}
				}

				// Fill the RegistryPathsFoldersHKEY_CLSS_ROOT list
				List<string> RegistryPathsFoldersHKEY_CLSS_ROOT = new List<string> ();
				using (RegistryKey classRootKey = Registry.ClassesRoot) {
					Additional.CheckAndAddRegPaths (classRootKey, RegistryPathsFoldersHKEY_CLSS_ROOT, new string[] {
				    @"FirefoxHTML",
				    @"FirefoxURL",
                    @"CLSID\{0D68D6D0-D93D-4D08-A30D-F00DD1F45B24}", // XP key
				    @"Wow6432Node\CLSID\{0D68D6D0-D93D-4D08-A30D-F00DD1F45B24}",
				    @"Wow6432Node\Interface\{0D68D6D0-D93D-4D08-A30D-F00DD1F45B24}",
				    @"Wow6432Node\Interface\{1814CEEB-49E2-407F-AF99-FA755A7D2607}",
				    @"Wow6432Node\Interface\{4E747BE5-2052-4265-8AF0-8ECAD7AAD1C0}"
			    }
					);
				}

				// Fill the RegistryPathsFoldersHKEY_CUR_USER list
				List<string> RegistryPathsFoldersHKEY_CUR_USER = new List<string> ();
				using (RegistryKey currentUserKey = Registry.CurrentUser) {
					Additional.CheckAndAddRegPaths (currentUserKey, RegistryPathsFoldersHKEY_CUR_USER, new string[] {
				    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\DDECache\Firefox",
			 	    @"SOFTWARE\Microsoft\Mozilla\Firefox",
			 	    @"SOFTWARE\Mozilla\Firefox"
			    }
					);
					RegistryKey propertyStoreKey = currentUserKey
                     .OpenSubKey (@"Software\Microsoft\Internet Explorer\LowRegistry\Audio\PolicyConfig\PropertyStore", true);
					Additional.AddRegPathsThatRegKeyContains (propertyStoreKey, RegistryPathsFoldersHKEY_CUR_USER);
					if (propertyStoreKey != null)
						propertyStoreKey.Dispose ();
				}

				// Fill the RegistryPathsFoldersHKEY_USERS list
				List<string> RegistryPathsFoldersHKEY_USERS = new List<string> ();
				using (RegistryKey usersKey = Registry.Users) {
					// Check all users and add their PropertyStore sub key that contains "firefox"
					foreach (string usersRootSubKey in usersKey.GetSubKeyNames()) {
						RegistryKey propertyStoreKey = usersKey
                         .OpenSubKey (usersRootSubKey + @"\Software\Microsoft\Internet Explorer\LowRegistry\Audio\PolicyConfig\PropertyStore", true);
						Additional.AddRegPathsThatRegKeyContains (propertyStoreKey, RegistryPathsFoldersHKEY_USERS);
						if (propertyStoreKey != null)
							propertyStoreKey.Dispose ();
					}
				}

				// Fill the RegPathsValuesLCL_MCHN dictionary
				Dictionary<string, List<string>> RegPathsValuesLCL_MCHN = new Dictionary<string, List<string>> ();
				// Temp list for saving values of keys
				List<string> tmpList = new List<string> ();
				tmpList.Add (@"Firefox");
				Additional.CheckAndAddRegKeys (Registry.LocalMachine,
                RegPathsValuesLCL_MCHN,
                new string[] {
                    @"SOFTWARE\RegisteredApplications",
                    @"SOFTWARE\Wow6432Node\RegisteredApplications" 
                },
                tmpList);

				// Fill the RegPathsValuesCLSS_ROOT dictionary
				Dictionary<string, List<string>> RegPathsValuesCLSS_ROOT = new Dictionary<string, List<string>> ();
				using (RegistryKey classesRootKey = Registry.ClassesRoot) {
					RegistryKey classesRootSubKey = classesRootKey.OpenSubKey (@"Local Settings\Software\Microsoft\Windows\Shell\MuiCache");
					Additional.AddRegPathValue (classesRootSubKey, RegPathsValuesCLSS_ROOT);
					if (classesRootSubKey != null)
						classesRootSubKey.Dispose ();
				}

				// Fill the RegPathsValuesCUR_USER dictionary
				Dictionary<string, List<string>> RegPathsValuesCUR_USER = new Dictionary<string, List<string>> ();
				string[] currentUserPaths = new string[] {
                @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.rar\OpenWithList",
                @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.sqlite\OpenWithList",
                @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.zip\OpenWithList",
                @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartPage\NewShortcuts",
                @"Software\Microsoft\Windows\CurrentVersion\Explorer\TypedPaths",
                @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache"
            };
				using (RegistryKey currentUserKey = Registry.CurrentUser) {
					foreach (string currentUserPath in currentUserPaths) {
						RegistryKey currentUserSubKey = currentUserKey.OpenSubKey (currentUserPath);
						Additional.AddRegPathValue (currentUserSubKey, RegPathsValuesCUR_USER);
						if (currentUserSubKey != null)
							currentUserSubKey.Dispose ();
					}
				}

				// Fill the RegPathsValuesUSERS dictionary
				Dictionary<string, List<string>> RegPathsValuesUSERS = new Dictionary<string, List<string>> ();
				using (RegistryKey usersKey = Registry.Users) {
					foreach (string usersRootSubKey in usersKey.GetSubKeyNames()) {
						RegistryKey propertyStoreKey = usersKey
                         .OpenSubKey (usersRootSubKey + @"\Software\Microsoft\Windows\CurrentVersion\Explorer\TypedPaths", true);
						Additional.AddRegPathValue (propertyStoreKey, RegPathsValuesUSERS);
						if (propertyStoreKey != null)
							propertyStoreKey.Dispose ();
					}
				}

				#region Set default browser

				Dictionary<string, string> extsDictionary = new Dictionary<string, string> ();
				extsDictionary.Add (".htm", "htmlfile");
				extsDictionary.Add (".html", "htmlfile");
				extsDictionary.Add (".shtml", "shtmlfile");
				extsDictionary.Add (".xht", "xhtmlfile");
				extsDictionary.Add (".xhtml", "xhtmlfile");
				extsDictionary.Add (@"ftp\DefaultIcon", defaultIconValue);
				extsDictionary.Add (@"ftp\shell\open\command", shellOpenCommandValue);
				extsDictionary.Add (@"http\DefaultIcon", defaultIconValue);
				extsDictionary.Add (@"http\shell\open\command", shellOpenCommandValue);
				extsDictionary.Add (@"https\DefaultIcon", defaultIconValue);
				extsDictionary.Add (@"https\shell\open\command", shellOpenCommandValue);

				Dictionary<string, string> explorerFileExtsDictionary = new Dictionary<string, string> ();
				explorerFileExtsDictionary.Add (".htm", "IE.AssocFile.HTM");
				explorerFileExtsDictionary.Add (".html", "IE.AssocFile.HTM");
				explorerFileExtsDictionary.Add (".xht", "IE.AssocFile.XHT");
				explorerFileExtsDictionary.Add (".xhtml", "IE.AssocFile.XHT");

				Dictionary<string, string> urlAssociationsDictionary = new Dictionary<string, string> ();
				urlAssociationsDictionary.Add ("ftp", "IE.FTP");
				urlAssociationsDictionary.Add ("http", "IE.HTTP");
				urlAssociationsDictionary.Add ("https", "IE.HTTPS");

				// Change HKEY_CLASSES_ROOT values
				using (RegistryKey classesRootKey = Registry.ClassesRoot) {
					Additional.SetValuesToPathsValues (extsDictionary, "", classesRootKey, "", "");
				}

				// Change HKEY_CURRENT_USER values
				using (RegistryKey currentUserKey = Registry.CurrentUser) {
					Additional.SetValuesToPathsValues (extsDictionary, "", currentUserKey, @"Software\Classes\", "");

					RegistryKey startMenuInternetKey = currentUserKey.OpenSubKey (@"Software\Clients\StartMenuInternet", true);
					if (startMenuInternetKey != null) {
						if ((string)startMenuInternetKey.GetValue ("") == "FIREFOX.EXE") {
							startMenuInternetKey.SetValue ("", "IEXPLORE.EXE");
							isFirefoxWasDefaultBrowser = true;
						}
						startMenuInternetKey.Dispose ();
					}

					Additional.SetValuesToPathsValues (explorerFileExtsDictionary, "Progid", currentUserKey,
                    @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\", @"\UserChoice");

					Additional.SetValuesToPathsValues (urlAssociationsDictionary, "Progid", currentUserKey,
                    @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\", @"\UserChoice");
				}

				// Change HKEY_USERS values
				using (RegistryKey usersKey = Registry.Users) {
					foreach (string user in usersKey.GetSubKeyNames()) {
						if (user.Contains ("_Classes")) {
							Additional.SetValuesToPathsValues (extsDictionary, "", usersKey, user + @"\", "");
						} else {
							Additional.SetValuesToPathsValues (extsDictionary, "", usersKey, user + @"\Software\Classes\", "");

							RegistryKey startMenuInternetKey = usersKey.OpenSubKey (user + @"\Software\Clients\StartMenuInternet", true);
							if (startMenuInternetKey != null) {
								if ((string)startMenuInternetKey.GetValue ("") == "FIREFOX.EXE")
									startMenuInternetKey.SetValue ("", "IEXPLORE.EXE");
								startMenuInternetKey.Dispose ();
							}

							Additional.SetValuesToPathsValues (explorerFileExtsDictionary, "Progid", usersKey,
                            user + @"\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\", @"\UserChoice");

							Additional.SetValuesToPathsValues (urlAssociationsDictionary, "Progid", usersKey,
                            user + @"\Software\Microsoft\Windows\Shell\Associations\UrlAssociations\", @"\UserChoice");
						}
					}
				}

				#endregion

				try {
					foreach (string path in RegistryPathsFoldersHKEY_LCL_MCHN)
						using (RegistryKey rk = Registry.LocalMachine)
							rk.DeleteSubKeyTree (path, false);

					foreach (string path in RegistryPathsFoldersHKEY_CLSS_ROOT)
						using (RegistryKey rk = Registry.ClassesRoot)
							rk.DeleteSubKeyTree (path, false);

					foreach (string path in RegistryPathsFoldersHKEY_CUR_USER)
						using (RegistryKey rk = Registry.CurrentUser)
							rk.DeleteSubKeyTree (path, false);

					foreach (string path in RegistryPathsFoldersHKEY_USERS)
						using (RegistryKey rk = Registry.Users)
							rk.DeleteSubKeyTree (path, false);

					foreach (KeyValuePair<string, List<string>> key in RegPathsValuesLCL_MCHN) {
						RegistryKey mainKey = Registry.LocalMachine;
						RegistryKey subKey = mainKey.OpenSubKey (key.Key, true);
						foreach (string keyVal in key.Value)
							subKey.DeleteValue (keyVal, true);
						subKey.Dispose ();
						mainKey.Dispose ();
					}

					foreach (KeyValuePair<string, List<string>> key in RegPathsValuesCLSS_ROOT) {
						RegistryKey mainKey = Registry.ClassesRoot;
						RegistryKey subKey = mainKey.OpenSubKey (key.Key, true);
						foreach (string keyVal in key.Value)
							subKey.DeleteValue (keyVal, false);
						subKey.Dispose ();
						mainKey.Dispose ();
					}

					foreach (KeyValuePair<string, List<string>> key in RegPathsValuesCUR_USER) {
						RegistryKey mainKey = Registry.CurrentUser;
						RegistryKey subKey = mainKey.OpenSubKey (key.Key, true); 
						foreach (string keyVal in key.Value)
							subKey.DeleteValue (keyVal, false);
						subKey.Dispose ();
						mainKey.Dispose ();
					}

					foreach (KeyValuePair<string, List<string>> key in RegPathsValuesUSERS) {
						RegistryKey mainKey = Registry.Users;
						RegistryKey subKey = mainKey.OpenSubKey (key.Key, true);
						foreach (string keyVal in key.Value)
							subKey.DeleteValue (keyVal, false);
						subKey.Dispose ();
						mainKey.Dispose ();
					}
				} catch { }
			}

            return isFirefoxWasDefaultBrowser;
		}

		private static void SortOutputHandler (object sendingProcess, 
            DataReceivedEventArgs outLine)
		{
			if(outLine.Data == null) return;

			if (outLine.Data.Contains ("disk space will be freed")) {
				term.StandardInput.WriteLine ("y");
				term.StandardInput.Close();
				term.CancelOutputRead();
			} else if (outLine.Data.Contains ("[sudo] password for"))
				term.StandardInput.WriteLine (UserPasswd);
			else if (outLine.Data.Contains ("Package 'firefox' is not installed")) {
				term.StandardInput.Close();
				term.CancelOutputRead();
			}
		}
	}
	 
}

