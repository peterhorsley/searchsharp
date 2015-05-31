using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using SearchSharp.ViewModels;

namespace SearchSharp.Converters
{
    public class FileToFlowDocumentConverter : IMultiValueConverter
    {
        private SolidColorBrush _blue = new SolidColorBrush(Colors.Blue);
        private SolidColorBrush _yellow = new SolidColorBrush(Colors.Yellow);
        private int _linesOfContext = 2;

        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FlowDocument doc = new FlowDocument();
            var fileContent = value[1] as string;

            if (!(value[0] is int) || fileContent == null)
            {
                return doc;
            }

            var fileCount = (int)value[0];
            if (fileCount != 1)
            {
                var paragraph = new Paragraph();
                paragraph.Inlines.Add(String.Format("{0} files selected.", fileCount));
                doc.Blocks.Add(paragraph);
            }
            else
            {
                var tooBig = (bool)value[2];
                if (tooBig)
                {
                    doc.Blocks.Add(new Paragraph(new Run("Select file is too big to display.")));
                }
                else
                {
                    var contentSearchParameters = value[5] as FileContentSearchParameters;
                    var showFullContent = (bool)value[3] || String.IsNullOrEmpty(contentSearchParameters.ContainingText) || contentSearchParameters.ContainingTextNot;
                    _linesOfContext = (int)value[4];
                    var paragraphs = new List<Paragraph>();

                    if (!String.IsNullOrEmpty(contentSearchParameters.ContainingText) &&
                        !contentSearchParameters.ContainingTextNot &&
                        contentSearchParameters.ContainingTextRegex)
                    {
                        // Regex highlighting must support multi-line so we do this way differently.
                        var options = contentSearchParameters.ContainingTextMatchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
                        options |= contentSearchParameters.ContainingTextRegexOptions;

                        Regex regex = null;
                        try
                        {
                            regex = new Regex(contentSearchParameters.ContainingText, options);
                        }
                        catch (ArgumentException)
                        {
                            // It's possible for the user to enter an invalid regex, just ignore.
                        }

                        int index = 0;
                        if (regex != null)
                        {
                            var matches = regex.Matches(fileContent);
                            foreach (Match match in matches)
                            {
                                var nonHighlighted = fileContent.Substring(index, match.Index - index);
                                var highlighted = fileContent.Substring(match.Index, match.Length);
                                if (nonHighlighted.EndsWith("\r"))
                                {
                                    nonHighlighted = nonHighlighted.Substring(0, nonHighlighted.Length - 1);
                                }
                                if (highlighted.EndsWith("\r"))
                                {
                                    highlighted = highlighted.Substring(0, highlighted.Length - 1);
                                }


                                bool append = paragraphs.Count > 0;
                                foreach (var line in TextContentViewModel.SplitStringIntoLines(nonHighlighted))
                                {
                                    var run = new Run(line);
                                    if (append)
                                    {
                                        paragraphs[paragraphs.Count - 1].Inlines.Add(run);
                                        append = false;
                                    }
                                    else
                                    {
                                        var paragraph = new Paragraph();
                                        paragraph.Inlines.Add(run);
                                        paragraphs.Add(paragraph);
                                    }
                                }

                                append = true;
                                foreach (var line in TextContentViewModel.SplitStringIntoLines(highlighted))
                                {
                                    var run = new Run(line) { Background = _yellow };
                                    if (append)
                                    {
                                        paragraphs[paragraphs.Count - 1].Inlines.Add(run);
                                        append = false;
                                    }
                                    else
                                    {
                                        var paragraph = new Paragraph();
                                        paragraph.Inlines.Add(run);
                                        paragraphs.Add(paragraph);
                                    }
                                }

                                index = match.Index + match.Length;
                            }

                            if (index < fileContent.Length)
                            {
                                bool append = paragraphs.Count > 0;
                                foreach (var line in TextContentViewModel.SplitStringIntoLines(fileContent.Substring(index)))
                                {
                                    var run = new Run(line);
                                    if (append)
                                    {
                                        paragraphs[paragraphs.Count - 1].Inlines.Add(run);
                                        append = false;
                                    }
                                    else
                                    {
                                        var paragraph = new Paragraph();
                                        paragraph.Inlines.Add(run);
                                        paragraphs.Add(paragraph);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var line in TextContentViewModel.SplitStringIntoLines(fileContent))
                            {
                                var paragraph = new Paragraph();
                                paragraph.Inlines.Add(new Run(line));
                                paragraphs.Add(paragraph);
                            }
                        }
                    }
                    else
                    {
                        using (StringReader reader = new StringReader(fileContent))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                var paragraph = new Paragraph();
                                if (String.IsNullOrEmpty(contentSearchParameters.ContainingText) || contentSearchParameters.ContainingTextNot)
                                {
                                    paragraph.Inlines.Add(new Run(line));
                                }
                                else
                                {
                                    var comparison = contentSearchParameters.ContainingTextMatchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
                                    int index = 0;

                                    while (true)
                                    {
                                        var nextIndex = line.IndexOf(contentSearchParameters.ContainingText, index, comparison);
                                        if (nextIndex >= 0)
                                        {
                                            paragraph.Inlines.Add(new Run(line.Substring(index, nextIndex - index)));
                                            var run = new Run(line.Substring(nextIndex, contentSearchParameters.ContainingText.Length));
                                            run.Background = _yellow;
                                            paragraph.Inlines.Add(run);
                                            index = nextIndex + contentSearchParameters.ContainingText.Length;
                                        }
                                        else
                                        {
                                            var run = new Run(line.Substring(index));
                                            paragraph.Inlines.Add(run);
                                            break;
                                        }
                                    }
                                }

                                paragraphs.Add(paragraph);
                            }
                        }
                    }

                    if (!showFullContent)
                    {
                        var lineNumbers = new List<string>();
                        bool elipsesAdded = false;
                        var indexesAdded = new List<int>();
                        for (int i = 0; i < paragraphs.Count; i++)
                        {
                            var para = paragraphs[i];
                            if (ContainsHighlighting(para))
                            {
                                int originalLineIndex = i;
                                int indexAFewLinesAgo = Math.Max(0, i - _linesOfContext);
                                i = Math.Min(paragraphs.Count - 1, i + _linesOfContext);
                                while (indexAFewLinesAgo <= i)
                                {
                                    if (!indexesAdded.Contains(indexAFewLinesAgo))
                                    {
                                        if (indexAFewLinesAgo > originalLineIndex && ContainsHighlighting(paragraphs[indexAFewLinesAgo]))
                                        {
                                            i = indexAFewLinesAgo - 1;
                                            break;
                                        }
                                        doc.Blocks.Add(paragraphs[indexAFewLinesAgo]);
                                        indexesAdded.Add(indexAFewLinesAgo);
                                        lineNumbers.Add((indexAFewLinesAgo + 1).ToString());
                                    }
                                    indexAFewLinesAgo++;
                                }
                                elipsesAdded = false;
                            }
                            else if (!elipsesAdded)
                            {
                                var elipsesPara = new Paragraph();
                                var elipsesText = GetElipsesText();
                                elipsesPara.Inlines.Add(elipsesText);
                                lineNumbers.Add(""); // Add a blank line number
                                doc.Blocks.Add(elipsesPara);
                                elipsesAdded = true;
                            }
                        }
                        contentSearchParameters.ContentViewModel.SetLineNumbers(lineNumbers);
                    }
                    else
                    {
                        doc.Blocks.AddRange(paragraphs);
                        var lineNumbers = new List<string>();
                        for (int i = 1; i <= paragraphs.Count; i++)
                        {
                            lineNumbers.Add(i.ToString());
                        }
                        contentSearchParameters.ContentViewModel.SetLineNumbers(lineNumbers);
                    }
                }
            }

            double longestLine = 0;

            foreach (Paragraph p in doc.Blocks)
            {
                StringBuilder lineText = new StringBuilder();

                foreach (Run r in p.Inlines)
                {
                    lineText.Append(r.Text);
                }

                var line = lineText.ToString();
                FormattedText f = new FormattedText(line,
                    Thread.CurrentThread.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Consolas"), 12, Brushes.Black);

                longestLine = f.Width > longestLine ? f.Width : longestLine;
            }

            doc.PageWidth = longestLine + 50;
            return doc;
        }

        private Run GetElipsesText()
        {
            var elipsesText = new Run("...") { Foreground = _blue };
            return elipsesText;
        }

        private bool ContainsHighlighting(Paragraph para)
        {
            foreach (Run run in para.Inlines)
            {
                if (run.Background == _yellow)
                {
                    return true;
                }
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
