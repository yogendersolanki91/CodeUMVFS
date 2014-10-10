// DokanXNative.h
#include "include\dokan.h"
#include "include\fileinfo.h"
#include <map>
#include <vcclr.h>
#include <AccCtrl.h>
#include <AclAPI.h>
#include <sddl.h>

#pragma once
namespace INTROP = System::Runtime::InteropServices;

using namespace System;
using namespace System::IO;
using namespace System::Collections::Generic;
using namespace DokanXBase;
#define MarshalCopy(srcHandle,dstHandle,start,end) System::Runtime::InteropServices::Marshal::Copy(srcHandle, dstHandle, start,end);


namespace DokanXNative {

	const wchar_t* FileSystemName=L"Google";
	const wchar_t* VolumeName=L"Yogi";
	unsigned long FreeBytesAvailableV = 9999999999999;
	unsigned long TotalNumberOfBytesV = 9999999999999;
	unsigned long TotalNumberOfFreeBytesV=999999999999;
	DWORD SerialNumber = 0x19831116;
	gcroot<DokanXBase::FileSystemCoreFunctions^> ManagedFunction;
	//gcroot<Dictionary<unsigned long, DOKAN_FILE_INFO>> ManagedDokanContext =gcnew Dictionary<unsigned long,DOKAN_FILE_INFO>();;
	typedef std::map <unsigned long, DOKAN_FILE_INFO> ContextMap;
	typedef std::pair<unsigned long, DOKAN_FILE_INFO> ContextPair;
	ContextMap DokanContextMap;
	//gcroot<Object^> infoTableLock_ = gcnew Object();
	gcroot<System::Threading::Mutex ^>infoLock = gcnew System::Threading::Mutex();
	unsigned long infoId_ = 0;
	void SetDokanContext(DOKAN_FILE_INFO fileInfo, PDOKAN_FILE_INFO currentInfo){
		currentInfo->Context = fileInfo.Context;
		currentInfo->DeleteOnClose = fileInfo.DeleteOnClose;
		currentInfo->IsDirectory = fileInfo.IsDirectory;
		currentInfo->Nocache = fileInfo.Nocache;
		currentInfo->ProcessId = fileInfo.Nocache;
		currentInfo->SynchronousIo = fileInfo.SynchronousIo;
	}
	void AddNewFileInfo(PDOKAN_FILE_INFO rawFileInfo)
	{
		infoLock->WaitOne();
		rawFileInfo->Context = ++infoId_;
		DokanContextMap.insert(ContextPair(infoId_, *rawFileInfo));
		infoLock->ReleaseMutex();
	}
	void GetExistingInfo(PDOKAN_FILE_INFO rawFileInfo){
		if (rawFileInfo->Context <= 0){
			infoLock->WaitOne();
			DOKAN_FILE_INFO getFileInfo = DokanContextMap[rawFileInfo->Context];
			infoLock->ReleaseMutex();
			SetDokanContext(getFileInfo, rawFileInfo);
		//	return getFileInfo;
		}
		else
		{
			//return *rawFileInfo;
		}

	}
	NTSTATUS CreateFileOperation(LPCWSTR FileName, DWORD DesiredAccess, DWORD	ShareMode, DWORD	CreationDisposition, DWORD FlagsAndAttributes, PDOKAN_FILE_INFO DokanFile)
	{
		try
		{
			String^ File_Name = gcnew String(FileName);
			FileAccess access = FileAccess::Read;
			FileShare share = FileShare::None;
			FileMode mode = FileMode::Open;
			FileOptions options = FileOptions::None;
			if ((DesiredAccess & FILE_READ_DATA) != 0 && (DesiredAccess & FILE_WRITE_DATA) != 0)
			{
				access = FileAccess::ReadWrite;
			}
			else if ((DesiredAccess & FILE_WRITE_DATA) != 0)
			{
				access = FileAccess::Write;
			}
			else
			{
				access = FileAccess::Read;
			}

			if ((ShareMode & FILE_SHARE_READ) != 0)
			{
				share = FileShare::Read;
			}

			if ((ShareMode & FILE_SHARE_WRITE) != 0)
			{
				share = share | FileShare::Write;
			}

			if ((ShareMode & FILE_SHARE_DELETE) != 0)
			{
				share = share | FileShare::Delete;
			}

			if ((FlagsAndAttributes & FILE_FLAG_DELETE_ON_CLOSE) != 0)
			{
				options = options | FileOptions::DeleteOnClose;
			}

			if ((FlagsAndAttributes & FILE_FLAG_WRITE_THROUGH) != 0)
			{
				options = options | FileOptions::WriteThrough;
			}

			if ((FlagsAndAttributes & FILE_FLAG_SEQUENTIAL_SCAN) != 0)
			{
				options = options | FileOptions::SequentialScan;
			}

			if ((FlagsAndAttributes & FILE_FLAG_RANDOM_ACCESS) != 0)
			{
				options = options | FileOptions::RandomAccess;
			}

			if ((FlagsAndAttributes & FILE_FLAG_OVERLAPPED) != 0)
			{
				options = options | FileOptions::Asynchronous;
			}
			switch (CreationDisposition)
			{
			case CREATE_NEW:
				mode = FileMode::CreateNew;
				break;
			case CREATE_ALWAYS:
				mode = FileMode::Create;
				break;
			case OPEN_EXISTING:
				mode = FileMode::Open;
				break;
			case OPEN_ALWAYS:
				mode = FileMode::OpenOrCreate;
				break;
			case TRUNCATE_EXISTING:
				mode = FileMode::Truncate;
				break;
			}
			IntPtr ^x = gcnew IntPtr(DokanFile);
		   //info = GetNewFileInfo(ref rawFileInfo);
			AddNewFileInfo(DokanFile);
			ManagedFunction->Def_CreateFile(File_Name, access, share, mode, options, *gcnew IntPtr(DokanFile));
			return 0;// 
		}
		catch (Exception ^e)
		{
			Console::WriteLine(e->Message);
			return STATUS_FILE_INVALID;
		}
	}
	NTSTATUS CreateDirectoryOperation(LPCWSTR FileName, PDOKAN_FILE_INFO	DokanFile)
	{	
		try
		{
			//DokanXBase::DOKAN_FILE_INFO %PassDokan = *gcnew DokanXBase::DOKAN_FILE_INFO();
			AddNewFileInfo(DokanFile);
			return ManagedFunction->Def_CreateDirectory(gcnew String(FileName), *gcnew IntPtr(DokanFile));
		}
		catch (Exception ^e)
		{

			return STATUS_FILE_INVALID;
		}
	}
	NTSTATUS OpenDirectoryOperation(LPCWSTR	FileName, PDOKAN_FILE_INFO DokanFile)
	{
		try
		{
			//*DokanFile = GetExistingInfo(DokanFile);
		//	DokanXBase::DOKAN_FILE_INFO %PassDokan = *gcnew DokanXBase::DOKAN_FILE_INFO();
			return ManagedFunction->Def_OpenDirectory(gcnew String(FileName), *gcnew IntPtr(DokanFile));
		}
		catch (Exception ^e)
		{
			return STATUS_FILE_INVALID;
		}
	}

	void CloseFileOperation(LPCWSTR	FileName, PDOKAN_FILE_INFO DokanInfo)
	{
		try
		{
			//DokanInfo-> = GetExistingInfo(DokanInfo);
		//	DokanXBase::DOKAN_FILE_INFO %PassDokan = *gcnew DokanXBase::DOKAN_FILE_INFO();
			ManagedFunction->Def_CloseFile(gcnew String(FileName), *gcnew IntPtr(DokanInfo));
		}
		catch (Exception ^e)
		{
			//return STATUS_FILE_INVALID;
		}
	}

	void CleanupOperation(LPCWSTR FileName, PDOKAN_FILE_INFO	DokanFileInfo)
	{
		try
		{
			//*DokanFileInfo = GetExistingInfo(DokanFileInfo);
			//DokanXBase::DOKAN_FILE_INFO %PassDokan = *gcnew DokanXBase::DOKAN_FILE_INFO();
			ManagedFunction->Def_Cleanup(gcnew String(FileName), *gcnew IntPtr(DokanFileInfo));
		}
		catch (Exception ^e)
		{
			//return STATUS_FILE_INVALID;
		}
	}

	NTSTATUS ReadFileOperation(LPCWSTR FileName, LPVOID Buffer, DWORD	BufferLength, LPDWORD ReadLength, LONGLONG Offset, PDOKAN_FILE_INFO DokanFileInfo)
	{
		try
		{
		
			
			unsigned int readsuccess;
			//*DokanFileInfo = GetExistingInfo(DokanFileInfo);
			NTSTATUS ret=ManagedFunction->Def_ReadFile(gcnew String(FileName),*(new IntPtr(Buffer)),BufferLength,readsuccess, (long long)Offset, *gcnew IntPtr(DokanFileInfo));
			
			if (ret == 0){
			
			//	Console::WriteLine("size told " + readsuccess);
				///pin_ptr<byte> data_array_start = &buffer[0];				
				//memcpy(Buffer, data_array_start, Convert::ToUInt32(readsuccess));
				*ReadLength = readsuccess;
	

			}
			return ret;
		}
		catch (Exception ^e)
		{
			Console::WriteLine("File Read Error " + e->Message);
			return STATUS_FILE_INVALID;
		}
		
	}

	NTSTATUS WriteFileOperation(LPCWSTR	FileName, LPCVOID Buffer, DWORD NumberOfBytesToWrite, LPDWORD NumberOfBytesWritten, LONGLONG Offset, PDOKAN_FILE_INFO DokanFileInfo)
	{

		//try
		//{
		
			array < unsigned char, 1 >^% buffer = gcnew array<unsigned char, 1>(NumberOfBytesToWrite);
			//Console::WriteLine("*****Write FILe");
			pin_ptr<byte> data_array_start = &buffer[0];
			memcpy(data_array_start, Buffer, NumberOfBytesToWrite);
			unsigned int writesuccess;
			//*DokanFileInfo = GetExistingInfo(DokanFileInfo);
			NTSTATUS ret = ManagedFunction->Def_WriteFile(gcnew String(FileName), buffer,writesuccess, (long long)Offset, *gcnew IntPtr(DokanFileInfo));
			if (ret == 0){
				*NumberOfBytesWritten = writesuccess;
				delete buffer;
				return 0;
			}
			else
			{
				Console::WriteLine("Return Fail {0}", ret);
				delete buffer;
				return ret;
			}
		//}
		/*catch (Exception ^e)
		{
			Console::WriteLine("Write File Failed " +e->Message);
			return STATUS_FILE_INVALID;
		}*/
	}

	NTSTATUS GetFileInformationOperation(LPCWSTR FileName, LPBY_HANDLE_FILE_INFORMATION HandleFileInformation, PDOKAN_FILE_INFO DokanFileInfo)
	{
		try
		{
			//DokanXBase::DOKAN_FILE_INFO ^%PassDokan = gcnew DokanXBase::DOKAN_FILE_INFO();
			DokanXBase::BY_HANDLE_FILE_INFORMATION %info =*gcnew DokanXBase::BY_HANDLE_FILE_INFORMATION();	
			//*DokanFileInfo = GetExistingInfo(DokanFileInfo);
			NTSTATUS ret=ManagedFunction->Def_GetFileInformation(gcnew String(FileName),info, *gcnew IntPtr(DokanFileInfo));
			if (ret == 0)
			{
				pin_ptr<DokanXBase::BY_HANDLE_FILE_INFORMATION> data_start = &info;
				memcpy(HandleFileInformation, data_start, INTROP::Marshal::SizeOf(info));
				return 0;
			
			}
			else
			{
				//Console::WriteLine("Returning Bad " + ret);
				return ret;
			}
		}
		catch (Exception ^e)
		{
			Console::WriteLine(e->Message);
			return STATUS_FILE_INVALID;
		}

		
	}

	NTSTATUS FindFilesOperation(LPCWSTR	FileName, PFillFindData FillFindData, PDOKAN_FILE_INFO DokanFileInfo){
		try
		{
			//DokanXBase::DOKAN_FILE_INFO %PassDokan = *gcnew DokanXBase::DOKAN_FILE_INFO();
			//*DokanFileInfo = GetExistingInfo(DokanFileInfo);
			DokanXBase::BY_HANDLE_FILE_INFORMATION ^%PassHandleInfo = gcnew DokanXBase::BY_HANDLE_FILE_INFORMATION();
			IntPtr ^NativeFillPointer = gcnew IntPtr(FillFindData);
			DokanXBase::FillFindData ^% ManagedFillDelegate = dynamic_cast<DokanXBase::FillFindData^>(System::Runtime::InteropServices::Marshal::GetDelegateForFunctionPointer(*NativeFillPointer, DokanXBase::FillFindData::typeid));			
			return ManagedFunction->Def_FindFiles(gcnew String(FileName), ManagedFillDelegate, *gcnew IntPtr(DokanFileInfo));
		}
		catch (Exception ^e)
		{
			return STATUS_FILE_INVALID;
		}
	}

	NTSTATUS DeleteFileOperation(LPCWSTR FileName, PDOKAN_FILE_INFO	DokanFileInfo)
	{
		try
		{
			//DokanXBase::DOKAN_FILE_INFO %PassDokan = *gcnew DokanXBase::DOKAN_FILE_INFO();			
			//*DokanFileInfo = GetExistingInfo(DokanFileInfo);
			return ManagedFunction->Def_DeleteFile(gcnew String(FileName), *gcnew IntPtr(DokanFileInfo));
		}
		catch (Exception ^e)
		{
			return STATUS_FILE_INVALID;
		}
	}

	NTSTATUS DeleteDirectoryOperation(LPCWSTR FileName, PDOKAN_FILE_INFO	DokanFileInfo)
	{
		try
		{
			//DokanXBase::DOKAN_FILE_INFO %PassDokan = *gcnew DokanXBase::DOKAN_FILE_INFO();
			//*DokanFileInfo = GetExistingInfo(DokanFileInfo);
			return ManagedFunction->Def_DeleteDirectory(gcnew String(FileName), *gcnew IntPtr(DokanFileInfo));
		}
		catch (Exception ^e)
		{
			return STATUS_FILE_INVALID;
		}
	}

	NTSTATUS MoveFileOperation(LPCWSTR FileName, LPCWSTR NewFileName, BOOL ReplaceIfExisting, PDOKAN_FILE_INFO	DokanFileInfo)
	{
		try
		{
			//DokanXBase::DOKAN_FILE_INFO %PassDokan = *gcnew DokanXBase::DOKAN_FILE_INFO();
			bool x = ReplaceIfExisting;
			//*DokanFileInfo = GetExistingInfo(DokanFileInfo);
			return ManagedFunction->Def_MoveFile(gcnew String(FileName), gcnew String(NewFileName), ReplaceIfExisting, *gcnew IntPtr(DokanFileInfo));
		}
		catch (Exception ^e)
		{
			return STATUS_FILE_INVALID;
		}
	}

	NTSTATUS LockFileOperation(LPCWSTR FileName, LONGLONG ByteOffset, LONGLONG Length, PDOKAN_FILE_INFO DokanFileInfo)
	{
		try
		{
			//DokanXBase::DOKAN_FILE_INFO %PassDokan = *gcnew DokanXBase::DOKAN_FILE_INFO();
			//*DokanFileInfo = GetExistingInfo(DokanFileInfo);
			return ManagedFunction->Def_LockFile(gcnew String(FileName), ByteOffset, Length, *gcnew IntPtr(DokanFileInfo));
		}
		catch (Exception ^e)
		{
			return STATUS_FILE_INVALID;
		}
	}
	

	NTSTATUS SetEndOfFileOperation(LPCWSTR FileName, LONGLONG ByteOffset, PDOKAN_FILE_INFO DokanFileInfo)
	{
		try
		{
			//DokanXBase::DOKAN_FILE_INFO %PassDokan = *gcnew DokanXBase::DOKAN_FILE_INFO();
			//*DokanFileInfo = GetExistingInfo(DokanFileInfo);
			return ManagedFunction->Def_SetEndOfFile(gcnew String(FileName), ByteOffset, *gcnew IntPtr(DokanFileInfo));
		}
		catch (Exception ^e)
		{
			return STATUS_FILE_INVALID;
		}
	}

	NTSTATUS SetAllocationSizeOperation(LPCWSTR	FileName, LONGLONG AllocSize, PDOKAN_FILE_INFO DokanFileInfo)
	{
		try
		{
			//DokanXBase::DOKAN_FILE_INFO %PassDokan = *gcnew DokanXBase::DOKAN_FILE_INFO();
			//*DokanFileInfo = GetExistingInfo(DokanFileInfo);
			return ManagedFunction->Def_SetAllocationSize(gcnew String(FileName), AllocSize, *gcnew IntPtr(DokanFileInfo));
		}
		catch (Exception ^e)
		{
			return STATUS_FILE_INVALID;
		}
	}

	NTSTATUS SetFileAttributesOperation(LPCWSTR	 FileName, DWORD	FileAttributes, PDOKAN_FILE_INFO	DokanFileInfo)
	{
		try
		{
			//DokanXBase::DOKAN_FILE_INFO %PassDokan = *gcnew DokanXBase::DOKAN_FILE_INFO();
			//*DokanFileInfo = GetExistingInfo(DokanFileInfo);
			return ManagedFunction->Def_SetFileAttributes(gcnew String(FileName), FileAttributes, *gcnew IntPtr(DokanFileInfo));
		}
		catch (Exception ^e)
		{
			return STATUS_FILE_INVALID;
		}
	}

	NTSTATUS UnlockFileOperation(LPCWSTR FileName, LONGLONG ByteOffset, LONGLONG Length, PDOKAN_FILE_INFO DokanFileInfo)
	{

		try
		{
			//DokanXBase::DOKAN_FILE_INFO %PassDokan = *gcnew DokanXBase::DOKAN_FILE_INFO();
			//*DokanFileInfo = GetExistingInfo(DokanFileInfo);
			return ManagedFunction->Def_UnlockFile(gcnew String(FileName), ByteOffset, Length, *gcnew IntPtr(DokanFileInfo));
		}
		catch (Exception ^e)
		{
			return STATUS_FILE_INVALID;
		}
	}
	BOOL CreateMyDACL(SECURITY_ATTRIBUTES * pSA)
	{
		// Define the SDDL for the DACL. This example sets 
		// the following access:
		//     Built-in guests are denied all access.
		//     Anonymous logon is denied all access.
		//     Authenticated users are allowed 
		//     read/write/execute access.
		//     Administrators are allowed full control.
		// Modify these values as needed to generate the proper
		// DACL for your application. 
		TCHAR * szSD = TEXT("D:")       // Discretionary ACL
			TEXT("(D;OICI;GA;;;BG)")     // Deny access to 
			// built-in guests
			TEXT("(D;OICI;GA;;;AN)")     // Deny access to 
			// anonymous logon
			TEXT("(A;OICI;GRGWGX;;;AU)") // Allow 
			// read/write/execute 
			// to authenticated 
			// users
			TEXT("(A;OICI;GA;;;BA)");    // Allow full control 
		// to administrators

		if (NULL == pSA)
			return FALSE;

		return ConvertStringSecurityDescriptorToSecurityDescriptor(
			szSD,
			SDDL_REVISION_1,
			&(pSA->lpSecurityDescriptor),
			NULL);
	}
	NTSTATUS GetFileSecurityOperation(LPCWSTR FileName, PSECURITY_INFORMATION SecurityInformation, PSECURITY_DESCRIPTOR pSD, ULONG BufferLength, PULONG LengthNeeded, PDOKAN_FILE_INFO DokanFileInfo)
	{
		SECURITY_ATTRIBUTES  sa;
		sa.nLength = sizeof(SECURITY_ATTRIBUTES);
		sa.bInheritHandle = FALSE;
		if (!CreateMyDACL(&sa))
		{
			// Error encountered; generate message and exit.
			printf("Failed CreateMyDACL\n");
			return STATUS_INVALID_FILE_FOR_SECTION;
		}
		pSD= sa.lpSecurityDescriptor;
		*LengthNeeded = sa.nLength;

		return 0;
	}

	NTSTATUS SetFileSecurityOperation(LPCWSTR FileName, PSECURITY_INFORMATION SecurityInformation, PSECURITY_DESCRIPTOR SecurityDescriptor, ULONG SecurityDescriptorLength, PDOKAN_FILE_INFO DokanFileInfo)
	{
		////*DokanFileInfo = GetExistingInfo(DokanFileInfo);
		return STATUS_FILE_SYSTEM_LIMITATION;
		
	}

	NTSTATUS Unmount(PDOKAN_FILE_INFO DokanFileInfo)
	{
		
		return STATUS_SUCCESS;
	}
	const wchar_t* strToWchart(System::String ^str)
	{		
		pin_ptr<const wchar_t> convertedValue = PtrToStringChars(str);  // <-- #include <vcclr.h>
		const wchar_t *constValue = convertedValue;
		return constValue;
	}
	NTSTATUS GetDiskSpaceInfo(PULONGLONG FreeBytesAvailable, PULONGLONG TotalNumberOfBytes, PULONGLONG TotalNumberOfFreeBytes, PDOKAN_FILE_INFO Info){
		try
		{
			unsigned long long FreeByte;
			unsigned long long TotalByte;
			unsigned long long TotalAvail;
			if (ManagedFunction->Def_GetDiskInfo(TotalAvail, TotalByte, FreeByte) != 0){
				*FreeBytesAvailable = FreeBytesAvailableV;
				*TotalNumberOfBytes = TotalNumberOfBytesV;
				*TotalNumberOfFreeBytes = TotalNumberOfFreeBytesV;
				return STATUS_SUCCESS;
			}
			else
			{
				*FreeBytesAvailable = TotalAvail;
				*TotalNumberOfBytes = TotalByte;
				*TotalNumberOfFreeBytes = FreeByte;
				return STATUS_SUCCESS;


			}
		}
		catch (Exception ^e){
		
			*FreeBytesAvailable = FreeBytesAvailableV;
			*TotalNumberOfBytes = TotalNumberOfBytesV;
			*TotalNumberOfFreeBytes = TotalNumberOfFreeBytesV;
			return STATUS_SUCCESS;
		}
	
		//Console::WriteLine("Call");
		
	}

	NTSTATUS GetVolumeInformationOperation(LPWSTR VolumeNameBuffer, DWORD VolumeNameSize, LPDWORD VolumeSerialNumber, LPDWORD MaximumComponentLength, LPDWORD FileSystemFlags, LPWSTR FileSystemNameBuffer, DWORD FileSystemNameSize, PDOKAN_FILE_INFO	DokanFileInfo)
	{
		String ^VolumeNameT;// = gcnew String(" ");// gcnew String("");
		System::Text::StringBuilder ^VolumeNameBuilder = gcnew System::Text::StringBuilder();
		String ^FSName;
		unsigned int MyVolumeSerialNumber;
		unsigned int MyMaxCompLegnth;
		unsigned int MyFSfeatures;

		try
		{
			if (ManagedFunction->Def_GetVolumeInfo(*gcnew IntPtr(VolumeNameBuffer), static_cast<unsigned int>(VolumeNameSize), MyVolumeSerialNumber, MyMaxCompLegnth, MyFSfeatures, *gcnew IntPtr(FileSystemNameBuffer), static_cast<unsigned int>(FileSystemNameSize))!=0)
			{
				wcscpy_s(VolumeNameBuffer, VolumeNameSize / sizeof(WCHAR), VolumeName);

				*VolumeSerialNumber = SerialNumber;
				*MaximumComponentLength = 256;
				*FileSystemFlags = FILE_CASE_SENSITIVE_SEARCH |
					FILE_CASE_PRESERVED_NAMES |
					FILE_SUPPORTS_REMOTE_STORAGE |
					FILE_UNICODE_ON_DISK |
					FILE_PERSISTENT_ACLS;
				wcscpy_s(FileSystemNameBuffer, FileSystemNameSize / sizeof(WCHAR), FileSystemName);
				
				return STATUS_SUCCESS;
				
				
			}
			else{
				*VolumeSerialNumber = MyVolumeSerialNumber;
				*MaximumComponentLength = MyMaxCompLegnth;
				*FileSystemFlags = MyFSfeatures;

				return STATUS_SUCCESS;
			}
		}
		catch (Exception ^e)
		{
			return STATUS_FILE_INVALID;
		}
		
		
	}
	public ref class DokanNative
	{
		
	PDOKAN_OPERATIONS NativeOperation=new DOKAN_OPERATIONS();
	PDOKAN_OPTIONS dokanOptions = new DOKAN_OPTIONS();
	public:
		DokanNative(FileSystemCoreFunctions^ functionDelegates,USHORT threadCount,String^ MountPoint,String^ FSName,String^ VolName){
			ZeroMemory(NativeOperation, sizeof(DOKAN_OPERATIONS));
			ZeroMemory(dokanOptions, sizeof(DOKAN_OPTIONS));
			dokanOptions->Version = DOKAN_VERSION;
			dokanOptions->ThreadCount = threadCount;
			dokanOptions->Options = DOKAN_OPTION_KEEP_ALIVE;
			pin_ptr<const wchar_t> convertedValue = PtrToStringChars(MountPoint);
			const wchar_t* constValue = convertedValue;
			dokanOptions->MountPoint = constValue;
			ManagedFunction = functionDelegates;

			NativeOperation->CreateFileW = CreateFileOperation;
			
			NativeOperation->OpenDirectory = OpenDirectoryOperation;
		
			NativeOperation->CreateDirectory = CreateDirectoryOperation;
			
			NativeOperation->Cleanup = CleanupOperation; 
			
			NativeOperation->CloseFile = CloseFileOperation;
			
			NativeOperation->ReadFile = ReadFileOperation; 
			
			NativeOperation->WriteFile = WriteFileOperation;
		
			NativeOperation->FlushFileBuffers = nullptr;
			
			NativeOperation->GetFileInformation = GetFileInformationOperation;
			
			NativeOperation->FindFiles = FindFilesOperation;
			
			NativeOperation->SetEndOfFile = SetEndOfFileOperation;
			
			NativeOperation->SetAllocationSize = SetAllocationSizeOperation;
			
			NativeOperation->SetFileAttributesW = SetFileAttributesOperation;
			
			NativeOperation->SetFileTime = nullptr;
			
			NativeOperation->DeleteFile = DeleteFileOperation;
			
			NativeOperation->DeleteDirectory = DeleteDirectoryOperation;
			
			NativeOperation->MoveFile = MoveFileOperation;
			
			NativeOperation->LockFile = LockFileOperation;
			
			NativeOperation->UnlockFile = UnlockFileOperation;
		
			NativeOperation->GetVolumeInformationW =  GetVolumeInformationOperation;
			
			NativeOperation->GetDiskFreeSpaceW =  GetDiskSpaceInfo;
		
			NativeOperation->Unmount = Unmount;
		
			NativeOperation->GetFileSecurityW = GetFileSecurityOperation;
			
			NativeOperation->SetFileSecurityW = SetFileSecurityOperation;

			
		}
		void StartDokan(){
			//NTSTATUS(*pFunc)(LPWSTR, DWORD, LPDWORD, LPDWORD, LPDWORD, LPWSTR, DWORD, PDOKAN_FILE_INFO) = &DokanNative::GetVolumeInformationOperation;
			//NativeOperation->GetVolumeInformationW =&(MyOP->GetVolumeInformationOperation);
			DokanMain(dokanOptions, NativeOperation);
		}
		void StopDokan(){
		 DokanUnmount(dokanOptions->MountPoint[0]);
			
		}
		void UpdateDiskRelatedInfo(UINT64 FreeBytesAvailable, UInt64 TotalNumberOfBytes, UInt64 TotalNumberOfFreeBytes,UInt32 SerialNumberV){
			FreeBytesAvailableV = FreeBytesAvailable;
			TotalNumberOfBytesV = TotalNumberOfBytes;
			TotalNumberOfFreeBytesV = TotalNumberOfFreeBytes;
			SerialNumber = SerialNumberV;
		}
	};
}
