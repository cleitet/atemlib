using BMDSwitcherAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SwitcherLib
{
    public class Switcher
    {
        protected IBMDSwitcher switcher;
        protected String deviceAddress;
        protected bool connected;

        public Switcher(string deviceAddress)
        {
            this.deviceAddress = deviceAddress;
        }

        public IBMDSwitcher GetSwitcher()
        {
            return this.switcher;
        }

        public void Connect()
        {
            if (this.connected)
            {
                return;
            }

            IBMDSwitcherDiscovery switcherDiscovery = new CBMDSwitcherDiscovery();
            _BMDSwitcherConnectToFailure failReason = 0;

            try
            {
                switcherDiscovery.ConnectTo(this.deviceAddress, out this.switcher, out failReason);
                this.connected = true;
            }
            catch (COMException ex)
            {
                switch (failReason)
                {
                    case _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureIncompatibleFirmware:
                        throw new SwitcherLibException("Incompatible firmware");

                    case _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureNoResponse:
                        throw new SwitcherLibException(String.Format("No response from {0}", this.deviceAddress));

                    default:
                        throw new SwitcherLibException(String.Format("Unknown Error: {0}", ex.Message));
                }
            }
            catch (Exception ex)
            {
                throw new SwitcherLibException(String.Format("Unable to connect to switcher: {0}", ex.Message));
            }
        }

        public String GetProductName()
        {
            this.Connect();
            String productName;
            this.switcher.GetProductName(out productName);
            return productName;
        }

        public int GetVideoHeight()
        {
            this.Connect();
            _BMDSwitcherVideoMode videoMode;
            this.switcher.GetVideoMode(out videoMode);
            _BMDSwitcherVideoMode switcherVideoMode = videoMode;
            switch (switcherVideoMode)
            {
                case _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp2398:
                case _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp24:
                case _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp25:
                case _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp2997:
                    return 2160;

                case _BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50:
                case _BMDSwitcherVideoMode.bmdSwitcherVideoMode720p5994:
                    return 720;

                case _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080i50:
                case _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080i5994:
                case _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p50:
                case _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p2398:
                case _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p24:
                case _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p25:
                case _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p2997:
                case _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p5994:
                    return 1080;

                case _BMDSwitcherVideoMode.bmdSwitcherVideoMode525i5994NTSC:
                    return 480;
                default:
                    throw new SwitcherLibException(String.Format("Unsupported resolution: {0}", videoMode.ToString()));
            }
        }

        public int GetVideoWidth()
        {
            int videoHeight = this.GetVideoHeight();
            switch (videoHeight)
            {
                case 720:
                    return 1280;

                case 1080:
                    return 1920;

                case 2160:
                    return 3840;

                case 480:
                    return 720;
                default:
                    throw new SwitcherLibException(String.Format("Unsupported video height: {0}", videoHeight.ToString()));
            }
        }

        public IList<MediaSlot> getMediaSlots()
        {
            IList<MediaSlot> list = new List<MediaSlot>();

            IBMDSwitcherMediaPool switcherMediaPool = (IBMDSwitcherMediaPool)this.switcher;


            // Handling the stills
            IBMDSwitcherStills stills;
            switcherMediaPool.GetStills(out stills);

            uint stillsCount;
            stills.GetCount(out stillsCount);
            Log.Debug(string.Format("The unit has {0} stills slots", stillsCount));

            for (uint index = 0; index < stillsCount; index++)
            {
                MediaSlot mediaStill = new MediaSlot(stills, index);
                list.Add(mediaStill);
            }

            // Handling the clips
            IBMDSwitcherClip clip;
            uint clipCount;
            switcherMediaPool.GetClipCount(out clipCount);
            Log.Debug(String.Format("The unit has {0} clip slots", clipCount));

            for (uint index = 0; index < clipCount; index++)
            {
                switcherMediaPool.GetClip(index, out clip);
                MediaSlot mediaClip = new MediaSlot(clip, index);
                list.Add(mediaClip);
            }


            IntPtr mediaPlayerIteratorPtr;
            Guid mediaIteratorIID = typeof(IBMDSwitcherMediaPlayerIterator).GUID;
            this.switcher.CreateIterator(ref mediaIteratorIID, out mediaPlayerIteratorPtr);
            IBMDSwitcherMediaPlayerIterator mediaPlayerIterator = (IBMDSwitcherMediaPlayerIterator)Marshal.GetObjectForIUnknown(mediaPlayerIteratorPtr);

            IBMDSwitcherMediaPlayer mediaPlayer;
            mediaPlayerIterator.Next(out mediaPlayer);
            int currentMediaPlayerSlot = 1;
            while (mediaPlayer != null)
            {
                _BMDSwitcherMediaPlayerSourceType mediaPlayerSourceType;
                string mediaPlayerSlotType = "";
                uint index;
                mediaPlayer.GetSource(out mediaPlayerSourceType, out index);

                if (mediaPlayerSourceType == _BMDSwitcherMediaPlayerSourceType.bmdSwitcherMediaPlayerSourceTypeStill)
                {
                    mediaPlayerSlotType = "Still";
                }
                else if (mediaPlayerSourceType == _BMDSwitcherMediaPlayerSourceType.bmdSwitcherMediaPlayerSourceTypeClip)
                {
                    mediaPlayerSlotType = "Clip";
                }

                int mediaSlotUsedByMediaPlayer = (int)index + 1;
                foreach (MediaSlot mediaSlot in list)
                 {
                    if (currentMediaPlayerSlot == mediaSlot.Slot && mediaPlayerSlotType == mediaSlot.Type )
                    {
                        mediaSlot.MediaPlayer = currentMediaPlayerSlot;
                        break;
                    }
                }
                
                currentMediaPlayerSlot++;
                mediaPlayerIterator.Next(out mediaPlayer);
            }
            return list;
        }

    }
}
