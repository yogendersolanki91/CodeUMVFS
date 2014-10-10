using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Utility
{
    /// <summary>
    /// File attributes are metadata values stored by the file system on disk and are used by the system and are available to developers via various file I/O APIs.
    /// </summary>
    
    public class FileAttributes
    {
        /// <summary>
        /// A file that is read-only. Applications can read the file, but cannot write to it or delete it. This attribute is not honored on directories. For more information, see "You cannot view or change the Read-only or the System attributes of folders in Windows Server 2003, in Windows XP, or in Windows Vista".
        /// </summary>
       public static readonly uint Readonly = 0x00000001;

        /// <summary>
        /// The file or directory is hidden. It is not included in an ordinary directory listing.
        /// </summary>
       public static readonly uint Hidden = 0x00000002;

        /// <summary>
        /// A file or directory that the operating system uses a part of, or uses exclusively.
        /// </summary>
      public static readonly uint  System = 0x00000004;

        /// <summary>
        /// The handle that identifies a directory.
        /// </summary>
      public static readonly uint  Directory = 0x00000010;

        /// <summary>
        /// A file or directory that is an archive file or directory. Applications typically use this attribute to mark files for backup or removal.
        /// </summary>
      public static readonly uint  Archive = 0x00000020;

        /// <summary>
        /// This value is reserved for system use.
        /// </summary>
      public static readonly uint  Device = 0x00000040;

        /// <summary>
        /// A file that does not have other attributes set. This attribute is valid only when used alone.
        /// </summary>
       public static readonly uint Normal = 0x00000080;

        /// <summary>
        /// A file that is being used for temporary storage. File systems avoid writing data back to mass storage if sufficient cache memory is available, because typically, an application deletes a temporary file after the handle is closed. In that scenario, the system can entirely avoid writing the data. Otherwise, the data is written after the handle is closed.
        /// </summary>
       public static readonly uint Temporary = 0x00000100;

        /// <summary>
        /// A file that is a sparse file.
        /// </summary>
       public static readonly uint SparseFile = 0x00000200;

        /// <summary>
        /// A file or directory that has an associated reparse point, or a file that is a symbolic link.
        /// </summary>
       public static readonly uint ReparsePoint = 0x00000400;

        /// <summary>
        /// A file or directory that is compressed. For a file, all of the data in the file is compressed. For a directory, compression is the default for newly created files and subdirectories.
        /// </summary>
       public static readonly uint Compressed = 0x00000800;

        /// <summary>
        /// The data of a file is not available immediately. This attribute indicates that the file data is physically moved to offline storage. This attribute is used by Remote Storage, which is the hierarchical storage management software. Applications should not arbitrarily change this attribute.
        /// </summary>
      public static readonly uint  Offline = 0x00001000;

        /// <summary>
        /// The file or directory is not to be indexed by the content indexing service.
        /// </summary>
       public static readonly uint NotContentIndexed = 0x00002000;

        /// <summary>
        /// A file or directory that is encrypted. For a file, all data streams in the file are encrypted. For a directory, encryption is the default for newly created files and subdirectories.
        /// </summary>
       public static readonly uint Encrypted = 0x00004000;

        /// <summary>
        /// This value is reserved for system use.
        /// </summary>
       public static readonly uint Virtual = 0x00010000;
    }
    public class VirtualNode
    {
        public string NodePath;
        public string[] AllNodes;
        public string CurrentNodeDir;
        public string CurrentNodeFile;
        public string RootNode;
        public bool isFile;
        public bool isRemote;

        public VirtualNode(string Path)
        {
            AllNodes = Path.Split('\\').Where(x => !string.IsNullOrEmpty(x)).ToArray();
            NodePath = Path;
            if (!Path.EndsWith("Desktop.inf", StringComparison.InvariantCultureIgnoreCase) && !Path.EndsWith("Autorun.inf", StringComparison.InvariantCultureIgnoreCase))
            {
                //System.Net.IPAddress ip = System.Net.IPAddress.Any;
                Regex ip = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");
                if (AllNodes.Length>=1 && ip.IsMatch(AllNodes[0]))
                {
                   
                    //remote node 
                    RootNode = AllNodes[0];
                    isRemote = true;
                    if(AllNodes[AllNodes.Length-1].EndsWith(".inf")){
                        if (AllNodes.Length <= 2)
                        {
                            RootNode = AllNodes[AllNodes.Length - 2];
                            CurrentNodeDir = AllNodes[AllNodes.Length - 2];
                            CurrentNodeFile = AllNodes[AllNodes.Length - 1];
                            isFile = true;
                        }
                        else if (AllNodes.Length > 2)
                        {
                            RootNode = AllNodes[AllNodes.Length - 3];
                            CurrentNodeDir = AllNodes[AllNodes.Length - 2];
                            CurrentNodeFile = AllNodes[AllNodes.Length - 1];
                            isFile = true;
                        }
        
                        
                    }
                    else
                    {
                        if (AllNodes.Length < 2)
                        {
                            RootNode = AllNodes[0];
                            CurrentNodeDir = AllNodes[0];
                            CurrentNodeFile = "";
                            isFile = false;

                        }

                        else if (AllNodes.Length == 2)
                        {
                            RootNode = AllNodes[0];
                            CurrentNodeDir = AllNodes[1];
                            CurrentNodeFile = "";
                            isFile = false;
                        }
                        else
                        {
                            RootNode = AllNodes[AllNodes.Length - 1];
                            CurrentNodeDir = AllNodes[AllNodes.Length - 1];
                            CurrentNodeFile = "";
                            isFile = false;
                        }
                    }
                    

                }

                else if (AllNodes.Length >= 2 && !Path.EndsWith(".inf", StringComparison.InvariantCultureIgnoreCase))
                {
                    //for (int i = 1; i < AllNodes.Length - 2;i++ )
                    CurrentNodeDir = AllNodes[AllNodes.Length - 1].Trim();
                    CurrentNodeFile = "";
                    isFile = false;
                    RootNode = AllNodes[0];

                }
                
                else if (AllNodes.Length >= 2 && NodePath.EndsWith(".inf", StringComparison.InvariantCultureIgnoreCase))
                {
                    CurrentNodeDir = AllNodes[AllNodes.Length - 2].Trim();
                    CurrentNodeFile = AllNodes[AllNodes.Length - 1].Trim();
                    isFile = true;
                    RootNode = AllNodes[0].Trim();
                    if (RootNode.Contains(".inf"))
                        RootNode = @"\";



                    //      isFile = true;
                }
                else
                {
                    if (AllNodes.Length == 1 && AllNodes[0].EndsWith(".inf", StringComparison.InvariantCultureIgnoreCase))
                    {

                        CurrentNodeFile = AllNodes[0].Trim();
                        RootNode = "\\";
                        CurrentNodeDir = "";
                        isFile = true;
                        //RootNode = "\\";
                    }
                    else if (AllNodes.Length == 1)
                    {
                        CurrentNodeFile = "";
                        CurrentNodeDir = AllNodes[0].Trim();
                        isFile = false;
                        RootNode = "\\";
                    }
                    else
                    {
                        CurrentNodeFile = "";
                        CurrentNodeDir = "";
                        isFile = false;
                        RootNode = "\\";
                    }
                    //isFile = true;


                }
            }
            else {
                CurrentNodeFile = "";
                CurrentNodeDir = "";
                isFile = false;
                RootNode = "\\";
            
            }



        }



    }
    public class Self
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

         public static bool isDebug(){
           bool res=false;
            CheckRemoteDebuggerPresent(System.Diagnostics.Process.GetCurrentProcess().Handle,ref res);
            return res;
        }

    }
    
    /// <summary>
    /// A NT status value.
    /// </summary>
    public class NtStatus 
    {
       
        public static readonly uint MediaWriteProtected=0xC00000A2;
        public static readonly uint Success = 0x00000000;
        public static readonly uint Wait0 = 0x00000000;
        public static readonly uint Wait1 = 0x00000001;
        public static readonly uint Wait2 = 0x00000002;
        public static readonly uint Wait3 = 0x00000003;
        public static readonly uint Wait63 = 0x0000003f;
        public static readonly uint Abandoned = 0x00000080;
        public static readonly uint AbandonedWait0 = 0x00000080;
        public static readonly uint AbandonedWait1 = 0x00000081;
        public static readonly uint AbandonedWait2 = 0x00000082;
        public static readonly uint AbandonedWait3 = 0x00000083;
        public static readonly uint AbandonedWait63 = 0x000000bf;
        public static readonly uint UserApc = 0x000000c0;
        public static readonly uint KernelApc = 0x00000100;
        public static readonly uint Alerted = 0x00000101;
        public static readonly uint Timeout = 0x00000102;
        public static readonly uint Pending = 0x00000103;
        public static readonly uint Reparse = 0x00000104;
        public static readonly uint MoreEntries = 0x00000105;
        public static readonly uint NotAllAssigned = 0x00000106;
        public static readonly uint SomeNotMapped = 0x00000107;
        public static readonly uint OpLockBreakInProgress = 0x00000108;
        public static readonly uint VolumeMounted = 0x00000109;
        public static readonly uint RxActCommitted = 0x0000010a;
        public static readonly uint NotifyCleanup = 0x0000010b;
        public static readonly uint NotifyEnumDir = 0x0000010c;
        public static readonly uint NoQuotasForAccount = 0x0000010d;
        public static readonly uint PrimaryTransportConnectFailed = 0x0000010e;
        public static readonly uint PageFaultTransition = 0x00000110;
        public static readonly uint PageFaultDemandZero = 0x00000111;
        public static readonly uint PageFaultCopyOnWrite = 0x00000112;
        public static readonly uint PageFaultGuardPage = 0x00000113;
        public static readonly uint PageFaultPagingFile = 0x00000114;
        public static readonly uint CrashDump = 0x00000116;
        public static readonly uint ReparseObject = 0x00000118;
        public static readonly uint NothingToTerminate = 0x00000122;
        public static readonly uint ProcessNotInJob = 0x00000123;
        public static readonly uint ProcessInJob = 0x00000124;
        public static readonly uint ProcessCloned = 0x00000129;
        public static readonly uint FileLockedWithOnlyReaders = 0x0000012a;
        public static readonly uint FileLockedWithWriters = 0x0000012b;

        // Informational
        public static readonly uint Informational = 0x40000000;
        public static readonly uint ObjectNameExists = 0x40000000;
        public static readonly uint ThreadWasSuspended = 0x40000001;
        public static readonly uint WorkingSetLimitRange = 0x40000002;
        public static readonly uint ImageNotAtBase = 0x40000003;
        public static readonly uint RegistryRecovered = 0x40000009;

     // Warning
        public static readonly uint Warning = 0x80000000;
        public static readonly uint GuardPageViolation = 0x80000001;
        public static readonly uint DatatypeMisalignment = 0x80000002;
        public static readonly uint Breakpoint = 0x80000003;
        public static readonly uint SingleStep = 0x80000004;
        public static readonly uint BufferOverflow = 0x80000005;
        public static readonly uint NoMoreFiles = 0x80000006;
        public static readonly uint HandlesClosed = 0x8000000a;
        public static readonly uint PartialCopy = 0x8000000d;
        public static readonly uint DeviceBusy = 0x80000011;
        public static readonly uint InvalidEaName = 0x80000013;
        public static readonly uint EaListInconsistent = 0x80000014;
        public static readonly uint NoMoreEntries = 0x8000001a;
        public static readonly uint LongJump = 0x80000026;
        public static readonly uint DllMightBeInsecure = 0x8000002b;

        // Error
        public static readonly uint Error = 0xc0000000;
        public static readonly uint Unsuccessful = 0xc0000001;
        public static readonly uint NotImplemented = 0xc0000002;
        public static readonly uint InvalidInfoClass = 0xc0000003;
        public static readonly uint InfoLengthMismatch = 0xc0000004;
        public static readonly uint AccessViolation = 0xc0000005;
        public static readonly uint InPageError = 0xc0000006;
        public static readonly uint PagefileQuota = 0xc0000007;
        public static readonly uint InvalidHandle = 0xc0000008;
        public static readonly uint BadInitialStack = 0xc0000009;
        public static readonly uint BadInitialPc = 0xc000000a;
        public static readonly uint InvalidCid = 0xc000000b;
        public static readonly uint TimerNotCanceled = 0xc000000c;
        public static readonly uint InvalidParameter = 0xc000000d;
        public static readonly uint NoSuchDevice = 0xc000000e;
        public static readonly uint NoSuchFile = 0xc000000f;
        public static readonly uint InvalidDeviceRequest = 0xc0000010;
        public static readonly uint EndOfFile = 0xc0000011;
        public static readonly uint WrongVolume = 0xc0000012;
        public static readonly uint NoMediaInDevice = 0xc0000013;
        public static readonly uint NoMemory = 0xc0000017;
        public static readonly uint NotMappedView = 0xc0000019;
        public static readonly uint UnableToFreeVm = 0xc000001a;
        public static readonly uint UnableToDeleteSection = 0xc000001b;
        public static readonly uint IllegalInstruction = 0xc000001d;
        public static readonly uint AlreadyCommitted = 0xc0000021;
        public static readonly uint AccessDenied = 0xc0000022;
        public static readonly uint BufferTooSmall = 0xc0000023;
        public static readonly uint ObjectTypeMismatch = 0xc0000024;
        public static readonly uint NonContinuableException = 0xc0000025;
        public static readonly uint BadStack = 0xc0000028;
        public static readonly uint NotLocked = 0xc000002a;
        public static readonly uint NotCommitted = 0xc000002d;
        public static readonly uint InvalidParameterMix = 0xc0000030;
        public static readonly uint ObjectNameInvalid = 0xc0000033;
        public static readonly uint ObjectNameNotFound = 0xc0000034;
        public static readonly uint ObjectNameCollision = 0xc0000035;
        public static readonly uint ObjectPathInvalid = 0xc0000039;
        public static readonly uint ObjectPathNotFound = 0xc000003a;
        public static readonly uint ObjectPathSyntaxBad = 0xc000003b;
        public static readonly uint DataOverrun = 0xc000003c;
        public static readonly uint DataLate = 0xc000003d;
        public static readonly uint DataError = 0xc000003e;
        public static readonly uint CrcError = 0xc000003f;
        public static readonly uint SectionTooBig = 0xc0000040;
        public static readonly uint PortConnectionRefused = 0xc0000041;
        public static readonly uint InvalidPortHandle = 0xc0000042;
        public static readonly uint SharingViolation = 0xc0000043;
        public static readonly uint QuotaExceeded = 0xc0000044;
        public static readonly uint InvalidPageProtection = 0xc0000045;
        public static readonly uint MutantNotOwned = 0xc0000046;
        public static readonly uint SemaphoreLimitExceeded = 0xc0000047;
        public static readonly uint PortAlreadySet = 0xc0000048;
        public static readonly uint SectionNotImage = 0xc0000049;
        public static readonly uint SuspendCountExceeded = 0xc000004a;
        public static readonly uint ThreadIsTerminating = 0xc000004b;
        public static readonly uint BadWorkingSetLimit = 0xc000004c;
        public static readonly uint IncompatibleFileMap = 0xc000004d;
        public static readonly uint SectionProtection = 0xc000004e;
        public static readonly uint EasNotSupported = 0xc000004f;
        public static readonly uint EaTooLarge = 0xc0000050;
        public static readonly uint NonExistentEaEntry = 0xc0000051;
        public static readonly uint NoEasOnFile = 0xc0000052;
        public static readonly uint EaCorruptError = 0xc0000053;
        public static readonly uint FileLockConflict = 0xc0000054;
        public static readonly uint LockNotGranted = 0xc0000055;
        public static readonly uint DeletePending = 0xc0000056;
        public static readonly uint CtlFileNotSupported = 0xc0000057;
        public static readonly uint UnknownRevision = 0xc0000058;
        public static readonly uint RevisionMismatch = 0xc0000059;
        public static readonly uint InvalidOwner = 0xc000005a;
        public static readonly uint InvalidPrimaryGroup = 0xc000005b;
        public static readonly uint NoImpersonationToken = 0xc000005c;
        public static readonly uint CantDisableMandatory = 0xc000005d;
        public static readonly uint NoLogonServers = 0xc000005e;
        public static readonly uint NoSuchLogonSession = 0xc000005f;
        public static readonly uint NoSuchPrivilege = 0xc0000060;
        public static readonly uint PrivilegeNotHeld = 0xc0000061;
        public static readonly uint InvalidAccountName = 0xc0000062;
        public static readonly uint UserExists = 0xc0000063;
        public static readonly uint NoSuchUser = 0xc0000064;
        public static readonly uint GroupExists = 0xc0000065;
        public static readonly uint NoSuchGroup = 0xc0000066;
        public static readonly uint MemberInGroup = 0xc0000067;
        public static readonly uint MemberNotInGroup = 0xc0000068;
        public static readonly uint LastAdmin = 0xc0000069;
        public static readonly uint WrongPassword = 0xc000006a;
        public static readonly uint IllFormedPassword = 0xc000006b;
        public static readonly uint PasswordRestriction = 0xc000006c;
        public static readonly uint LogonFailure = 0xc000006d;
        public static readonly uint AccountRestriction = 0xc000006e;
        public static readonly uint InvalidLogonHours = 0xc000006f;
        public static readonly uint InvalidWorkstation = 0xc0000070;
        public static readonly uint PasswordExpired = 0xc0000071;
        public static readonly uint AccountDisabled = 0xc0000072;
        public static readonly uint NoneMapped = 0xc0000073;
        public static readonly uint TooManyLuidsRequested = 0xc0000074;
        public static readonly uint LuidsExhausted = 0xc0000075;
        public static readonly uint InvalidSubAuthority = 0xc0000076;
        public static readonly uint InvalidAcl = 0xc0000077;
        public static readonly uint InvalidSid = 0xc0000078;
        public static readonly uint InvalidSecurityDescr = 0xc0000079;
        public static readonly uint ProcedureNotFound = 0xc000007a;
        public static readonly uint InvalidImageFormat = 0xc000007b;
        public static readonly uint NoToken = 0xc000007c;
        public static readonly uint BadInheritanceAcl = 0xc000007d;
        public static readonly uint RangeNotLocked = 0xc000007e;
        public static readonly uint DiskFull = 0xc000007f;
        public static readonly uint ServerDisabled = 0xc0000080;
        public static readonly uint ServerNotDisabled = 0xc0000081;
        public static readonly uint TooManyGuidsRequested = 0xc0000082;
        public static readonly uint GuidsExhausted = 0xc0000083;
        public static readonly uint InvalidIdAuthority = 0xc0000084;
        public static readonly uint AgentsExhausted = 0xc0000085;
        public static readonly uint InvalidVolumeLabel = 0xc0000086;
        public static readonly uint SectionNotExtended = 0xc0000087;
        public static readonly uint NotMappedData = 0xc0000088;
        public static readonly uint ResourceDataNotFound = 0xc0000089;
        public static readonly uint ResourceTypeNotFound = 0xc000008a;
        public static readonly uint ResourceNameNotFound = 0xc000008b;
        public static readonly uint ArrayBoundsExceeded = 0xc000008c;
        public static readonly uint FloatDenormalOperand = 0xc000008d;
        public static readonly uint FloatDivideByZero = 0xc000008e;
        public static readonly uint FloatInexactResult = 0xc000008f;
        public static readonly uint FloatInvalidOperation = 0xc0000090;
        public static readonly uint FloatOverflow = 0xc0000091;
        public static readonly uint FloatStackCheck = 0xc0000092;
        public static readonly uint FloatUnderflow = 0xc0000093;
        public static readonly uint IntegerDivideByZero = 0xc0000094;
        public static readonly uint IntegerOverflow = 0xc0000095;
        public static readonly uint PrivilegedInstruction = 0xc0000096;
        public static readonly uint TooManyPagingFiles = 0xc0000097;
        public static readonly uint FileInvalid = 0xc0000098;
        public static readonly uint InstanceNotAvailable = 0xc00000ab;
        public static readonly uint PipeNotAvailable = 0xc00000ac;
        public static readonly uint InvalidPipeState = 0xc00000ad;
        public static readonly uint PipeBusy = 0xc00000ae;
        public static readonly uint IllegalFunction = 0xc00000af;
        public static readonly uint PipeDisconnected = 0xc00000b0;
        public static readonly uint PipeClosing = 0xc00000b1;
        public static readonly uint PipeConnected = 0xc00000b2;
        public static readonly uint PipeListening = 0xc00000b3;
        public static readonly uint InvalidReadMode = 0xc00000b4;
        public static readonly uint IoTimeout = 0xc00000b5;
        public static readonly uint FileForcedClosed = 0xc00000b6;
        public static readonly uint ProfilingNotStarted = 0xc00000b7;
        public static readonly uint ProfilingNotStopped = 0xc00000b8;
        public static readonly uint NotSameDevice = 0xc00000d4;
        public static readonly uint FileRenamed = 0xc00000d5;
        public static readonly uint CantWait = 0xc00000d8;
        public static readonly uint PipeEmpty = 0xc00000d9;
        public static readonly uint CantTerminateSelf = 0xc00000db;
        public static readonly uint InternalError = 0xc00000e5;
        public static readonly uint InvalidParameter1 = 0xc00000ef;
        public static readonly uint InvalidParameter2 = 0xc00000f0;
        public static readonly uint InvalidParameter3 = 0xc00000f1;
        public static readonly uint InvalidParameter4 = 0xc00000f2;
        public static readonly uint InvalidParameter5 = 0xc00000f3;
        public static readonly uint InvalidParameter6 = 0xc00000f4;
        public static readonly uint InvalidParameter7 = 0xc00000f5;
        public static readonly uint InvalidParameter8 = 0xc00000f6;
        public static readonly uint InvalidParameter9 = 0xc00000f7;
        public static readonly uint InvalidParameter10 = 0xc00000f8;
        public static readonly uint InvalidParameter11 = 0xc00000f9;
        public static readonly uint InvalidParameter12 = 0xc00000fa;
        public static readonly uint MappedFileSizeZero = 0xc000011e;
        public static readonly uint TooManyOpenedFiles = 0xc000011f;
        public static readonly uint Cancelled = 0xc0000120;
        public static readonly uint CannotDelete = 0xc0000121;
        public static readonly uint InvalidComputerName = 0xc0000122;
        public static readonly uint FileDeleted = 0xc0000123;
        public static readonly uint SpecialAccount = 0xc0000124;
        public static readonly uint SpecialGroup = 0xc0000125;
        public static readonly uint SpecialUser = 0xc0000126;
        public static readonly uint MembersPrimaryGroup = 0xc0000127;
        public static readonly uint FileClosed = 0xc0000128;
        public static readonly uint TooManyThreads = 0xc0000129;
        public static readonly uint ThreadNotInProcess = 0xc000012a;
        public static readonly uint TokenAlreadyInUse = 0xc000012b;
        public static readonly uint PagefileQuotaExceeded = 0xc000012c;
        public static readonly uint CommitmentLimit = 0xc000012d;
        public static readonly uint InvalidImageLeFormat = 0xc000012e;
        public static readonly uint InvalidImageNotMz = 0xc000012f;
        public static readonly uint InvalidImageProtect = 0xc0000130;
        public static readonly uint InvalidImageWin16 = 0xc0000131;
        public static readonly uint LogonServer = 0xc0000132;
        public static readonly uint DifferenceAtDc = 0xc0000133;
        public static readonly uint SynchronizationRequired = 0xc0000134;
        public static readonly uint DllNotFound = 0xc0000135;
        public static readonly uint IoPrivilegeFailed = 0xc0000137;
        public static readonly uint OrdinalNotFound = 0xc0000138;
        public static readonly uint EntryPointNotFound = 0xc0000139;
        public static readonly uint ControlCExit = 0xc000013a;
        public static readonly uint PortNotSet = 0xc0000353;
        public static readonly uint DebuggerInactive = 0xc0000354;
        public static readonly uint CallbackBypass = 0xc0000503;
        public static readonly uint PortClosed = 0xc0000700;
        public static readonly uint MessageLost = 0xc0000701;
        public static readonly uint InvalidMessage = 0xc0000702;
        public static readonly uint RequestCanceled = 0xc0000703;
        public static readonly uint RecursiveDispatch = 0xc0000704;
        public static readonly uint LpcReceiveBufferExpected = 0xc0000705;
        public static readonly uint LpcInvalidConnectionUsage = 0xc0000706;
        public static readonly uint LpcRequestsNotAllowed = 0xc0000707;
        public static readonly uint ResourceInUse = 0xc0000708;
        public static readonly uint ProcessIsProtected = 0xc0000712;
        public static readonly uint VolumeDirty = 0xc0000806;
        public static readonly uint FileCheckedOut = 0xc0000901;
        public static readonly uint CheckOutRequired = 0xc0000902;
        public static readonly uint BadFileType = 0xc0000903;
        public static readonly uint FileTooLarge = 0xc0000904;
        public static readonly uint FormsAuthRequired = 0xc0000905;
        public static readonly uint VirusInfected = 0xc0000906;
        public static readonly uint VirusDeleted = 0xc0000907;
        public static readonly uint TransactionalConflict = 0xc0190001;
        public static readonly uint InvalidTransaction = 0xc0190002;
        public static readonly uint TransactionNotActive = 0xc0190003;
        public static readonly uint TmInitializationFailed = 0xc0190004;
        public static readonly uint RmNotActive = 0xc0190005;
        public static readonly uint RmMetadataCorrupt = 0xc0190006;
        public static readonly uint TransactionNotJoined = 0xc0190007;
        public static readonly uint DirectoryNotRm = 0xc0190008;
        public static readonly uint CouldNotResizeLog = 0xc0190009;
        public static readonly uint TransactionsUnsupportedRemote = 0xc019000a;
        public static readonly uint LogResizeInvalidSize = 0xc019000b;
        public static readonly uint RemoteFileVersionMismatch = 0xc019000c;
        public static readonly uint CrmProtocolAlreadyExists = 0xc019000f;
        public static readonly uint TransactionPropagationFailed = 0xc0190010;
        public static readonly uint CrmProtocolNotFound = 0xc0190011;
        public static readonly uint TransactionSuperiorExists = 0xc0190012;
        public static readonly uint TransactionRequestNotValid = 0xc0190013;
        public static readonly uint TransactionNotRequested = 0xc0190014;
        public static readonly uint TransactionAlreadyAborted = 0xc0190015;
        public static readonly uint TransactionAlreadyCommitted = 0xc0190016;
        public static readonly uint TransactionInvalidMarshallBuffer = 0xc0190017;
        public static readonly uint CurrentTransactionNotValid = 0xc0190018;
        public static readonly uint LogGrowthFailed = 0xc0190019;
        public static readonly uint ObjectNoLongerExists = 0xc0190021;
        public static readonly uint StreamMiniversionNotFound = 0xc0190022;
        public static readonly uint StreamMiniversionNotValid = 0xc0190023;
        public static readonly uint MiniversionInaccessibleFromSpecifiedTransaction = 0xc0190024;
        public static readonly uint CantOpenMiniversionWithModifyIntent = 0xc0190025;
        public static readonly uint CantCreateMoreStreamMiniversions = 0xc0190026;
        public static readonly uint HandleNoLongerValid = 0xc0190028;
        public static readonly uint NoTxfMetadata = 0xc0190029;
        public static readonly uint LogCorruptionDetected = 0xc0190030;
        public static readonly uint CantRecoverWithHandleOpen = 0xc0190031;
        public static readonly uint RmDisconnected = 0xc0190032;
        public static readonly uint EnlistmentNotSuperior = 0xc0190033;
        public static readonly uint RecoveryNotNeeded = 0xc0190034;
        public static readonly uint RmAlreadyStarted = 0xc0190035;
        public static readonly uint FileIdentityNotPersistent = 0xc0190036;
        public static readonly uint CantBreakTransactionalDependency = 0xc0190037;
        public static readonly uint CantCrossRmBoundary = 0xc0190038;
        public static readonly uint TxfDirNotEmpty = 0xc0190039;
        public static readonly uint IndoubtTransactionsExist = 0xc019003a;
        public static readonly uint TmVolatile = 0xc019003b;
        public static readonly uint RollbackTimerExpired = 0xc019003c;
        public static readonly uint TxfAttributeCorrupt = 0xc019003d;
        public static readonly uint EfsNotAllowedInTransaction = 0xc019003e;
        public static readonly uint TransactionalOpenNotAllowed = 0xc019003f;
        public static readonly uint TransactedMappingUnsupportedRemote = 0xc0190040;
        public static readonly uint TxfMetadataAlreadyPresent = 0xc0190041;
        public static readonly uint TransactionScopeCallbacksNotSet = 0xc0190042;
        public static readonly uint TransactionRequiredPromotion = 0xc0190043;
        public static readonly uint CannotExecuteFileInTransaction = 0xc0190044;
        public static readonly uint TransactionsNotFrozen = 0xc0190045;

        public static readonly uint MaximumNtStatus = 0xffffffff;
    }
   
}