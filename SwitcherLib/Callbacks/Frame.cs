using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMDSwitcherAPI;

namespace SwitcherLib.Callbacks
{
    class StillsFrame : IBMDSwitcherStillsCallback
    {
        private Upload upload;

        public StillsFrame(Upload upload)
        {
            this.upload = upload;
        }

        public void Notify(_BMDSwitcherMediaPoolEventType eventType, IBMDSwitcherFrame frame, int frameIndex)
        {
            Log.Debug(String.Format("Stills Callback: {0}", eventType.ToString()));
            _BMDSwitcherMediaPoolEventType mediaPoolEventType = eventType;

            if (mediaPoolEventType != _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCompleted)
            {
                return;
            }
            this.upload.onStillsFrameUploadCompleted();
        }
    }

    class ClipFrame : IBMDSwitcherClipCallback
    {
        private Upload upload;

        public ClipFrame(Upload upload)
        {
            this.upload = upload;
        }

        public void Notify(_BMDSwitcherMediaPoolEventType eventType, IBMDSwitcherFrame frame, int frameIndex, IBMDSwitcherAudio audio, int clipIndex)
        {
            Log.Debug(String.Format("Clip Callback: {0}", eventType.ToString()));
            _BMDSwitcherMediaPoolEventType mediaPoolEventType = eventType;

            if (mediaPoolEventType != _BMDSwitcherMediaPoolEventType.bmdSwitcherMediaPoolEventTypeTransferCompleted)
            {
                return;
            }
            this.upload.onClipFrameUploadCompleted((uint)frameIndex);
        }
    }

}
