using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using SearchSharp.ViewModels;

namespace SearchSharp
{
    class SearchExecutor
    {
        private MainWindowViewModel mViewModel;
        private BackgroundWorker _worker;

        public SearchExecutor(MainWindowViewModel viewModel)
        {
            mViewModel = viewModel;
        }


        public void DirSearch(string path)
        {
            ProcessFilesInFolder(path);

            if (!mViewModel.Recurse)
                return;

            string[] dirs;
            try
            {
                dirs = Directory.GetDirectories(path);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(String.Format("Failed to enter '{0}' - {1}", path, ex.Message));
                return;
            }

            foreach (string dir in dirs)
            {
                if (_worker.CancellationPending)
                {
                    return;
                }

                DirSearch(dir);
            }
        }

        private void ProcessFilesInFolder(string dir)
        {
            string[] allFiles;
            try
            {
                allFiles = Directory.GetFiles(dir);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(String.Format("Failed to list files in '{0}' - {1}", dir, ex.Message));
                return;
            }

            Regex regex = null;
            RegexOptions options = mViewModel.FileSpecMatchCase ? RegexOptions.Compiled : RegexOptions.Compiled | RegexOptions.IgnoreCase;
            if (mViewModel.FileSpecRegex)
            {
                options |= mViewModel.FileSpecRegexOptions;
                try
                {
                    regex = new Regex(mViewModel.FileSpec, options);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("Invalid regex pattern '{0}' - {1}", mViewModel.FileSpecRegex, ex.Message));
                    return;
                }
            }
            else
            {
                try
                {
                    regex = FindFilesPatternToRegex.Convert(mViewModel.FileSpec, options);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("Invalid wildcard pattern '{0}' - {1}", mViewModel.FileSpecRegex, ex.Message));
                    return;
                }
            }

            var matchingFileNames = new List<string>();
            foreach (var file in allFiles)
            {
                if (_worker.CancellationPending)
                {
                    return;
                }

                var isMatch = regex.IsMatch(Path.GetFileName(file));
                if (isMatch != mViewModel.FileSpecNot)
                {
                    matchingFileNames.Add(file);
                }
            }

            foreach (string file in matchingFileNames)
            {
                if (_worker.CancellationPending)
                {
                    return;
                }

                ProcessFile(file);
            }
        }

        private void ProcessFile(string file)
        {
            bool match = true;
            if (!String.IsNullOrEmpty(mViewModel.ContainingText))
            {
                match = false;  // Assume false
                string content = null;
                try
                {
                    content = File.ReadAllText(file);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("Failed to read '{0}' - {1}", file, ex.Message));
                }

                if (content != null)
                {
                    if (mViewModel.ContainingTextRegex)
                    {
                        RegexOptions options = mViewModel.ContainingTextMatchCase ? RegexOptions.Compiled : RegexOptions.Compiled | RegexOptions.IgnoreCase;
                        options |= mViewModel.ContainingTextRegexOptions;
                        Regex regex = null;
                        try
                        {
                            regex = new Regex(mViewModel.ContainingText, options);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(String.Format("Invalid regex pattern '{0}' - {1}", mViewModel.ContainingText, ex.Message));
                            return;
                        }

                        match = regex.IsMatch(content);
                    }
                    else
                    {
                        match = mViewModel.ContainingTextMatchCase ? 
                            content.Contains(mViewModel.ContainingText) : 
                            content.ToLowerInvariant().Contains(mViewModel.ContainingText.ToLowerInvariant());
                    }

                    if (mViewModel.ContainingTextNot)
                    {
                        match = !match;
                    }
                }
            }

            if (match)
            {
                mViewModel.AddFileToResult(file);
            }
        }

        public void StartSearch(BackgroundWorker worker)
        {
            _worker = worker;
            DirSearch(mViewModel.SearchPath);
        }
    }
}
