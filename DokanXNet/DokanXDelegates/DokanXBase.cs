using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace DokanXBase
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BY_HANDLE_FILE_INFORMATION
    {
        public uint FileAttributes;
        public ComTypes.FILETIME CreationTime;
        public ComTypes.FILETIME LastAccessTime;
        public ComTypes.FILETIME LastWriteTime;
        public uint VolumeSerialNumber;
        public uint FileSizeHigh;
        public uint FileSizeLow;
        public uint NumberOfLinks;
        public uint FileIndexHigh;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DOKAN_FILE_INFO_X
    {
        public ulong Context;
        public ulong DokanContext;
        public IntPtr DokanOptions;
        public uint ProcessId;
        public byte IsDirectory;
        public byte DeleteOnClose;
        public byte PagingIo;
        public byte SynchronousIo;
        public byte Nocache;
        public byte WriteToEndOfFile;
    }
    [Flags]
    public  enum SECURITY_INFORMATION : uint
    {
        OWNER_SECURITY_INFORMATION = 0x00000001,
        GROUP_SECURITY_INFORMATION = 0x00000002,
        DACL_SECURITY_INFORMATION = 0x00000004,
        SACL_SECURITY_INFORMATION = 0x00000008,
        UNPROTECTED_SACL_SECURITY_INFORMATION = 0x10000000,
        UNPROTECTED_DACL_SECURITY_INFORMATION = 0x20000000,
        PROTECTED_SACL_SECURITY_INFORMATION = 0x40000000,
        PROTECTED_DACL_SECURITY_INFORMATION = 0x80000000
    }
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 4)]
    public struct SECURITY_DESCRIPTOR
    {
        public byte revision;
        public byte size;
        public short control;
        public IntPtr owner;
        public IntPtr group;
        public IntPtr sacl;
        public IntPtr dacl;
    }
    public delegate int FillFindData(ref WIN32_FIND_DATA file, IntPtr context);     
   // The CharSet must match the CharSet of the corresponding PInvoke signature
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct WIN32_FIND_DATA
    {
        public uint dwFileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    } 
    [Flags]
    public enum FILE_FS_ATTRIBUTE_INFORMATION
    {
        /// <summary>
        /// The file system supports case-sensitive file names. 
        /// </summary>
        FILE_CASE_SENSITIVE_SEARCH = 0x00000001,
        /// <summary>
        /// The file system preserves the case of file names when it places a name on disk. 
        /// </summary>
        FILE_CASE_PRESERVED_NAMES = 0x00000002,
        /// <summary>
        /// The file system supports Unicode in file names.  
        /// </summary>
        FILE_UNICODE_ON_DISK = 0x00000004,
        /// <summary>
        /// The file system preserves and enforces access control lists (ACL).  
        /// </summary>
        FILE_PERSISTENT_ACLS = 0x00000008,
        /// <summary>
        /// The file system supports file-based compression. This flag is incompatible with the FILE_VOLUME_IS_COMPRESSED flag.  
        /// </summary>
        FILE_FILE_COMPRESSION = 0x00000010,
        /// <summary>
        /// The file system supports per-user quotas.  
        /// </summary>
        FILE_VOLUME_QUOTAS = 0x00000020,
        /// <summary>
        /// The file system supports sparse files.  
        /// </summary>
        FILE_SUPPORTS_SPARSE_FILES = 0x00000040,
        /// <summary>
        /// The file system supports reparse points.  
        /// </summary>
        FILE_SUPPORTS_REPARSE_POINTS = 0x00000080,
        /// <summary>
        /// The file system supports remote storage.  
        /// </summary>
        FILE_SUPPORTS_REMOTE_STORAGE = 0x00000100,
        FS_LFN_APIS = 0x00004000,
        /// <summary>
        /// The specified volume is a compressed volume. This flag is incompatible with the FILE_FILE_COMPRESSION flag.  
        /// </summary>
        FILE_VOLUME_IS_COMPRESSED = 0x00008000,
        /// <summary>
        /// The file system supports object identifiers.  
        /// </summary>
        FILE_SUPPORTS_OBJECT_IDS = 0x00010000,
        /// <summary>
        /// The file system supports the Encrypted File System (EFS).  
        /// </summary>
        FILE_SUPPORTS_ENCRYPTION = 0x00020000,
        /// <summary>
        /// The file system supports named streams.  
        /// </summary>
        FILE_NAMED_STREAMS = 0x00040000,
        /// <summary>
        /// Microsoft Windows XP and later: The specified volume is read-only.  
        /// </summary>
        FILE_READ_ONLY_VOLUME = 0x00080000

    }
    public class HelperFunction
    {
        public static ComTypes.FILETIME DateTimeToFileTime(long d)
        {
            ComTypes.FILETIME ft = new ComTypes.FILETIME();
            ft.dwHighDateTime = (int)(d >> 32);
            ft.dwLowDateTime = (int)(d & 0xffffffff);
            return ft;
        }
       
        public static bool SetVolumeInfo(IntPtr VolumeNameBuffer,string VolumeName,int VolumeNameSize,IntPtr FSNameBuffer,string FSName,int FSNameSize){
            try
            {
                byte[] volumeByte = System.Text.Encoding.Unicode.GetBytes(VolumeName);
                byte[] FSNameByte = System.Text.Encoding.Unicode.GetBytes(FSName);
                if (FSNameSize <= FSNameByte.Length)
                {
                    Marshal.Copy(FSNameByte, 0, FSNameBuffer, FSNameSize);
                }
                else
                {
                    Marshal.Copy(FSNameByte, 0, FSNameBuffer, FSNameByte.Length);

                }
                if (VolumeNameSize <= volumeByte.Length)
                {
                    Marshal.Copy(FSNameByte, 0, FSNameBuffer, FSNameSize);
                }
                else
                {
                    Marshal.Copy(volumeByte, 0, VolumeNameBuffer, volumeByte.Length);
                }
                return true;
            }
            catch (Exception e) {

                return false;
            }
        }
    }
    
    public interface FileSystemCoreFunctions
    {

        uint Def_CreateFile(string filename, FileAccess access, FileShare share, FileMode mode, FileOptions options, IntPtr  info);
        uint Def_OpenDirectory(string filename, IntPtr  info);
        uint Def_CreateDirectory(string filename, IntPtr  info);
        uint Def_Cleanup(string filename, IntPtr  info);
        uint Def_CloseFile(string filename, IntPtr  info);
        uint Def_ReadFile(string filename, IntPtr buffer,uint bufferSize ,ref uint readBytes, long offset, IntPtr  info);
        uint Def_WriteFile(string filename, byte[] buffer, ref uint writtenBytes, long offset, IntPtr  info);
        uint Def_FlushFileBuffers(string filename, IntPtr  info);
        uint Def_GetFileInformation(string filename, ref BY_HANDLE_FILE_INFORMATION Information, IntPtr  info);
        uint Def_FindFiles(string filename, ref FillFindData FillFunction, IntPtr  info);
        uint Def_SetFileAttributes(string filename, uint Attribute, IntPtr  info);
        uint Def_SetFileTime(string filename, ComTypes.FILETIME ctime, ComTypes.FILETIME atime, ComTypes.FILETIME mtime, IntPtr  info);
        uint Def_DeleteFile(string filename, IntPtr  info);
        uint Def_DeleteDirectory(string filename, IntPtr  info);
        uint Def_MoveFile(string filename, string newname, bool replace, IntPtr  info);
        uint Def_SetEndOfFile(string filename, long length, IntPtr  info);
        uint Def_SetAllocationSize(string filename, long length, IntPtr  info);
        uint Def_LockFile(string filename, long offset, long length, IntPtr  info);
        uint Def_UnlockFile(string filename, long offset, long length, IntPtr  info);
        uint Def_GetDiskInfo(ref ulong Available, ref ulong Total, ref ulong Free);
        uint Def_GetVolumeInfo(IntPtr VolumeNameBuffer,uint VolumeNameSize,ref uint SerialNumber,ref uint MaxComponenetLegnth,ref uint FileSystemFeatures,IntPtr FileSystemNameBuffer,uint FileSystemNameSize);
        uint Def_Unmount(IntPtr  info);
    }
}