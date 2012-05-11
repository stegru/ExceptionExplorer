namespace ExceptionExplorer.Config
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Most recently used
    /// </summary>
    public class MRU : IEnumerable<string>, IStorable
    {
        public MRU(string name)
        {
            this.Name = name;
            this.files = new LinkedList<string>();
        }

        /// <summary>Gets the name.</summary>
        public string Name { get; private set; }

        /// <summary>The list of files, starting from the most recently used.</summary>
        private LinkedList<string> files;

        /// <summary>Raised when the list has changed.</summary>
        public event EventHandler<EventArgs> Changed;

        /// <summary>Call when the specified file has been used.</summary>
        /// <param name="file">The file.</param>
        public void Use(string file)
        {
            // get the real casing
            if (File.Exists(file))
            {
                string[] files = Directory.GetFiles(Path.GetDirectoryName(file), Path.GetFileName(file));
                if (files.Length == 1)
                {
                    file = files[0];
                }
            }

            this.files.Remove(file);
            this.files.AddFirst(file);

            if (this.Changed != null)
            {
                this.Changed(this, new EventArgs());
            }
        }

        public void Remove(string file)
        {
            this.files.Remove(file);
        }

        public int Count
        {
            get { return this.files.Count; }
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<string> GetEnumerator()
        {
            return this.files.GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.files.GetEnumerator();
        }

        /// <summary>Save to the specified store.</summary>
        /// <param name="store">The store.</param>
        public void Save(Storage store)
        {
            int n = 0;

            foreach (string file in this)
            {
                store.SetValue(n.ToString(), file);
                n++;
            }
        }

        /// <summary>Load from the specified store.</summary>
        /// <param name="store">The store.</param>
        public void Load(Storage store)
        {
            int n = 0;
            this.files.Clear();

            string s = store.GetValue(n.ToString(), (string)null);

            while (s != null)
            {
                this.files.AddLast(s);
                n++;
                s = store.GetValue(n.ToString(), (string)null);
            }

            if (this.Changed != null)
            {
                this.Changed(this, new EventArgs());
            }
        }
    }
}