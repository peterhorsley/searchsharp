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

            var matchingFileNames = new List<string>();
            if (mViewModel.FileSpec == "")
            {
                matchingFileNames.AddRange(allFiles);
            }
            else
            {
                foreach (var file in allFiles)
                {
                    if (_worker.CancellationPending || mViewModel.CompiledFileSpecRegex == null)
                    {
                        return;
                    }

                    var fileName = Path.GetFileName(file);
                    var isMatch = mViewModel.CompiledFileSpecRegex.IsMatch(fileName);

                    // We also check if the filename contains the file spec string directly
                    // if we are not using regex, so as to assume wildcards on both sides of the file spec.
                    if (!isMatch && !mViewModel.FileSpecRegex)
                    {
                        var spec = mViewModel.FileSpec;
                        if (!mViewModel.FileSpecMatchCase)
                        {
                            fileName = fileName.ToLowerInvariant();
                            spec = spec.ToLowerInvariant();
                        }

                        if (fileName.Contains(spec))
                        {
                            isMatch = true;
                        }
                    }

                    if (isMatch != mViewModel.FileSpecNot)
                    {
                        matchingFileNames.Add(file);
                    }
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
                        match = mViewModel.CompiledContainingTextRegex.IsMatch(content);
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
