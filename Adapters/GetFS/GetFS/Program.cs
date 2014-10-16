using System;
using System.Threading.Tasks;

using Google.Apis.Discovery.v1;
using Google.Apis.Discovery.v1.Data;
using Google.Apis.Services;
using Google.Apis.Customsearch;
using Google.Apis.Customsearch.v1;
using DokanXBase;
using DokanXNative;
using Utility;
using NodeRsolver;
using System.Text;
using System.Collections.Generic;
using Kaliko.ImageLibrary;
using Kaliko.ImageLibrary.Filters;


namespace Discovery.ListAPIs
{
    /// <summary>
    /// This example uses the discovery API to list all APIs in the discovery repository.
    /// https://developers.google.com/discovery/v1/using.
    /// <summary>
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Discovery API Sample");

            Console.WriteLine("====================");
            string[] st = { "" };
           string[] test = { @"\sdasd", @"\Serc\ram.kp", @"\Serc\ram.kp?", @"\Serc\ram.kp!q=9088&go=!9088", @"\Serc\ram.kp!gp=4343&yu=997887", @"\Serc\ram.kp!", @"\Serc\ram.kp!", "Ram!", "Ramkimaa.jpg!", "Ramkimaa.jpg!q=0", "Ramkimaa.jpg!q90", "Ramkimaa.jpg!S=09009&sss=990790HAJKSJK" };
            try
            {
                /*      foreach (string t in test)
                     {
                         Console.WriteLine("\n==========={0}==============",t);
                         NodeRsolver.VNode s = new NodeRsolver.VNode(t, ref st);
                         Console.WriteLine("{0} {1} {2} {3}",s.isFile ,s.isValid, s.fileExtention, s.fileName);
                         foreach (string key in s.param.Keys)
                         {
                             Console.WriteLine("{0}  {1}", key, s.param[key]);
                         }
                     }


                     Task<Google.Apis.Customsearch.v1.Data.Search> ret =SearchGoogle("YOgender");
                     ret.Wait();
                     foreach (Google.Apis.Customsearch.v1.Data.Result s in ret.Result.Items)
                     {
                         Console.WriteLine("{0}-{1}",s.Snippet,s.HtmlFormattedUrl);
                     }    */
                DokanNative native = new DokanNative(new RestFS(), 10, "O", "REST", "REST");
                native.StartDokan();
          

                
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                }
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        public class ServiceProvider
        {

            public static async Task<Google.Apis.Customsearch.v1.Data.Search> SearchGoogle(string query,int start)
            {
                // Create the service.

                var service = new CustomsearchService(new BaseClientService.Initializer
                {
                    ApiKey = "AIzaSyDb2_RDVUifJvYH3Xq6B_J9pEpTyylXnGI",
                });

                var lisreq = service.Cse.List(query);
                lisreq.Start = start;
                lisreq.Cx = "009117456931165509268:km0cf-wdaws";
                // Run the request.
                Console.WriteLine("Executing a list request...");
                var result = await lisreq.ExecuteAsync();
                lisreq.Start = start;

                return result;

            }
            public static byte[] ImageCrop(string completePath,int x,int y,int w,int h)
            {
                return null;
            }
            public static byte[] ImageResize(string completePath,int w,int h)
            {
                return null;
            }
            public static byte[] ImageFilter(string completePath,string fileterCode)
            {
                return null;
            }

        }

        public class RestFS:FileSystemCoreFunctions
        {
            string[] services={"WebSearch","ImagePass"};
            string[] WebSearch = {"google.srv",};
            List<string> imageFiles = new List<string>();
            Dictionary<string, byte[]> cache = new Dictionary<string, byte[]>();
            string filepath = @"C:\Users\Dr. Devil\Pictures";
            public uint Def_CreateFile(string filename, System.IO.FileAccess access, System.IO.FileShare share, System.IO.FileMode mode, System.IO.FileOptions options, IntPtr info)
            {
                if (new VNode(filename).isValid)
                    return 0;
                else
                    return 0xC000000F;
            }
            public RestFS()
            {
                foreach(string file in System.IO.Directory.GetFiles(filepath)){
                    if (!System.IO.Path.GetFileName(file).EndsWith("inf") && !System.IO.Path.GetFileName(file).EndsWith("ini"))
                    imageFiles.Add(System.IO.Path.GetFileName(file));
                }
            }
            public uint Def_OpenDirectory(string filename, IntPtr info)
            {
                if (new VNode(filename).isValid)
                    return 0;
                else
                    return 0xC000000F;
            }

            public uint Def_CreateDirectory(string filename, IntPtr info)
            {
             
                    return 0xC00000A2;
            }

            public uint Def_Cleanup(string filename, IntPtr info)
            {
                return 0;
            }

            public uint Def_CloseFile(string filename, IntPtr info)
            {
                return 0;
            }

            public uint Def_ReadFile(string filename, IntPtr buffer, uint BufferSize, ref uint NumberByteReadSuccess, long Offset, IntPtr info)
            {
                StringBuilder data = new StringBuilder();
                VNode Node = new VNode(filename);
                string query="";
                byte[] file=null;
                
                Console.WriteLine("reading {0} {1} {2}", Node.isValid, Node.fileName,Node.curDir);
                    if (Node.isValid && Node.param.TryGetValue("q", out query))
                    {
                        #region This Is google Service
                        if (filename.StartsWith("google", StringComparison.CurrentCultureIgnoreCase))
                        {
                            Task<Google.Apis.Customsearch.v1.Data.Search> ret;
                            if (Node.param.ContainsKey("n"))
                            {
                                int n = 1;
                                string temp = "";
                                Node.param.TryGetValue("n", out temp);
                                int.TryParse(temp, out n);
                                ret = ServiceProvider.SearchGoogle(query, n);
                            }
                            else
                            {
                                ret = ServiceProvider.SearchGoogle(query, 1);
                            }
                            ret.Wait();
                            if (ret.Result.Items != null)
                                foreach (Google.Apis.Customsearch.v1.Data.Result s in ret.Result.Items)
                                {
                                    data.AppendLine(s.Title);
                                    data.AppendLine("----------------------------------------------------------------");
                                    data.AppendLine(s.Snippet);
                                    data.AppendLine(s.Link);
                                    data.Append("\n");
                                }
                            Console.WriteLine(data.ToString());
                            file = System.Text.Encoding.ASCII.GetBytes(data.ToString());


                        }
                        #endregion

                    }
                    else if (Node.isValid && Node.param.TryGetValue("op", out query))
                    {
                        #region ImagePass




                        #endregion


                    }
                    else if (Node.curDir == "ImagePass")
                    {
                        if (imageFiles.Contains(Node.fileName + "." + Node.fileExtention))
                        {
                            Console.WriteLine("Direct Read");
                            KalikoImage image = new KalikoImage(filepath + @"\" + Node.fileName + "." + Node.fileExtention);
                            file = image.ByteArray;
                        }
                    }
                

                if (file != null && file.Length != 0 && Offset < file.Length)
                {
                    if (BufferSize > file.Length - Offset)
                    {
                        NumberByteReadSuccess = (uint)(file.Length - Offset);
                        System.Runtime.InteropServices.Marshal.Copy(file, (int)Offset, buffer, (int)NumberByteReadSuccess);
                    }
                    else
                    {
                        NumberByteReadSuccess = BufferSize;
                        System.Runtime.InteropServices.Marshal.Copy(file, (int)Offset, buffer, (int)BufferSize);
                    }
                    return 0;
                }
                else
                {
                    Console.WriteLine("Error param {0}", Node.fileName);
                    return 0xC000000F;
                }
                
                
            }

            public uint Def_WriteFile(string filename, byte[] buffer, ref uint writtenBytes, long offset, IntPtr info)
            {
                return 0;
            }

            public uint Def_FlushFileBuffers(string filename, IntPtr info)
            {
                throw new NotImplementedException();
            }

            public uint Def_GetFileInformation(string filename, ref BY_HANDLE_FILE_INFORMATION Information, IntPtr info)
            {
                VNode Node=new VNode(filename);
                Console.WriteLine("{0} {1} {2}", Node.isFile, Node.rootDir, Node.curDir);
                if (Node.isValid) {
                   
                    Information.CreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                    Information.LastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                    Information.LastWriteTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                    Information.FileSizeLow = 100000;
                    if (Node.isFile)
                        Information.FileAttributes = FileAttributes.Readonly;
                    else
                        Information.FileAttributes = FileAttributes.Directory | FileAttributes.Readonly;
                   
                }
                return 0;
            }

            public uint Def_FindFiles(string filename, ref FillFindData FillFunction, IntPtr info)
            {
                VNode node=new VNode(filename);
                if(node.isValid){
                    Console.WriteLine("{0} {1} {2}",node.isFile,node.rootDir,node.curDir);
                    if(node.curDir==node.rootDir){
                        foreach (string s in services)
                        {
                            WIN32_FIND_DATA Information = new WIN32_FIND_DATA();
                            Information.cFileName = s;
                            Information.ftCreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                            Information.ftLastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                            Information.ftLastWriteTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                            Information.dwFileAttributes = FileAttributes.Directory | FileAttributes.Readonly;
                            Information.nFileSizeLow = 100000;
                            FillFunction(ref Information, info);
                        }
                    }else{
                        switch (node.curDir)
                        {
                            case "WebSearch":
                                foreach (string s in WebSearch)
                            {     
                                WIN32_FIND_DATA Information = new WIN32_FIND_DATA();
                                Information.cFileName = s;
                                Information.ftCreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                                Information.ftLastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                                Information.ftLastWriteTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                                Information.dwFileAttributes = FileAttributes.Readonly;
                                Information.nFileSizeLow = 100000;
                                FillFunction(ref Information, info);
                            }
                                break;
                            case "ImagePass":
                                foreach (string s in imageFiles)
                                {
                                    WIN32_FIND_DATA Information = new WIN32_FIND_DATA();
                                    Information.cFileName = s;
                                    Information.ftCreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                                    Information.ftLastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                                    Information.ftLastWriteTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                                    Information.dwFileAttributes =  FileAttributes.Readonly;
                                    Information.nFileSizeLow = 100000;
                                    FillFunction(ref Information, info);
                                }
                                break;
                            default:
                                break;
                        }

                    }
                }
                return 0;
            }

            public uint Def_SetFileAttributes(string filename, uint Attribute, IntPtr info)
            {
                return 0xC00000A2;
                
            }

            public uint Def_SetFileTime(string filename, System.Runtime.InteropServices.ComTypes.FILETIME ctime, System.Runtime.InteropServices.ComTypes.FILETIME atime, System.Runtime.InteropServices.ComTypes.FILETIME mtime, IntPtr info)
            {
                return 0xC00000A2;
            }

            public uint Def_DeleteFile(string filename, IntPtr info)
            {
                return 0xC00000A2;
            }

            public uint Def_DeleteDirectory(string filename, IntPtr info)
            {
                return 0xC00000A2;
            }

            public uint Def_MoveFile(string filename, string newname, bool replace, IntPtr info)
            {
                return 0xC00000A2;
            }

            public uint Def_SetEndOfFile(string filename, long length, IntPtr info)
            {
                return 0xC00000A2;
            }

            public uint Def_SetAllocationSize(string filename, long length, IntPtr info)
            {
                return 0xC00000A2;
            }

            public uint Def_LockFile(string filename, long offset, long length, IntPtr info)
            {
                return 0;
            }

            public uint Def_UnlockFile(string filename, long offset, long length, IntPtr info)
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

            public uint Def_GetVolumeInfo(IntPtr VolumeNameBuffer, uint VolumeNameSize, ref uint SerialNumber, ref uint MaxComponenetLegnth, ref uint FileSystemFeatures, IntPtr FileSystemNameBuffer, uint FileSystemNameSize)
            {
                HelperFunction.SetVolumeInfo(VolumeNameBuffer, "REST", (int)VolumeNameSize, FileSystemNameBuffer, "REST", (int)FileSystemNameSize);
                FileSystemFeatures = (uint)(FILE_FS_ATTRIBUTE_INFORMATION.FILE_UNICODE_ON_DISK | FILE_FS_ATTRIBUTE_INFORMATION.FILE_UNICODE_ON_DISK);
                MaxComponenetLegnth = 409600;
                SerialNumber = (uint)GetHashCode();
                return 0;
            }

            public uint Def_Unmount(IntPtr info)
            {
                return 0;
            }
        }


    


       
    }
}