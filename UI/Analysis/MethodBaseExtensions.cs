namespace ExceptionExplorer.Analysis
{
    using System;
    using System.Reflection;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extension methods for <see cref="Methodbase"/>
    /// </summary>
    public static class MethodBaseExtensions
    {
        /// <summary>Returns the signature of a method.</summary>
        /// <param name="method">The method.</param>
        /// <returns>The get signature</returns>
        public static string GetSignature(this MethodBase method)
        {
            return method.GetSignature(false);
        }

        /// <summary>Returns the signature of a method.</summary>
        /// <param name="method">The method.</param>
        /// <param name="includeClass">if set to <c>true</c> [include class].</param>
        /// <returns>The get signature</returns>
        public static string GetSignature(this MethodBase method, bool includeClass)
        {
            StringBuilder sig = new StringBuilder();

            // naughty: invoke the private ConstructParameters()
            MethodInfo mi = method.GetType().GetMethod(
                "ConstructParameters",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy,
                null,
                new Type[] { typeof(ParameterInfo[]), typeof(CallingConventions) },
                null);

            string paras = mi.Invoke(method, new object[] { method.GetParameters(), method.CallingConvention }) as string ?? string.Empty;

            string name = method.Name;
            if (includeClass)
            {
                if (method.IsConstructor)
                {
                    sig.Append(method.DeclaringType.Name);
                    name = string.Empty;
                }
                else
                {
                    sig.Append(method.DeclaringType.Name).Append('.');
                }
            }

            sig.Append(name)
                .Append("(")
                .Append(paras)
                .Append(")");

            return sig.ToString();
        }

        /// <summary>
        /// Determines whether the specified method is property getter or setter.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>
        ///   <c>true</c> if the specified method is property; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsProperty(this MethodBase method)
        {
            return method.IsSpecialName && method.IsHideBySig && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"));
        }

        /// <summary>Gets the property for a getter/setter method.</summary>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        public static PropertyInfo GetMethodProperty(this MethodBase method)
        {
            PropertyInfo propertyInfo = null;

            if (method.IsProperty())
            {
                string name = method.Name.Substring(4);

                try
                {
                    propertyInfo = method.DeclaringType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                }
                catch (AmbiguousMatchException)
                {
                    // be a bit more specific with the search
                    Type returnType = typeof(void);

                    if (method is MethodInfo)
                    {
                        returnType = ((MethodInfo)method).ReturnType;
                    }

                    IEnumerable<Type> paras = from p in method.GetParameters() select p.ParameterType;
                    bool getter = method.Name.StartsWith("get_");
                    if (!getter)
                    {
                        returnType = paras.Last();
                        paras = paras.Take(paras.Count() - 1);
                    }

                    try
                    {
                        propertyInfo = method.DeclaringType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, returnType, paras.ToArray(), null);
                    }
                    catch (AmbiguousMatchException)
                    {
                        return null;
                    }
                }

                if (propertyInfo != null)
                {
                    return propertyInfo;
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether the specified method is an event set/remove method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>
        ///   <c>true</c> if the specified method is for an event; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEvent(this MethodBase method)
        {
            return method.IsSpecialName && method.IsHideBySig && (method.Name.StartsWith("add_") || method.Name.StartsWith("remove_"));
        }

        public static bool IsException(this Type type)
        {
            return typeof(Exception).IsAssignableFrom(type);
        }
    }
}