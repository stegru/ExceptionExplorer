namespace ExceptionExplorer.IL
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// An IL Instruction
    /// </summary>
    internal class Instruction
    {
        /// <summary>The il code reader</summary>
        private ILCode codeReader;

        /// <summary>The generic method args</summary>
        private Type[] genericMethodArgs;

        /// <summary>The generic type args</summary>
        private Type[] genericTypeArgs;

        /// <summary>The module</summary>
        private Module module;

        /// <summary>The token</summary>
        private Int32 token;

        /// <summary>
        /// Initializes a new instance of the <see cref="Instruction"/> class.
        /// </summary>
        /// <param name="codeReader">The il code.</param>
        /// <param name="reader">The reader.</param>
        /// <param name="opCode">The op code.</param>
        public Instruction(ILCode codeReader, BinaryReader reader, OpCode opCode)
        {
            this.codeReader = codeReader;
            this.module = this.codeReader.Method.Module;


            int offs = (int)reader.BaseStream.Position - 1;
            if ((ushort)opCode.Value > 0xff)
            {
                // multi-byte opcode values started 1 byte earlier
                offs--;
            }

            this.Offset = offs;//            (int)reader.BaseStream.Position;
            this.OpCode = opCode;
            this.Value = (OpCodeValue)(ushort)this.OpCode.Value;

            // eat the operand from the buffer
            switch (opCode.OperandType)
            {
                case OperandType.InlineNone:
                    break;

                case OperandType.ShortInlineBrTarget:
                    this.Operand = reader.ReadSByte();
                    break;

                case OperandType.InlineBrTarget:
                case OperandType.InlineI:
                case OperandType.InlineString:
                case OperandType.InlineSig:
                case OperandType.InlineType:
                case OperandType.InlineTok:
                    this.Operand = reader.ReadInt32();
                    break;

                case OperandType.InlineMethod:
                    this.token = reader.ReadInt32();
                    break;

                case OperandType.InlineField:
                    this.token = reader.ReadInt32();
                    break;

                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar:
                    this.Operand = reader.ReadByte();
                    break;

                case OperandType.InlineI8:
                    this.Operand = reader.ReadInt64();
                    break;

                case OperandType.ShortInlineR:
                    this.Operand = reader.ReadSingle();
                    break;

                case OperandType.InlineR:
                    this.Operand = reader.ReadDouble();
                    break;

                case OperandType.InlineVar:
                    this.Operand = reader.ReadUInt16();
                    break;

                case OperandType.InlineSwitch:
                    // <uint count>, <int value[1]>, ..., <int value[count]>
                    UInt32 caseCnt = reader.ReadUInt32();
                    Int32[] caseValues = new Int32[caseCnt];
                    for (Int32 i = 0; i < caseCnt; i++)
                    {
                        caseValues[i] = reader.ReadInt32();
                    }

                    break;

                default:
                    // can't just ignore it, as there is an unknown size of operand after this.
                    throw new BadImageFormatException("unknown OperandType " + opCode.OperandType);
            }
        }

        /// <summary>Gets the field info.</summary>
        public FieldInfo FieldInfo
        {
            get
            {
                if (this.OpCode.OperandType != OperandType.InlineField)
                {
                    throw new InvalidOperationException("FieldInfo called for an instruction that is not an InLineField");
                }

                this.module.ResolveField(this.token);
                return null;
            }
        }

        /// <summary>Gets or sets the offset.</summary>
        /// <value>The offset.</value>
        public int Offset { get; protected set; }

        /// <summary>Gets or sets the op code.</summary>
        /// <value>The op code.</value>
        public OpCode OpCode { get; protected set; }

        /// <summary>Gets or sets the operand.</summary>
        /// <value>The operand.</value>
        public object Operand { get; protected set; }

        /// <summary>Gets or sets the value.</summary>
        /// <value>The value.</value>
        public OpCodeValue Value { get; set; }

        /// <summary>Resolves the field.</summary>
        /// <returns>FieldInfo for the field</returns>
        public FieldInfo ResolveField()
        {
            return this.module.ResolveField(this.token, this.codeReader.GetGenericTypeArgs(), this.codeReader.GetGenericMethodArgs());
            //this.genericMethodArgs = this.codeReader.GetGenericMethodArgs();
            //this.genericTypeArgs = this.codeReader.GetGenericTypeArgs();
        }

        /// <summary>Resolves the method.</summary>
        /// <returns>The resolved method</returns>
        public MethodBase ResolveMethod()
        {
            return this.module.ResolveMethod(this.token, this.codeReader.GetGenericTypeArgs(), this.codeReader.GetGenericMethodArgs());
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}", this.OpCode.Name);
        }
    }
}