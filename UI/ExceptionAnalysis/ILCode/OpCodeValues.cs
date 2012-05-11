﻿// <autogenerated/>

////foreach (FieldInfo fi in typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public))
////{
////    OpCode op = (OpCode)fi.GetValue(null);
////    string n = op.Name;
////    if (n.EndsWith("."))
////    {
////        n = n.Substring(0, n.Length - 1);
////    }
////    n = n.Replace('.', '_');
////    n = n[0].ToString().ToUpper() + n.Substring(1, n.Length - 1);
////    Console.WriteLine("/// <summary>0x{1:X} {2}</summary>\n{0} = 0x{1:X},", n, op.Value, op.Name);
////}

namespace ExceptionExplorer.IL
{
    /// <summary>
    /// Opcode values
    /// </summary>
    internal enum OpCodeValue
    {
        /// <summary>0x0 nop</summary>
        Nop = 0x0,

        /// <summary>0x1 break</summary>
        Break = 0x1,

        /// <summary>0x2 ldarg.0</summary>
        Ldarg_0 = 0x2,

        /// <summary>0x3 ldarg.1</summary>
        Ldarg_1 = 0x3,

        /// <summary>0x4 ldarg.2</summary>
        Ldarg_2 = 0x4,

        /// <summary>0x5 ldarg.3</summary>
        Ldarg_3 = 0x5,

        /// <summary>0x6 ldloc.0</summary>
        Ldloc_0 = 0x6,

        /// <summary>0x7 ldloc.1</summary>
        Ldloc_1 = 0x7,

        /// <summary>0x8 ldloc.2</summary>
        Ldloc_2 = 0x8,

        /// <summary>0x9 ldloc.3</summary>
        Ldloc_3 = 0x9,

        /// <summary>0xA stloc.0</summary>
        Stloc_0 = 0xA,

        /// <summary>0xB stloc.1</summary>
        Stloc_1 = 0xB,

        /// <summary>0xC stloc.2</summary>
        Stloc_2 = 0xC,

        /// <summary>0xD stloc.3</summary>
        Stloc_3 = 0xD,

        /// <summary>0xE ldarg.s</summary>
        Ldarg_s = 0xE,

        /// <summary>0xF ldarga.s</summary>
        Ldarga_s = 0xF,

        /// <summary>0x10 starg.s</summary>
        Starg_s = 0x10,

        /// <summary>0x11 ldloc.s</summary>
        Ldloc_s = 0x11,

        /// <summary>0x12 ldloca.s</summary>
        Ldloca_s = 0x12,

        /// <summary>0x13 stloc.s</summary>
        Stloc_s = 0x13,

        /// <summary>0x14 ldnull</summary>
        Ldnull = 0x14,

        /// <summary>0x15 ldc.i4.m1</summary>
        Ldc_i4_m1 = 0x15,

        /// <summary>0x16 ldc.i4.0</summary>
        Ldc_i4_0 = 0x16,

        /// <summary>0x17 ldc.i4.1</summary>
        Ldc_i4_1 = 0x17,

        /// <summary>0x18 ldc.i4.2</summary>
        Ldc_i4_2 = 0x18,

        /// <summary>0x19 ldc.i4.3</summary>
        Ldc_i4_3 = 0x19,

        /// <summary>0x1A ldc.i4.4</summary>
        Ldc_i4_4 = 0x1A,

        /// <summary>0x1B ldc.i4.5</summary>
        Ldc_i4_5 = 0x1B,

        /// <summary>0x1C ldc.i4.6</summary>
        Ldc_i4_6 = 0x1C,

        /// <summary>0x1D ldc.i4.7</summary>
        Ldc_i4_7 = 0x1D,

        /// <summary>0x1E ldc.i4.8</summary>
        Ldc_i4_8 = 0x1E,

        /// <summary>0x1F ldc.i4.s</summary>
        Ldc_i4_s = 0x1F,

        /// <summary>0x20 ldc.i4</summary>
        Ldc_i4 = 0x20,

        /// <summary>0x21 ldc.i8</summary>
        Ldc_i8 = 0x21,

        /// <summary>0x22 ldc.r4</summary>
        Ldc_r4 = 0x22,

        /// <summary>0x23 ldc.r8</summary>
        Ldc_r8 = 0x23,

        /// <summary>0x25 dup</summary>
        Dup = 0x25,

        /// <summary>0x26 pop</summary>
        Pop = 0x26,

        /// <summary>0x27 jmp</summary>
        Jmp = 0x27,

        /// <summary>0x28 call</summary>
        Call = 0x28,

        /// <summary>0x29 calli</summary>
        Calli = 0x29,

        /// <summary>0x2A ret</summary>
        Ret = 0x2A,

        /// <summary>0x2B br.s</summary>
        Br_s = 0x2B,

        /// <summary>0x2C brfalse.s</summary>
        Brfalse_s = 0x2C,

        /// <summary>0x2D brtrue.s</summary>
        Brtrue_s = 0x2D,

        /// <summary>0x2E beq.s</summary>
        Beq_s = 0x2E,

        /// <summary>0x2F bge.s</summary>
        Bge_s = 0x2F,

        /// <summary>0x30 bgt.s</summary>
        Bgt_s = 0x30,

        /// <summary>0x31 ble.s</summary>
        Ble_s = 0x31,

        /// <summary>0x32 blt.s</summary>
        Blt_s = 0x32,

        /// <summary>0x33 bne.un.s</summary>
        Bne_un_s = 0x33,

        /// <summary>0x34 bge.un.s</summary>
        Bge_un_s = 0x34,

        /// <summary>0x35 bgt.un.s</summary>
        Bgt_un_s = 0x35,

        /// <summary>0x36 ble.un.s</summary>
        Ble_un_s = 0x36,

        /// <summary>0x37 blt.un.s</summary>
        Blt_un_s = 0x37,

        /// <summary>0x38 br</summary>
        Br = 0x38,

        /// <summary>0x39 brfalse</summary>
        Brfalse = 0x39,

        /// <summary>0x3A brtrue</summary>
        Brtrue = 0x3A,

        /// <summary>0x3B beq</summary>
        Beq = 0x3B,

        /// <summary>0x3C bge</summary>
        Bge = 0x3C,

        /// <summary>0x3D bgt</summary>
        Bgt = 0x3D,

        /// <summary>0x3E ble</summary>
        Ble = 0x3E,

        /// <summary>0x3F blt</summary>
        Blt = 0x3F,

        /// <summary>0x40 bne.un</summary>
        Bne_un = 0x40,

        /// <summary>0x41 bge.un</summary>
        Bge_un = 0x41,

        /// <summary>0x42 bgt.un</summary>
        Bgt_un = 0x42,

        /// <summary>0x43 ble.un</summary>
        Ble_un = 0x43,

        /// <summary>0x44 blt.un</summary>
        Blt_un = 0x44,

        /// <summary>0x45 switch</summary>
        Switch = 0x45,

        /// <summary>0x46 ldind.i1</summary>
        Ldind_i1 = 0x46,

        /// <summary>0x47 ldind.u1</summary>
        Ldind_u1 = 0x47,

        /// <summary>0x48 ldind.i2</summary>
        Ldind_i2 = 0x48,

        /// <summary>0x49 ldind.u2</summary>
        Ldind_u2 = 0x49,

        /// <summary>0x4A ldind.i4</summary>
        Ldind_i4 = 0x4A,

        /// <summary>0x4B ldind.u4</summary>
        Ldind_u4 = 0x4B,

        /// <summary>0x4C ldind.i8</summary>
        Ldind_i8 = 0x4C,

        /// <summary>0x4D ldind.i</summary>
        Ldind_i = 0x4D,

        /// <summary>0x4E ldind.r4</summary>
        Ldind_r4 = 0x4E,

        /// <summary>0x4F ldind.r8</summary>
        Ldind_r8 = 0x4F,

        /// <summary>0x50 ldind.ref</summary>
        Ldind_ref = 0x50,

        /// <summary>0x51 stind.ref</summary>
        Stind_ref = 0x51,

        /// <summary>0x52 stind.i1</summary>
        Stind_i1 = 0x52,

        /// <summary>0x53 stind.i2</summary>
        Stind_i2 = 0x53,

        /// <summary>0x54 stind.i4</summary>
        Stind_i4 = 0x54,

        /// <summary>0x55 stind.i8</summary>
        Stind_i8 = 0x55,

        /// <summary>0x56 stind.r4</summary>
        Stind_r4 = 0x56,

        /// <summary>0x57 stind.r8</summary>
        Stind_r8 = 0x57,

        /// <summary>0x58 add</summary>
        Add = 0x58,

        /// <summary>0x59 sub</summary>
        Sub = 0x59,

        /// <summary>0x5A mul</summary>
        Mul = 0x5A,

        /// <summary>0x5B div</summary>
        Div = 0x5B,

        /// <summary>0x5C div.un</summary>
        Div_un = 0x5C,

        /// <summary>0x5D rem</summary>
        Rem = 0x5D,

        /// <summary>0x5E rem.un</summary>
        Rem_un = 0x5E,

        /// <summary>0x5F and</summary>
        And = 0x5F,

        /// <summary>0x60 or</summary>
        Or = 0x60,

        /// <summary>0x61 xor</summary>
        Xor = 0x61,

        /// <summary>0x62 shl</summary>
        Shl = 0x62,

        /// <summary>0x63 shr</summary>
        Shr = 0x63,

        /// <summary>0x64 shr.un</summary>
        Shr_un = 0x64,

        /// <summary>0x65 neg</summary>
        Neg = 0x65,

        /// <summary>0x66 not</summary>
        Not = 0x66,

        /// <summary>0x67 conv.i1</summary>
        Conv_i1 = 0x67,

        /// <summary>0x68 conv.i2</summary>
        Conv_i2 = 0x68,

        /// <summary>0x69 conv.i4</summary>
        Conv_i4 = 0x69,

        /// <summary>0x6A conv.i8</summary>
        Conv_i8 = 0x6A,

        /// <summary>0x6B conv.r4</summary>
        Conv_r4 = 0x6B,

        /// <summary>0x6C conv.r8</summary>
        Conv_r8 = 0x6C,

        /// <summary>0x6D conv.u4</summary>
        Conv_u4 = 0x6D,

        /// <summary>0x6E conv.u8</summary>
        Conv_u8 = 0x6E,

        /// <summary>0x6F callvirt</summary>
        Callvirt = 0x6F,

        /// <summary>0x70 cpobj</summary>
        Cpobj = 0x70,

        /// <summary>0x71 ldobj</summary>
        Ldobj = 0x71,

        /// <summary>0x72 ldstr</summary>
        Ldstr = 0x72,

        /// <summary>0x73 newobj</summary>
        Newobj = 0x73,

        /// <summary>0x74 castclass</summary>
        Castclass = 0x74,

        /// <summary>0x75 isinst</summary>
        Isinst = 0x75,

        /// <summary>0x76 conv.r.un</summary>
        Conv_r_un = 0x76,

        /// <summary>0x79 unbox</summary>
        Unbox = 0x79,

        /// <summary>0x7A throw</summary>
        Throw = 0x7A,

        /// <summary>0x7B ldfld</summary>
        Ldfld = 0x7B,

        /// <summary>0x7C ldflda</summary>
        Ldflda = 0x7C,

        /// <summary>0x7D stfld</summary>
        Stfld = 0x7D,

        /// <summary>0x7E ldsfld</summary>
        Ldsfld = 0x7E,

        /// <summary>0x7F ldsflda</summary>
        Ldsflda = 0x7F,

        /// <summary>0x80 stsfld</summary>
        Stsfld = 0x80,

        /// <summary>0x81 stobj</summary>
        Stobj = 0x81,

        /// <summary>0x82 conv.ovf.i1.un</summary>
        Conv_ovf_i1_un = 0x82,

        /// <summary>0x83 conv.ovf.i2.un</summary>
        Conv_ovf_i2_un = 0x83,

        /// <summary>0x84 conv.ovf.i4.un</summary>
        Conv_ovf_i4_un = 0x84,

        /// <summary>0x85 conv.ovf.i8.un</summary>
        Conv_ovf_i8_un = 0x85,

        /// <summary>0x86 conv.ovf.u1.un</summary>
        Conv_ovf_u1_un = 0x86,

        /// <summary>0x87 conv.ovf.u2.un</summary>
        Conv_ovf_u2_un = 0x87,

        /// <summary>0x88 conv.ovf.u4.un</summary>
        Conv_ovf_u4_un = 0x88,

        /// <summary>0x89 conv.ovf.u8.un</summary>
        Conv_ovf_u8_un = 0x89,

        /// <summary>0x8A conv.ovf.i.un</summary>
        Conv_ovf_i_un = 0x8A,

        /// <summary>0x8B conv.ovf.u.un</summary>
        Conv_ovf_u_un = 0x8B,

        /// <summary>0x8C box</summary>
        Box = 0x8C,

        /// <summary>0x8D newarr</summary>
        Newarr = 0x8D,

        /// <summary>0x8E ldlen</summary>
        Ldlen = 0x8E,

        /// <summary>0x8F ldelema</summary>
        Ldelema = 0x8F,

        /// <summary>0x90 ldelem.i1</summary>
        Ldelem_i1 = 0x90,

        /// <summary>0x91 ldelem.u1</summary>
        Ldelem_u1 = 0x91,

        /// <summary>0x92 ldelem.i2</summary>
        Ldelem_i2 = 0x92,

        /// <summary>0x93 ldelem.u2</summary>
        Ldelem_u2 = 0x93,

        /// <summary>0x94 ldelem.i4</summary>
        Ldelem_i4 = 0x94,

        /// <summary>0x95 ldelem.u4</summary>
        Ldelem_u4 = 0x95,

        /// <summary>0x96 ldelem.i8</summary>
        Ldelem_i8 = 0x96,

        /// <summary>0x97 ldelem.i</summary>
        Ldelem_i = 0x97,

        /// <summary>0x98 ldelem.r4</summary>
        Ldelem_r4 = 0x98,

        /// <summary>0x99 ldelem.r8</summary>
        Ldelem_r8 = 0x99,

        /// <summary>0x9A ldelem.ref</summary>
        Ldelem_ref = 0x9A,

        /// <summary>0x9B stelem.i</summary>
        Stelem_i = 0x9B,

        /// <summary>0x9C stelem.i1</summary>
        Stelem_i1 = 0x9C,

        /// <summary>0x9D stelem.i2</summary>
        Stelem_i2 = 0x9D,

        /// <summary>0x9E stelem.i4</summary>
        Stelem_i4 = 0x9E,

        /// <summary>0x9F stelem.i8</summary>
        Stelem_i8 = 0x9F,

        /// <summary>0xA0 stelem.r4</summary>
        Stelem_r4 = 0xA0,

        /// <summary>0xA1 stelem.r8</summary>
        Stelem_r8 = 0xA1,

        /// <summary>0xA2 stelem.ref</summary>
        Stelem_ref = 0xA2,

        /// <summary>0xA3 ldelem</summary>
        Ldelem = 0xA3,

        /// <summary>0xA4 stelem</summary>
        Stelem = 0xA4,

        /// <summary>0xA5 unbox.any</summary>
        Unbox_any = 0xA5,

        /// <summary>0xB3 conv.ovf.i1</summary>
        Conv_ovf_i1 = 0xB3,

        /// <summary>0xB4 conv.ovf.u1</summary>
        Conv_ovf_u1 = 0xB4,

        /// <summary>0xB5 conv.ovf.i2</summary>
        Conv_ovf_i2 = 0xB5,

        /// <summary>0xB6 conv.ovf.u2</summary>
        Conv_ovf_u2 = 0xB6,

        /// <summary>0xB7 conv.ovf.i4</summary>
        Conv_ovf_i4 = 0xB7,

        /// <summary>0xB8 conv.ovf.u4</summary>
        Conv_ovf_u4 = 0xB8,

        /// <summary>0xB9 conv.ovf.i8</summary>
        Conv_ovf_i8 = 0xB9,

        /// <summary>0xBA conv.ovf.u8</summary>
        Conv_ovf_u8 = 0xBA,

        /// <summary>0xC2 refanyval</summary>
        Refanyval = 0xC2,

        /// <summary>0xC3 ckfinite</summary>
        Ckfinite = 0xC3,

        /// <summary>0xC6 mkrefany</summary>
        Mkrefany = 0xC6,

        /// <summary>0xD0 ldtoken</summary>
        Ldtoken = 0xD0,

        /// <summary>0xD1 conv.u2</summary>
        Conv_u2 = 0xD1,

        /// <summary>0xD2 conv.u1</summary>
        Conv_u1 = 0xD2,

        /// <summary>0xD3 conv.i</summary>
        Conv_i = 0xD3,

        /// <summary>0xD4 conv.ovf.i</summary>
        Conv_ovf_i = 0xD4,

        /// <summary>0xD5 conv.ovf.u</summary>
        Conv_ovf_u = 0xD5,

        /// <summary>0xD6 add.ovf</summary>
        Add_ovf = 0xD6,

        /// <summary>0xD7 add.ovf.un</summary>
        Add_ovf_un = 0xD7,

        /// <summary>0xD8 mul.ovf</summary>
        Mul_ovf = 0xD8,

        /// <summary>0xD9 mul.ovf.un</summary>
        Mul_ovf_un = 0xD9,

        /// <summary>0xDA sub.ovf</summary>
        Sub_ovf = 0xDA,

        /// <summary>0xDB sub.ovf.un</summary>
        Sub_ovf_un = 0xDB,

        /// <summary>0xDC endfinally</summary>
        Endfinally = 0xDC,

        /// <summary>0xDD leave</summary>
        Leave = 0xDD,

        /// <summary>0xDE leave.s</summary>
        Leave_s = 0xDE,

        /// <summary>0xDF stind.i</summary>
        Stind_i = 0xDF,

        /// <summary>0xE0 conv.u</summary>
        Conv_u = 0xE0,

        /// <summary>0xF8 prefix7</summary>
        Prefix7 = 0xF8,

        /// <summary>0xF9 prefix6</summary>
        Prefix6 = 0xF9,

        /// <summary>0xFA prefix5</summary>
        Prefix5 = 0xFA,

        /// <summary>0xFB prefix4</summary>
        Prefix4 = 0xFB,

        /// <summary>0xFC prefix3</summary>
        Prefix3 = 0xFC,

        /// <summary>0xFD prefix2</summary>
        Prefix2 = 0xFD,

        /// <summary>0xFE prefix1</summary>
        Prefix1 = 0xFE,

        /// <summary>0xFF prefixref</summary>
        Prefixref = 0xFF,

        /// <summary>0xFE00 arglist</summary>
        Arglist = 0xFE00,

        /// <summary>0xFE01 ceq</summary>
        Ceq = 0xFE01,

        /// <summary>0xFE02 cgt</summary>
        Cgt = 0xFE02,

        /// <summary>0xFE03 cgt.un</summary>
        Cgt_un = 0xFE03,

        /// <summary>0xFE04 clt</summary>
        Clt = 0xFE04,

        /// <summary>0xFE05 clt.un</summary>
        Clt_un = 0xFE05,

        /// <summary>0xFE06 ldftn</summary>
        Ldftn = 0xFE06,

        /// <summary>0xFE07 ldvirtftn</summary>
        Ldvirtftn = 0xFE07,

        /// <summary>0xFE09 ldarg</summary>
        Ldarg = 0xFE09,

        /// <summary>0xFE0A ldarga</summary>
        Ldarga = 0xFE0A,

        /// <summary>0xFE0B starg</summary>
        Starg = 0xFE0B,

        /// <summary>0xFE0C ldloc</summary>
        Ldloc = 0xFE0C,

        /// <summary>0xFE0D ldloca</summary>
        Ldloca = 0xFE0D,

        /// <summary>0xFE0E stloc</summary>
        Stloc = 0xFE0E,

        /// <summary>0xFE0F localloc</summary>
        Localloc = 0xFE0F,

        /// <summary>0xFE11 endfilter</summary>
        Endfilter = 0xFE11,

        /// <summary>0xFE12 unaligned.</summary>
        Unaligned = 0xFE12,

        /// <summary>0xFE13 volatile.</summary>
        Volatile = 0xFE13,

        /// <summary>0xFE14 tail.</summary>
        Tail = 0xFE14,

        /// <summary>0xFE15 initobj</summary>
        Initobj = 0xFE15,

        /// <summary>0xFE16 constrained.</summary>
        Constrained = 0xFE16,

        /// <summary>0xFE17 cpblk</summary>
        Cpblk = 0xFE17,

        /// <summary>0xFE18 initblk</summary>
        Initblk = 0xFE18,

        /// <summary>0xFE1A rethrow</summary>
        Rethrow = 0xFE1A,

        /// <summary>0xFE1C sizeof</summary>
        Sizeof = 0xFE1C,

        /// <summary>0xFE1D refanytype</summary>
        Refanytype = 0xFE1D,

        /// <summary>0xFE1E readonly.</summary>
        Readonly = 0xFE1E,
    }
}