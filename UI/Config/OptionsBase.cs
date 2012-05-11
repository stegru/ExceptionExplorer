namespace ExceptionExplorer.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;

    /// <summary>
    /// Event data for the Changed event of Setting classes
    /// </summary>
    public class SettingChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the setting.
        /// </summary>
        /// <value>The setting.</value>
        public ISetting Setting { get; protected set; }

        /// <summary>
        /// Gets or sets the previous value of the setting.
        /// </summary>
        /// <value>The previous value.</value>
        public object PreviousValue { get; protected set; }

        /// <summary>
        /// Gets or sets new the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingChangedEventArgs"/> class.
        /// </summary>
        /// <param name="setting">The setting.</param>
        /// <param name="previousValue">The previous value.</param>
        public SettingChangedEventArgs(ISetting setting, object previousValue)
        {
            this.Setting = setting;
            this.PreviousValue = previousValue;
            this.Value = this.Setting.GetObjectValue();
        }
    }

    /// <summary>A setting</summary>
    /// <typeparam name="T">The setting value's type</typeparam>
    public class Setting<T> : ISetting, IDisposable
    {
        /// <summary>The value</summary>
        private T value;

        /// <summary>The menu item</summary>
        private ToolStripMenuItem menuItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="Setting&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Setting(string name)
        {
            this.Name = name;

            // initialise the value
            if (typeof(T).IsValueType)
            {
                this.Value = default(T);
            }
            else
            {
                try
                {
                    this.Value = Activator.CreateInstance<T>();
                }
                catch (MissingMethodException)
                {
                    // there isn't a parameterless constructor - set it to null
                    this.Value = default(T);
                }
            }
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="ExceptionExplorer.UI.Setting&lt;T&gt;"/> to <see cref="T"/>.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator T(Setting<T> s)
        {
            return s.Value;
        }

        /// <summary>Gets or sets the value.</summary>
        /// <value>The value.</value>
        public T Value
        {
            get
            {
                return this.value;
            }

            set
            {
                this.SetValue(value);
            }
        }

        /// <summary>Gets the type.</summary>
        public Type Type
        {
            get
            {
                return typeof(T);
            }
        }

        /// <summary>Gets the name.</summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether menu's checked value should be inverted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [menu value invert]; otherwise, <c>false</c>.
        /// </value>
        public bool MenuValueInvert { get; set; }

        /// <summary>
        /// Gets or sets the menu item associated with this setting.
        /// </summary>
        /// <value>The menu item.</value>
        public ToolStripMenuItem MenuItem
        {
            get
            {
                return this.menuItem;
            }

            set
            {
                if (this.Value is bool)
                {
                    if (this.menuItem != null)
                    {
                        this.menuItem.CheckedChanged -= MenuItem_CheckedChanged;
                    }

                    this.menuItem = value;

                    if (this.menuItem != null)
                    {
                        this.menuItem.CheckedChanged += new EventHandler(MenuItem_CheckedChanged);
                        this.menuItem.CheckOnClick = true;
                        bool check = (this.Value as bool? ?? false) != this.MenuValueInvert;
                        if (this.menuItem.Checked != check)
                        {
                            this.OnChanged(new SettingChangedEventArgs(this, this.Value));
                            this.menuItem.Checked = check;
                        }
                    }
                }
            }
        }

        /// <summary>Handles the control.</summary>
        /// <param name="setControlValue">if set to <c>true</c> set the control's value, otherwise read the control's value.</param>
        /// <param name="control">The control.</param>
        public void HandleControl(bool setControlValue, Control control)
        {
            if ((control is CheckBox) || (control is RadioButton))
            {
                this.GetSetControlValue(setControlValue, control, "Checked");
            }
            else if (control is TextBox)
            {
                this.GetSetControlValue(setControlValue, control, "Text");
            }
        }

        /// <summary>Get or set a control's value</summary>
        /// <param name="setControlValue">if set to <c>true</c>, set the control's value, otherwise read it.</param>
        /// <param name="control">The control.</param>
        /// <param name="propertyName">Name of the property.</param>
        private void GetSetControlValue(bool setControlValue, Control control, string propertyName)
        {
            PropertyInfo pi = control.GetType().GetProperty(propertyName);

            if ((pi == null) || !pi.PropertyType.IsAssignableFrom(typeof(T)))
            {
                return;
            }

            if (setControlValue)
            {
                pi.SetValue(control, this.Value, null);
            }
            else
            {
                this.Value = (T)pi.GetValue(control, null);
            }
        }

        /// <summary>Reads the menu item value.</summary>
        private void ReadMenuItemValue()
        {
            if (this.MenuItem != null)
            {
                Setting<bool> setting = this.GetSetting<bool>();
                if (setting != null)
                {
                    setting.SetValue(this.MenuItem.Checked != this.MenuValueInvert);
                }
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the MenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void MenuItem_CheckedChanged(object sender, EventArgs e)
        {
            this.ReadMenuItemValue();
        }

        /// <summary>Occurs when the setting has been changed.</summary>
        public event EventHandler<SettingChangedEventArgs> Changed;

        /// <summary>Occurs when this object is being disposed.</summary>
        public event EventHandler Disposed;

        /// <summary>Occurs when the value is set.</summary>
        public event Action<T> Set;

        /// <summary>
        /// Raises the <see cref="E:Changed"/> event.
        /// </summary>
        /// <param name="e">The <see cref="ExceptionExplorer.UI.SettingChangedEventArgs"/> instance containing the event data.</param>
        internal void OnChanged(SettingChangedEventArgs e)
        {
            if (this.MenuItem != null)
            {
                this.MenuItem.Checked = (this.Value as bool? ?? false) != this.MenuValueInvert;
            }

            if (this.Changed != null)
            {
                this.Changed(this, e);
            }

            if (this.Set != null)
            {
                this.Set(this.Value);
            }
        }

        /// <summary>Sets the value.</summary>
        /// <param name="newValue">The new value.</param>
        public void SetValue(T newValue)
        {
            bool equal = false;

            if (newValue is IComparable)
            {
                equal = ((IComparable)newValue).CompareTo((IComparable)this.Value) == 0;
            }
            else if (newValue != null)
            {
                equal = newValue.Equals(this.Value);
            }
            else
            {
                equal = this.Value == null;
            }

            if (!equal)
            {
                SettingChangedEventArgs e = new SettingChangedEventArgs(this, this.Value);
                this.value = newValue;
                this.OnChanged(e);
            }
        }

        /// <summary>Gets the value.</summary>
        /// <returns>The value.</returns>
        public object GetObjectValue()
        {
            return (object)this.Value;
        }

        /// <summary>Gets the setting.</summary>
        /// <typeparam name="TSetting">The type of the setting value.</typeparam>
        /// <returns>The setting.</returns>
        public Setting<TSetting> GetSetting<TSetting>()
        {
            return this as Setting<TSetting>;
        }

        /// <summary>Gets the value.</summary>
        /// <typeparam name="TSetting">The type of the setting.</typeparam>
        /// <returns>The value.</returns>
        public TSetting GetValue<TSetting>()
        {
            if (this.Value is TSetting)
            {
                return this.GetSetting<TSetting>().Value;
            }
            else
            {
                throw new InvalidCastException(this.Name);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Disposed != null)
                {
                    this.Disposed(this, new EventArgs());
                }

                this.MenuItem = null;
            }
        }

        /// <summary>
        /// Determines whether an object is a basic type.
        /// </summary>
        /// <param name="o">The object to test.</param>
        /// <returns>
        ///   <c>true</c> if the object is either an int, string, bool, long, enum, or byte array; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsBasicType(object o)
        {
            return (o is int) || (o is string) || (o is bool) || (o is long) || (o is Enum) || (o is byte[]);
        }

        /// <summary>Save to the specified store.</summary>
        /// <param name="store">The store.</param>
        public void Save(Storage store)
        {
            IStorable storable = this.Value as IStorable;
            if (storable != null)
            {
                using (Storage s = store.CreateSubStore(this.Name))
                {
                    storable.Save(s);
                }
            }
            else if (IsBasicType(this.value))
            {
                dynamic d = this.Value;
                store.SetValue(this.Name, d);
            }
            else if (typeof(T) == typeof(DateTime))
            {
                dynamic d = this.value;
                DateTime date = (DateTime)d;
                store.SetValue(this.Name, date.ToBinary());
            }
        }

        /// <summary>Load from the specified store.</summary>
        /// <param name="store">The store.</param>
        public void Load(Storage store)
        {
            IStorable storable = this.Value as IStorable;
            if (storable != null)
            {
                using (Storage s = store.CreateSubStore(this.Name))
                {
                    storable.Load(s);
                }
            }
            else if (IsBasicType(this.value) || (typeof(T) == typeof(string)))
            {
                if (this.value == null)
                {
                    dynamic d = string.Empty;
                    this.SetValue(store.GetValue(this.Name, d));
                }
                else
                {
                    dynamic d = this.Value;
                    this.SetValue(store.GetValue(this.Name, d));
                }
            }
            else if (typeof(T) == typeof(DateTime))
            {
                DateTime date = DateTime.FromBinary(store.GetValue(this.Name, DateTime.MinValue.ToBinary()));
                if (date != DateTime.MinValue)
                {
                    dynamic d = date;
                    this.SetValue(d);
                }
            }
        }
    }

    /// <summary>
    /// Setting interface.
    /// </summary>
    public interface ISetting : IStorable
    {
        /// <summary>Occurs when the value has changed.</summary>
        event EventHandler<SettingChangedEventArgs> Changed;

        /// <summary>Gets the name.</summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>Gets the type.</summary>
        Type Type { get; }

        /// <summary>Gets the value, as an object.</summary>
        /// <returns>The object value.</returns>
        object GetObjectValue();

        /// <summary>Gets the setting.</summary>
        /// <typeparam name="TSetting">The type of the setting.</typeparam>
        /// <returns>The value.</returns>
        Setting<TSetting> GetSetting<TSetting>();
    }

    /// <summary>Base class for options.</summary>
    public class OptionsBase : IStorable
    {
        /// <summary>The registry key name where there settings are stored.</summary>
        private const string registryKeyName = @"Software\ExceptionExplorer";

        /// <summary>Currently in a bulk change.</summary>
        private bool inBulkChange = false;

        /// <summary>Gets or sets the storage location.</summary>
        /// <value>The location.</value>
        protected virtual IStoreLocation Location { get; set; }

        /// <summary>Occurs when a setting has changed.</summary>
        public event EventHandler<SettingChangedEventArgs> Changed;

        /// <summary>Occurs when a bulk change has been entered or exited.</summary>
        public event EventHandler<EventArgs> BulkChange;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsBase"/> class.
        /// </summary>
        public OptionsBase()
        {
            // initialises the setting properties
            foreach (PropertyInfo pi in this.GetType().GetProperties())
            {
                if (pi.PropertyType.GetInterface(typeof(ISetting).FullName) != null)
                {
                    ISetting setting = (ISetting)Activator.CreateInstance(pi.PropertyType, pi.Name);
                    setting.Changed += new EventHandler<SettingChangedEventArgs>(setting_Changed);
                    pi.SetValue(this, setting, null);
                }
            }
        }

        /// <summary>
        /// Handles the Changed event of the setting control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExceptionExplorer.Config.SettingChangedEventArgs"/> instance containing the event data.</param>
        private void setting_Changed(object sender, SettingChangedEventArgs e)
        {
            if (this.Changed != null)
            {
                this.Changed(this, e);
            }
        }

        /// <summary>Gets or sets a value indicating whether it's in a bulk change.</summary>
        /// <value><c>true</c> if in a bulk change; otherwise, <c>false</c>.</value>
        public bool InBulkChange
        {
            get
            {
                return this.inBulkChange;
            }

            set
            {
                if (value != this.inBulkChange)
                {
                    this.inBulkChange = value;
                    if (this.BulkChange != null)
                    {
                        this.BulkChange(this, new EventArgs());
                    }
                }
            }
        }

        /// <summary>Gets the store.</summary>
        /// <param name="write">open for writing, if set to <c>true</c>.</param>
        /// <returns>
        /// The storage that the settings are read from, or saved to.
        /// </returns>
        internal Storage GetStore(bool write)
        {
            if (this.Location == null)
            {
                return new DummyStore();
            }

            return Storage.OpenLocation(this.Location, write);
        }

        /// <summary>Save to the specified store.</summary>
        /// <param name="store">The store.</param>
        public virtual void Save(Storage store)
        {
            this.Location = store.Location;
            this.LoadSave(store, true);
        }

        /// <summary>Load from the specified store.</summary>
        /// <param name="store">The store.</param>
        public virtual void Load(Storage store)
        {
            this.Location = store.Location;
            this.LoadSave(store, false);
        }

        /// <summary>Load or save from the specified store.</summary>
        /// <param name="store">The store.</param>
        /// <param name="save">if set to <c>true</c>, save, otherwise load.</param>
        private void LoadSave(Storage store, bool save)
        {
            foreach (PropertyInfo pi in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // go through all the storable properties
                if (pi.PropertyType.GetInterface(typeof(IStorable).FullName) != null)
                {
                    object propertyValue = pi.GetValue(this, null);

                    if (propertyValue == null)
                    {
                        if (save)
                        {
                            continue;
                        }

                        propertyValue = Activator.CreateInstance(pi.PropertyType);
                        pi.SetValue(this, propertyValue, null);
                    }

                    IStorable storable = propertyValue as IStorable;

                    // properties that aren't settings are wrote in a child store
                    bool isSetting = pi.PropertyType.GetInterface(typeof(ISetting).FullName) != null;

                    Storage s = isSetting ? store : store.CreateSubStore(pi.Name);

                    if (save)
                    {
                        storable.Save(s);
                    }
                    else
                    {
                        storable.Load(s);
                    }

                    if (!isSetting)
                    {
                        s.Dispose();
                        s = null;
                    }
                }
            }
        }

        /// <summary>Gets an enumeration of the properties of type T.</summary>
        /// <typeparam name="T">The type of property to return</typeparam>
        /// <returns>An enmeration of </returns>
        private IEnumerable<T> GetProperties<T>() where T : class
        {
            return from pi in this.GetType().GetProperties()
                   where pi.PropertyType.GetInterface(typeof(T).FullName) != null
                   select pi.GetValue(this, null) as T;
        }

        /// <summary>Gets the storable properties in the class.</summary>
        /// <returns>An enumeration of storable properties.</returns>
        public IEnumerable<IStorable> GetStorables()
        {
            return this.GetProperties<IStorable>();
        }
    }
}