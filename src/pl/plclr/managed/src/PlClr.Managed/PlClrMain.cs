﻿using System;
using System.Runtime.InteropServices;

namespace PlClr
{
    public delegate IntPtr PAllocDelegate(ulong size);
    public delegate IntPtr RePAllocDelegate(IntPtr ptr, ulong size);
    public delegate void PFreeDelegate(IntPtr ptr);
    public delegate void ELogDelegate(int level, IntPtr message);
    // ReSharper disable once UnusedMember.Global
    public delegate IntPtr PlClrMainDelegate(IntPtr args, int sizeBytes);
    public delegate IntPtr FunctionCallDelegate(ReadOnlySpan<NullableDatum> values);


    public static class PlClrMain
    {
        #region Private structs for marshalling

        // ReSharper disable FieldCanBeMadeReadOnly.Local

        [StructLayout(LayoutKind.Sequential)]
        private struct ClrSetupInfo
        {
            public IntPtr PallocFunctionPtr;
            public IntPtr Palloc0FunctionPtr;
            public IntPtr RePallocFunctionPtr;
            public IntPtr PFreeFunctionPtr;
            public IntPtr ELogFunctionPtr;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HostSetupInfo
        {
            public IntPtr CompileFunctionPtr;
            public IntPtr ExecuteFunctionPtr;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FunctionCompileInfoPrivate
        {
            public uint FunctionOid;
            public IntPtr FunctionNamePtr;
            public IntPtr FunctionBodyPtr;
            public uint ReturnValueType;
            public bool ReturnsSet;
            public int NumberOfArguments;
            public IntPtr ArgumentTypes;
            public IntPtr ArgumentNames;
            public IntPtr ArgumentModes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FunctionCompileResult
        {
            public IntPtr ExecuteDelegatePtr;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FunctionCallInfoPrivate
        {
            public IntPtr ExecuteDelegatePtr;
            public int NumberOfArguments;
            public IntPtr ArgumentValues;
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct FunctionExecuteResult
        {
            
        }

        // ReSharper restore FieldCanBeMadeReadOnly.Local

        #endregion

        private static readonly PlClrMainDelegate CompileDelegate = Compile;
        private static readonly PlClrMainDelegate ExecuteDelegate = Execute;

        /// <summary>
        /// This is the initial setup Method.
        /// It initializes all delegates that are used for communication
        /// between the clr an the backend.
        /// </summary>
        /// <param name="arg">Pointer to a <see cref="ClrSetupInfo"/> struct</param>
        /// <param name="argLength">The size of the passed <see cref="ClrSetupInfo"/> struct</param>
        /// <returns></returns>
        public static IntPtr Setup(IntPtr arg, int argLength)
        {
            try
            {
                if (arg == IntPtr.Zero)
                {
                    Console.Error.WriteLine(
                        $"Argument {nameof(arg)} must not be NULL");
                    return IntPtr.Zero;
                }

                if (argLength < System.Runtime.InteropServices.Marshal.SizeOf<ClrSetupInfo>())
                {
                    Console.Error.WriteLine(
                        $"Argument {nameof(argLength)} is {argLength} but is expected to be greater than or equal to {System.Runtime.InteropServices.Marshal.SizeOf<ClrSetupInfo>()}");
                    return IntPtr.Zero;
                }

                var clrSetupInfo = System.Runtime.InteropServices.Marshal.PtrToStructure<ClrSetupInfo>(arg);

                if (clrSetupInfo.PallocFunctionPtr == IntPtr.Zero)
                {
                    Console.Error.WriteLine($"Field {nameof(clrSetupInfo.PallocFunctionPtr)} in struct {nameof(ClrSetupInfo)} must not be NULL");
                    return IntPtr.Zero;
                }
                if (clrSetupInfo.Palloc0FunctionPtr == IntPtr.Zero)
                {
                    Console.Error.WriteLine($"Field {nameof(clrSetupInfo.Palloc0FunctionPtr)} in struct {nameof(ClrSetupInfo)} must not be NULL");
                    return IntPtr.Zero;
                }
                if (clrSetupInfo.RePallocFunctionPtr == IntPtr.Zero)
                {
                    Console.Error.WriteLine($"Field {nameof(clrSetupInfo.RePallocFunctionPtr)} in struct {nameof(ClrSetupInfo)} must not be NULL");
                    return IntPtr.Zero;
                }
                if (clrSetupInfo.PFreeFunctionPtr == IntPtr.Zero)
                {
                    Console.Error.WriteLine($"Field {nameof(clrSetupInfo.PFreeFunctionPtr)} in struct {nameof(ClrSetupInfo)} must not be NULL");
                    return IntPtr.Zero;
                }
                if (clrSetupInfo.ELogFunctionPtr == IntPtr.Zero)
                {
                    Console.Error.WriteLine($"Field {nameof(clrSetupInfo.ELogFunctionPtr)} in struct {nameof(ClrSetupInfo)} must not be NULL");
                    return IntPtr.Zero;
                }

                var palloc = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<PAllocDelegate>(clrSetupInfo.PallocFunctionPtr);
                var palloc0 = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<PAllocDelegate>(clrSetupInfo.Palloc0FunctionPtr);
                var repalloc = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<RePAllocDelegate>(clrSetupInfo.RePallocFunctionPtr);
                var pfree = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<PFreeDelegate>(clrSetupInfo.PFreeFunctionPtr);
                var elog = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<ELogDelegate>(clrSetupInfo.ELogFunctionPtr);

                ServerMemory.Initialize(palloc, palloc0, repalloc, pfree);
                ServerLog.Initialize(elog);

                HostSetupInfo hostSetupInfo;
                hostSetupInfo.CompileFunctionPtr =
                    System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(CompileDelegate);
                hostSetupInfo.ExecuteFunctionPtr =
                    System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(ExecuteDelegate);

                var size = System.Runtime.InteropServices.Marshal.SizeOf<HostSetupInfo>();
                var ptr = ServerMemory.Palloc((ulong)size);
                System.Runtime.InteropServices.Marshal.StructureToPtr(hostSetupInfo, ptr, false);
                return ptr;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"An unexpected exception occured during PL/CLR setup: {e}");
                return IntPtr.Zero;
            }
        }

        public static IntPtr Compile(IntPtr arg, int argLength)
        {
            if (argLength < System.Runtime.InteropServices.Marshal.SizeOf(typeof(FunctionCompileInfoPrivate)))
                return IntPtr.Zero;

            var compileInfo = GetFunctionCompileInfo(arg);

            var executeDelegate = CSharpCompiler.Compile(compileInfo);
            if (executeDelegate == null)
            {
                return IntPtr.Zero;
            }

            FunctionCompileResult result;
            result.ExecuteDelegatePtr =
                System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(executeDelegate);
            var ret = ServerMemory.Palloc((ulong)System.Runtime.InteropServices.Marshal.SizeOf<FunctionCompileResult>());
            System.Runtime.InteropServices.Marshal.StructureToPtr(result, ret, false);
            return ret;

            static unsafe FunctionCompileInfo GetFunctionCompileInfo(IntPtr arg)
            {
                var ci = System.Runtime.InteropServices.Marshal.PtrToStructure<FunctionCompileInfoPrivate>(arg);

                var functionName = Marshal.PtrToStringPFree(ci.FunctionNamePtr);
                if (functionName == null)
                {
                    ServerLog.ELog(SeverityLevel.Error, "The Function name must not be NULL");
                    // unreachable as Elog >= Error will tear down th process.
                    throw new Exception("Unreachable");
                }

                var functionBody = Marshal.PtrToStringPFree(ci.FunctionBodyPtr);
                if (functionBody == null)
                {
                    ServerLog.ELog(SeverityLevel.Error, "The Function body must not be NULL");
                    // unreachable as Elog >= Error will tear down th process.
                    throw new Exception("Unreachable");
                }

                var nArgs = ci.NumberOfArguments;

                if (nArgs == 0)
                    return new FunctionCompileInfo(ci.FunctionOid, functionName, functionBody, ci.ReturnValueType, ci.ReturnsSet);

                if (ci.ArgumentTypes == IntPtr.Zero)
                {
                    ServerLog.ELog(SeverityLevel.Error, $"The Function has {nArgs} Arguments but the array of argument types is empty");
                    // unreachable as Elog >= Error will tear down th process.
                    throw new Exception("Unreachable");
                }

                var argTypes = new uint[nArgs];
                new Span<uint>((void*)ci.ArgumentTypes, nArgs).CopyTo(new Span<uint>(argTypes, 0, nArgs));
                ServerMemory.PFree(ci.ArgumentTypes);

                string[]? argNames = null;
                if (ci.ArgumentNames != IntPtr.Zero)
                {
                    var argNamePtrs = new IntPtr[nArgs];
                    new Span<IntPtr>((void*)ci.ArgumentNames, nArgs).CopyTo(new Span<IntPtr>(argNamePtrs, 0, nArgs));
                    argNames = new string[nArgs];
                    for (int i = 0; i < nArgs; i++)
                    {
                        // Missing argument names are an empty string not null so we don't expect null here
                        argNames[i] = Marshal.PtrToStringPFree(argNamePtrs[i])!;
                    }
                    ServerMemory.PFree(ci.ArgumentNames);
                }

                byte[]? argModes = null;
                if (ci.ArgumentModes != IntPtr.Zero)
                {
                    argModes = new byte[nArgs];
                    new Span<byte>((void*)ci.ArgumentModes, nArgs).CopyTo(new Span<byte>(argModes, 0, nArgs));
                    ServerMemory.PFree(ci.ArgumentModes);
                }

                return new FunctionCompileInfo(ci.FunctionOid, functionName, functionBody, ci.ReturnValueType, ci.ReturnsSet, nArgs, argTypes, argNames,
                    argModes);
            }
        }

        public static unsafe IntPtr Execute(IntPtr arg, int argLength)
        {
            var ci = System.Runtime.InteropServices.Marshal.PtrToStructure<FunctionCallInfoPrivate>(arg);
            var nArgs = ci.NumberOfArguments;
            var callDelegate =
                System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<FunctionCallDelegate>(
                    ci.ExecuteDelegatePtr);

            return callDelegate(new ReadOnlySpan<NullableDatum>((void*) ci.ArgumentValues, nArgs));
        }
    }
}
