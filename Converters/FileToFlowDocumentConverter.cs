using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace SearchSharp.Converters
{
    public class FileToFlowDocumentConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FlowDocument doc = new FlowDocument();
            var fileContent = value[1] as string;

            if (!(value[0] is int) || fileContent == null)
            {
                return doc;
            }

            var blue = new SolidColorBrush(Colors.Blue);
            var yellow = new SolidColorBrush(Colors.Yellow);
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
                    var contentSearchParameters = value[3] as FileContentSearchParameters;

                    if (!String.IsNullOrEmpty(contentSearchParameters.ContainingText) && 
                        !contentSearchParameters.ContainingTextNot && 
                        contentSearchParameters.ContainingTextRegex)
                    {
                        // Regex highlighting must support multi-line so we do this way differently.
                        var paragraph = new Paragraph();
                        var options = contentSearchParameters.ContainingTextMatchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
                        options |= contentSearchParameters.ContainingTextRegexOptions;
                        var regex = new Regex(contentSearchParameters.ContainingText, options);
                        var matches = regex.Matches(fileContent);
                        int index = 0;
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

                            paragraph.Inlines.Add(new Run(nonHighlighted));
                            paragraph.Inlines.Add(new Run(highlighted){ Background = yellow});
                        
                            index = match.Index + match.Length;
                        }

                        if (index < fileContent.Length)
                        {
                            paragraph.Inlines.Add(new Run(fileContent.Substring(index)));
                        }

                        doc.Blocks.Add(paragraph);
                    }
                    else
                    {
                        using (StringReader reader = new StringReader(fileContent))
                        {
                            string line;
                            int lineNumber = 1;
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
                                            run.Background = yellow;
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

                                doc.Blocks.Add(paragraph);
                                lineNumber++;
                            }
                        }
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
