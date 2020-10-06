using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using BMDSwitcherAPI;
using System.IO;
using System.Runtime.InteropServices;
using SwitcherLib.Callbacks;

namespace SwitcherLib
{

    public class Upload
    {
        private enum Status
        {
            NotStarted,
            Started,
            Completed,
        }

        private Upload.Status currentStatus;
        private String path;
        private Array framepaths;
        private bool isClip;
        private uint uploadSlot;
        private String currentframepath;
        private UInt32 currentFrameNumber;
        private String name;
        private Switcher switcher;
        private IBMDSwitcherFrame frame;
        private IBMDSwitcherStills stills;
        private IBMDSwitcherClip clip;
        private IBMDSwitcherLockCallback lockCallback;
        private IBMDSwitcherMediaPool switcherMediaPool;

        public Upload(Switcher switcher, String path, uint uploadSlot)
        {
            this.isClip = false;
            this.switcher = switcher;
            this.path = path;
            this.uploadSlot = uploadSlot;
            this.switcher.Connect();
            this.switcherMediaPool = (IBMDSwitcherMediaPool)this.switcher.GetSwitcher();
            this.clip = this.GetClip();
            this.stills = this.GetStills();

            // Is a directory of clips
            if (Directory.Exists(path))
            {
                this.isClip = true;
                this.framepaths = (Array)Directory.GetFiles(path, "*.bmp").OrderBy(f => f).ToArray();
                if (this.framepaths.Length < 1)
                    this.framepaths = (Array)Directory.GetFiles(path, "*.jpg").OrderBy(f => f).ToArray();
                if (this.framepaths.Length < 1)
                    this.framepaths = (Array)Directory.GetFiles(path, "*.jpeg").OrderBy(f => f).ToArray();
                if (this.framepaths.Length < 1)
                    this.framepaths = (Array)Directory.GetFiles(path, "*.gif").OrderBy(f => f).ToArray();
                if (this.framepaths.Length < 1)
                    this.framepaths = (Array)Directory.GetFiles(path, "*.png").OrderBy(f => f).ToArray();
                if (this.framepaths.Length < 1)
                    this.framepaths = (Array)Directory.GetFiles(path, "*.tiff").OrderBy(f => f).ToArray();
                if (this.framepaths.Length < 1)
                    this.framepaths = (Array)Directory.GetFiles(path, "*.tif").OrderBy(f => f).ToArray();
                if (this.framepaths.Length < 1)
                    throw new SwitcherLibException(String.Format("No bmp, jpg, jpeg, gif, png, tiff or tif files found in {0}", path));
                Log.Debug(String.Format("Found {0} media files in {1}", this.framepaths.Length, path));


                UInt32 maxclips;
                this.switcherMediaPool.GetFrameTotalForClips(out maxclips);
                // Checking whether the files can fit
                if (maxclips < this.framepaths.Length)
                {
                    throw new SwitcherLibException(String.Format("The clip pool can contain up to {0} clips, but there are {1} files found in {2}", maxclips, this.framepaths.Length, this.path));
                }
            }
            // Is a file with a still
            else if (File.Exists(path))
            {
                this.currentframepath = path;
                
            }
            else
            {
                throw new SwitcherLibException(String.Format("The file or directory {0} could not be found", path));
            }
        }

        public bool InProgress()
        {
            return this.currentStatus == Upload.Status.Started;
        }

        public void SetName(String name)
        {
            this.name = name;
        }

        public string GetProgress()
        {
            if (this.currentStatus == Upload.Status.NotStarted)
            {
                return "0%";
            }
            if (this.currentStatus == Upload.Status.Completed)
            {
                return "100%";
            }

            double progress;
            if (this.isClip)
            {

                progress = (double)(this.currentFrameNumber + 1) / (double)this.framepaths.Length;
                return String.Format("{0} of {1} frames - {2}%", this.currentFrameNumber + 1, this.framepaths.Length, (int)Math.Round(progress * 100.0));
            } else
            {
                this.stills.GetProgress(out progress);
                return String.Format("{0}%", (int)Math.Round(progress * 100.0));
            }

            
        }

        public void Start()
        {
            if (this.isClip)
            {
                this.StartClipUpload();
            }
            else
            {
                this.StartStillUpload();
            }
        }

        protected void StartStillUpload()
        {
            this.currentStatus = Upload.Status.Started;
            this.frame = this.GetFrame();
            this.lockCallback = (IBMDSwitcherLockCallback)new UploadLock(this);
            this.stills.Lock(this.lockCallback);
        }

        protected void StartClipUpload()
        {
            this.currentStatus = Upload.Status.Started;
            this.lockCallback = (IBMDSwitcherLockCallback)new UploadLock(this);
            this.clip.Lock(this.lockCallback);
        }

        protected IBMDSwitcherFrame GetFrame()
        {
            IBMDSwitcherFrame frame;
            this.switcherMediaPool.CreateFrame(_BMDSwitcherPixelFormat.bmdSwitcherPixelFormat8BitARGB, (uint)this.switcher.GetVideoWidth(), (uint)this.switcher.GetVideoHeight(), out frame);
            IntPtr buffer;
            frame.GetBytes(out buffer);
            byte[] source = this.ConvertImage();
            Marshal.Copy(source, 0, buffer, source.Length);
            return frame;
        }

        protected byte[] ConvertImage()
        {
            try
            {
                Bitmap image = new Bitmap(this.currentframepath);

                if (image.Width != this.switcher.GetVideoWidth() || image.Height != this.switcher.GetVideoHeight())
                {
                    throw new SwitcherLibException(String.Format("Image is {0}x{1} it needs to be the same resolution as the switcher", image.Width.ToString(), image.Height.ToString()));
                }

                byte[] numArray = new byte[image.Width * image.Height * 4];
                for (int index1 = 0; index1 < image.Width * image.Height; index1++)
                {
                    Color pixel = this.GetPixel(image, index1);
                    int index2 = index1 * 4;
                    numArray[index2] = pixel.B;
                    numArray[index2 + 1] = pixel.G;
                    numArray[index2 + 2] = pixel.R;
                    numArray[index2 + 3] = pixel.A;
                }
                return numArray;
            }
            catch (Exception ex)
            {
                throw new SwitcherLibException(ex.Message, ex);
            }
        }

        protected Color GetPixel(Bitmap image, int index)
        {
            int x = index % image.Width;
            int y = (index - x) / image.Width;
            return image.GetPixel(x, y);
        }

        protected IBMDSwitcherStills GetStills()
        {
            IBMDSwitcherStills stills;
            this.switcherMediaPool.GetStills(out stills);
            return stills;
        }

        protected IBMDSwitcherClip GetClip()
        {
            IBMDSwitcherClip clip;
            this.switcherMediaPool.GetClip(this.uploadSlot, out clip);
            return clip;
       }

        public void UnlockCallback()
        {
            this.currentStatus = Upload.Status.Completed;
        }

        public void onPoolLockObtained()
        {
            if (this.isClip)
            {
                Log.Debug("Clearing clip slot");
                this.clip.SetInvalid();

                // Uploading the first frame, the callback event handling will trigger upload of the rest.
                this.currentFrameNumber = 0;
                IBMDSwitcherClipCallback callback = new ClipFrame(this);
                this.clip.AddCallback(callback);
                this.uploadClipFrame();
            }
            else
            {
                IBMDSwitcherStillsCallback callback = new StillsFrame(this);
                this.stills.AddCallback(callback);
                this.stills.Upload((uint)this.uploadSlot, this.GetName(), this.frame);
            }
        }

        protected void uploadClipFrame()
        {
            // Switcher frame counter is 1-indexed while this.framepaths is 0-indexed
            this.currentframepath = (string)this.framepaths.GetValue(this.currentFrameNumber);
            this.frame = this.GetFrame();
            Log.Debug(String.Format("Uploading clip frame {0}: {1}", this.currentFrameNumber+1, this.currentframepath));
            
            //UploadFrame triggers onClipFrameUploadCompleted via a callback when finished
            this.clip.UploadFrame(this.currentFrameNumber, this.frame);
        }

        public void onClipFrameUploadCompleted(uint frameIndex)
        {
            Log.Debug(String.Format("Uploading of clip frame {0} finished", frameIndex));
            // process the next frame
            if (frameIndex < this.framepaths.Length-1)
            {
                this.currentFrameNumber = frameIndex + 1;
                this.uploadClipFrame();
            }
            else // is  finished
            {
                this.clip.SetValid(this.GetName(), frameIndex);
                this.onFinished();
            }
            
        }

        public void onStillsFrameUploadCompleted()
        {
            onFinished();
        }

        protected void onFinished()
        {
            Log.Debug("Completed upload");
            if (this.isClip)
            {
                this.clip.Unlock(this.lockCallback);
            }
            else
            {
                this.stills.Unlock(this.lockCallback);
            }

            this.currentStatus = Upload.Status.Completed;
        }

        public String GetName()
        {
            if (this.name != null)
            {
                return this.name;
            }

            if (this.isClip) {
                return new DirectoryInfo(this.path).Name;
            }
            else
            {
                return Path.GetFileNameWithoutExtension(this.path);
            }
            
        }
    }
}
