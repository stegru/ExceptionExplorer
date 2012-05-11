namespace ExceptionExplorer.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// A collection ThrownException instances
    /// </summary>
    public class ThrownExceptionCollection : Collection<ThrownException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThrownExceptionCollection"/> class.
        /// </summary>
        public ThrownExceptionCollection()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrownExceptionCollection"/> class.
        /// </summary>
        /// <param name="list">The list.</param>
        public ThrownExceptionCollection(IList<ThrownException> list)
            : base(list)
        {
        }
    }

    /// <summary>
    /// A dictionary of ThrownExceptions, using the Exception type as the key.
    /// </summary>
    public class ThrownExceptionDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThrownExceptionDictionary"/> class.
        /// </summary>
        public ThrownExceptionDictionary()
        {
            this.Dictionary = new Dictionary<Type, ThrownExceptionCollection>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrownExceptionDictionary"/> class.
        /// </summary>
        /// <param name="thrownExceptions">The thrown exceptions.</param>
        public ThrownExceptionDictionary(IEnumerable<ThrownException> thrownExceptions)
            : this()
        {
            this.AddRange(thrownExceptions);
        }

        /// <summary>
        /// Gets or sets the dictionary.
        /// </summary>
        /// <value>
        /// The dictionary.
        /// </value>
        protected Dictionary<Type, ThrownExceptionCollection> Dictionary { get; set; }

        /// <summary>
        /// Adds the specified thrown exception to the dictionary.
        /// </summary>
        /// <param name="thrownException">The thrown exception.</param>
        public void Add(ThrownException thrownException)
        {
            ThrownExceptionCollection list;

            if (!this.Dictionary.TryGetValue(thrownException.Exception, out list))
            {
                list = new ThrownExceptionCollection();
                this.Dictionary.Add(thrownException.Exception, list);
            }

            list.Add(thrownException);
        }

        /// <summary>Adds the range.</summary>
        /// <param name="thrownExceptions">The thrown exceptions.</param>
        public void AddRange(IEnumerable<ThrownException> thrownExceptions)
        {
            thrownExceptions.ToList().ForEach(this.Add);
        }

        /// <summary>Clears this instance.</summary>
        public void Clear()
        {
            this.Dictionary.Clear();
        }

        /// <summary>Gets a list of exceptions of the given type</summary>
        /// <param name="type">The exception type.</param>
        /// <returns>The list of exceptions</returns>
        public ThrownExceptionCollection GetList(Type type)
        {
            if (type != null)
            {
                ThrownExceptionCollection list;
                if (this.Dictionary.TryGetValue(type, out list))
                {
                    return list;
                }
            }
            return new ThrownExceptionCollection();
        }

        /// <summary>
        /// Gets the lists of thrown exceptions. Every list will have at least 1 item
        /// </summary>
        /// <returns>The lists</returns>
        public IEnumerable<ThrownExceptionCollection> GetLists()
        {
            foreach (ThrownExceptionCollection list in this.Dictionary.Values)
            {
                if (list.Count > 0)
                {
                    yield return new ThrownExceptionCollection(list);
                }
            }
        }
    }
}