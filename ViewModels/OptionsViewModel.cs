using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SearchSharp.ViewModels
{
    public class OptionsViewModel : ViewModel
    {
        private const string _searchSharpKeyName = "SearchSharp";
        private const string _commandKeyName = "command";
        private const string _shellKeyName = "shell";
        private const string _directoryKeyName = "Directory";
        private const string _driveName = "Drive";

        private bool _contextMenuEnabled;

        public OptionsViewModel()
        {
            _contextMenuEnabled = IsShellMenuEnabled();
        }

        public bool ContextMenuEnabled
        {
            get { return _contextMenuEnabled; }
            set
            {
                if (_contextMenuEnabled != value)
                {
                    _contextMenuEnabled = value;
                    SetContextMenu(value);
                    RaisePropertyChanged("ContextMenuEnabled");
                }
            }
        }

        private void SetContextMenu(bool enabled)
        {
            if (IsShellMenuEnabled() == enabled)
            {
                // Nothing to do.
                return;
            }

            if (!enabled)
            {
                // Get rid of the menus.
                DeleteSearchSharpKey(GetDirectoryShellKey());
                DeleteSearchSharpKey(GetDriveShellKey());
            }
            else
            {
                // Create the menus.
                // Drive uses a different command argument to directory.
                CreateSearchSharpKey(GetDirectoryShellKey(), "%1");
                CreateSearchSharpKey(GetDriveShellKey(), "%L"); 
            }
        }

        private static void CreateSearchSharpKey(RegistryKey shellKey, string commandArg)
        {
            var searchSharpKey = shellKey.CreateSubKey(_searchSharpKeyName);
            searchSharpKey.SetValue("", "Search using SearchSharp");
            searchSharpKey.SetValue("Icon", String.Format("\"{0}\",0", Assembly.GetExecutingAssembly().Location),
                RegistryValueKind.String);
            var commandKey = searchSharpKey.CreateSubKey(_commandKeyName);
            commandKey.SetValue("", String.Format("\"{0}\" \"{1}\"", Assembly.GetExecutingAssembly().Location, commandArg),
                RegistryValueKind.String);
            commandKey.Close();
            searchSharpKey.Close();
            shellKey.Close();
        }

        private static void DeleteSearchSharpKey(RegistryKey shell)
        {
            var searchSharpKey = shell.OpenSubKey(_searchSharpKeyName, true);
            searchSharpKey.DeleteSubKey(_commandKeyName);
            searchSharpKey.Close();
            shell.DeleteSubKey(_searchSharpKeyName);
            shell.Close();
        }

        private RegistryKey GetDirectoryShellKey()
        {
            return GetShellKey(_directoryKeyName);
        }

        private RegistryKey GetDriveShellKey()
        {
            return GetShellKey(_driveName);
        }

        private RegistryKey GetShellKey(string rootKeyName)
        {
            RegistryKey dirKey = Registry.ClassesRoot.OpenSubKey(rootKeyName, true);
            if (dirKey == null)
            {
                dirKey = Registry.ClassesRoot.CreateSubKey(rootKeyName);
            }

            RegistryKey shellKey = dirKey.OpenSubKey(_shellKeyName, true);
            if (shellKey == null)
            {
                shellKey = dirKey.CreateSubKey(rootKeyName);
            }

            return shellKey;
        }

        private RegistryKey GetSearchSharpDirectoryKey()
        {
            return GetDirectoryShellKey().OpenSubKey(_searchSharpKeyName);
        }

        private RegistryKey GetSearchSharpDriveKey()
        {
            return GetDriveShellKey().OpenSubKey(_searchSharpKeyName);
        }

        private bool IsShellMenuEnabled()
        {
            return GetSearchSharpDirectoryKey() != null &&
                GetSearchSharpDriveKey() != null;
        }
    }
}
