using BMDSwitcherAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwitcherLib
{

    public class MediaSlot
    {
        public String Type;
        public String Name;
        public String Hash;
        public int Slot;
        public int MediaPlayer;

        public MediaSlot(IBMDSwitcherClip clip, uint index)
        {
            BMDSwitcherHash hash;
            // Only getting the hash of the first frame
            clip.GetFrameHash(0, out hash);
            this.Hash = String.Join("", BitConverter.ToString(hash.data).Split('-'));
            clip.GetName(out this.Name);
            this.Slot = (int)index + 1;
            this.Type = "Clip";
        }

        public MediaSlot(IBMDSwitcherStills stills, uint index)
        {
            BMDSwitcherHash hash;
            stills.GetHash(index, out hash);
            this.Hash = String.Join("", BitConverter.ToString(hash.data).Split('-'));
            stills.GetName(index, out this.Name);
            this.Slot = (int)index + 1;
            this.Type = "Still";
        }

        public String ToCSV()
        {
            return String.Join(",", this.Slot.ToString(), "\"" + this.Type + "\"", "\"" + this.Name + "\"", "\"" + this.Hash + "\"", this.MediaPlayer.ToString());
        }
    }
}
