using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMI.CD40.Module.BusinessEntities
{
    class WindowsSoundDevices
    {
        public void GetWindowsSoundDeviceNames()
        {
            NAudio.CoreAudioApi.MMDeviceEnumerator MMDE = new NAudio.CoreAudioApi.MMDeviceEnumerator();
            //Get all the devices, no matter what condition or status
            NAudio.CoreAudioApi.MMDeviceCollection DevCol = MMDE.EnumerateAudioEndPoints(NAudio.CoreAudioApi.DataFlow.All, NAudio.CoreAudioApi.DeviceState.All);
            //Loop through all devices
            foreach (NAudio.CoreAudioApi.MMDevice dev in DevCol)
            {
                try
                {
                    //Get its audio volume
                    System.Diagnostics.Debug.Print("Volume of " + dev.FriendlyName + " is " + dev.AudioEndpointVolume.MasterVolumeLevel.ToString());

                    //Get its audio volume
                    System.Diagnostics.Debug.Print(dev.AudioEndpointVolume.MasterVolumeLevel.ToString());
                }
                catch (Exception ex)
                {
                    //Do something with exception when an audio endpoint could not be muted
                    System.Diagnostics.Debug.Print(dev.FriendlyName + " could not be loaded");
                }
            }

        }

    }
}
