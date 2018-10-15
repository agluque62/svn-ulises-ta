using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

using NLog;


///  <summary>
///  For communicating with HID-class USB devices.
///  The ReportIn class handles Input reports and Feature reports that carry data to the host.
///  The ReportOut class handles Output reports and Feature reports that that carry data to the device.
///  Other routines retrieve information about and configure the HID.
///  </summary>
///  

namespace GenericHid
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FileIO
    {
        public const Int32 FILE_FLAG_OVERLAPPED = 0X40000000;
        public const Int16 FILE_SHARE_READ = 0X1;
        public const Int16 FILE_SHARE_WRITE = 0X2;
        public const Int32 GENERIC_READ = unchecked((int)0X80000000);
        public const Int32 GENERIC_WRITE = 0X40000000;
        public const Int32 INVALID_HANDLE_VALUE = -1;
        public const Int16 OPEN_EXISTING = 3;
        public const Int32 WAIT_TIMEOUT = 0X102;
        public const Int16 WAIT_OBJECT_0 = 0;

        [StructLayout(LayoutKind.Sequential)]
        public class OVERLAPPED
        {
            public Int32 Internal;
            public Int32 InternalHigh;
            public Int32 Offset;
            public Int32 OffsetHigh;
            public SafeWaitHandle hEvent;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class SECURITY_ATTRIBUTES
        {
            public Int32 nLength;
            public Int32 lpSecurityDescriptor;
            public Int32 bInheritHandle;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Int32 CancelIo(SafeFileHandle hFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeWaitHandle CreateEvent(SECURITY_ATTRIBUTES SecurityAttributes, Int32 bManualReset, Int32 bInitialState, String lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(String lpFileName, Int32 dwDesiredAccess, Int32 dwShareMode, SECURITY_ATTRIBUTES lpSecurityAttributes, Int32 dwCreationDisposition, Int32 dwFlagsAndAttributes, Int32 hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Int32 ReadFile(SafeFileHandle hFile, Byte[] lpBuffer, Int32 nNumberOfBytesToRead, ref Int32 lpNumberOfBytesRead, OVERLAPPED lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Int32 WaitForSingleObject(SafeWaitHandle hHandle, Int32 dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Boolean WriteFile(SafeFileHandle hFile, Byte[] lpBuffer, Int32 nNumberOfBytesToWrite, ref Int32 lpNumberOfBytesWritten, OVERLAPPED lpOverlapped);
    }

    public sealed partial class Hid
    {
        //  API declarations for HID communications.

        //  from hidpi.h
        //  Typedef enum defines a set of integer constants for HidP_Report_Type

        public const Int16 HidP_Input = 0;
        public const Int16 HidP_Output = 1;
        public const Int16 HidP_Feature = 2;

        [StructLayout(LayoutKind.Sequential)]
        public struct HIDD_ATTRIBUTES
        {
            public Int32 Size;
            public Int16 VendorID;
            public Int16 ProductID;
            public Int16 VersionNumber;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HIDP_CAPS
        {
            public Int16 Usage;
            public Int16 UsagePage;
            public Int16 InputReportByteLength;
            public Int16 OutputReportByteLength;
            public Int16 FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public Int16[] Reserved;
            public Int16 NumberLinkCollectionNodes;
            public Int16 NumberInputButtonCaps;
            public Int16 NumberInputValueCaps;
            public Int16 NumberInputDataIndices;
            public Int16 NumberOutputButtonCaps;
            public Int16 NumberOutputValueCaps;
            public Int16 NumberOutputDataIndices;
            public Int16 NumberFeatureButtonCaps;
            public Int16 NumberFeatureValueCaps;
            public Int16 NumberFeatureDataIndices;
        }

        //  If IsRange is false, UsageMin is the Usage and UsageMax is unused.
        //  If IsStringRange is false, StringMin is the String index and StringMax is unused.
        //  If IsDesignatorRange is false, DesignatorMin is the designator index and DesignatorMax is unused.

        [StructLayout(LayoutKind.Sequential)]
        public struct HidP_Value_Caps
        {
            public Int16 UsagePage;
            public Byte ReportID;
            public Int32 IsAlias;
            public Int16 BitField;
            public Int16 LinkCollection;
            public Int16 LinkUsage;
            public Int16 LinkUsagePage;
            public Int32 IsRange;
            public Int32 IsStringRange;
            public Int32 IsDesignatorRange;
            public Int32 IsAbsolute;
            public Int32 HasNull;
            public Byte Reserved;
            public Int16 BitSize;
            public Int16 ReportCount;
            public Int16 Reserved2;
            public Int16 Reserved3;
            public Int16 Reserved4;
            public Int16 Reserved5;
            public Int16 Reserved6;
            public Int32 LogicalMin;
            public Int32 LogicalMax;
            public Int32 PhysicalMin;
            public Int32 PhysicalMax;
            public Int16 UsageMin;
            public Int16 UsageMax;
            public Int16 StringMin;
            public Int16 StringMax;
            public Int16 DesignatorMin;
            public Int16 DesignatorMax;
            public Int16 DataIndexMin;
            public Int16 DataIndexMax;
        }

        [DllImport("hid.dll", SetLastError = true)]
        public static extern Boolean HidD_FlushQueue(SafeFileHandle HidDeviceObject);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern Boolean HidD_FreePreparsedData(ref IntPtr PreparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern Boolean HidD_GetAttributes(SafeFileHandle HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern Boolean HidD_GetFeature(SafeFileHandle HidDeviceObject, ref Byte lpReportBuffer, Int32 ReportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern Boolean HidD_GetInputReport(SafeFileHandle HidDeviceObject, ref Byte lpReportBuffer, Int32 ReportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern void HidD_GetHidGuid(ref System.Guid HidGuid);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern Boolean HidD_GetNumInputBuffers(SafeFileHandle HidDeviceObject, ref Int32 NumberBuffers);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern Boolean HidD_GetPreparsedData(SafeFileHandle HidDeviceObject, ref IntPtr PreparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern Boolean HidD_SetFeature(SafeFileHandle HidDeviceObject, ref Byte lpReportBuffer, Int32 ReportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern Boolean HidD_SetNumInputBuffers(SafeFileHandle HidDeviceObject, Int32 NumberBuffers);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern Boolean HidD_SetOutputReport(SafeFileHandle HidDeviceObject, ref Byte lpReportBuffer, Int32 ReportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern Int32 HidP_GetCaps(IntPtr PreparsedData, ref HIDP_CAPS Capabilities);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern Int32 HidP_GetValueCaps(Int16 ReportType, ref Byte ValueCaps, ref Int16 ValueCapsLength, IntPtr PreparsedData);
        /** 20180409. Para obtener la version del FIRMWARE*/
        [DllImport("hid.dll", SetLastError = true)]
        private static extern Boolean HidD_GetSerialNumberString(SafeFileHandle HidDeviceObject, ref byte lpReportBuffer, int reportBufferLength);
        public static string GetSerialNumberString(SafeFileHandle hidHandle)
        {
            byte[] data = new byte[254];
            bool success = false;
            try
            {
                success = HidD_GetSerialNumberString(hidHandle, ref data[0], data.Length);
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format("Error accessing HID device '{0}'.", hidHandle), exception);
            }
            finally
            {
            }
            return success ? System.Text.Encoding.Unicode.GetString(data) : "No Serial Number";
        }
    }   

    /// <summary>
    /// 
    /// </summary>
    public sealed partial class Hid  
    {         
        //  Used in error messages.
        
        private const String MODULE_NAME = "Hid";

        public HIDP_CAPS Capabilities;
        public HIDD_ATTRIBUTES DeviceAttributes; 
        
       
        ///  <summary>
        ///  For reports the device sends to the host.
        ///  </summary>

        public abstract class ReportIn  
        {             
            ///  <summary>
            ///  Each class that handles reading reports defines a Read method for reading 
            ///  a type of report. Read is declared as a Sub rather
            ///  than as a Function because asynchronous reads use a callback method 
            ///  that can access parameters passed by ByRef but not Function return values.
            ///  </summary>

            public abstract void Read(SafeFileHandle hidHandle, SafeFileHandle readHandle, SafeFileHandle writeHandle, ref Boolean myDeviceDetected, ref Byte[] readBuffer, ref Boolean success);           
         }      
        
        ///  <summary>
        ///  For reading Feature reports.
        ///  </summary>

        public class InFeatureReport : ReportIn 
        {             
            ///  <summary>
            ///  reads a Feature report from the device.
            ///  </summary>
            ///  
            ///  <param name="hidHandle"> the handle for learning about the device and exchanging Feature reports. </param>
            ///  <param name="readHandle"> the handle for reading Input reports from the device. </param>
            ///  <param name="writeHandle"> the handle for writing Output reports to the device. </param>
            ///  <param name="myDeviceDetected"> tells whether the device is currently attached.</param>
            ///  <param name="inFeatureReportBuffer"> contains the requested report.</param>
            ///  <param name="success"> read success</param>

            public override void Read(SafeFileHandle hidHandle, SafeFileHandle readHandle, SafeFileHandle writeHandle, ref Boolean myDeviceDetected, ref Byte[] inFeatureReportBuffer, ref Boolean success) 
            {                 
                try 
                { 
                    //  ***
                    //  API function: HidD_GetFeature
                    //  Attempts to read a Feature report from the device.
                    
                    //  Requires:
                    //  A handle to a HID
                    //  A pointer to a buffer containing the report ID and report
                    //  The size of the buffer. 
                    
                    //  Returns: true on success, false on failure.
                    //  ***                    
                   
                    success = HidD_GetFeature(hidHandle, ref inFeatureReportBuffer[0], inFeatureReportBuffer.Length); 
                                        
                    Debug.Print( "HidD_GetFeature success = " + success );                     
                } 
                catch ( Exception ex ) 
                { 
                    DisplayException( MODULE_NAME, ex ); 
                    throw ; 
                }                 
            }             
        }         
        
        ///  <summary>
        ///  For reading Input reports via control transfers
        ///  </summary>

        public class InputReportViaControlTransfer : ReportIn 
        {             
            ///  <summary>
            ///  reads an Input report from the device using a control transfer.
            ///  </summary>
            ///  
            ///  <param name="hidHandle"> the handle for learning about the device and exchanging Feature reports. </param>
            ///  <param name="readHandle"> the handle for reading Input reports from the device. </param>
            ///  <param name="writeHandle"> the handle for writing Output reports to the device. </param>
            ///  <param name="myDeviceDetected"> tells whether the device is currently attached. </param>
            ///  <param name="inputReportBuffer"> contains the requested report. </param>
            ///  <param name="success"> read success </param>

            public override void Read(SafeFileHandle hidHandle, SafeFileHandle readHandle, SafeFileHandle writeHandle, ref Boolean myDeviceDetected, ref Byte[] inputReportBuffer, ref Boolean success) 
            {                 
                try 
                {                     
                    //  ***
                    //  API function: HidD_GetInputReport
                    
                    //  Purpose: Attempts to read an Input report from the device using a control transfer.
                    //  Supported under Windows XP and later only.
                    
                    //  Requires:
                    //  A handle to a HID
                    //  A pointer to a buffer containing the report ID and report
                    //  The size of the buffer. 
                    
                    //  Returns: true on success, false on failure.
                    //  ***
                    
                    success = HidD_GetInputReport(hidHandle, ref inputReportBuffer[0], inputReportBuffer.Length + 1); 
                    
                    Debug.Print( "HidD_GetInputReport success = " + success );                     
                } 
                catch ( Exception ex ) 
                { 
                    DisplayException( MODULE_NAME, ex ); 
                    throw ; 
                }                 
            }      
        }         
        
        ///  <summary>
        ///  For reading Input reports.
        ///  </summary>

        public class InputReportViaInterruptTransfer : ReportIn 
        {             
            public Boolean readyForOverlappedTransfer; //  initialize to false
            
            ///  <summary>
            ///  closes open handles to a device.
            ///  </summary>
            ///  
            ///  <param name="hidHandle"> the handle for learning about the device and exchanging Feature reports. </param>
            ///  <param name="readHandle"> the handle for reading Input reports from the device. </param>
            ///  <param name="writeHandle"> the handle for writing Output reports to the device. </param>

            public void CancelTransfer(SafeFileHandle hidHandle, SafeFileHandle readHandle, SafeFileHandle writeHandle) 
            {                 
                try 
                { 
                    //  ***
                    //  API function: CancelIo
                    
                    //  Purpose: Cancels a call to ReadFile
                    
                    //  Accepts: the device handle.
                    
                    //  Returns: True on success, False on failure.
                    //  ***
                    
                    FileIO.CancelIo(readHandle);               
                                        
                    /** TODO DEB
                    Debug.WriteLine( "************ReadFile error*************" ); 
                    String functionName = "CancelIo";
                    Debug.WriteLine(MyDebugging.ResultOfAPICall(functionName)); 
                    Debug.WriteLine( "" ); 
                     * */
                    LogManager.GetCurrentClassLogger().Debug("ReadFile error. CancelIo => ");
                    
                    //  The failure may have been because the device was removed,
                    //  so close any open handles and
                    //  set myDeviceDetected=False to cause the application to
                    //  look for the device on the next attempt.
                    
                    if ( ( !( hidHandle.IsInvalid ) ) ) 
                    { 
                        hidHandle.Close(); 
                    } 
                    
                    if ( ( !( readHandle.IsInvalid ) ) ) 
                    { 
                        readHandle.Close(); 
                    } 
                    
                    if ( ( !( writeHandle.IsInvalid ) ) ) 
                    { 
                        writeHandle.Close(); 
                    }                     
                } 
                catch ( Exception ex ) 
                { 
                    DisplayException( MODULE_NAME, ex ); 
                    throw ; 
                }                 
            }             
            
            ///  <summary>
            ///  Creates an event object for the overlapped structure used with 
            ///  ReadFile. Called before the first call to ReadFile.
            ///  </summary>
            ///  
            ///  <param name="hidOverlapped"> the overlapped structure </param>
            ///  <param name="eventObject"> the event object </param>

            public void PrepareForOverlappedTransfer(ref FileIO.OVERLAPPED hidOverlapped, ref SafeWaitHandle eventObject) 
            {                 
                try 
                { 
                    //  ***
                    //  API function: CreateEvent
                    
                    //  Purpose: Creates an event object for the overlapped structure used with ReadFile.
                    
                    //  Accepts:
                    //  A security attributes structure.
                    //  Manual Reset = False (The system automatically resets the state to nonsignaled 
                    //  after a waiting thread has been released.)
                    //  Initial state = True (signaled)
                    //  An event object name (optional)
                    
                    //  Returns: a handle to the event object
                    //  ***

                    eventObject = FileIO.CreateEvent(null, System.Convert.ToInt32(false), System.Convert.ToInt32(true), ""); 
                    
                    //  Debug.WriteLine(MyDebugging.ResultOfAPICall("CreateEvent"))
                    
                    //  Set the members of the overlapped structure.
                    
                    hidOverlapped.Offset = 0; 
                    hidOverlapped.OffsetHigh = 0; 
                    hidOverlapped.hEvent = eventObject; 
                    readyForOverlappedTransfer = true;                     
                } 
                catch ( Exception ex ) 
                { 
                    DisplayException( MODULE_NAME, ex ); 
                    throw ; 
                }                 
            }             
            
            ///  <summary>
            ///  reads an Input report from the device using interrupt transfers.
            ///  </summary>
            ///  
            ///  <param name="hidHandle"> the handle for learning about the device and exchanging Feature reports. </param>
            ///  <param name="readHandle"> the handle for reading Input reports from the device. </param>
            ///  <param name="writeHandle"> the handle for writing Output reports to the device. </param>
            ///  <param name="myDeviceDetected"> tells whether the device is currently attached. </param>
            ///  <param name="inputReportBuffer"> contains the requested report. </param>
            ///  <param name="success"> read success </param>

            public override void Read(SafeFileHandle hidHandle, SafeFileHandle readHandle, SafeFileHandle writeHandle, ref Boolean myDeviceDetected, ref Byte[] inputReportBuffer, ref Boolean success) 
            {                 
                SafeWaitHandle eventObject = null;
                FileIO.OVERLAPPED HidOverlapped = new FileIO.OVERLAPPED(); 
                Int32 numberOfBytesRead = 0; 
                Int32 result = 0;                 
                try 
                { 
                    //  If it's the first attempt to read, set up the overlapped structure for ReadFile.
                    
                    if ( readyForOverlappedTransfer == false ) 
                    { 
                        PrepareForOverlappedTransfer( ref HidOverlapped, ref eventObject ); 
                    } 
                    
                    //  ***
                    //  API function: ReadFile
                    //  Purpose: Attempts to read an Input report from the device.
                    
                    //  Accepts:
                    //  A device handle returned by CreateFile
                    //  (for overlapped I/O, CreateFile must have been called with FILE_FLAG_OVERLAPPED),
                    //  A pointer to a buffer for storing the report.
                    //  The Input report length in bytes returned by HidP_GetCaps,
                    //  A pointer to a variable that will hold the number of bytes read. 
                    //  An overlapped structure whose hEvent member is set to an event object.
                    
                    //  Returns: the report in ReadBuffer.
                    
                    //  The overlapped call returns immediately, even if the data hasn't been received yet.
                    
                    //  To read multiple reports with one ReadFile, increase the size of ReadBuffer
                    //  and use NumberOfBytesRead to determine how many reports were returned.
                    //  Use a larger buffer if the application can't keep up with reading each report
                    //  individually. 
                    //  ***                    
                   
                    result = FileIO.ReadFile(readHandle, inputReportBuffer, inputReportBuffer.Length, ref numberOfBytesRead, HidOverlapped); 
                    
                    Debug.WriteLine( "waiting for ReadFile" ); 
                    
                    //  API function: WaitForSingleObject
                    
                    //  Purpose: waits for at least one report or a timeout.
                    //  Used with overlapped ReadFile.
                    
                    //  Accepts:
                    //  An event object created with CreateEvent
                    //  A timeout value in milliseconds.
                    
                    //  Returns: A result code.

                    result = FileIO.WaitForSingleObject(eventObject, 3000); 
                    
                    //  Find out if ReadFile completed or timeout.
                    
                    switch ( result ) 
                    {
                        case (System.Int32)FileIO.WAIT_OBJECT_0:
                            
                            //  ReadFile has completed
                            
                            success = true; 
                            Debug.WriteLine( "ReadFile completed successfully." ); 
                            break;
                        case FileIO.WAIT_TIMEOUT:
                            
                            //  Cancel the operation on timeout
                            
                            CancelTransfer( hidHandle, readHandle, writeHandle ); 
                            Debug.WriteLine( "Readfile timeout" ); 
                            success = false; 
                            myDeviceDetected = false; 
                            break;
                        default:
                            
                            //  Cancel the operation on other error.
                            
                            CancelTransfer( hidHandle, readHandle, writeHandle ); 
                            Debug.WriteLine( "Readfile undefined error" ); 
                            success = false; 
                            myDeviceDetected = false; 
                            break;
                    }                    
                    
                    if ( result == 0 ) 
                    { 
                        success = true; 
                    } 
                    else 
                    { 
                        success = false; 
                    }                    
                } 
                catch ( Exception ex ) 
                { 
                    DisplayException( MODULE_NAME, ex ); 
                    throw ; 
                }                 
            }             
        } 
                
        ///  <summary>
        ///  For reports the host sends to the device.
        ///  </summary>

        public abstract class ReportOut  
        {            
            ///  <summary>
            ///  Each class that handles writing reports defines a Write method for 
            ///  writing a type of report.
            ///  </summary>
            ///  
            ///  <param name="reportBuffer"> contains the report ID and report data. </param>
            ///   <param name="deviceHandle"> handle to the device.  </param>
            ///  
            ///  <returns>
            ///   True on success. False on failure.
            ///  </returns>             

            public abstract Boolean Write(Byte[] reportBuffer, SafeFileHandle deviceHandle);      
        } 
        
        ///  <summary>
        ///  For Feature reports the host sends to the device.
        ///  </summary>
        
        public class OutFeatureReport : ReportOut 
        {            
            ///  <summary>
            ///  writes a Feature report to the device.
            ///  </summary>
            ///  
            ///  <param name="outFeatureReportBuffer"> contains the report ID and report data. </param>
            ///  <param name="hidHandle"> handle to the device.  </param>
            ///  
            ///  <returns>
            ///   True on success. False on failure.
            ///  </returns>            

            public override Boolean Write(Byte[] outFeatureReportBuffer, SafeFileHandle hidHandle) 
            {                 
                Boolean success = false; 
                
                try 
                { 
                    //  ***
                    //  API function: HidD_SetFeature
                    
                    //  Purpose: Attempts to send a Feature report to the device.
                    
                    //  Accepts:
                    //  A handle to a HID
                    //  A pointer to a buffer containing the report ID and report
                    //  The size of the buffer. 
                    
                    //  Returns: true on success, false on failure.
                    //  ***
                                      
                    success = HidD_SetFeature(hidHandle, ref outFeatureReportBuffer[0], outFeatureReportBuffer.Length); 
                    
                    Debug.Print( "HidD_SetFeature success = " + success ); 
                    
                    return success;                     
                } 
                catch ( Exception ex ) 
                { 
                    DisplayException( MODULE_NAME, ex ); 
                    throw ; 
                }                 
            }             
        } 
                
        ///  <summary>
        ///  For writing Output reports via control transfers
        ///  </summary>
        
        public class OutputReportViaControlTransfer : ReportOut 
        {             
            ///  <summary>
            ///  writes an Output report to the device using a control transfer.
            ///  </summary>
            ///  
            ///  <param name="outputReportBuffer"> contains the report ID and report data. </param>
            ///  <param name="hidHandle"> handle to the device.  </param>
            ///  
            ///  <returns>
            ///   True on success. False on failure.
            ///  </returns>            

            public override Boolean Write(Byte[] outputReportBuffer, SafeFileHandle hidHandle) 
            {                 
                Boolean success = false; 
                
                try 
                { 
                    //  ***
                    //  API function: HidD_SetOutputReport
                    
                    //  Purpose: 
                    //  Attempts to send an Output report to the device using a control transfer.
                    //  Requires Windows XP or later.
                    
                    //  Accepts:
                    //  A handle to a HID
                    //  A pointer to a buffer containing the report ID and report
                    //  The size of the buffer. 
                    
                    //  Returns: true on success, false on failure.
                    //  ***                    
                   
                    success = HidD_SetOutputReport(hidHandle, ref outputReportBuffer[0], outputReportBuffer.Length + 1); 
                    
                    Debug.Print( "HidD_SetOutputReport success = " + success ); 
                    
                    return success;                     
                } 
                catch ( Exception ex ) 
                { 
                    DisplayException( MODULE_NAME, ex ); 
                    throw ; 
                }                 
            }           
        }       
        
        ///  <summary>
        ///  For Output reports the host sends to the device.
        ///  Uses interrupt or control transfers depending on the device and OS.
        ///  </summary>
        
        public class OutputReportViaInterruptTransfer : ReportOut 
        {             
            ///  <summary>
            ///  writes an Output report to the device.
            ///  </summary>
            ///  
            ///  <param name="outputReportBuffer"> contains the report ID and report data. </param>
            ///  <param name="writeHandle"> handle to the device.  </param>
            ///  
            ///  <returns>
            ///   True on success. False on failure.
            ///  </returns>            

            public override Boolean Write(Byte[] outputReportBuffer, SafeFileHandle writeHandle) 
            {                 
                Int32 numberOfBytesWritten = 0; 
                Boolean success = false; 
                
                try 
                { 
                    //  The host will use an interrupt transfer if the the HID has an interrupt OUT
                    //  endpoint (requires USB 1.1 or later) AND the OS is NOT Windows 98 Gold (original version). 
                    //  Otherwise the the host will use a control transfer.
                    //  The application doesn't have to know or care which type of transfer is used.
                    
                    numberOfBytesWritten = 0; 
                    
                    //  ***
                    //  API function: WriteFile
                    
                    //  Purpose: writes an Output report to the device.
                    
                    //  Accepts:
                    //  A handle returned by CreateFile
                    //  An integer to hold the number of bytes written.
                    
                    //  Returns: True on success, False on failure.
                    //  ***
                    
                    success = FileIO.WriteFile(writeHandle, outputReportBuffer, outputReportBuffer.Length, ref numberOfBytesWritten, null);
                    
                    Debug.Print( "WriteFile success = " + success ); 
                    
                    if ( !( ( success ) ) ) 
                    { 
                        
                        if ( ( !( writeHandle.IsInvalid ) ) ) 
                        { 
                            writeHandle.Close(); 
                        } 
                    }                     
                    return success;                     
                } 
                catch ( Exception ex ) 
                { 
                    DisplayException( MODULE_NAME, ex ); 
                    throw ; 
                }                 
            }           
        } 
     
        ///  <summary>
        ///  Remove any Input reports waiting in the buffer.
        ///  </summary>
        ///  
        ///  <param name="hidHandle"> a handle to a device.   </param>
        ///  
        ///  <returns>
        ///  True on success, False on failure.
        ///  </returns>

        public Boolean FlushQueue(SafeFileHandle hidHandle) 
        {             
            Boolean success = false; 
            
            try 
            { 
                //  ***
                //  API function: HidD_FlushQueue
                
                //  Purpose: Removes any Input reports waiting in the buffer.
                
                //  Accepts: a handle to the device.
                
                //  Returns: True on success, False on failure.
                //  ***
                
                success = HidD_FlushQueue( hidHandle ); 
                
                return success;                 
            } 
            catch ( Exception ex ) 
            { 
                DisplayException( MODULE_NAME, ex ); 
                throw ; 
            }             
        }         
        
        ///  <summary>
        ///  Retrieves a structure with information about a device's capabilities. 
        ///  </summary>
        ///  
        ///  <param name="hidHandle"> a handle to a device. </param>
        ///  
        ///  <returns>
        ///  An HIDP_CAPS structure.
        ///  </returns>

        public HIDP_CAPS GetDeviceCapabilities(SafeFileHandle hidHandle) 
        {             
            Byte[] preparsedDataBytes = new Byte[ 30 ]; 
            String preparsedDataString = null; 
            IntPtr preparsedDataPointer = new System.IntPtr(); 
            Int32 result = 0; 
            Boolean success = false; 
            Byte[] valueCaps = new Byte[ 1024 ]; // (the array size is a guess)
            
            try 
            {                 
                //  ***
                //  API function: HidD_GetPreparsedData
                
                //  Purpose: retrieves a pointer to a buffer containing information about the device's capabilities.
                //  HidP_GetCaps and other API functions require a pointer to the buffer.
                
                //  Requires: 
                //  A handle returned by CreateFile.
                //  A pointer to a buffer.
                
                //  Returns:
                //  True on success, False on failure.
                //  ***
                
                success = HidD_GetPreparsedData( hidHandle, ref preparsedDataPointer ); 
                
                //  Copy the data at PreparsedDataPointer into a byte array.
                
                preparsedDataString = Convert.ToBase64String( preparsedDataBytes ); 
                
                //  ***
                //  API function: HidP_GetCaps
                
                //  Purpose: find out a device's capabilities.
                //  For standard devices such as joysticks, you can find out the specific
                //  capabilities of the device.
                //  For a custom device where the software knows what the device is capable of,
                //  this call may be unneeded.
                
                //  Accepts:
                //  A pointer returned by HidD_GetPreparsedData
                //  A pointer to a HIDP_CAPS structure.
                
                //  Returns: True on success, False on failure.
                //  ***
                
                result = HidP_GetCaps( preparsedDataPointer, ref Capabilities ); 
                if ( ( result != 0 ) ) 
                {                     
                    Debug.WriteLine( "" );          
                    Debug.WriteLine("  Usage: " + Convert.ToString(Capabilities.Usage, 16));
                    Debug.WriteLine("  Usage Page: " + Convert.ToString(Capabilities.UsagePage, 16)); 
                    Debug.WriteLine( "  Input Report Byte Length: " + Capabilities.InputReportByteLength ); 
                    Debug.WriteLine( "  Output Report Byte Length: " + Capabilities.OutputReportByteLength ); 
                    Debug.WriteLine( "  Feature Report Byte Length: " + Capabilities.FeatureReportByteLength ); 
                    Debug.WriteLine( "  Number of Link Collection Nodes: " + Capabilities.NumberLinkCollectionNodes ); 
                    Debug.WriteLine( "  Number of Input Button Caps: " + Capabilities.NumberInputButtonCaps ); 
                    Debug.WriteLine( "  Number of Input Value Caps: " + Capabilities.NumberInputValueCaps ); 
                    Debug.WriteLine( "  Number of Input Data Indices: " + Capabilities.NumberInputDataIndices ); 
                    Debug.WriteLine( "  Number of Output Button Caps: " + Capabilities.NumberOutputButtonCaps ); 
                    Debug.WriteLine( "  Number of Output Value Caps: " + Capabilities.NumberOutputValueCaps ); 
                    Debug.WriteLine( "  Number of Output Data Indices: " + Capabilities.NumberOutputDataIndices ); 
                    Debug.WriteLine( "  Number of Feature Button Caps: " + Capabilities.NumberFeatureButtonCaps ); 
                    Debug.WriteLine( "  Number of Feature Value Caps: " + Capabilities.NumberFeatureValueCaps ); 
                    Debug.WriteLine( "  Number of Feature Data Indices: " + Capabilities.NumberFeatureDataIndices ); 
                    
                    //  ***
                    //  API function: HidP_GetValueCaps
                    
                    //  Purpose: retrieves a buffer containing an array of HidP_ValueCaps structures.
                    //  Each structure defines the capabilities of one value.
                    //  This application doesn't use this data.
                    
                    //  Accepts:
                    //  A report type enumerator from hidpi.h,
                    //  A pointer to a buffer for the returned array,
                    //  The NumberInputValueCaps member of the device's HidP_Caps structure,
                    //  A pointer to the PreparsedData structure returned by HidD_GetPreparsedData.
                    
                    //  Returns: True on success, False on failure.
                    //  ***                    
                    
                    result = HidP_GetValueCaps(HidP_Input, ref valueCaps[0], ref Capabilities.NumberInputValueCaps, preparsedDataPointer); 
                    
                    // (To use this data, copy the ValueCaps byte array into an array of structures.)
                    
                    //  ***
                    //  API function: HidD_FreePreparsedData
                    
                    //  Purpose: frees the buffer reserved by HidD_GetPreparsedData.
                    
                    //  Accepts: A pointer to the PreparsedData structure returned by HidD_GetPreparsedData.
                    
                    //  Returns: True on success, False on failure.
                    //  ***
                    
                    success = HidD_FreePreparsedData( ref preparsedDataPointer );                     
                }                 
            } 
            catch ( Exception ex ) 
            { 
                DisplayException( MODULE_NAME, ex ); 
                throw ; 
            } 
            
            return Capabilities;             
        }         
        
        ///  <summary>
        ///  Creates a 32-bit Usage from the Usage Page and Usage ID. 
        ///  Determines whether the Usage is a system mouse or keyboard.
        ///  Can be modified to detect other Usages.
        ///  </summary>
        ///  
        ///  <param name="MyCapabilities"> a HIDP_CAPS structure retrieved with HidP_GetCaps. </param>
        ///  
        ///  <returns>
        ///  A String describing the Usage.
        ///  </returns>

        public String GetHidUsage(HIDP_CAPS MyCapabilities) 
        {             
            Int32 usage = 0; 
            String usageDescription = ""; 
            
            try 
            { 
                //  Create32-bit Usage from Usage Page and Usage ID.
                
                usage = MyCapabilities.UsagePage * 256 + MyCapabilities.Usage; 
                
                if ( usage == Convert.ToInt32( 0X102 ) )
                 { 
                    usageDescription = "mouse"; } 
                
                if ( usage == Convert.ToInt32( 0X106 ) )
                 { 
                    usageDescription = "keyboard"; }                   
            } 
            catch ( Exception ex ) 
            { 
                DisplayException( MODULE_NAME, ex ); 
                throw ; 
            } 
            
            return usageDescription;             
        }         
        
        ///  <summary>
        ///  Retrieves the number of Input reports the host can store.
        ///  </summary>
        ///  
        ///  <param name="hidDeviceObject"> a handle to a device  </param>
        ///  <param name="numberOfInputBuffers"> an integer to hold the returned value. </param>
        ///  
        ///  <returns>
        ///  True on success, False on failure.
        ///  </returns>

        public Boolean GetNumberOfInputBuffers(SafeFileHandle hidDeviceObject, Int32 numberOfInputBuffers) 
        {             
            Boolean success = false;

            try
            {
                if (!((IsWindows98Gold())))
                {
                    //  ***
                    //  API function: HidD_GetNumInputBuffers

                    //  Purpose: retrieves the number of Input reports the host can store.
                    //  Not supported by Windows 98 Gold.
                    //  If the buffer is full and another report arrives, the host drops the 
                    //  ldest report.

                    //  Accepts: a handle to a device and an integer to hold the number of buffers. 

                    //  Returns: True on success, False on failure.
                    //  ***

                    success = HidD_GetNumInputBuffers(hidDeviceObject, ref numberOfInputBuffers);
                }
                else
                {
                    //  Under Windows 98 Gold, the number of buffers is fixed at 2.

                    numberOfInputBuffers = 2;
                    success = true;
                }

                return success;
            }
            catch (Exception ex)
            {
                DisplayException( MODULE_NAME, ex ); 
                throw ; 
            }                       
        } 
                
        ///  <summary>
        ///  sets the number of input reports the host will store.
        ///  Requires Windows XP or later.
        ///  </summary>
        ///  
        ///  <param name="hidDeviceObject"> a handle to the device.</param>
        ///  <param name="numberBuffers"> the requested number of input reports.  </param>
        ///  
        ///  <returns>
        ///  True on success. False on failure.
        ///  </returns>

        public Boolean SetNumberOfInputBuffers(SafeFileHandle hidDeviceObject, Int32 numberBuffers) 
        {              
            try 
            { 
                if ( !IsWindows98Gold() ) 
                {                     
                    //  ***
                    //  API function: HidD_SetNumInputBuffers
                    
                    //  Purpose: Sets the number of Input reports the host can store.
                    //  If the buffer is full and another report arrives, the host drops the 
                    //  oldest report.
                    
                    //  Requires:
                    //  A handle to a HID
                    //  An integer to hold the number of buffers. 
                    
                    //  Returns: true on success, false on failure.
                    //  ***
                    
                    HidD_SetNumInputBuffers( hidDeviceObject, numberBuffers );
                    return true;                    
                } 
                else 
                { 
                    //  Not supported under Windows 98 Gold.
                    
                    return false; 
                }
                            } 
            catch ( Exception ex ) 
            { 
                DisplayException( MODULE_NAME, ex ); 
                throw ; 
            }            
        } 
                
        ///  <summary>
        ///  Find out if the current operating system is Windows XP or later.
        ///  (Windows XP or later is required for HidD_GetInputReport and HidD_SetInputReport.)
        ///  </summary>

        public Boolean IsWindowsXpOrLater() 
        {        
            try 
            { 
                OperatingSystem myEnvironment = Environment.OSVersion; 
                
                //  Windows XP is version 5.1.
                
                System.Version versionXP = new System.Version( 5, 1 );

                if (myEnvironment.Version >= versionXP)                 
                { 
                    Debug.Write( "The OS is Windows XP or later." ); 
                    return true; 
                } 
                else 
                { 
                    Debug.Write( "The OS is earlier than Windows XP." ); 
                    return false; 
                }                 
            } 
            catch ( Exception ex ) 
            { 
                DisplayException( MODULE_NAME, ex ); 
                throw ; 
            }          
        }         
        
        ///  <summary>
        ///  Find out if the current operating system is Windows 98 Gold (original version).
        ///  Windows 98 Gold does not support the following:
        ///  Interrupt OUT transfers (WriteFile uses control transfers and Set_Report).
        ///  HidD_GetNumInputBuffers and HidD_SetNumInputBuffers
        ///  (Not yet tested on a Windows 98 Gold system.)
        ///  </summary>

        public Boolean IsWindows98Gold() 
        {
            Boolean result = false;
            try 
            { 
                OperatingSystem myEnvironment = Environment.OSVersion; 
                
                //  Windows 98 Gold is version 4.10 with a build number less than 2183.
                
                System.Version version98SE = new System.Version( 4, 10, 2183 );

                if (myEnvironment.Version < version98SE)                
                { 
                    Debug.Write( "The OS is Windows 98 Gold." );
                    result = true; 
                } 
                else 
                { 
                    Debug.Write( "The OS is more recent than Windows 98 Gold." );
                    result = false; 
                }
                return result;
            } 
            catch ( Exception ex ) 
            { 
                DisplayException( MODULE_NAME, ex ); 
                throw ;                 
            }                     
        }         
        
        ///  <summary>
        ///  Provides a central mechanism for exception handling.
        ///  Displays a message box that describes the exception.
        ///  </summary>
        ///  
        ///  <param name="moduleName">  the module where the exception occurred. </param>
        ///  <param name="e"> the exception </param>
        
        public static void DisplayException( String moduleName, Exception e ) 
        {             
            String message = null; 
            String caption = null; 
            
            //  Create an error message.
            
            message = "Exception: " + e.Message + System.Environment.NewLine + "Module: " + moduleName + System.Environment.NewLine + "Method: " + e.TargetSite.Name; 
            
            caption = "Unexpected Exception"; 
            
            /** TODO DEB
            MessageBox.Show( message, caption, MessageBoxButtons.OK ); 
            Debug.Write( message );             
             * */
            LogManager.GetCurrentClassLogger().Debug(caption + " => " + message);
            LogManager.GetCurrentClassLogger().Debug(caption, e);
        }         
    }

    /// <summary>
    /// 
    /// </summary>
    public static class HidDeviceManagement
    {
        public const Int32 DBT_DEVICEARRIVAL = 0X8000;
        public const Int32 DBT_DEVICEREMOVECOMPLETE = 0X8004;
        public const Int32 DBT_DEVTYP_DEVICEINTERFACE = 5;
        public const Int32 DBT_DEVTYP_HANDLE = 6;
        public const Int32 DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;
        public const Int32 DEVICE_NOTIFY_SERVICE_HANDLE = 1;
        public const Int32 DEVICE_NOTIFY_WINDOW_HANDLE = 0;
        public const Int32 WM_DEVICECHANGE = 0X219;

        // from setupapi.h

        public const Int16 DIGCF_PRESENT = 0X2;
        public const Int16 DIGCF_DEVICEINTERFACE = 0X10;

        // There are two declarations for the DEV_BROADCAST_DEVICEINTERFACE class.

        // Use this in the call to RegisterDeviceNotification() and
        // in checking dbch_devicetype in a DEV_BROADCAST_HDR structure.

        [StructLayout(LayoutKind.Sequential)]
        public class DEV_BROADCAST_DEVICEINTERFACE
        {
            public Int32 dbcc_size;
            public Int32 dbcc_devicetype;
            public Int32 dbcc_reserved;
            public Guid dbcc_classguid;
            public Int16 dbcc_name;
        }

        // Use this to read the dbcc_name String and classguid.

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class DEV_BROADCAST_DEVICEINTERFACE_1
        {
            public Int32 dbcc_size;
            public Int32 dbcc_devicetype;
            public Int32 dbcc_reserved;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
            public Byte[] dbcc_classguid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
            public char[] dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class DEV_BROADCAST_HANDLE
        {
            public Int32 dbch_size;
            public Int32 dbch_devicetype;
            public Int32 dbch_reserved;
            public Int32 dbch_handle;
            public Int32 dbch_hdevnotify;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class DEV_BROADCAST_HDR
        {
            public Int32 dbch_size;
            public Int32 dbch_devicetype;
            public Int32 dbch_reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public Int32 cbSize;
            public System.Guid InterfaceClassGuid;
            public Int32 Flags;
            public Int32 Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public Int32 cbSize;
            public String DevicePath;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public Int32 cbSize;
            public System.Guid ClassGuid;
            public Int32 DevInst;
            public Int32 Reserved;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, Int32 Flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern Int32 SetupDiCreateDeviceInfoList(ref System.Guid ClassGuid, Int32 hwndParent);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern Int32 SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, Int32 DeviceInfoData, ref System.Guid InterfaceClassGuid, Int32 MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SetupDiGetClassDevs(ref System.Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, Int32 Flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern Boolean SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, Int32 DeviceInterfaceDetailDataSize, ref Int32 RequiredSize, IntPtr DeviceInfoData);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern Boolean UnregisterDeviceNotification(IntPtr Handle);
        
        /// <summary>
        /// 
        /// </summary>
        public class DeviceDescription
        {
            public string id { get; set; }
            public string path { get; set; }
            public int seq { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class DeviceIdentification
        {
            public string id { get; set; }
            public string vid { get; set; }
            public string pid { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        static public List<DeviceDescription> DeviceList(List<DeviceIdentification> devids)
        {
            List<DeviceDescription>_dl = new List<DeviceDescription>();

            
            IntPtr hdev = new System.IntPtr();


            int device_count = 0;
            int requiredSize = 0;

            Guid guid = new Guid();
            Hid.HidD_GetHidGuid(ref guid);

            hdev = SetupDiGetClassDevs(ref guid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
            SP_DEVICE_INTERFACE_DATA did = new SP_DEVICE_INTERFACE_DATA();
            did.cbSize = Marshal.SizeOf(did);

            while (SetupDiEnumDeviceInterfaces(hdev, 0, ref guid, device_count, ref did) != false)
            {
                SetupDiGetDeviceInterfaceDetail(hdev, ref did, IntPtr.Zero, 0, ref requiredSize, IntPtr.Zero);

                SP_DEVICE_INTERFACE_DETAIL_DATA didd = new SP_DEVICE_INTERFACE_DETAIL_DATA();
                didd.cbSize = Marshal.SizeOf(didd);
                IntPtr detailDataBuffer = Marshal.AllocHGlobal(requiredSize);
                if (!(System.IntPtr.Size == 8))
                {
                    Marshal.WriteInt32(detailDataBuffer, 4 + Marshal.SystemDefaultCharSize);
                }
                else
                {
                    Marshal.WriteInt32(detailDataBuffer, 8);
                }
                
                SetupDiGetDeviceInterfaceDetail(hdev, ref did, detailDataBuffer, requiredSize, ref requiredSize, IntPtr.Zero);
                IntPtr pdevicePathName = new IntPtr(detailDataBuffer.ToInt32() + 4);

                DeviceIdentification devid = QueDispositivo(Marshal.PtrToStringAuto(pdevicePathName), devids);
                if (devid != null)
                {
                    DeviceDescription dev = new DeviceDescription() { id = devid.id, path = Marshal.PtrToStringAuto(pdevicePathName), seq = device_count };
                    _dl.Add(dev);
                }
                device_count++;
            }             

            return _dl;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        static DeviceIdentification QueDispositivo(string id, List<DeviceIdentification> devids)
        {
            foreach (DeviceIdentification devid in devids)
            {
                string encontrar = devid.vid.ToLower() + "&" + devid.pid.ToLower();
                if (id.IndexOf(encontrar) > 0)
                    return devid;
            }

            return null;
        }

    }


} 
