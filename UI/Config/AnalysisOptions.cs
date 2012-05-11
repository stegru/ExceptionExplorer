// -----------------------------------------------------------------------
// <copyright file="AnalysisOptions.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ExceptionExplorer.Config
{
    /// <summary>
    /// How to use the XML documentation
    /// </summary>
    public enum XmlDocumentationUsage
    {
        /// <summary>Never use it.</summary>
        Never = 0,

        /// <summary>Use it, if available.</summary>
        Prefer = 1,

        /// <summary>Use it, if available and combine with the analysis.</summary>
        Combine = 2,

        /// <summary>Only use it.</summary>
        Only = 3
    }

    public class AnalysisOptions : OptionsBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether to only analyse code in the same assembly.
        /// </summary>
        /// <value>
        ///   <c>true</c> to keep within the assembly; otherwise, <c>false</c>.
        /// </value>
        public Setting<bool> SameAssembly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to only analyse code in the same class.
        /// </summary>
        /// <value>
        ///   <c>true</c> to keep within the class; otherwise, <c>false</c>.
        /// </value>
        public Setting<bool> SameClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to not analyse system/framework code.
        /// </summary>
        /// <value>
        ///   <c>true</c> include the framework; otherwise, <c>false</c>.
        /// </value>
        public Setting<bool> IncludeFramework { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to only use the documented exceptions for the framework.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to use framework documentation; otherwise, <c>false</c>.
        /// </value>
        public Setting<bool> UseFrameworkDocumented { get; set; }

        /// <summary>
        /// Gets or sets the XML documentation usage.
        /// </summary>
        /// <value>The XML documentation.</value>
        public Setting<XmlDocumentationUsage> XmlDocumentation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore the auto-generated event methods.
        /// </summary>
        /// <value>
        ///   <c>true</c> to ignore event methods; otherwise, <c>false</c>.
        /// </value>
        public Setting<bool> IgnoreEventMethods { get; set; }

        public AnalysisOptions()
            : base()
        {
            this.IgnoreEventMethods.Value = true;
            this.IncludeFramework.Value = true;
            this.SameAssembly.Value = false;
            this.SameClass.Value = false;
            this.UseFrameworkDocumented.Value = false;
            this.XmlDocumentation.Value = XmlDocumentationUsage.Prefer;
        }
    }
}