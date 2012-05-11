using Decompiler;
namespace Decompiler.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.Xml.Linq;
    using ICSharpCode.NRefactory.Documentation;
    using Mono.Cecil;
    using ICSharpCode.ILSpy;
    using System.Globalization;

    /// <summary>
    /// Extension methods related to XmlDocumentation.
    /// </summary>
    public class XmlDocumentation
    {
        /// <summary>Finds the type. Like Type.GetType, but tries harder.</summary>
        /// <param name="referringType">The method that refers to the type.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <returns>The type, as specified by the typeName parameter.</returns>
        public static Type FindType(Type referringType, string typeName)
        {
            Type gettype = Type.GetType(typeName);

            if (gettype != null)
            {
                return gettype;
            }

            string dotTypeName = "." + typeName;

            // returns true if the type matches the name
            Func<Type, bool> typeMatches = (type) =>
            {
                if (typeName.Equals(type.Name, StringComparison.InvariantCultureIgnoreCase)
                    || typeName.Equals(type.FullName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }

                if (type.FullName.EndsWith(dotTypeName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }

                return false;
            };

            if (referringType != null)
            {

                // look in the method's class
                Type[] types = referringType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
                foreach (Type t in types)
                {
                    if (typeMatches(t))
                    {
                        return t;
                    }
                }

                // look in the method's assembly
                types = referringType.Assembly.GetTypes();
                foreach (Type t in types)
                {
                    if (typeMatches(t))
                    {
                        return t;
                    }
                }

                // look in the assemblies referenced by the method's assembly
                IEnumerable<Assembly> referenced = referringType.Assembly.GetReferencedAssemblies().Select(
                    assName => Assembly.ReflectionOnlyLoad(assName.FullName));

                foreach (Assembly ass in referenced)
                {
                    types = ass.GetTypes();
                    foreach (Type t in types)
                    {
                        if (typeMatches(t))
                        {
                            return t;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>The framework paths.</summary>
        private static Dictionary<TargetRuntime, string[]> frameworkPaths;

        static readonly string referencePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Reference Assemblies\Microsoft\Framework");
        static readonly string frameworkPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Microsoft.NET\Framework");

        public static string GetXmlDocFile(Module module)
        {
            string fqn = module.FullyQualifiedName;
            string ver = module.Assembly.ImageRuntimeVersion;
            

            string filename = FindXmlDocPath(module.FullyQualifiedName, ver) ??
                FindXmlDocPath(Path.GetFileName(module.FullyQualifiedName), ver);

            return filename;
        }

        /// <summary>Finds the XML document path.</summary>
        /// <param name="assemblyFilename">The assembly filename.</param>
        /// <param name="version">The version.</param>
        /// <returns></returns>
        private static string FindXmlDocPath(string assemblyFilename, string version)
        {
            string path = Path.Combine(frameworkPath, version, assemblyFilename);
            string localised = GetLocalXmlDoc(path);

            if (localised == null)
            {
                // try the reference assemblies path
                int major = "0123456789".IndexOf(version[1]);

                if (major < 4)
                {
                    localised = GetLocalXmlDoc(Path.Combine(referencePath, "v3.5", assemblyFilename))
                        ?? GetLocalXmlDoc(Path.Combine(referencePath, "v3.0", assemblyFilename))
                        ?? GetLocalXmlDoc(Path.Combine(referencePath, @".NETFramework\v3.5\Profile\Client", assemblyFilename));
                }
                else
                {
                    localised = GetLocalXmlDoc(Path.Combine(referencePath, @".NETFramework\v4.0", assemblyFilename));
                }
            }

            return localised;
        }

        /// <summary>Gets a localised copy of the xml doc</summary>
        /// <param name="path">The path.</param>
        /// <returns>The get local XML doc</returns>
        private static string GetLocalXmlDoc(string path)
        {
            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                return null;
            }

            string filename = Path.ChangeExtension(path, ".xml");
            string culture = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

            string localised = Path.Combine(directory, culture, filename);

            // try the localised: dir/LANG/a.xml
            if (File.Exists(localised))
            {
                return localised;
            }

            // non-localised: dir/a.xml
            if (File.Exists(filename))
            {
                return filename;
            }

            // english: dir/en/a.xml
            localised = Path.Combine(directory, culture, filename);
            if (File.Exists(localised))
            {
                return localised;
            }

            return null;         
        }

        /// <summary>Gets the documented exceptions for a method.</summary>
        /// <param name="method">The method.</param>
        /// <returns>The documented exceptions.</returns>
        public static IEnumerable<Type> GetDocumentedExceptions(MemberInfo member)
        {
            List<string> list = GetXmlDocList(member, "exception");
            if (list.Count > 0)
            {
                return list.Select(s => FindType(member.DeclaringType, s)).Where(t => t != null);
            }
            else
            {
                return Type.EmptyTypes;
            }
        }

        /// <summary>Gets a letter for the specified member's type.</summary>
        /// <param name="member">The member.</param>
        /// <returns>The member type letter</returns>
        private static string GetMemberTypeLetter(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Method:
                case MemberTypes.Constructor:
                    return "M";
                case MemberTypes.Property:
                    return "P";
                case MemberTypes.Event:
                    return "E";
                case MemberTypes.Field:
                    return "F";
                case MemberTypes.TypeInfo:
                case MemberTypes.NestedType:
                    return "T";
            }
            return "";
        }

        /// <summary>Appends the name of the member.</summary>
        /// <param name="str">The string to append to.</param>
        /// <param name="member">The member.</param>
        /// <returns>
        /// The StringBuilder str.
        /// </returns>
        private static StringBuilder AppendMemberName(StringBuilder str, MemberInfo member)
        {
            if (member.DeclaringType != null)
            {
                str.Append(member.DeclaringType.FullName)
                    .Append('.')
                    .Append(member.Name);
            }
            else
            {
                str.Append(member.Name);
            }

            return str;
        }

        private static StringBuilder AppendMemberParameters(StringBuilder str, MemberInfo member)
        {
            if ((member.MemberType == MemberTypes.Method) || (member.MemberType == MemberTypes.Constructor))
            {
                MethodBase method = member as MethodBase;
                if (method != null)
                {

                    IEnumerable<string> parameters = method.GetParameters().Select(pi => pi.ParameterType.FullName);

                    str.Append('(')
                        .Append(String.Join(",", parameters))
                        .Append(')');
                }
            }

            return str;
        }

        /// <summary>Gets an identifier for the given member.</summary>
        /// <param name="member">The member.</param>
        /// <returns>The get member id</returns>
        private static string GetMemberId(MemberInfo member)
        {
            StringBuilder name = new StringBuilder();

            name.Append(GetMemberTypeLetter(member))
                .Append(':');
            AppendMemberName(name, member);
            AppendMemberParameters(name, member);



            return name.ToString();
        }

        /// <summary>Gets the XML documentation, as a string, for a method.</summary>
        /// <param name="member">The method.</param>
        /// <returns>The XML documentation.</returns>
        public static XmlDocMember GetXmlDoc(MemberInfo member)
        {
            string memberId = GetMemberId(member);
            XmlDocumentation doc = GetDocument(member.Module);

            if (doc != null)
            {

                XmlDocMember memberInfo;
                if (doc.members.TryGetValue(memberId, out memberInfo))
                {
                    return memberInfo;
                }
            }
            return null;
        }

        public static List<string> GetXmlDocList(MemberInfo member, string docSection)
        {
            XmlDocMember memberDoc = GetXmlDoc(member);
            if (memberDoc != null)
            {
                List<string> list;
                if (memberDoc.TryGetValue(docSection, out list))
                {
                    return list;
                }
            }

            return new List<string>();
        }

        /// <summary>Gets the XML documentation, as a string, for a method.</summary>
        /// <param name="method">The method.</param>
        /// <returns>The XML documentation.</returns>
        public static string GetXmlDoc(MemberInfo member, string docSection)
        {
            List<string> list = GetXmlDocList(member, docSection);
            return string.Join("\r\n", list.ToArray());
        }

        /// <summary>Loads a XML document for the given module.</summary>
        /// <param name="module">The module.</param>
        /// <returns></returns>
        private static XmlDocumentation LoadDocument(Module module)
        {
            string file = GetXmlDocFile(module);

            if (file == null)
            {
                return null;
            }

            XDocument xdoc = XDocument.Load(file);
            XmlDocumentation doc = new XmlDocumentation();
            
            foreach (XElement member in xdoc.Descendants("members").Descendants("member"))
            {
                XmlDocMember memberDoc = new XmlDocMember();

                foreach (XElement item in member.Elements())
                {
                    List<string> list;
                    string name = item.Name.LocalName;
                    string value;

                    if (name == "exception")
                    {
                        XAttribute cref = item.Attribute("cref");
                        if (cref == null)
                        {
                            continue;
                        }

                        value = cref.Value.Substring(2);
                    }
                    else
                    {
                        value = item.Value;
                    }

                    if (!memberDoc.TryGetValue(name, out list))
                    {
                        list = new List<string>();
                        memberDoc.Add(name, list);
                    }

                    list.Add(value);
                }

                if (memberDoc.Count > 0)
                {
                    doc.members.Add(member.Attribute("name").Value, memberDoc);
                }
            }

            return doc;
        }

        /// <summary>Gets the document for the given module.</summary>
        /// <param name="module">The module.</param>
        /// <returns>The get document</returns>
        private static XmlDocumentation GetDocument(Module module)
        {
            string key = module.Name;
            XmlDocumentation doc = null;

            lock (documents)
            {
                if (!documents.TryGetValue(key, out doc))
                {
                    doc = LoadDocument(module);
                    documents.Add(key, doc);
                }
            }
            return doc;
        }

        /// <summary>
        /// The cached documents.
        /// </summary>
        private static Dictionary<string, XmlDocumentation> documents = new Dictionary<string, XmlDocumentation>();

        /// <summary>
        /// The members
        /// </summary>
        Dictionary<string, XmlDocMember> members = new Dictionary<string, XmlDocMember>();

        /// <summary>
        /// Prevents a default instance of the <see cref="XmlDocumentation"/> class from being created.
        /// </summary>
        private XmlDocumentation()
        {
            this.members = new Dictionary<string, XmlDocMember>();
        }
    }

    public class XmlDocMember : Dictionary<string, List<string>>
    {

    }



}