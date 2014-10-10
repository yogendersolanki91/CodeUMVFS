using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using DokanXBase;
using System.IO;
using System.Diagnostics;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleFS fs=new SimpleFS();
            DokanXNative.DokanNative x = new DokanXNative.DokanNative(fs, 0, "y", "GOOG", "TA");
            x.StartDokan();
        }
    }
    [Flags]
    [CLSCompliant(false)]
    enum FileAttributes : uint
    {
        /// <summary>
        /// A file that is read-only. Applications can read the file, but cannot write to it or delete it. This attribute is not honored on directories. For more information, see "You cannot view or change the Read-only or the System attributes of folders in Windows Server 2003, in Windows XP, or in Windows Vista".
        /// </summary>
        Readonly = 0x00000001,

        /// <summary>
        /// The file or directory is hidden. It is not included in an ordinary directory listing.
        /// </summary>
        Hidden = 0x00000002,

        /// <summary>
        /// A file or directory that the operating system uses a part of, or uses exclusively.
        /// </summary>
        System = 0x00000004,

        /// <summary>
        /// The handle that identifies a directory.
        /// </summary>
        Directory = 0x00000010,

        /// <summary>
        /// A file or directory that is an archive file or directory. Applications typically use this attribute to mark files for backup or removal.
        /// </summary>
        Archive = 0x00000020,

        /// <summary>
        /// This value is reserved for system use.
        /// </summary>
        Device = 0x00000040,

        /// <summary>
        /// A file that does not have other attributes set. This attribute is valid only when used alone.
        /// </summary>
        Normal = 0x00000080,

        /// <summary>
        /// A file that is being used for temporary storage. File systems avoid writing data back to mass storage if sufficient cache memory is available, because typically, an application deletes a temporary file after the handle is closed. In that scenario, the system can entirely avoid writing the data. Otherwise, the data is written after the handle is closed.
        /// </summary>
        Temporary = 0x00000100,

        /// <summary>
        /// A file that is a sparse file.
        /// </summary>
        SparseFile = 0x00000200,

        /// <summary>
        /// A file or directory that has an associated reparse point, or a file that is a symbolic link.
        /// </summary>
        ReparsePoint = 0x00000400,

        /// <summary>
        /// A file or directory that is compressed. For a file, all of the data in the file is compressed. For a directory, compression is the default for newly created files and subdirectories.
        /// </summary>
        Compressed = 0x00000800,

        /// <summary>
        /// The data of a file is not available immediately. This attribute indicates that the file data is physically moved to offline storage. This attribute is used by Remote Storage, which is the hierarchical storage management software. Applications should not arbitrarily change this attribute.
        /// </summary>
        Offline = 0x00001000,

        /// <summary>
        /// The file or directory is not to be indexed by the content indexing service.
        /// </summary>
        NotContentIndexed = 0x00002000,

        /// <summary>
        /// A file or directory that is encrypted. For a file, all data streams in the file are encrypted. For a directory, encryption is the default for newly created files and subdirectories.
        /// </summary>
        Encrypted = 0x00004000,

        /// <summary>
        /// This value is reserved for system use.
        /// </summary>
        Virtual = 0x00010000
    }
    class SimpleFS : FileSystemCoreFunctions
    {
        public static string[] RegClasses =new string[5]{ "ClassesRoot", "CurrentUser", "CurrentConfig", "LocalMachine","Users"};
        
        static RegistryValueKind[] RegValueType = new RegistryValueKind[7] { RegistryValueKind.Binary, RegistryValueKind.DWord, RegistryValueKind.MultiString, RegistryValueKind.QWord, RegistryValueKind.String, RegistryValueKind.ExpandString, RegistryValueKind.Unknown };
        public RegistryBlock ExtractInfo(string FileName)
        {
            RegistryBlock regInfo = new RegistryBlock();
            return regInfo;
        }
       public struct RegistryBlock
        {
           public string Root{get;set;}
           public string DirectoryPath{get;set;}
           public string BaseName{get;set;}
           public RegistryKey key { get; set; }
           public RegistryValueKind KeyType { get; set; }
           public ushort Level { get; set; }

        }
        struct RegistryFileBlock
        {
            public short level;
            public string KeyPointer;
            public string RootDir;
            public string ValueName;
            public int ValueKind;
            public RegistryFileBlock(string FilePath)
            {
                try
                {
                    if (!FilePath.EndsWith("autorun.inf", StringComparison.InvariantCultureIgnoreCase) && !FilePath.EndsWith("desktop.ini", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int tempType = -1, count = 0;
                        foreach (string s in regtypes)
                        {
                            if (FilePath.EndsWith(s, StringComparison.InvariantCultureIgnoreCase))
                            {
                                tempType = count;
                                break;
                            }
                            count++;
                        }
                        
                        if (FilePath == "\\") {
                            level = 1;
                            RootDir = "";
                            KeyPointer = "";
                            ValueName = "";
                            ValueKind = -1;
                        
                        }                       
                        //It is a Value type Means it is a file
                        else if (tempType >= 0)
                        {
                            
                            ValueKind = tempType;
                            level = 3;
                            int tempPos = FilePath.IndexOf(@"\", 1);
                            RootDir = FilePath.Substring(1, tempPos - 1);
                            FilePath = FilePath.Replace("\\" + RootDir + "\\", "");
                            FilePath = FilePath.Replace(regtypes[tempType], "");
                            int PointLastFile = FilePath.LastIndexOf("\\");
                            ValueName = FilePath.Substring(PointLastFile + 1);
                            FilePath = FilePath.Remove(PointLastFile);
                            // int SecondFile = FilePath.IndexOf("\\", 1);
                            KeyPointer = FilePath;//.Substring(SecondFile + 1);
                        }
                        //It is a folder
                        
                        else
                        {
                            int tempPos = FilePath.IndexOf(@"\", 1);
                            RootDir = FilePath.Substring(1);
                            int rootPoint = RootDir.IndexOf("\\");                            
                            //File Has SubFolder in Root
                            level = 2;
                            ValueKind = 0;
                            ValueName = "";
                            KeyPointer = "";
                            if (rootPoint >= 0)
                            {
                                RootDir = RootDir.Remove(rootPoint);                              
                                KeyPointer = FilePath.Substring(rootPoint+2);                                                                
                                return;
                            }
                            
                           
                        }


                    }
                    else
                    {
                        level = -1;// tempType;
                        ValueKind = -1;
                        KeyPointer = "";
                        RootDir = "";
                        ValueName = "";   
                    }
                    
                   
                }
                catch (Exception e) {
                    level = -1;// tempType;
                    ValueKind = -1;
                    KeyPointer = "";
                    RootDir = "";
                    ValueName = "";   
                }
            }
        }
        public Dictionary<string, RegistryKey> RootDirectory;     
        private Dictionary<string, bool> FileLocks;
        
        private RegistryKey GetRegistoryEntry(RegistryFileBlock block)
        {
            try
            {
                switch (block.level)
                {
                    //case 1:
                      // if(block.RootDir=""
                    case 2:
                        if (block.KeyPointer == "")
                            if (RootDirectory.ContainsKey(block.RootDir))
                            {
                                return RootDirectory[block.RootDir];
                            }
                            else
                                return null;
                        else
                            return RootDirectory[block.RootDir].OpenSubKey(block.KeyPointer, true);
                        break;
                    case 3:
                        return RootDirectory[block.RootDir].OpenSubKey(block.KeyPointer, true);
                        break;
                    default:
                        return null;
                        break;


                   
                }
                
            }
            catch (Exception ex)
            {
               // Console.WriteLine("Error:-" + ex.Message + ":" + name);
                return null;
            }
        }
        public SimpleFS()
        {
            RootDirectory = new Dictionary<string, RegistryKey>();
            RootDirectory["ClassesRoot"] = Registry.ClassesRoot;
            RootDirectory["CurrentUser"] = Registry.CurrentUser;
            RootDirectory["CurrentConfig"] = Registry.CurrentConfig;
            RootDirectory["LocalMachine"] = Registry.LocalMachine;
            RootDirectory["Users"] = Registry.Users;
        }
        static string[] regtypes = { ".regbin", ".regdword", ".regmltstr", ".regqwrod", ".regstr", ".regexpstr", ".regunk" };
        public uint Def_CreateFile(string filename, System.IO.FileAccess access, System.IO.FileShare share, System.IO.FileMode mode, System.IO.FileOptions options, IntPtr  info)
        {
            //return 0;
            RegistryFileBlock rb = new RegistryFileBlock(filename);
            if (rb.ValueKind > 0)
            {
                if (mode == FileMode.CreateNew || mode == FileMode.OpenOrCreate)
                {
                    if (rb.level == 2)
                    {
                        GetRegistoryEntry(rb).CreateSubKey(rb.KeyPointer.Substring(rb.KeyPointer.LastIndexOf("\\") + 1));
                        return 0;
                    }
                    int check;
                    
                    if (rb.level == 3 && filename.EndsWith(regtypes[rb.ValueKind],StringComparison.CurrentCultureIgnoreCase))
                    {
                        switch(rb.ValueKind)
                        {
                        case 1:
                            GetRegistoryEntry(rb).SetValue(rb.ValueName,0,RegValueType[rb.ValueKind]);
                            break;
                        case 2:
                            GetRegistoryEntry(rb).SetValue(rb.ValueName, "", RegValueType[rb.ValueKind]);
                            break;
                        case 3:
                            GetRegistoryEntry(rb).SetValue(rb.ValueName, 0, RegValueType[rb.ValueKind]);
                            break;
                        case 4:
                            GetRegistoryEntry(rb).SetValue(rb.ValueName , "", RegValueType[rb.ValueKind]);
                            break;
                            default:
                            return 0xC000000F;
                        
                        }
                        return 0;
                    }
                }               
            }
            else
            {
                if (filename == "\\")
                {
                    return 0;
                }
                if (filename.EndsWith("dekstop.ini", StringComparison.CurrentCultureIgnoreCase) || filename.EndsWith("autorun.inf", StringComparison.CurrentCultureIgnoreCase))
                    return 0;
                    // else
                   // Console.WriteLine("Invalid New File {0}",filename);
                return 0xC000000F;
            }
            return 0xC000000F;
          
        }

        public uint Def_OpenDirectory(string filename, IntPtr  info)
        {
            
            RegistryFileBlock rb = new RegistryFileBlock(filename);
            
            if (rb.ValueKind >= 0)
            {
               // Console.WriteLine("----------{0}---------", filename);
                //Console.WriteLine("KeyPointer={0},\n RootDIr={1},\n ValueKind={2},\n ValueNmae={3},\n level={4},\n", rb.KeyPointer, rb.RootDir, rb.ValueKind, rb.ValueName, rb.level);
                return 0;
            }
            else
            {
                if(filename=="\\")
                    return 0;
                return 0xC000000F;
            }
        }

        public uint Def_CreateDirectory(string filename, IntPtr  info)
        {
            return 0;
        }

        public uint Def_Cleanup(string filename, IntPtr  info)
        {
            return 0;
        }

        public uint Def_CloseFile(string filename, IntPtr  info)
        {
            return 0;
        }

        public uint Def_ReadFile(string fileName, IntPtr buffer,uint BufferSize, ref uint NumberByteReadSuccess, long Offset, IntPtr  info)
        {
            int fileLengnth = 0;
            try
            {
              //  Console.WriteLine("Read {0}", fileName);
                RegistryFileBlock FileBlock = new RegistryFileBlock(fileName);
            
                if (FileBlock.level==3)
                {
                    RegistryKey key = GetRegistoryEntry(FileBlock);
                    fileName = fileName.Remove(fileName.LastIndexOf("."));
                    if (key == null)
                    {
                       // Console.WriteLine("Key Error {0}", fileName.Replace(".value", ""));
                        return 0xC000046;
                    }
                    string value;
                    if (key.GetValue(FileBlock.ValueName) == null)//.ToString();
                        value = "";
                    else
                        value = key.GetValue(FileBlock.ValueName).ToString();

                    byte[] file = Encoding.ASCII.GetBytes(value);
                    fileLengnth = file.Length;
                    if (BufferSize > file.Length - Offset)
                    {
                        NumberByteReadSuccess = (uint)(fileLengnth - Offset);
                        System.Runtime.InteropServices.Marshal.Copy(file, (int)Offset, buffer, (int)NumberByteReadSuccess);
                    }
                    else
                    {
                        NumberByteReadSuccess = BufferSize;
                        System.Runtime.InteropServices.Marshal.Copy(file, (int)Offset, buffer, (int)BufferSize);
                    }
                    return 0;
                }
                    
                else if (FileBlock.level > 0)
                {
                    RegistryKey x = GetRegistoryEntry(FileBlock);
                    if (x != null)
                    {
                        string val = "Description of the folder\n---------------------------- \n \n1 Total Sub Key=" + x.GetSubKeyNames().Length + " \nTotal Value at this node " + x.GetValueNames().Length+"\n";
                        byte[] file = Encoding.ASCII.GetBytes(val);
                        // Create a memory stream from those bytes.
                        fileLengnth = file.Length;
                        if (BufferSize > file.Length - Offset)
                        {
                            NumberByteReadSuccess = (uint)(fileLengnth - Offset);
                            System.Runtime.InteropServices.Marshal.Copy(file, (int)Offset, buffer, (int)NumberByteReadSuccess);
                        }
                        else
                        {
                            NumberByteReadSuccess = BufferSize;
                            System.Runtime.InteropServices.Marshal.Copy(file, (int)Offset, buffer, (int)BufferSize);
                        }

                    }
                    else
                        return 0xC000000F;
                    return 0;           
                }


                return 0;
            }
            catch (Exception ex)
            {
                
                //Get a StackTrace object for the exception
                StackTrace st = new StackTrace(0,true);

                //Get the first stack frame
                StackFrame frame = st.GetFrame(0);

                //Get the file name
                string fileNa = frame.GetFileName();

                //Get the method name
                string methodName = frame.GetMethod().Name;

                //Get the line number from the stack frame
                int line = frame.GetFileLineNumber();

                //Get the column number
                int col = frame.GetFileColumnNumber();

                Console.WriteLine("Read Error {0} with| BufferSize {1}| \n| FileLegnth {2} | \n |Offset {3}|", ex.Message,BufferSize,fileLengnth,Offset);


                return 0xC0000467;

            }

        }
       
        public uint Def_WriteFile(string fileName, byte[] buffer, ref uint writtenBytes, long offset, IntPtr  info)
        {
            try
            {
             //   Console.WriteLine("Write {0}", fileName);
                RegistryFileBlock FileBlock = new RegistryFileBlock(fileName);

                if (FileBlock.level == 3)
                {
                    string extension = fileName.Substring(fileName.LastIndexOf("."));
                    RegistryKey key = GetRegistoryEntry(FileBlock);
                   // fileName = fileName.Remove(fileName.LastIndexOf("."));
                    if (key == null)
                    {
                        Console.WriteLine("Key Error {0}", fileName.Replace(".value", ""));
                        return 0xC000046;
                    }
                    string value;
                    if (key.GetValue(FileBlock.ValueName) == null)//.ToString();
                        value = "";
                    else
                        value = key.GetValue(FileBlock.ValueName).ToString();
                    byte[] file = Encoding.ASCII.GetBytes(value);
                    using (MemoryStream memory = new MemoryStream())
                    {

                        memory.Seek(offset, SeekOrigin.Begin);
                        //memory.Write(file,offset, file.Length);
                        memory.Write(buffer,(int)offset, buffer.Length);
                        
                        byte[] finalValue=memory.ToArray();
                        
                        string val =System.Text.Encoding.UTF8.GetString(finalValue);
                        switch(extension.Replace(".reg",""))
                        {
                            case "str":
                                Console.WriteLine("Value Wroting STR {0}",val);
                                key.SetValue(FileBlock.ValueName, val, RegistryValueKind.String);
                                break;
                            case "qword":
                                long lval;
                                if (long.TryParse(val, out lval))
                                {
                                    key.SetValue(FileBlock.ValueName, lval, RegistryValueKind.QWord);
                                }
                                else
                                {
                                    Console.WriteLine("Very Bad Format[Qword]  {0}", val);
                                    return 0xC0000467;
                                }
                                break;
                            case "mtlstr":
                                key.SetValue(FileBlock.ValueName,val, RegistryValueKind.MultiString);
                                break;
                            case "dword":
                                //System.Text.Encoding.UTF8.GetString(finalValue);// BitConverter.ToString(finalValue, 0);
                                int x;
                                if (int.TryParse(val, out x))
                                {
                                    Console.WriteLine("Format Accepted  {0} \n {1} \n {2} {3}", val,FileBlock.ValueName,FileBlock.KeyPointer,key.Name);                                    
                                    key.SetValue(FileBlock.ValueName, x, RegistryValueKind.DWord);
                                    return 0;
                                }
                                else
                                {
                                    Console.WriteLine("Very Bad Forma[Dword]t  {0}", val);
                                    return 0xC0000467;
                                }
                                break;
                            default:
                         //       Console.WriteLine("Not Implemented format " + extension.Replace("reg", ""));
                                return 0xC0000467;
                                break;

                         }
                        
                    }
                    return 0;
               }
                else 
                {
                    //Console.WriteLine("Trying to reader folder" + fileName);
                }


                return 0;
              }
             catch (Exception ex)
              {
                 Console.WriteLine("Write File Error {0}  Line # {1} ", ex.Message, (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber());
                  return 0xC0000467;

              }
        }

        public uint Def_FlushFileBuffers(string filename, IntPtr  info)
        {
            return 0;
        }

        public uint Def_GetFileInformation(string fileName, ref BY_HANDLE_FILE_INFORMATION information, IntPtr  info)
        {
            try
            {
                RegistryFileBlock FileBlock = new RegistryFileBlock(fileName);

                
                if (FileBlock.level == 1)
                {
                    information.FileAttributes = (uint)FileAttributes.Directory;
                    information.LastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                    information.LastWriteTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                    information.CreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());


                    return 0;
                }
                RegistryKey key = GetRegistoryEntry(FileBlock);
                if (key == null)
                {
                    return 0xC0000467;
                }
                if (FileBlock.level==3)
                {
                  //  fileName = fileName.Remove(fileName.LastIndexOf("."));
                    string value; 
                    if(key.GetValue(FileBlock.ValueName)==null)//.ToString();
                    value = "";
                     else
                    value=key.GetValue(FileBlock.ValueName).ToString();
                    information.FileAttributes = (uint)FileAttributes.Readonly;
                    information.FileSizeLow = (uint)value.Length;
                    information.LastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                    information.LastWriteTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                    information.CreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());

                    return 0;
                }
                information.FileAttributes = (uint)FileAttributes.Directory;
                information.LastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                information.LastWriteTime =HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                information.CreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                return 0;
            }
            catch (Exception ex)
            {
              Console.WriteLine("Error : {0}", ex.Message);
                return 0xC0000467;
            }
        }

        public uint Def_FindFiles(string fileName, ref FillFindData NativeFillFunction, IntPtr  info)
        {
            try
            {
                RegistryFileBlock FileBlock = new RegistryFileBlock(fileName);        
                if (FileBlock.level == 1)
                {
                    foreach (string name in RootDirectory.Keys)
                    {
                        WIN32_FIND_DATA finfo = new WIN32_FIND_DATA();
                        finfo.cFileName = name;
                        finfo.dwFileAttributes = (uint)FileAttributes.Directory;
                        finfo.ftLastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                        finfo.ftLastWriteTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                        finfo.ftCreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                        NativeFillFunction(ref finfo,info);
                    }
                    return 0;
                }
                else
                {
                    RegistryKey key = GetRegistoryEntry(FileBlock);
                    if (key == null)
                    {
                        Console.WriteLine("Find File Error {0}", fileName);
                        return 0xC0000467;
                    }
                    foreach (string name in key.GetSubKeyNames())
                    {
                        WIN32_FIND_DATA finfo = new WIN32_FIND_DATA();
                        finfo.cFileName = name;
                        finfo.dwFileAttributes = (uint)FileAttributes.Directory;
                        finfo.ftLastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                        finfo.ftLastWriteTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                        finfo.ftCreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                        NativeFillFunction(ref finfo, info);

                    }
                    foreach (string name in key.GetValueNames())
                    {

                        WIN32_FIND_DATA finfo = new WIN32_FIND_DATA();
                        switch (key.GetValueKind(name))
                        {
                            case RegistryValueKind.Binary:
                                finfo.cFileName = name + ".regbin";
                                break;
                            case RegistryValueKind.DWord:
                                finfo.cFileName = name + ".regdword";
                                break;
                            case RegistryValueKind.MultiString:
                                finfo.cFileName = name + ".regmltstr";
                                break;
                            case RegistryValueKind.QWord:
                                finfo.cFileName = name + ".regqwrod";
                                break;
                            case RegistryValueKind.String:
                                finfo.cFileName = name + ".regstr";
                                break;
                            case RegistryValueKind.ExpandString:
                                finfo.cFileName = name + ".regexpstr";
                                break;
                            case RegistryValueKind.Unknown:
                                finfo.cFileName = name + ".regunk";
                                break;
                            default:
                                break;
                        }


                       // finfo.cFileName = name;
                        finfo.dwFileAttributes = (uint)FileAttributes.Readonly;
                        finfo.ftLastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                        finfo.ftLastWriteTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                        finfo.ftCreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                        finfo.nFileSizeLow = (uint)key.GetValue(name).ToString().Length;
                        NativeFillFunction(ref finfo,info);
                    }
                    return 0;
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                return 0xC0000467;
            }
        }

        public uint Def_SetFileAttributes(string filename, uint Attribute, IntPtr  info)
        {
            return 0;
        }

        public uint Def_SetFileTime(string filename, System.Runtime.InteropServices.ComTypes.FILETIME ctime, System.Runtime.InteropServices.ComTypes.FILETIME atime, System.Runtime.InteropServices.ComTypes.FILETIME mtime, IntPtr  info)
        {
            return 0;
        }

        public uint Def_DeleteFile(string filename, IntPtr  info)
        {
            return 0;
        }

        public uint Def_DeleteDirectory(string filename, IntPtr  info)
        {
            return 0;
        }

        public uint Def_MoveFile(string filename, string newname, bool replace, IntPtr  info)
        {
            return 0;
        }

        public uint Def_SetEndOfFile(string filename, long length, IntPtr  info)
        {
            return 0;
        }

        public uint Def_SetAllocationSize(string filename, long length, IntPtr  info)
        {
            return 0;
        }

        public uint Def_LockFile(string filename, long offset, long length, IntPtr  info)
        {
            return 0;
        }

        public uint Def_UnlockFile(string filename, long offset, long length, IntPtr  info)
        {
            return 0;
        }
        public uint Def_GetDiskInfo(ref ulong Available, ref ulong Total, ref ulong Free) {
            Available = 5000000;
            Total = 10000000;
            Free = 5000000;
            return 0;
        
        }
        public uint Def_GetVolumeInfo(IntPtr VolumeNameBuffer,uint VolumeNameSize,ref uint SerialNumber,ref uint MaxComponenetLegnth,ref uint FileSystemFeatures,IntPtr FileSystemNameBuffer,uint FileSystemNameSize)
        {
            try
            {
                string VolumeName = "Test123";
                string FileSystemName = "Bacho";
                HelperFunction.SetVolumeInfo(VolumeNameBuffer, VolumeName, (int)VolumeNameSize, FileSystemNameBuffer, FileSystemName, (int)FileSystemNameSize);             
                MaxComponenetLegnth = 256;
                FileSystemFeatures = (uint)FILE_FS_ATTRIBUTE_INFORMATION.FILE_READ_ONLY_VOLUME | (uint)FILE_FS_ATTRIBUTE_INFORMATION.FILE_UNICODE_ON_DISK;
                SerialNumber = (uint)this.GetHashCode();
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0xC0000467;
            }
        
        }
        public uint Def_Unmount(IntPtr  info)
        {
            return 0;
        }

        
    }
}
