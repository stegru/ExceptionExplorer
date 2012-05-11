namespace ExceptionExplorer.IL
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>Reads IL code for a method</summary>
    internal class ILCode : IDisposable
    {
        /// <summary>Multi-byte opcodes start with this</summary>
        private static readonly int MultibyteValue = 0xFE;

        /// <summary>single byte opcodes</summary>
        private static OpCode[] opcodeLookup;

        /// <summary>two byte opcodes</summary>
        private static OpCode[] opcodeLookup2;

        /// <summary>The method body</summary>
        private MethodBody body;

        /// <summary>The ilbytes</summary>
        private byte[] ilbytes;

        /// <summary>A reader for IL</summary>
        private BinaryReader reader;

        /// <summary>The generic method args</summary>
        private Type[] genericMethodArgs;

        /// <summary>The generic type args</summary>
        private Type[] genericTypeArgs;

        public Type[] GetGenericMethodArgs()
        {
            try
            {
                return this.genericMethodArgs ??
                    (this.genericMethodArgs = ((this.Method is ConstructorInfo) ? null : this.Method.GetGenericArguments()));
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }
        public Type[] GetGenericTypeArgs()
        {
            try
            {
                return this.genericTypeArgs ??
                    (this.genericTypeArgs = ((this.Method.DeclaringType == null) ? null : this.Method.DeclaringType.GetGenericArguments()));
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ILCode"/> class.
        /// </summary>
        /// <param name="method">The method to read the IL code from.</param>
        public ILCode(MethodBase method)
        {
            this.Method = method;
            this.body = this.Method.GetMethodBody();

            if (this.body == null)
            {
                this.ilbytes = new byte[0];
            }
            else
            {
                this.ilbytes = this.body.GetILAsByteArray();
            }

            this.reader = new BinaryReader(new MemoryStream(this.ilbytes, false));
            
        }

        /// <summary>
        /// Initializes static members of the <see cref="ILCode"/> class.
        /// </summary>
        static ILCode()
        {
            ILCode.InitLookupCache();
        }

        /// <summary>Gets the method.</summary>
        public MethodBase Method { get; private set; }

        /// <summary>Gets the position of the reader within the IL Code stream.</summary>
        protected int Position
        {
            get { return (int)this.reader.BaseStream.Position; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.reader.BaseStream.Close();
        }

        /// <summary>
        /// Gets an enumeration of the method's instructions.
        /// </summary>
        /// <returns>The instructions.</returns>
        public IEnumerable<Instruction> GetAllInstructions()
        {
            while (this.reader.BaseStream.Position < this.reader.BaseStream.Length)
            {
                yield return this.GetNextInstruction();
            }
        }

        /// <summary>Get the next instruction</summary>
        /// <returns>The next instruction</returns>
        public Instruction GetNextInstruction()
        {
            return new Instruction(this, this.reader, this.GetNextOpCode());
        }

        /// <summary>Gets the next op code.</summary>
        /// <returns>The next op code</returns>
        protected OpCode GetNextOpCode()
        {
            byte opByte = this.reader.ReadByte();
            bool multibyte = opByte == MultibyteValue;
            if (multibyte)
            {
                opByte = this.reader.ReadByte();
            }

            return OpCodeFromValue(opByte, multibyte);
        }

        /// <summary>
        /// Initialises the opcode lookup.
        /// </summary>
        private static void InitLookupCache()
        {
            opcodeLookup = new OpCode[0x100];
            opcodeLookup2 = new OpCode[0x100];

            // add every opcode to the array, using the opcode's value for the index
            foreach (FieldInfo fi in typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                OpCode op = (OpCode)fi.GetValue(null);
                UInt16 value = (UInt16)op.Value;
                if (value < 0x100)
                {
                    opcodeLookup[value] = op;
                }
                else if ((value & 0xff00) == MultibyteValue << 8)
                {
                    opcodeLookup2[value & 0xff] = op;
                }
            }
        }

        /// <summary>
        /// Gets the OpCode that has the given value.
        /// </summary>
        /// <param name="value">The value (or 2nd byte of a multibyte opcode value).</param>
        /// <param name="multibyte">set to <c>true</c> if it's from a multibyte opcode value.</param>
        /// <returns>The OpCode that has the value</returns>
        private static OpCode OpCodeFromValue(byte value, bool multibyte)
        {
            if (multibyte)
            {
                return opcodeLookup2[value];
            }
            else
            {
                return opcodeLookup[value];
            }
        }
    }
}