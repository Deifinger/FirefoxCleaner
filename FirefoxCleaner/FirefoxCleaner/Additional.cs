using Gtk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using Microsoft.Win32;
using Mono.Data.Sqlite;

namespace FirefoxCleaner
{
	static public class Additional
	{
		public static void SetDBCommand(string sqlite_connection, string sqlite_command)
		{
			try
			{
				// Data connection
				SqliteConnection sql_con = new SqliteConnection(sqlite_connection);

				// Open the Conn
				sql_con.Open();

				// Create command
				SqliteCommand sql_cmd = new SqliteCommand(sqlite_command, sql_con);

				sql_cmd.ExecuteNonQuery();

				// Clean up
				sql_con.Dispose();
			}
			catch(Exception ex) {
					throw new ApplicationException("ERROR DATABASE. Database path: " + 
						sqlite_connection.Substring(sqlite_connection.IndexOf('=') + 1, sqlite_connection.IndexOf(';') - (sqlite_connection.IndexOf('=') + 1)) +
				        "\nMessage: " + ex.Message);
			}
		}

		public static void DeleteFromTable(string dbname, string table, string folder, string conditionWithWHERE = "")
		{
			// FireFox database file
			string dbPath = folder + "/" + dbname;

			// If file exists
			if (File.Exists(dbPath))
			{
				// Delete Query
				SetDBCommand ("Data Source=" + dbPath + "; Version=3; New=False; Compress=True;", "delete from " + table + " " + conditionWithWHERE);				
			}
		}

		public static void DeleteFromTableInRoamingUserProfiles(string dbname, string[] tables, string roamingProfilesPath, string conditionWithWHERE = "")
		{
			// Check if directory exists
			if (Directory.Exists(roamingProfilesPath))
			{
				// Loop each Firefox Profile
				foreach (string folder in Directory.GetDirectories(roamingProfilesPath))
				{
					if(folder.Contains("Crash Reports")) continue; // For Unix system
					// Remove Info from tables
					foreach(string table in tables)
						Additional.DeleteFromTable(dbname, table, folder, conditionWithWHERE);
				}
			}
		}

		/// <summary>
		/// Clears the folder.
		/// </summary>
		/// <param name="folder">Folder.</param>
		/// <param name="extension">Extension of file. If extention set - method clear only files with this extension. Example ".ini"</param>
		public static void ClearFolder (string folder, string extension = "")
		{
			DirectoryInfo directory = new DirectoryInfo (folder);

			// Delete files:
			foreach (FileInfo f in directory.GetFiles()) {
				if (extension != "") {
					if (f.Extension == extension)
						f.Delete ();
				} else
					f.Delete ();
			}
		

			// Delete folders inside choosen folder
			if(extension == "")
				foreach (DirectoryInfo dir in directory.GetDirectories())
					dir.Delete(true);
		}

		public static void ClearAllFoldersInDirectory(string directory, string[] pathInDiretory = null,
			string[] extension = null)
		{
			// Check if directory exists
			if (Directory.Exists(directory))
			{
				// Check pathInProfiles and extension
				if (pathInDiretory == null)
					pathInDiretory = new string[] { "" };
				if (extension == null)
					extension = new string[] { "" };

				// Alignment of arrays if their length is not equal
				int lenPIP = pathInDiretory.Length, lenExt = extension.Length;
				if (lenPIP > lenExt) {
					string[] tmp = new string[lenPIP];
					for (int i = 0; i < lenExt; i++)
						tmp [i] = extension [i];
					for (int i = lenExt; i < lenPIP; i++)
						tmp [i] = "";
					extension = tmp;
				} else if (lenExt > lenPIP) {
					string[] tmp = new string[lenExt];
					for (int i = 0; i < lenPIP; i++)
						tmp [i] = pathInDiretory [i];
					for (int i = lenPIP; i < lenExt; i++)
						tmp [i] = "";
					pathInDiretory = tmp;
				}

				// Loop each Firefox Profile
				foreach (string folder in Directory.GetDirectories(directory))
				{
					// Clear Folders
					for (int i = 0; i < lenPIP; i++) {
                        if (Directory.Exists(folder + pathInDiretory[i]))
						    Additional.ClearFolder (folder + pathInDiretory[i], extension[i]);
					}
				}
			}
		}

		public static void DeleteFilesFromAllProfiles(string directory, string[] pathInProfiles)
		{
			// Check if directory exists
			if (Directory.Exists(directory))
			{
				// Loop each Firefox Profile
				foreach (string folder in Directory.GetDirectories(directory))
				{
					// Delete files from Profile
					foreach(string subfolder in pathInProfiles)
						File.Delete(folder + subfolder);
				}
			}
		}

		public static void IsFirefoxRunning()
		{
			int procCount = Process.GetProcessesByName ("firefox").Length;
			if (procCount > 0)
				throw new ApplicationException (string.Format ("There are {0} instanses of Firefox still running", procCount));
		}

		public static void CreateEmptyFileInAllProfiles(string directory, string filename)
		{
			// Check if directory exists
			if (Directory.Exists(directory))
			{
				// Loop each Firefox Profile
				foreach (string folder in Directory.GetDirectories(directory))
				{
					// Remove old file and create new empty file in Profile
					using (File.Create (folder + "/" + filename)){};
				}
			}
		}

		public static string[] GetFilesFromDirectories(string[] directories, string[] searchPatterns, SearchOption so)
		{
			// List for save get results
			List<string[]> result = new List<string[]> ();

			// Get needed files
			foreach (string dir in directories) {
				foreach (string pattern in searchPatterns) {
                    if (Directory.Exists(dir))
					    // Get pattern files from folder
					    result.Add (Directory.GetFiles (dir, pattern, so));
				}
			}

			// Get count of files
			int count = 0;
			foreach (string[] files in result)
				count += files.Length;

			// Create static array
			string[] dirsArray = new string[count];

			// Write all files to array
			int counter = 0;
			foreach (string[] array in result) {
				foreach (string file in array) {
					dirsArray[counter++] = file;
				}
			}

			// Return result of method
			return dirsArray;
		}

		public static void SearchFilesAndRemoveIt (string[] directories, string[] searchPatterns, SearchOption so)
		{
			// Get all pattern files
			string[] files = Additional.GetFilesFromDirectories (directories, searchPatterns, so);

			// Delete this files
			foreach (string file in files)
			{
				if (File.Exists (file)) {
					try
					{
						//Process.Start("sudo chmod", "777 '" + Path.GetDirectoryName(file) + "'");
						File.Delete (file);
					}
					catch
					{
						if(Cleaner.osPlatform == PlatformID.Unix)
						{
							Process term = new Process();
							term.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
							term.StartInfo.FileName = "sudo";
							term.StartInfo.Arguments = "rm -r" + file;
							term.StartInfo.UseShellExecute = false;
							term.StartInfo.RedirectStandardInput = true;
							term.Start ();

							StreamWriter swTerm = term.StandardInput;
							swTerm.WriteLine(Cleaner.UserPasswd);
						}
					}
				}
			}
		}

        /// <summary>
        /// Checks keys name and keys value in RegPath and
        /// if one of they contains "firefox" add keys name to RegPathsValues dictionary
        /// </summary>
        /// <param name="regPath"></param>
        /// <param name="regPathsValues"></param>
		public static void AddRegPathValue(RegistryKey regPath, Dictionary<string, List<string>> regPathsValues)
		{
			if (regPath == null)
				return;

			foreach(string val in regPath.GetValueNames())
			{
				string key = regPath.Name.Remove (0, (regPath.Name.IndexOf ('/') + 1));
				List<string> listValues = new List<string> ();

				//Name of key
				if (val.ToLower().Contains ("firefox")) {
					if (regPathsValues.ContainsKey (key))
						regPathsValues [key].Add (val);
					else {
						listValues.Add (val);
						regPathsValues.Add (key, listValues);
					}
					continue;
				}
				try
				{
					//Value of key
					string value = (string)regPath.GetValue (val);
					if (value.ToLower().Contains ("firefox")) {
						if (regPathsValues.ContainsKey (key))
							regPathsValues [key].Add (val);
						else {
							listValues.Add (val);
							regPathsValues.Add (key, listValues);
						}
					}
				}
				catch {
					continue;
				}
			}
		}
        /// <summary>
        /// Analog AddRegPathValue method, but add to List<string> RegPathsValues only paths
        /// </summary>
        /// <param name="regPath"></param>
        /// <param name="regPathsValues"></param>
        public static void AddRegPathValue(RegistryKey regPath, List<string> regPathsValues)
        {
            if (regPath == null) return;
            Dictionary<string, List<string>> tmpRegPathsValues = new Dictionary<string, List<string>>();
            AddRegPathValue(regPath, tmpRegPathsValues);
            foreach (KeyValuePair<string, List<string>> kvp in tmpRegPathsValues)
                regPathsValues.Add(kvp.Key);
        }
        /// <summary>
        /// Uses Additional.AddRegPathValue(). Checks all sub keys that subKey contains.
        /// If sub keys of subKey or any key name or key value contains "firefox" when they adds to registryPathsFolders
        /// </summary>
        /// <param name="subKey"></param>
        /// <param name="registryPathsFolders"></param>
        public static void AddRegPathsThatRegKeyContains(RegistryKey subKey, List<string> registryPathsFolders)
        {
            if (subKey == null) return;
            string[] subKeyFoldersNames = subKey.GetSubKeyNames();
            foreach (string foldersName in subKeyFoldersNames)
            {
                using (RegistryKey folderSubKey = subKey.OpenSubKey(foldersName))
                {
                    Additional.AddRegPathValue(folderSubKey, registryPathsFolders);
                }
            }
        }

        /// <summary>
        /// Returns all sub keys that contains searchValue
        /// </summary>
        /// <param name="regPath"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
		public static string[] GetSubKeys(RegistryKey regPath, string searchValue)
		{
            if (regPath == null) return new string[0];
			List<string> res = new List<string> ();
			foreach (string subKey in regPath.GetSubKeyNames()) {
				if (subKey.Contains (searchValue))
					res.Add (regPath.Name.Remove(0, (regPath.Name.IndexOf('/') + 1)) + "/" + subKey);
			}
			return res.ToArray ();
		}

        public static void CheckAndAddRegPaths(RegistryKey regPath, List<string> registryPathsFolders, string[] paths)
        {
            if (regPath == null) return;
            foreach (string path in paths)
            {
                if(regPath.OpenSubKey(path) != null)
                    registryPathsFolders.Add(path);
            }
        }

        public static void CheckAndAddRegKeys(RegistryKey regPath, Dictionary<string, List<string>> regPathsValues, 
            string[] paths, List<string> keynames)
        {
            if (regPath == null) return;
            foreach (string path in paths)
            {
                if (regPath.OpenSubKey(path) != null)
                    regPathsValues.Add(path, keynames);
            }
        }

        /// <summary>
        /// Changes values if they contains "Firefox"
        /// 
        /// Changes all value in keys named "keyNameForValueChange" to "subkeysValues.Value" in subkey "subkeysValues.Key"
        /// Path construct: "root".OpenSubKey("leftPath" + "subkeyValue.Key" + "rightPath", true);
        /// If you want change values in root, set leftPath = ""
        /// </summary>
        /// <param name="subkeysValues"></param>
        /// <param name="keyNameForValueChange"></param>
        /// <param name="root"></param>
        /// <param name="leftPath"></param>
        /// <param name="rightPath"></param>
        public static void SetValuesToPathsValues(Dictionary<string, string> subkeysValues, string keyNameForValueChange,
            RegistryKey root, string leftPath = "", string rightPath = "")
        {
            foreach (KeyValuePair<string, string> subkeyValue in subkeysValues)
            {
                try
                {
                    RegistryKey extKey = root.OpenSubKey(leftPath + subkeyValue.Key + rightPath, true);

                    if (extKey == null) continue;

                    if (((string)extKey.GetValue(keyNameForValueChange)).Contains("Firefox"))
                        extKey.SetValue(keyNameForValueChange, subkeyValue.Value);

                    extKey.Dispose();
                }
                catch
                { }
            }
        }

		/// <summary>
		/// Gets the message box.
		/// </summary>
		/// <returns>
		/// response values:
        /// -4 - MessageDialog was closed
		/// -5 - OK
		/// -6 - CANCEL
        /// -8 - YES
        /// -9 - NO
		/// </returns>
		/// <param name='wndw'>
		/// Wndw.
		/// </param>
		/// <param name='title'>
		/// Title.
		/// </param>
		/// <param name='text'>
		/// Text.
		/// </param>
		/// <param name='secondarytext'>
		/// Secondarytext.
		/// </param>
		/// <param name='dflag'>
		/// Dflag.
		/// </param>
		/// <param name='mesType'>
		/// Mes type.
		/// </param>
		/// <param name='butType'>
		/// </param>
		public static int GetMessageBox(Window wndw, string title, string text, string secondarytext, 
		                   DialogFlags dflag, MessageType mesType, ButtonsType butType)
		{
			MessageDialog md = new MessageDialog (wndw, dflag, mesType, butType, "");
			md.Title = title;
			md.Text = text;
			md.SecondaryText = secondarytext;
			int response = md.Run ();
			md.Destroy ();
			return response;
		}
	}
}

