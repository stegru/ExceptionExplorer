namespace Decompiler
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using Microsoft.Win32;

    /// <summary>Types of syntax parts.</summary>
    public enum SyntaxType
    {
        /// <summary>Text, nothing interesting.</summary>
        Default = 0,

        /// <summary>Built-in keywords.</summary>
        Keyword,

        /// <summary>References to classes.</summary>
        Type,
        
        /// <summary>String literals.</summary>
        String,
        
        /// <summary>Character literals.</summary>
        Char,
        
        /// <summary>All comments.</summary>
        Comment,

        /// <summary>Highlighted method call text.</summary>
        CallHighlight,

        /// <summary>Highlighted throw text.</summary>
        ThrowHighlight,

        /// <summary>XML Doc comment text.</summary>
        XmlDocComment,
        /// <summary>XML Doc tag.</summary>
        XmlDocTag,
        /// <summary>XML Doc attribute.</summary>
        XmlDocAttribute

    }

    /// <summary>
    /// Styles for syntax highlighting.
    /// </summary>
    public class SyntaxStyle
    {
        /// <summary>The default font.</summary>
        private static Font defaultFont;

        /// <summary>The styles.</summary>
        private static Dictionary<SyntaxType, SyntaxStyle> styles = new Dictionary<SyntaxType, SyntaxStyle>(6);

        /// <summary>
        /// Initializes static members of the <see cref="SyntaxStyle"/> class.
        /// </summary>
        static SyntaxStyle()
        {
            styles.Add(SyntaxType.Default, new SyntaxStyle(SyntaxType.Default, "Plain Text"));
            styles.Add(SyntaxType.Keyword, new SyntaxStyle(SyntaxType.Keyword, "Keyword", Color.Blue));
            styles.Add(SyntaxType.Type, new SyntaxStyle(SyntaxType.Type, "User Types", Color.DarkCyan));
            styles.Add(SyntaxType.String, new SyntaxStyle(SyntaxType.String, "String", Color.Maroon));
            styles.Add(SyntaxType.Char, new SyntaxStyle(SyntaxType.Char, "String", Color.Maroon));
            styles.Add(SyntaxType.Comment, new SyntaxStyle(SyntaxType.Comment, "Comment", Color.DarkGreen));
            styles.Add(SyntaxType.CallHighlight,
                new SyntaxStyle(
                    SyntaxType.CallHighlight,
                    "Call Return",
                    styles[SyntaxType.Default].Foreground,
                    Color.FromArgb(180, 228, 180)));

            styles.Add(SyntaxType.ThrowHighlight,
                new SyntaxStyle(
                    SyntaxType.ThrowHighlight,
                    "Current Statement",
                    styles[SyntaxType.Default].Foreground,
                    Color.FromArgb(255, 238, 98)));

            styles.Add(SyntaxType.XmlDocComment, new SyntaxStyle(SyntaxType.XmlDocComment, "Xml Doc Comment", Color.Green));
            styles.Add(SyntaxType.XmlDocTag, new SyntaxStyle(SyntaxType.XmlDocTag, "Xml Doc Tag", Color.Gray));
            styles.Add(SyntaxType.XmlDocAttribute, new SyntaxStyle(SyntaxType.XmlDocAttribute, "Xml Doc Attribute", Color.Gray));


        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxStyle"/> class.
        /// </summary>
        /// <param name="type">The type of syntax.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="foreground">The foreground.</param>
        /// <param name="background">The background.</param>
        /// <param name="fontStyle">The font style.</param>
        public SyntaxStyle(SyntaxType type, string settingName, Color foreground, Color background, FontStyle fontStyle)
        {
            this.Type = type;
            this.SettingName = settingName;

            using (RegistryKey key = OpenRegKey())
            {
                Func<string, int, int> getInt = (setting, defaultValue) =>
                {
                    return key.GetInt(string.Format("{0} {1}", this.SettingName, setting), defaultValue);
                };

                Func<string, Color, Color> getColor = (setting, defaultValue) =>
                {
                    int d = defaultValue.ToArgb();
                    int n = getInt(setting, d);

                    // 0x??BBGGRR
                    // 0x01000000 = use default foreground?
                    // 0x02000000 = use default background?

                    if ((n == d) || ((n & 0x01000000) != 0))
                    {
                        return defaultValue;
                    }

                    if ((n & 0x03000000) != 0)
                    {
                        return defaultValue;
                    }

                    n = n & 0xffffff;

                    return Color.FromArgb(
                        0xff,
                        n & 0xff,
                        (n >> 0x8) & 0xff,
                        (n >> 0x10) & 0xff);
                };

                this.Foreground = getColor("Foreground", foreground);
                this.Background = getColor("Background", background);
                this.FontStyle = (FontStyle)getInt("FontFlags", (int)fontStyle);
            }

            this.Font = new Font(DefaultFont, this.FontStyle);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxStyle"/> class.
        /// </summary>
        /// <param name="type">The type of syntax.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="foreground">The foreground.</param>
        /// <param name="background">The background.</param>
        public SyntaxStyle(SyntaxType type, string settingName, Color foreground, Color background)
            : this(type, settingName, foreground, background, FontStyle.Regular)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxStyle"/> class.
        /// </summary>
        /// <param name="type">The type of syntax.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="foreground">The foreground.</param>
        public SyntaxStyle(SyntaxType type, string settingName, Color foreground)
            : this(type, settingName, foreground, Color.Transparent)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxStyle"/> class.
        /// </summary>
        /// <param name="type">The type of syntax.</param>
        /// <param name="settingName">Name of the setting.</param>
        public SyntaxStyle(SyntaxType type, string settingName)
            : this(type, settingName, SystemColors.WindowText)
        {
        }

        /// <summary>Gets the default font.</summary>
        /// <value>The default font.</value>
        public static Font DefaultFont
        {
            get
            {
                if (defaultFont == null)
                {
                    using (RegistryKey key = OpenRegKey())
                    {
                        string fontName = key.GetValue<string>("FontName", "Consolas");
                        int fontSize = key.GetInt("FontPointSize", 10);

                        defaultFont = new Font(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Point);
                    }
                }

                return defaultFont;
            }
        }

        /// <summary>
        /// Gets or sets the background.
        /// </summary>
        /// <value>
        /// The background.
        /// </value>
        public Color Background { get; protected set; }

        /// <summary>Gets or sets the font.</summary>
        /// <value>The font..</value>
        public Font Font { get; protected set; }

        /// <summary>
        /// Gets or sets the font style.
        /// </summary>
        /// <value>
        /// The font style.
        /// </value>
        public FontStyle FontStyle { get; protected set; }

        /// <summary>
        /// Gets or sets the foreground.
        /// </summary>
        /// <value>
        /// The foreground.
        /// </value>
        public Color Foreground { get; protected set; }

        /// <summary>
        /// Gets or sets the kind of syntax.
        /// </summary>
        /// <value>
        /// The syntax kind.
        /// </value>
        public SyntaxType Type { get; protected set; }

        /// <summary>
        /// Gets or sets the name of the setting.
        /// </summary>
        /// <value>
        /// The name of the setting.
        /// </value>
        public string SettingName { get; protected set; }

        /// <summary>
        /// Gets the syntax style of the specified syntax kind.
        /// </summary>
        /// <param name="syntaxType">Type of syntax.</param>
        /// <returns>The style for the syntax.</returns>
        public static SyntaxStyle Style(SyntaxType syntaxType)
        {
            return styles[syntaxType];
        }

        /// <summary>Opens the registry key.</summary>
        /// <returns>The registry key.</returns>
        private static RegistryKey OpenRegKey()
        {
            const string KeyPath = @"Software\Microsoft\VisualStudio\10.0\FontAndColors\{A27B4E24-A735-4D1D-B8E7-9716E1E3D8E0}";
            return Registry.CurrentUser.OpenSubKey(KeyPath);
        }
    }

    /// <summary>
    /// Registry related methods.
    /// </summary>
    internal static class RegistryExtensions
    {
        /// <summary>Gets an integer value.</summary>
        /// <param name="key">The registry key.</param>
        /// <param name="name">The setting name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The integer value.</returns>
        public static int GetInt(this RegistryKey key, string name, int defaultValue)
        {
            if (key == null)
            {
                return defaultValue;
            }

            return key.GetValue(name, defaultValue) as int? ?? defaultValue;
        }

        /// <summary>Gets a generic value.</summary>
        /// <typeparam name="T">The value's type.</typeparam>
        /// <param name="key">The registry key.</param>
        /// <param name="name">The setting name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value.</returns>
        public static T GetValue<T>(this RegistryKey key, string name, T defaultValue) where T : class
        {
            if (key == null)
            {
                return defaultValue;
            }

            return key.GetValue(name, defaultValue) as T ?? defaultValue;
        }
    }
}