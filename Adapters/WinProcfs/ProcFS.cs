using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DokanXBase;
using DokanXNative;
using Utility;
using Native.Objects;
using ST=System.Threading;
using System.Management;
using System.Runtime.InteropServices;
using SD=System.Diagnostics;
using System.Globalization;
using NLog;


namespace WinProcfs
{
    class Program
    {
        static DokanNative native;
        
        static void Main(string[] args)
        {
           WinProcFS fs; fs = new WinProcFS();
            handler=new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);
            native = new DokanNative(fs,0,"S:","Proc","Proc");
            native.StartDokan();
        }
        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                native.StopDokan();
            }
            return false;
        }
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
        // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
    }

    class WinProcFS : FileSystemCoreFunctions
    {
        string[] baseinfos = { "Process.inf", "Network.inf", "Module.inf", "MemRegion.inf", "IOCounters.inf", "Window.inf", "Thread.inf", "Handle.inf", "Heap.inf", "Token.inf", "EnvVariable.inf" };
        string[] remoteBase = { "Module.inf", "Process.inf", "Thread.inf", "IOCounters.inf" };
        string[] remoteCommon = { "Services.inf" ,"System.inf"};
        string[] commonSystem={ "Services.inf", "SystemInfo.inf", "Jobs.inf", "EnvVariable.inf","NetStat.inf"};
        ST.Thread processGrabberThread;
        ST.Thread serviceThread;
        ST.Thread wmiThread;
    
        ManagementObjectSearcher processSearcher;
        Dictionary<string, processInfos> database;
       object critical;
       object criticalS;
       Logger log;
       public WinProcFS(){
           database=new Dictionary<string,processInfos>();
           critical=new object();
           criticalS = new object();
           log = LogManager.GetCurrentClassLogger();
           processGrabberThread = new ST.Thread(new ST.ThreadStart(this.UpdateProcessData)) ;
           

           serviceThread = new ST.Thread(new ST.ThreadStart(this.UpdateServiceData));
           processGrabberThread.Name = "DataProcessor";
           serviceThread.Start();
           processSearcher = new ManagementObjectSearcher("Select * from Win32_Process");
           processGrabberThread.Start();
       }
       Dictionary<string, byte[]> AuthByte = new Dictionary<string, byte[]>();


       public void UpdateServiceData()
       {
           while (true)
           {
               lock (critical)
               {
                   string err = "";
                   serviceCatch = FillServiceDetail();
                   if (err != "")
                   {
                       if(Self.isDebug())
                       log.Error("Grabber Error :" , err);
                   }
               }
               ST.Thread.Sleep(10000);
           }
       }

       public void UpdateProcessData()
       {
           while(true){
           lock(critical)
           {
               string err="";
               database = Native.Objects.Process.EnumerateHiddenProcessesHandleMethod();
               if (err != "")
               {
                   if (Self.isDebug())
                   log.Error("Grabber Error :",err);
               }
           }
           ST.Thread.Sleep(3000);
           }
       }

       string NullHandler(object str)
       {
           if(str==null)
           return "";

           return str.ToString();

       }

       #region  All Local Stuff
       private static bool IsWin64(int pid)
        {
               try{
            System.Diagnostics.Process process=  System.Diagnostics.Process.GetProcessById(pid);
            if ((Environment.OSVersion.Version.Major > 5)
                || ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor >= 1)))
            {
                        try
                        {
                            bool retVal;

                            return NativeMethods.IsWow64Process(process.Handle, out retVal) && retVal;
                        }
                        catch
                        {
                            return false; // access is denied to the process
                        }
                    }

                        return false; // not on 64-bit Windows
             }
               catch (Exception e)
               {
                   return false;
               }

        

    }

       internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);
    }
    
       private bool ProcessExists(int id)
        {
            return SD.Process.GetProcesses().Any(x => x.Id == id);
        }

       byte[] FillJobDetail()
       {
           StringBuilder builder = new StringBuilder();
           try
           {
                     Dictionary<string, jobInfos> info = Job.EnumerateJobs(); ;
                 
                   foreach (jobInfos minf in info.Values)
                   {
                       builder.AppendLine("[Job " + minf.Name + "]");
                       builder.AppendLine("ActiveProcess:" + NullHandler(minf.BasicAndIoAccountingInformation.BasicInfo.ActiveProcesses));
                       builder.AppendLine("TotalKernelTime:" + NullHandler(minf.BasicAndIoAccountingInformation.BasicInfo.ThisPeriodTotalKernelTime));
                       builder.AppendLine("TotalUserTime:" + NullHandler(minf.BasicAndIoAccountingInformation.BasicInfo.ThisPeriodTotalUserTime));
                       builder.AppendLine("KernelTime:" + NullHandler(minf.BasicAndIoAccountingInformation.BasicInfo.TotalKernelTime));
                       builder.AppendLine("TotalPageFaultCount:" + NullHandler(minf.BasicAndIoAccountingInformation.BasicInfo.TotalPageFaultCount));
                       builder.AppendLine("TotalProcess:" + NullHandler(minf.BasicAndIoAccountingInformation.BasicInfo.TotalProcesses));
                       builder.AppendLine("TotalTeminatedProcess:" + NullHandler(minf.BasicAndIoAccountingInformation.BasicInfo.TotalTerminatedProcesses));
                       builder.AppendLine("TotalUserTime:" + NullHandler(minf.BasicAndIoAccountingInformation.BasicInfo.TotalUserTime));
                       builder.AppendLine("ActiveProcessLimit:" + NullHandler(minf.BasicLimitInformation.ActiveProcessLimit));
                       builder.AppendLine("Affinity:" + NullHandler(minf.BasicLimitInformation.Affinity));
                       builder.AppendLine("LimitFlags:" + NullHandler(minf.BasicLimitInformation.LimitFlags));
                       builder.AppendLine("MaximumWorkingSetSize:" + NullHandler(minf.BasicLimitInformation.MaximumWorkingSetSize));
                       builder.AppendLine("MinimumWorkingSetSize:" + NullHandler(minf.BasicLimitInformation.MinimumWorkingSetSize));
                       builder.AppendLine("PerJobUserTimeLimit:" + NullHandler(minf.BasicLimitInformation.PerJobUserTimeLimit));
                       builder.AppendLine("PerProcessTimeLimit:" + NullHandler(minf.BasicLimitInformation.PerProcessUserTimeLimit));
                       builder.AppendLine("PriorityClass:" + NullHandler(minf.BasicLimitInformation.PriorityClass));
                       builder.AppendLine("SchedulingClass:" + NullHandler(minf.BasicLimitInformation.SchedulingClass));
                       builder.AppendLine("Name:" + NullHandler(minf.Name));
                       builder.AppendLine("PidList:" + NullHandler(minf.PidList.Select(i => i.ToString(CultureInfo.InvariantCulture)).Aggregate((s1, s2) => s1 + ", " + s2)));                       
                        builder.AppendLine("");
                   }

                  
               



           }
           catch (Exception e)
           {
               builder.AppendLine(e.Message);
               if (System.Diagnostics.Debugger.IsAttached)
               log.Fatal("Exception: {0}" , e.Message);

           }
           return System.Text.Encoding.UTF8.GetBytes(builder.ToString());

       } 

       byte[] FillSysDetail()
       {
           StringBuilder builder = new StringBuilder();

           try
           {


               Native.Api.NativeStructs.SystemInfo info = SystemInfo.GetSystemInfo();
               
                       builder.AppendLine("[System:" + Environment.MachineName + "]");

                       builder.AppendLine("ActiveProcessorMask:" + NullHandler(info.dwActiveProcessorMask));
                       builder.AppendLine("AllocationGranularity:" + NullHandler(info.dwAllocationGranularity));
                       builder.AppendLine("PageSize:" + NullHandler(info.dwPageSize));
                       builder.AppendLine("NumberOfProcessors:" + NullHandler(info.dwNumberOfProcessors));
                       builder.AppendLine("ProcessorLevel:" + NullHandler(info.dwProcessorLevel));
                       builder.AppendLine("ProcessorRevision:" + NullHandler(info.dwProcessorRevision));
                       builder.AppendLine("ProcessorType:" + NullHandler(info.dwProcessorType));
                       builder.AppendLine("MaximumApplicationAddress:" + NullHandler(info.lpMaximumApplicationAddress));
                       builder.AppendLine("MinimumApplicationAddress" + NullHandler(info.lpMinimumApplicationAddress));
                      
           }
           catch (Exception e)
           {
               builder.AppendLine(e.Message);
               if (System.Diagnostics.Debugger.IsAttached)
               log.Fatal("Exception: {0}", e.Message);

           }
           return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
       }
            
       byte[] FillServiceDetail()
       {
           StringBuilder builder = new StringBuilder();

           try
           {
               
               processInfos pfs = new processInfos();
           

                   Dictionary<string, serviceInfos> srv = new Dictionary<string, serviceInfos>();
                   Service.EnumerateServices(Service.GetSCManagerHandle(Native.Security.ServiceManagerAccess.EnumerateService), ref srv, true, true);
                   foreach (serviceInfos minf in srv.Values)
                   {
                       builder.AppendLine("[Service:" + minf.DisplayName + "]");
                       builder.AppendLine("AcceptedControl:" + NullHandler(minf.AcceptedControl));
                       builder.AppendLine("CheckPoint:" + NullHandler(minf.CheckPoint));
                       builder.AppendLine("Dependencies:" + NullHandler(minf.Dependencies));
                       builder.AppendLine("Desription:" + NullHandler(minf.Description));
                       builder.AppendLine("DigonosticMessageFile:" + NullHandler(minf.DiagnosticMessageFile));
                       builder.AppendLine("ErrorControl:" + NullHandler(minf.ErrorControl));                       
                       builder.AppendLine("ImagePath:" + NullHandler(minf.ImagePath));
                       builder.AppendLine("LoadOrderGroup:" + NullHandler(minf.LoadOrderGroup));
                       builder.AppendLine("Name:" + NullHandler(minf.Name));
                       builder.AppendLine("ObjectName:" + NullHandler(minf.ObjectName));
                       builder.AppendLine("ProcessId:" + NullHandler(minf.ProcessId));
                       builder.AppendLine("ProcessName:" + NullHandler(minf.ProcessName));
                       builder.AppendLine("ServiceFlag:" + NullHandler(minf.ServiceFlags));
                       builder.AppendLine("ServiceSpecificExitCode:" + NullHandler(minf.ServiceSpecificExitCode));
                       builder.AppendLine("ServiceStartName:" + NullHandler(minf.ServiceStartName));
                       builder.AppendLine("ServiceType:" + NullHandler(minf.ServiceType));
                       builder.AppendLine("StartType:" + NullHandler(minf.StartType));
                       builder.AppendLine("State:" + NullHandler(minf.State));
                       builder.AppendLine("Tag:" + NullHandler(minf.Tag));
                       builder.AppendLine("TagID:" + NullHandler(minf.TagID));
                       builder.AppendLine("WaitHint:" + NullHandler(minf.WaitHint));
                       builder.AppendLine("Win32ExitCode:" + NullHandler(minf.Win32ExitCode));
                       builder.AppendLine(NullHandler(minf.FileInfo));
                       builder.AppendLine("");

                       
                   

                  
               }



           }
           catch (Exception e)
           {
               builder.AppendLine(e.Message);
               if (System.Diagnostics.Debugger.IsAttached)
               log.Fatal("Exception: {0}", e.Message);

           }
           return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
       }

       byte[] FillMemRegionDetail(int Pid)
       {
           StringBuilder builder = new StringBuilder();

           try
           {
               log.Info("MemRegion Detail Filling");

               processInfos pfs = new processInfos();
               if (database.TryGetValue(Pid.ToString(), out pfs))
               {

                   
                   Dictionary<string,memRegionInfos> info=new Dictionary<string,memRegionInfos>();
                   MemRegion.EnumerateMemoryRegionsByProcessId(Pid, ref info);
                   foreach (memRegionInfos minf in info.Values)
                   {
                       builder.AppendLine("[MemoryRegion "+minf.Name+"]");
                       builder.AppendLine("BaseAddress:" + NullHandler(minf.BaseAddress));
                       builder.AppendLine("Protection:" + NullHandler(minf.Protection));
                       builder.AppendLine("RegionSize:"+NullHandler(minf.RegionSize));
                       builder.AppendLine("State:" + NullHandler(minf.State));
                       builder.AppendLine("Type:" + NullHandler(minf.Type));
                       builder.AppendLine("");
                   }
           
                  
               }



           }
           catch (Exception e)
           {
               builder.AppendLine(e.Message);
               if (System.Diagnostics.Debugger.IsAttached)
                   log.Fatal("Exception: {0}", e.Message);

           }
           return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
       }

       byte[] FillIODetail(int Pid)
       {
           StringBuilder builder = new StringBuilder();

           try
           {

               log.Info("IO Detail Filling");
               processInfos pfs = new processInfos();
               
               if (database.TryGetValue(Pid.ToString(), out pfs))
               {

                   
                  
                       builder.AppendLine("[IOCounter]");
                       builder.AppendLine("WriteOperationCount:" + NullHandler(pfs.IOValues.WriteOperationCount));
                       builder.AppendLine("WriteTransferCount:" + NullHandler(pfs.IOValues.WriteTransferCount));
                       builder.AppendLine("ReadOperationCount:" + NullHandler(pfs.IOValues.ReadOperationCount));
                       builder.AppendLine("ReadTransferCount:" + NullHandler(pfs.IOValues.ReadTransferCount));
                       builder.AppendLine("OtherOperationCount:" + NullHandler(pfs.IOValues.OtherOperationCount));
                       builder.AppendLine("OtherTransferCount:" + NullHandler(pfs.IOValues.OtherTransferCount));
                       builder.AppendLine("");
               

                  
               }



           }
           catch (Exception e)
           {
               builder.AppendLine(e.Message);
               if (System.Diagnostics.Debugger.IsAttached)
                   log.Fatal("Exception: {0}", e.Message);

           }
           return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
       }

       byte[] FillWindowDetail(int Pid)
       {
           StringBuilder builder = new StringBuilder();
           log.Info("Window Detail Filling");
           try
           {


               processInfos pfs = new processInfos();
               Dictionary<string, windowInfos> win = new Dictionary<string, windowInfos>();
               Window.EnumerateWindowsByProcessId(Pid,false,true,ref win,true);

               foreach(windowInfos info in win.Values)
               {
                   builder.AppendLine("[Windows ]");
                   builder.AppendLine("Enabled:" + NullHandler(info.Enabled));
                   builder.AppendLine("Handle:" + NullHandler(info.Handle));
                   builder.AppendLine("Height:" + NullHandler(info.Height));
                   builder.AppendLine("Width:" + NullHandler(info.Width));
                   builder.AppendLine("IsTask:" + NullHandler(info.IsTask));
                   builder.AppendLine("Top:" + NullHandler(info.Top));
                   builder.AppendLine("Left:" + NullHandler(info.Left));
                   builder.AppendLine("Opacity:" + NullHandler(info.Opacity));
                   builder.AppendLine("ThreadId:" + NullHandler(info.ThreadId));
                   builder.AppendLine("Visible:" + NullHandler(info.Visible));
                   builder.AppendLine("");
                   
               }
              


           }
           catch (Exception e)
           {
               builder.AppendLine(e.Message);
               if (System.Diagnostics.Debugger.IsAttached)
                   log.Fatal("Exception: {0}", e.Message);

           }
           return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
       }
       byte[] FillThreadDetail(int Pid)
       {
           StringBuilder builder = new StringBuilder();

           try
           {

               log.Info("Thread Detail Filling");
               processInfos pfs = new processInfos();
               Dictionary<string, threadInfos> thr = new Dictionary<string, threadInfos>();
               Thread.EnumerateThreadsByProcessId(ref thr, Pid);

               foreach (threadInfos info in thr.Values)
               {
                   builder.AppendLine("[Thread "+info.Id+" ]");
                   builder.AppendLine("KernelTime:" + NullHandler(info.KernelTime));
                   builder.AppendLine("Priority:" + NullHandler(info.Priority));
                   builder.AppendLine("ProcessID:" + NullHandler(info.ProcessId));
                   builder.AppendLine("StartAddress:" + NullHandler(info.StartAddress));
                   builder.AppendLine("State:" + NullHandler(info.State));
                   builder.AppendLine("TotalTime:" + NullHandler(info.TotalTime));
                   builder.AppendLine("UserTime:" + NullHandler(info.UserTime));
                   builder.AppendLine("WaitReason:" + NullHandler(info.WaitReason));
                   builder.AppendLine("WaitTime:" + NullHandler(info.WaitTime));                  
                   builder.AppendLine("");

               }
              


           }
           catch (Exception e)
           {
               builder.AppendLine(e.Message);
               if (System.Diagnostics.Debugger.IsAttached)
                   log.Fatal("Exception: {0}", e.Message);

           }
           return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
       }

       byte[] FillHandleDetail(int Pid)
       {
           StringBuilder builder = new StringBuilder();

           try
           {

               log.Info("Handle Detail Filling");
               processInfos pfs = new processInfos();
               Dictionary<string, handleInfos> hnd = new Dictionary<string, handleInfos>();
               Handle.EnumerateHandleByProcessId(Pid, true, ref hnd);

               foreach (handleInfos info in hnd.Values)
               {
                   builder.AppendLine("[Handle]");
                   builder.AppendLine("Attribute:" + NullHandler(info.Attributes));
                   builder.AppendLine("CreateTime:" + NullHandler(info.CreateTime));
                   builder.AppendLine("GrantedAccess:" + NullHandler(info.GrantedAccess));
                   builder.AppendLine("Handle:" + NullHandler(info.Handle));
                   builder.AppendLine("HandleCount:" + NullHandler(info.HandleCount));
                   builder.AppendLine("Key:" + NullHandler(info.Key));
                   builder.AppendLine("Name:" + NullHandler(info.Name));
                   builder.AppendLine("NonPagedPoolUsage:" + NullHandler(info.NonPagedPoolUsage));
                   builder.AppendLine("ObjectAddress:" + NullHandler(info.ObjectAddress));
                   builder.AppendLine("ObjectCount:" + NullHandler(info.ObjectCount));
                   builder.AppendLine("ObjectTypeNumber:" + NullHandler(info.ObjectTypeNumber));
                   builder.AppendLine("PagedPollUsage:" + NullHandler(info.PagedPoolUsage));
                   builder.AppendLine("PointerCount:" + NullHandler(info.PointerCount));
                   builder.AppendLine("Type:" + NullHandler(info.Type));
                   
                   builder.AppendLine("");

               }
              


           }
           catch (Exception e)
           {
               builder.AppendLine(e.Message);
               if (System.Diagnostics.Debugger.IsAttached)
                   log.Fatal("Exception: {0}", e.Message);

           }
           return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
       }

       byte[] FillHeapDetail(int Pid)
       {
           StringBuilder builder = new StringBuilder();
           if(System.Diagnostics.Debugger.IsAttached)
           log.Info("Heap Detail Filling");
           try
           {


               processInfos pfs = new processInfos();
               Dictionary<string, heapInfos> hnd = Heap.EnumerateHeapsByProcessId(Pid);

               foreach (heapInfos info in hnd.Values)
               {
                   builder.AppendLine("[Heap]");
                   builder.AppendLine("BaseAddress:" + NullHandler(info.BaseAddress));
                   builder.AppendLine("BlockCount:" + NullHandler(info.BlockCount));
                   builder.AppendLine("Flags:" + NullHandler(info.Flags));
                   builder.AppendLine("Granulraity:" + NullHandler(info.Granularity));
                   builder.AppendLine("MemAllocated:" + NullHandler(info.MemAllocated));
                   builder.AppendLine("MemCommited:" + NullHandler(info.MemCommitted));
                   builder.AppendLine("tagCount:" + NullHandler(info.TagCount));
                   builder.AppendLine("Tags:" + NullHandler(info.Tags));
                   
                   builder.AppendLine("");

               }
              


           }
           catch (Exception e)
           {
               builder.AppendLine(e.Message);
               if (System.Diagnostics.Debugger.IsAttached)
                   log.Fatal("Exception: {0}", e.Message);

           }
           return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
       }

       byte[] FillTokenDetail(int Pid)
       {
           StringBuilder builder = new StringBuilder();
           if (System.Diagnostics.Debugger.IsAttached)
           log.Info("Token Detail Filling");
           try
           {


               processInfos pfs = new processInfos();
               Native.Api.NativeStructs.PrivilegeInfo[] tkn = Token.GetPrivilegesListByProcessId(Pid);

               foreach (Native.Api.NativeStructs.PrivilegeInfo info in tkn)
               {
                   builder.AppendLine("[Privilages]");
                   builder.AppendLine("Name:" + NullHandler(info.Name));
                   builder.AppendLine("Privilage ID:" + NullHandler(info.pLuid));
                   builder.AppendLine("Status:" + NullHandler(info.Status));
                   
                   builder.AppendLine("");

               }
              


           }
           catch (Exception e)
           {
               builder.AppendLine(e.Message);
               if (System.Diagnostics.Debugger.IsAttached)
                   log.Fatal("Exception:{0}", e.Message);

           }
           return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
       } 
       
       byte[] FillEnvDetail(int Pid)
       {
           StringBuilder builder = new StringBuilder();
           if (System.Diagnostics.Debugger.IsAttached)
           log.Info("Env Detail Filling");
           try
           {


               processInfos pfs = new processInfos();
               builder.AppendLine("Pending");
              


           }
           catch (Exception e)
           {
               builder.AppendLine(e.Message);
               if (System.Diagnostics.Debugger.IsAttached)
                   log.Fatal("Exception: {0}", e.Message);

           }
           return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
       } 

       byte[] FillModuleDetail(int Pid)
       {
           if (System.Diagnostics.Debugger.IsAttached)
           log.Info("Module Detail Filling");
           StringBuilder builder = new StringBuilder();
           try
           {
              
               
               if(!ProcessExists(Pid) && IsWin64(Pid))
                   return System.Text.Encoding.UTF8.GetBytes("Process Expired......");
               System.Diagnostics.Process prs=System.Diagnostics.Process.GetProcessById(Pid);
               System.Diagnostics.ProcessModuleCollection module= prs.Modules;
              
                
               if ( module!=null)
               {
                   foreach(SD.ProcessModule mod in module){
                    builder.AppendLine("[ " +NullHandler(mod.FileName)+ " ]");
                    builder.AppendLine("BaseAddres:"+NullHandler(mod.BaseAddress));
                    builder.AppendLine("EntryPoint:" + NullHandler(mod.EntryPointAddress));
                    builder.AppendLine("ModuleMemorySize:" + NullHandler(mod.ModuleMemorySize));                    
                    builder.AppendLine("FileVersionInfo:" + NullHandler(mod.FileVersionInfo));
                    builder.AppendLine("[FileInfo]");
                    builder.AppendLine(NullHandler(mod.FileName));
                    builder.AppendLine("");
                      
                   }
                  
               }

           

           }
           catch (Exception e)
           {
               builder.AppendLine(e.Message);
               if (System.Diagnostics.Debugger.IsAttached)
                   log.Fatal("Exception: {0}", e.Message);
           }
           return System.Text.Encoding.UTF8.GetBytes(builder.ToString());

       }
       
        byte[] FillProcessDetail(int Pid)
       {
           StringBuilder builder=new StringBuilder();
           
          try
          {
              if (System.Diagnostics.Debugger.IsAttached)
                  if (System.Diagnostics.Debugger.IsAttached)
              log.Info("Procees Detail Filling");
              processInfos pfs = new processInfos();
              if (database.TryGetValue(Pid.ToString(), out pfs))
              {
                  
                  builder.AppendLine("[Process Property]");
                  builder.AppendLine("AffinityMask:" + NullHandler(pfs.AffinityMask));
                  builder.AppendLine("Name:" + NullHandler(pfs.Name));
                  builder.AppendLine("Path:" + NullHandler(pfs.Path));
                  builder.AppendLine("Priority:" + NullHandler(pfs.Priority));
                  builder.AppendLine("ProcessId:" + NullHandler(pfs.ProcessId));
                  builder.AppendLine("ParentProcessId:" + NullHandler(pfs.ParentProcessId));
                  builder.AppendLine("UserName:" + NullHandler(pfs.UserName));
                  builder.AppendLine("CommandLineArguments:" + NullHandler(pfs.CommandLine));
                  builder.AppendLine("DomainName" + NullHandler(pfs.DomainName));

                  builder.AppendLine("");
                  builder.AppendLine("[Perfomance and Resouces]");
                  builder.AppendLine("AverageCpuUsage:" + NullHandler(pfs.AverageCpuUsage * 100));
                  builder.AppendLine("ProcessorTime:" + NullHandler(pfs.ProcessorTime));
                  builder.AppendLine("UserModeTime" + NullHandler(pfs.UserTime));
                  builder.AppendLine("KernelTime:" + NullHandler(pfs.KernelTime));
                  builder.AppendLine("StartTime:" + NullHandler(pfs.StartTime));

                  builder.AppendLine("");
                  builder.AppendLine("\n[System Resources Count]");
                  builder.AppendLine("ThreadCount:" + NullHandler(pfs.ThreadCount));
                  builder.AppendLine("UserObjectsCount:" + NullHandler(pfs.UserObjects));
                  builder.AppendLine("GdiObjects:" + NullHandler(pfs.GdiObjects));
                  builder.AppendLine("HandleCounts:" + NullHandler(pfs.HandleCount));
                  
                  builder.AppendLine("");
                  builder.AppendLine("[ProcessMisc]");
                  builder.AppendLine("HasReanalize:" + NullHandler(pfs.HasReanalize));
                  builder.AppendLine("IsHidden:" + NullHandler(pfs.IsHidden));
                 
              }
             

              
          }
          catch(Exception e)
          {
              builder.AppendLine(e.Message);
              if (System.Diagnostics.Debugger.IsAttached)
                  log.Fatal("Exception: {0}", e.Message); 
             
          }
          return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
       }

       byte[] FillMemoryDetail(int Pid)
       {
           StringBuilder builder = new StringBuilder();

           try
           {
               if (System.Diagnostics.Debugger.IsAttached)
               log.Info("Memory Detail Filling");
               processInfos pfs = new processInfos();
               if (database.TryGetValue(Pid.ToString(), out pfs))
               {

                   builder.AppendLine("[Memory Count]");
                   builder.AppendLine("PageFaultCount:" + NullHandler(pfs.MemoryInfos.PageFaultCount));
                   builder.AppendLine("PagefileUsage:" + NullHandler(pfs.MemoryInfos.PagefileUsage));
                   builder.AppendLine("PeakPagefileUsage:" + NullHandler(pfs.MemoryInfos.PeakPagefileUsage));
                   builder.AppendLine("PeakVirtualSize:" + NullHandler(pfs.MemoryInfos.PeakVirtualSize));
                   builder.AppendLine("PeakWorkingSetSize:" + NullHandler(pfs.MemoryInfos.PeakWorkingSetSize));
                   builder.AppendLine("PrivateBytes:" + NullHandler(pfs.MemoryInfos.PrivateBytes));
                   builder.AppendLine("VirtualSize:" + NullHandler(pfs.MemoryInfos.VirtualSize));
                   builder.AppendLine("WorkingSetSize:" + NullHandler(pfs.MemoryInfos.WorkingSetSize));
                   

                   builder.AppendLine("");
                   builder.AppendLine("[Quota Details]");
                   builder.AppendLine("QuotaPeakPagedPoolUsage:" + NullHandler(pfs.MemoryInfos.QuotaPeakPagedPoolUsage));
                   builder.AppendLine("QuotaNonPagedPoolUsage:" + NullHandler(pfs.MemoryInfos.QuotaNonPagedPoolUsage));
                   builder.AppendLine("QuotaPagedPoolUsage:" + NullHandler(pfs.MemoryInfos.QuotaPagedPoolUsage));
                   builder.AppendLine("QuotaPeakNonPagedPoolUsage" + NullHandler(pfs.MemoryInfos.QuotaPeakNonPagedPoolUsage));
                   
      
                  
               }

  

           }
           catch (Exception e)
           {
               builder.AppendLine(e.Message);
               if (System.Diagnostics.Debugger.IsAttached)
                   log.Fatal("Exception: {0}", e.Message);
           }
           return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
       }

       byte[] FillNetworkStatDetail()
       {
           StringBuilder builder = new StringBuilder();
           if (System.Diagnostics.Debugger.IsAttached)
           log.Info("NetworkStat Detail Filling");
           try
           {               
               Native.Api.NativeStructs.MibTcpStats tcpState= Native.Objects.Network.GetTcpStatistics();
               Native.Api.NativeStructs.MibUdpStats udp = Native.Objects.Network.GetUdpStatistics();

              
               builder.AppendLine("[TCPStats]");
               builder.AppendLine("ActiveOpens:" + NullHandler(tcpState.ActiveOpens));
               builder.AppendLine("AttemptFails:" + NullHandler(tcpState.AttemptFails));
               builder.AppendLine("CurrEstab:" + NullHandler(tcpState.CurrEstab));
               builder.AppendLine("EstabResets:" + NullHandler(tcpState.EstabResets));
               builder.AppendLine("InErrs:" + NullHandler(tcpState.InErrs));
               builder.AppendLine("InSegs:" + NullHandler(tcpState.InSegs));
               builder.AppendLine("MaxConn:" + NullHandler(tcpState.MaxConn));
               builder.AppendLine("NumConns:" + NullHandler(tcpState.NumConns));
               builder.AppendLine("OutRsts:" + NullHandler(tcpState.OutRsts));
               builder.AppendLine("PassiveOpens:" + NullHandler(tcpState.PassiveOpens));
               builder.AppendLine("RetransSegs:" + NullHandler(tcpState.RetransSegs));
               builder.AppendLine("RtoAlgorithm:" + NullHandler(tcpState.RtoAlgorithm));
               builder.AppendLine("RtoMax:" + NullHandler(tcpState.RtoMax));
               builder.AppendLine("RtoMin:" + NullHandler(tcpState.RtoMin));
               
               builder.AppendLine("");

               builder.AppendLine("[UDPStats]");
               builder.AppendLine("InDatagrams:" + NullHandler(udp.InDatagrams));
               builder.AppendLine("InErrors:" + NullHandler(udp.InErrors));
               builder.AppendLine("NoPorts:" + NullHandler(udp.NoPorts));
               builder.AppendLine("NumAddrs:" + NullHandler(udp.NumAddrs));
               builder.AppendLine("OutDatagrams:" + NullHandler(udp.OutDatagrams));              

              

           }
           catch (Exception e)
           {
               builder.AppendLine(e.Message);
               if (System.Diagnostics.Debugger.IsAttached)
                   log.Fatal("Exception: {0}", e.Message);
           }
           return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
       } 

       byte[] FillNetworkkDetail(int Pid)
       {
           StringBuilder builder = new StringBuilder();
           if (System.Diagnostics.Debugger.IsAttached)
           log.Info("Network Detail Filling");
           try
           {
              Dictionary<string,networkInfos> SocketInfo=new Dictionary<string,networkInfos>();
              Native.Objects.Network.EnumerateTcpUdpConnections(ref SocketInfo,true,Pid);
               processInfos pfs = new processInfos();              
               if(SocketInfo.Values.Count>=1)
               {
                   builder.AppendLine("[Network Connection]");
                   foreach (networkInfos info in SocketInfo.Values)
                   {

                       if (info.ProcessId == Pid)
                       {
                           builder.AppendLine("Protocol:" + NullHandler(info.Protocol));
                           builder.AppendLine("State:" + NullHandler(info.State));
                           builder.AppendLine("LocalAddress:" + NullHandler(info.Local));
                           
                           builder.AppendLine("RemoteAdress:" + NullHandler(info.Remote));
                                                      
                           builder.AppendLine("");
                       }
                           
                       
                   }

                  
               }

              

           }
           catch (Exception e)
           {
               builder.AppendLine(e.Message);
               if (System.Diagnostics.Debugger.IsAttached)
                   log.Fatal("Exception:{0}", e.Message);
           }
           return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
       }
       #endregion

      #region All Remote WMI
      public List<string> WmiProcessCollection(VirtualNode node ,out string s) {
          List<string> strList = new List<string>();
          Dictionary<String, processInfos> details = new Dictionary<string, processInfos>();
          ConnectionOptions con = new ConnectionOptions();
          byte[] authentication=new byte[12];
           string[] str;
          if( AuthByte.TryGetValue(node.RootNode,out authentication)){
              if (System.Diagnostics.Debugger.IsAttached)
              log.Info("Authentication -"+System.Text.Encoding.ASCII.GetString(authentication));
              str=System.Text.Encoding.ASCII.GetString(authentication).Split(':');
          }else{
              s = "Access Denied";
              return strList;
          }
          
          
          if (str!=null && str.Length != 2) {
              s = "Access Denied";
              return strList;
          }
          con.Username =node.RootNode+@"\"+str[0];
          con.Password = str[1];
          con.Authentication = AuthenticationLevel.Packet;
          con.Impersonation = ImpersonationLevel.Impersonate;
          ManagementScope scope = new ManagementScope("\\\\"+node.RootNode+"\\root\\cimv2", con);
          ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Process");
          s = "";
          Wmi.Objects.Process.EnumerateProcesses(new ManagementObjectSearcher(scope, query), ref details, ref s);
         
          foreach (processInfos info in details.Values)
          {
              
              strList.Add(info.ProcessId.ToString());
          }
          if (System.Diagnostics.Debugger.IsAttached)
          log.Info("Total Process Found-"+details.Count+" IF Error"+s);
          return strList;
          
      
      }
      public byte[] WmiProcessFill(VirtualNode node ,out string s){
             StringBuilder builder=new StringBuilder();
            s="";           
         try
         {
          Dictionary<String, processInfos> details = new Dictionary<string, processInfos>();
          ConnectionOptions con = new ConnectionOptions();
          byte[] authentication=new byte[12];
          string[] str={"",""};
          if( AuthByte.TryGetValue(node.RootNode,out authentication)){
              if (System.Diagnostics.Debugger.IsAttached)
              log.Info("Authentication -"+System.Text.Encoding.ASCII.GetString(authentication));
              str=System.Text.Encoding.ASCII.GetString(authentication).Split(':');
          }else{
              s = "Access Denied";
              
          }
          
          
          if (str!=null && str.Length != 2) {
              s = "Access Denied";
              
          }
          con.Username =node.RootNode+@"\"+str[0];
          con.Password = str[1];
          con.Authentication = AuthenticationLevel.Packet;
          con.Impersonation = ImpersonationLevel.Impersonate;
          ManagementScope scope = new ManagementScope("\\\\"+node.RootNode+"\\root\\cimv2", con);
          ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Process");
          s = "";
          Wmi.Objects.Process.EnumerateProcesses(new ManagementObjectSearcher(scope, query), ref details, ref s);
       
          processInfos pfs=new processInfos();
          if(details.TryGetValue(node.CurrentNodeDir,out pfs)){              
                  builder.AppendLine("[Process Property]");
                  builder.AppendLine("AffinityMask:" + NullHandler(pfs.AffinityMask));
                  builder.AppendLine("Name:" + NullHandler(pfs.Name));
                  builder.AppendLine("Path:" + NullHandler(pfs.Path));
                  builder.AppendLine("Priority:" + NullHandler(pfs.Priority));
                  builder.AppendLine("ProcessId:" + NullHandler(pfs.ProcessId));
                  builder.AppendLine("ParentProcessId:" + NullHandler(pfs.ParentProcessId));
                  builder.AppendLine("UserName:" + NullHandler(pfs.UserName));
                  builder.AppendLine("CommandLineArguments:" + NullHandler(pfs.CommandLine));
                  builder.AppendLine("DomainName" + NullHandler(pfs.DomainName));

                  builder.AppendLine("");
                  builder.AppendLine("[Perfomance and Resouces]");
                  builder.AppendLine("AverageCpuUsage:" + NullHandler(pfs.AverageCpuUsage * 100));
                  builder.AppendLine("ProcessorTime:" + NullHandler(pfs.ProcessorTime));
                  builder.AppendLine("UserModeTime" + NullHandler(pfs.UserTime));
                  builder.AppendLine("KernelTime:" + NullHandler(pfs.KernelTime));
                  builder.AppendLine("StartTime:" + NullHandler(pfs.StartTime));

                  builder.AppendLine("");
                  builder.AppendLine("\n[System Resources Count]");
                  builder.AppendLine("ThreadCount:" + NullHandler(pfs.ThreadCount));
                  builder.AppendLine("UserObjectsCount:" + NullHandler(pfs.UserObjects));
                  builder.AppendLine("GdiObjects:" + NullHandler(pfs.GdiObjects));
                  builder.AppendLine("HandleCounts:" + NullHandler(pfs.HandleCount));
                  
                  builder.AppendLine("");
                  builder.AppendLine("[ProcessMisc]");
                  builder.AppendLine("HasReanalize:" + NullHandler(pfs.HasReanalize));
                  builder.AppendLine("IsHidden:" + NullHandler(pfs.IsHidden));
                
          }else{
              builder.AppendLine(s);
          }
          if (System.Diagnostics.Debugger.IsAttached)
          log.Info("Total Process Found-"+details.Count+" IF Error"+s);
      }
          catch (Exception e)
           {
               builder.AppendLine(e.Message);
               if (System.Diagnostics.Debugger.IsAttached)
               log.Fatal("Exception: {0}" , e.Message);

           }
          
           return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
      }

      public byte[] WmiModuleFill(VirtualNode node, out string s)
      {
          StringBuilder builder = new StringBuilder();
          s = "";
          try
          {
              Dictionary<String, moduleInfos> details = new Dictionary<string, moduleInfos>();
              ConnectionOptions con = new ConnectionOptions();
              byte[] authentication = new byte[12];
              string[] str = { };
              if (AuthByte.TryGetValue(node.RootNode, out authentication))
              {
                  if (System.Diagnostics.Debugger.IsAttached)
                      log.Info("Authentication -" + System.Text.Encoding.ASCII.GetString(authentication));
                  str = System.Text.Encoding.ASCII.GetString(authentication).Split(':');
              }
              else
              {
                  s = "Access Denied";

              }


              if (str != null && str.Length != 2)
              {
                  s = "Access Denied";

              }
              con.Username = node.RootNode + @"\" + str[0];
              con.Password = str[1];
              con.Authentication = AuthenticationLevel.Packet;
              con.Impersonation = ImpersonationLevel.Impersonate;
              ManagementScope scope = new ManagementScope("\\\\" + node.RootNode + "\\root\\cimv2", con);
              ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Process");
              s = "";
              Wmi.Objects.Module.EnumerateModuleById(int.Parse(node.CurrentNodeDir),new ManagementObjectSearcher(scope, query), ref details, ref s);
              
              processInfos pfs = new processInfos();
              if (details.Values.Count>0)
              {
                  foreach (moduleInfos mod in details.Values)
                  {
                      builder.AppendLine("[ " + NullHandler(mod.Name) + " ]");
                      builder.AppendLine("BaseAddres:" + NullHandler(mod.BaseAddress));
                      builder.AppendLine("EntryPoint:" + NullHandler(mod.EntryPoint));
                      builder.AppendLine("ModuleMemorySize:" + NullHandler(mod.Size));
                      builder.AppendLine("FileVersionInfo:" + NullHandler(mod.Version));
                      builder.AppendLine("LoadCount:" + NullHandler(mod.LoadCount));
                      builder.AppendLine("[FileInfo]");
                      builder.AppendLine(NullHandler(mod.FileInfo));
                      builder.AppendLine("");

                  }

              }
              else
              {
                  builder.AppendLine(s);
              }
              if (System.Diagnostics.Debugger.IsAttached)
                  log.Info("Total Module Found-" + details.Count + " IF Error" + s);
          }
          catch (Exception e)
          {
              builder.AppendLine(e.Message);
              if (System.Diagnostics.Debugger.IsAttached)
                  log.Fatal("Exception: {0}", e.Message);

          }

          return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
      }

      public byte[] WmiThreadFill(VirtualNode node, out string s)
      {
          StringBuilder builder = new StringBuilder();
          s = "";
          try
          {
              Dictionary<String, threadInfos> details = new Dictionary<string, threadInfos>();
              ConnectionOptions con = new ConnectionOptions();
              byte[] authentication = new byte[12];
              string[] str = { };
              if (AuthByte.TryGetValue(node.RootNode, out authentication))
              {
                  if (System.Diagnostics.Debugger.IsAttached)
                      log.Info("Authentication -" + System.Text.Encoding.ASCII.GetString(authentication));
                  str = System.Text.Encoding.ASCII.GetString(authentication).Split(':');
              }
              else
              {
                  s = "Access Denied";

              }


              if (str != null && str.Length != 2)
              {
                  s = "Access Denied";

              }
              con.Username = node.RootNode + @"\" + str[0];
              con.Password = str[1];
              con.Authentication = AuthenticationLevel.Packet;
              con.Impersonation = ImpersonationLevel.Impersonate;
              ManagementScope scope = new ManagementScope("\\\\" + node.RootNode + "\\root\\cimv2", con);
              ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Thread");
              s = "";
              Wmi.Objects.Thread.EnumerateThreadByIds(int.Parse(node.CurrentNodeDir), new ManagementObjectSearcher(scope, query), ref details, ref s);

              processInfos pfs = new processInfos();
              if (details.Values.Count > 0)
              {
                  foreach (threadInfos info in details.Values)
                  {
                      builder.AppendLine("[Thread " + info.Id + " ]");
                      builder.AppendLine("KernelTime:" + NullHandler(info.KernelTime));
                      builder.AppendLine("Priority:" + NullHandler(info.Priority));
                      builder.AppendLine("ProcessID:" + NullHandler(info.ProcessId));
                      builder.AppendLine("StartAddress:" + NullHandler(info.StartAddress));
                      builder.AppendLine("State:" + NullHandler(info.State));
                      builder.AppendLine("TotalTime:" + NullHandler(info.TotalTime));
                      builder.AppendLine("UserTime:" + NullHandler(info.UserTime));
                      builder.AppendLine("WaitReason:" + NullHandler(info.WaitReason));
                      builder.AppendLine("WaitTime:" + NullHandler(info.WaitTime));
                      builder.AppendLine("");
                  }

              }
              else
              {
                  builder.AppendLine(s);
              }
              if (System.Diagnostics.Debugger.IsAttached)
                  log.Info("Total Thread Found-" + details.Count + " IF Error" + s);
          }
          catch (Exception e)
          {
              builder.AppendLine(e.Message);
              if (System.Diagnostics.Debugger.IsAttached)
                  log.Fatal("Exception: {0}", e.Message);

          }

          return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
      }

      public byte[] WmiIOFill(VirtualNode node, out string s)
      {
          StringBuilder builder = new StringBuilder();
          s = "";
          try
          {
              Dictionary<String, processInfos> details = new Dictionary<string, processInfos>();
              ConnectionOptions con = new ConnectionOptions();
              byte[] authentication = new byte[12];
              string[] str = { };
              if (AuthByte.TryGetValue(node.RootNode, out authentication))
              {
                  if (System.Diagnostics.Debugger.IsAttached)
                      log.Info("Authentication -" + System.Text.Encoding.ASCII.GetString(authentication));
                  str = System.Text.Encoding.ASCII.GetString(authentication).Split(':');
              }
              else
              {
                  s = "Access Denied";

              }


              if (str != null && str.Length != 2)
              {
                  s = "Access Denied";

              }
              con.Username = node.RootNode + @"\" + str[0];
              con.Password = str[1];
              con.Authentication = AuthenticationLevel.Packet;
              con.Impersonation = ImpersonationLevel.Impersonate;
              ManagementScope scope = new ManagementScope("\\\\" + node.RootNode + "\\root\\cimv2", con);
              ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Process");
              s = "";
              Wmi.Objects.Process.EnumerateProcesses(new ManagementObjectSearcher(scope, query), ref details, ref s);

              processInfos pfs = new processInfos();
              if (details.TryGetValue(node.CurrentNodeDir,out pfs))
              {
                  builder.AppendLine("[IOCounter]");
                  builder.AppendLine("WriteOperationCount:" + NullHandler(pfs.IOValues.WriteOperationCount));
                  builder.AppendLine("WriteTransferCount:" + NullHandler(pfs.IOValues.WriteTransferCount));
                  builder.AppendLine("ReadOperationCount:" + NullHandler(pfs.IOValues.ReadOperationCount));
                  builder.AppendLine("ReadTransferCount:" + NullHandler(pfs.IOValues.ReadTransferCount));
                  builder.AppendLine("OtherOperationCount:" + NullHandler(pfs.IOValues.OtherOperationCount));
                  builder.AppendLine("OtherTransferCount:" + NullHandler(pfs.IOValues.OtherTransferCount));
                  builder.AppendLine("");

              }
              else
              {
                  builder.AppendLine(s);
              }
              if (System.Diagnostics.Debugger.IsAttached)
                  log.Info("Total IO Found-" + details.Count + " IF Error" + s);
          }
          catch (Exception e)
          {
              builder.AppendLine(e.Message);
              if (System.Diagnostics.Debugger.IsAttached)
                  log.Fatal("Exception: {0}", e.Message);

          }

          return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
      } 
      #endregion


       public uint Def_Cleanup(string filename, IntPtr info)
        {
            return 0;
        }

        public uint Def_CloseFile(string filename, IntPtr info)
        {
            return 0;
        }
        public uint Def_CreateDirectory(string filename, IntPtr info)
        {
            VirtualNode node=new VirtualNode(filename);
            
            if (node.isRemote && !node.isFile)
            {                               
                AuthByte.Add(node.RootNode, null);
                return NtStatus.Success;
            }
            else {
               
            
            }
                        
            return NtStatus.MediaWriteProtected;
        }

        public uint Def_CreateFile(string filename, System.IO.FileAccess access, System.IO.FileShare share, System.IO.FileMode mode, System.IO.FileOptions options, IntPtr info)
        {
            VirtualNode node = new VirtualNode(filename);
            if (node.isRemote)
            {
                if (node.CurrentNodeFile.EndsWith("auth.inf",StringComparison.CurrentCultureIgnoreCase))
                {
                    return 0;
                }
                else
                    return NtStatus.ObjectPathNotFound;

            }
           
            return 0;
        }
        byte[] serviceCatch=new byte[2000000];
        public uint Def_ReadFile(string filename, IntPtr buffer, uint BufferSize, ref uint NumberByteReadSuccess, long Offset, IntPtr info)
        {
            try
            {
                byte[] file=null;
                VirtualNode Node = new VirtualNode(filename);

                if (Node.isFile && Node.isRemote) {
                    if (System.Diagnostics.Debugger.IsAttached)
                    log.Info("Reading Remote Data Node-{0} Data-{1}",Node.RootNode,Node.CurrentNodeFile);
                    
                    string s="";
                    switch (Node.CurrentNodeFile)
                    {
                        case "Process.inf":
                            file = WmiProcessFill(Node, out s);
                            break;
                        case "Module.inf":
                            file = WmiModuleFill(Node, out s);
                            break;
                        case "IOCounters.inf":
                            file = WmiIOFill(Node, out s);
                            break;
                        case "Thread.inf":
                            file = WmiThreadFill(Node,out s);
                            break;
                        default:
                            return NtStatus.FileInvalid;                            
                    }            
                  
                }
                //Console.WriteLine("{0},{1},{2},{3}", filename,Node.CurrentNodeDir, Node.CurrentNodeFile,Node.isFile); 
                else if (Node.isFile)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    log.Info("Reading Local Data Node-{0} Data-{1}", Node.RootNode, Node.CurrentNodeFile);
                    if (Node.CurrentNodeFile.EndsWith(".inf"))
                    {
                       
                        int pid;
                        
                        if (int.TryParse(Node.CurrentNodeDir, out pid))
                        {

                           
                            switch (Node.CurrentNodeFile) { 
                                case "Process.inf":
                                file = FillProcessDetail(pid);
                                    break;
                                case "Memory.inf":
                                    file = FillMemoryDetail(pid);
                                    break;
                                case "Network.inf":
                                    file = FillNetworkkDetail(pid);
                                    break;
                                case "Module.inf":
                                    file = FillModuleDetail(pid);
                                    break;
                                case "Thread.inf":
                                    file = FillThreadDetail(pid);
                                    break;
                                case "Token.inf":
                                    file = FillTokenDetail(pid);
                                    break;
                                case "Handle.inf":
                                    file = FillHandleDetail(pid);
                                    break;
                                case "Window.inf":
                                    file = FillWindowDetail(pid);
                                    break;
                                case "MemRegion.inf":
                                    file = FillMemRegionDetail(pid);
                                    break;
                                case "IOCounters.inf":
                                    file = FillIODetail(pid);
                                    break;
                                case "Heap.inf":
                                    file = FillHeapDetail(pid);
                                    break;                                    
                                default:
                                    file = FillEnvDetail(pid);
                                    break;
                            }
                        }
                        else
                        {
                          
                            if(Node.CurrentNodeDir=="\\" || Node.CurrentNodeDir==""){                           
                           switch (Node.CurrentNodeFile) 
                            {
                                case "NetStat.inf":
                                file = FillNetworkStatDetail();
                                break;
                                case "SystemInfo.inf":
                                file = FillSysDetail();
                                break;
                                case "Jobs.inf":
                                file = FillJobDetail();
                                break;
                               case "Services.inf":
                                lock (criticalS)
                                {
                                    file = new byte[serviceCatch.Length];
                                    serviceCatch.CopyTo(file, 0);
                                }
                              
                                break;                              
                               default:
                                file=FillEnvDetail(0);
                                break;
                            }
                            }


                        }
                      

                        
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
                    NumberByteReadSuccess = 0;
                    return 0;
                }
               
            }
            catch(Exception e)
            {
                
                    if (System.Diagnostics.Debugger.IsAttached)
                    log.Error("Read Error "+e.Message);                    
                return NtStatus.FileInvalid;
            }
        }

    
        public uint Def_GetDiskInfo(ref ulong Available, ref ulong Total, ref ulong Free)
        {
            Available = 95245862458;
            Free = 95245862458;
            Total = 152458624580;
            return 0;
        }

        public uint Def_GetFileInformation(string filename, ref BY_HANDLE_FILE_INFORMATION Information, IntPtr info)
        {
            VirtualNode Node=new VirtualNode(filename);
            Information.CreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
            Information.LastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
            Information.LastWriteTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
            Information.FileSizeLow = 100000;
            if (Node.isFile)
                Information.FileAttributes = FileAttributes.Readonly;
            else
                Information.FileAttributes = FileAttributes.Directory|FileAttributes.Readonly;

            return 0;
        }

        public uint Def_GetVolumeInfo(IntPtr VolumeNameBuffer, uint VolumeNameSize, ref uint SerialNumber, ref uint MaxComponenetLegnth, ref uint FileSystemFeatures, IntPtr FileSystemNameBuffer, uint FileSystemNameSize)
        {
            HelperFunction.SetVolumeInfo(VolumeNameBuffer, "Process Data", (int)VolumeNameSize, FileSystemNameBuffer, "ProcFS", (int)FileSystemNameSize);
            SerialNumber = (uint)GetHashCode();
            MaxComponenetLegnth = 256;
           
            return 0;
        }

        public uint Def_DeleteDirectory(string filename, IntPtr info)
        {
            return NtStatus.MediaWriteProtected;
        }

        public uint Def_DeleteFile(string filename, IntPtr info)
        {
            return NtStatus.MediaWriteProtected;
        }

        public uint Def_FindFiles(string filename, ref FillFindData FillFunction, IntPtr info)
        {
            try
            {
                VirtualNode Node = new VirtualNode(filename);
                if (Node.isRemote)
                {
                    string s="";
                    if (System.Diagnostics.Debugger.IsAttached)
                    log.Info("Remote Node Accessed {0}",Node.RootNode);
                    if (s.Contains("Access"))
                        return NtStatus.AccessDenied;
                    int i = 0;
                    if (!int.TryParse(Node.CurrentNodeDir, out i))
                    foreach (string prcs in WmiProcessCollection(Node,out s))
                    {
                        WIN32_FIND_DATA Information = new WIN32_FIND_DATA();
                        Information.cFileName = prcs;
                        Information.ftCreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                        Information.ftLastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                        Information.ftLastWriteTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                        Information.dwFileAttributes = FileAttributes.Directory | FileAttributes.Readonly;
                        FillFunction(ref Information, info);
                    }
                  
                    if (int.TryParse(Node.CurrentNodeDir, out i))
                    {
                        foreach (string file in remoteBase) {
                            WIN32_FIND_DATA Information = new WIN32_FIND_DATA();
                            Information.cFileName = file;
                            Information.ftCreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                            Information.ftLastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                            Information.ftLastWriteTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                            Information.dwFileAttributes = FileAttributes.Readonly;
                            Information.nFileSizeLow = 100000;
                            FillFunction(ref Information, info);
                        }
                    }
                    else
                    {
                        foreach (string file in remoteCommon)
                        {
                            WIN32_FIND_DATA Information = new WIN32_FIND_DATA();
                            Information.cFileName = file;
                            Information.ftCreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                            Information.ftLastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                            Information.ftLastWriteTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                            Information.dwFileAttributes = FileAttributes.Readonly;
                            Information.nFileSizeLow = 100000;
                            FillFunction(ref Information, info);
                        }
                    }
                    return 0;

                }
                
                else if ((Node.RootNode == "\\" || Node.RootNode == "") && !Node.isFile && Node.CurrentNodeDir=="")
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                        log.Info("Local Node Accessed {0}", Node.RootNode);
                    lock (critical)
                    {
                        foreach (string prcs in database.Keys)
                        {
                            WIN32_FIND_DATA Information = new WIN32_FIND_DATA();
                            Information.cFileName = prcs;
                            Information.ftCreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                            Information.ftLastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                            Information.ftLastWriteTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                            Information.dwFileAttributes = FileAttributes.Directory | FileAttributes.Readonly;
                            FillFunction(ref Information, info);
                        }
                    }
                    foreach (string file in commonSystem)
                    {
                        WIN32_FIND_DATA Information = new WIN32_FIND_DATA();
                        Information.cFileName = file;
                        Information.ftCreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                        Information.ftLastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                        Information.ftLastWriteTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                        Information.dwFileAttributes = FileAttributes.Readonly;
                        Information.nFileSizeLow = 100000;
                        FillFunction(ref Information, info);
                    }
                    foreach (string remote in AuthByte.Keys)
                    {
                        WIN32_FIND_DATA Information = new WIN32_FIND_DATA();
                        Information.cFileName = remote;
                        Information.ftCreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                        Information.ftLastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                        Information.ftLastWriteTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                        Information.dwFileAttributes = FileAttributes.Directory | FileAttributes.Readonly;
                        FillFunction(ref Information, info);

                    }

                }
                else if (!Node.isFile && Node.CurrentNodeDir != "" && Node.RootNode!="")
                {
                    int id;
                    if (Node.CurrentNodeDir != "" && int.TryParse(Node.CurrentNodeDir, out id))
                    {
                        List<int> childProcess = Process.EnumerateChildProcessesById(id);
                        foreach (int prcs in childProcess)
                        {
                            WIN32_FIND_DATA Information = new WIN32_FIND_DATA();
                            Information.cFileName = prcs.ToString();
                            Information.ftCreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                            Information.ftLastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                            Information.ftLastWriteTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                            Information.dwFileAttributes = FileAttributes.Directory | FileAttributes.Readonly;                            
                            FillFunction(ref Information, info);
                        }
                        foreach (string file in baseinfos)
                        {
                            WIN32_FIND_DATA Information = new WIN32_FIND_DATA();
                            Information.cFileName = file;
                            Information.ftCreationTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                            Information.ftLastAccessTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                            Information.ftLastWriteTime = HelperFunction.DateTimeToFileTime(DateTime.Now.ToFileTime());
                            Information.nFileSizeLow = 100000;
                            Information.dwFileAttributes = FileAttributes.Readonly;
                            FillFunction(ref Information, info);
                        }


                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    log.Error("Find Error :" + e.Message);
                return NtStatus.FileInvalid;
            }
        }

        public uint Def_FlushFileBuffers(string filename, IntPtr info)
        {
            return 0;
        }

        
        public uint Def_LockFile(string filename, long offset, long length, IntPtr info)
        {
            return 0;
        }

        public uint Def_MoveFile(string filename, string newname, bool replace, IntPtr info)
        {
            return NtStatus.MediaWriteProtected;
        }

        public uint Def_OpenDirectory(string filename, IntPtr info)
        {
            return 0;
        }

       

        public uint Def_SetAllocationSize(string filename, long length, IntPtr info)
        {
            return NtStatus.MediaWriteProtected; ;
        }

        public uint Def_SetEndOfFile(string filename, long length, IntPtr info)
        {
            return NtStatus.MediaWriteProtected;
        }

        public uint Def_SetFileAttributes(string filename, uint Attribute, IntPtr info)
        {
            return NtStatus.MediaWriteProtected; ;
        }

        public uint Def_SetFileTime(string filename, System.Runtime.InteropServices.ComTypes.FILETIME ctime, System.Runtime.InteropServices.ComTypes.FILETIME atime, System.Runtime.InteropServices.ComTypes.FILETIME mtime, IntPtr info)
        {
            return NtStatus.MediaWriteProtected;
        }

        public uint Def_UnlockFile(string filename, long offset, long length, IntPtr info)
        {
            return NtStatus.MediaWriteProtected;
        }

        public uint Def_Unmount(IntPtr info)
        {
            return 0;
        }

        public uint Def_WriteFile(string filename, byte[] buffer, ref uint writtenBytes, long offset, IntPtr info)
        {
            VirtualNode node = new VirtualNode(filename);
            if (node.isRemote && node.CurrentNodeFile == "auth.inf")
            {

                if (AuthByte.ContainsKey(node.RootNode))
                {
                    AuthByte[node.RootNode] = buffer;
                }
                else if(!AuthByte.ContainsKey(node.RootNode)){
                    AuthByte.Add(node.RootNode, buffer);
                }
                writtenBytes = (uint)buffer.Length;
                return 0;
            }


            return NtStatus.MediaWriteProtected;
        }
    }
}
