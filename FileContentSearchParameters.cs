using System.Text.RegularExpressions;
using SearchSharp.ViewModels;

namespace SearchSharp
{
    /// <summary>
    /// Represents the parameters used to match content in a file.
    /// </summary>
    public class FileContentSearchParameters
    {
        public string ContainingText { get; set; }
        public bool ContainingTextMatchCase { get; set; }
        public bool ContainingTextNot { get; set; }
        public bool ContainingTextRegex { get; set; }
        public RegexOptions ContainingTextRegexOptions { get; set; }
        public TextContentViewModel ContentViewModel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileContentSearchParameters" /> class.
        /// </summary>
        /// <param name="containingText">The containing text.</param>
        /// <param name="containingTextMatchCase">if set to <c>true</c> [containing text match case].</param>
        /// <param name="containingTextNot">if set to <c>true</c> [containing text not].</param>
        /// <param name="containingTextRegex">if set to <c>true</c> [containing text regex].</param>
        /// <param name="containingTextRegexOptions">The containing text regex options.</param>
        /// <param name="contentViewModel">The content view model.</param>
        public FileContentSearchParameters(
            string containingText, 
            bool containingTextMatchCase, 
            bool containingTextNot, 
            bool containingTextRegex, 
            RegexOptions containingTextRegexOptions, 
            TextContentViewModel contentViewModel)
        {
            ContainingText = containingText;
            ContainingTextMatchCase = containingTextMatchCase;
            ContainingTextNot = containingTextNot;
            ContainingTextRegex = containingTextRegex;
            ContainingTextRegexOptions = containingTextRegexOptions;
            ContentViewModel = contentViewModel;
        }
    }
}
