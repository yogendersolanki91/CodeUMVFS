using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DokanXBase;
using DokanXNative;

namespace NullFileSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            DokanNative native = new DokanNative(new NullFileSystem(),215,"P:","NULL","NULL");
            native.StartDokan();
         
        }
    }
    class NullFileSystem : FileSystemCoreFunctions {


        public uint Def_Cleanup(string filename, IntPtr info)
        {
           return 0;
        }

        public uint Def_CloseFile(string filename, IntPtr info)
        {
           return 0;
        }

        public uint Def_CreateDirectory(string filename, IntPtr info)
        {  return 0;
        }

        public uint Def_CreateFile(string filename, System.IO.FileAccess access, System.IO.FileShare share, System.IO.FileMode mode, System.IO.FileOptions options, IntPtr info)
        {
            //Console.WriteLine("Wait");
            int x = 0;
           return 0;
        }

        public uint Def_DeleteDirectory(string filename, IntPtr info)
        {
           return 0;
        }

        public uint Def_DeleteFile(string filename, IntPtr info)
        {
           return 0;
        }

        public uint Def_FindFiles(string filename, ref FillFindData FillFunction, IntPtr info)
        {
           return 0;
        }

        public uint Def_FlushFileBuffers(string filename, IntPtr info)
        {
           return 0;
        }

        public uint Def_GetDiskInfo(ref ulong Available, ref ulong Total, ref ulong Free)
        {
            Total = 10000000000;
            Available = 10000000000;
            Free = 10000000000;
           return 0;
        }

        public uint Def_GetFileInformation(string filename, ref BY_HANDLE_FILE_INFORMATION Information, IntPtr info)
        {
           return 0;
        }

        public uint Def_GetVolumeInfo(IntPtr VolumeNameBuffer, uint VolumeNameSize, ref uint SerialNumber, ref uint MaxComponenetLegnth, ref uint FileSystemFeatures, IntPtr FileSystemNameBuffer, uint FileSystemNameSize)
        {
           return 0;
        }

        public uint Def_LockFile(string filename, long offset, long length, IntPtr info)
        {
           return 0;
        }

        public uint Def_MoveFile(string filename, string newname, bool replace, IntPtr info)
        {
           return 0;
        }

        public uint Def_OpenDirectory(string filename, IntPtr info)
        {
            int x = 0;
           return 0;
        }

        public uint Def_ReadFile(string filename, IntPtr buffer, uint bufferSize, ref uint readBytes, long offset, IntPtr info)
        {
            readBytes = bufferSize;
           return 0;
        }

        public uint Def_SetAllocationSize(string filename, long length, IntPtr info)
        {
           return 0;
        }

        public uint Def_SetEndOfFile(string filename, long length, IntPtr info)
        {
           return 0;
        }

        public uint Def_SetFileAttributes(string filename, uint Attribute, IntPtr info)
        {
           return 0;
        }

        public uint Def_SetFileTime(string filename, System.Runtime.InteropServices.ComTypes.FILETIME ctime, System.Runtime.InteropServices.ComTypes.FILETIME atime, System.Runtime.InteropServices.ComTypes.FILETIME mtime, IntPtr info)
        {
           return 0;
        }

        public uint Def_UnlockFile(string filename, long offset, long length, IntPtr info)
        {
           return 0;
        }

        public uint Def_Unmount(IntPtr info)
        {
           return 0;
        }

        public uint Def_WriteFile(string filename, byte[] buffer, ref uint writtenBytes, long offset, IntPtr info)
        {
            //writtenBytes=buffer.Length
           return 0;
        }
    }
}
