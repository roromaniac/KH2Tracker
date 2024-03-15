using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;


namespace KhTracker
{
    class VisitNew : ImportantCheck
    {
        private int level;
        public int Level
        {
            get { return level; }
            set
            {
                level = value;
                OnPropertyChanged("Level");
            }
        }
        public VisitNew(MemoryReader mem, int address, int offset, string name) : base(mem, address, offset, name)
        {

        }

        public override byte[] UpdateMemory()
        {
            byte[] data;

            data = base.UpdateMemory();

            if (Obtained == false && data[0] > 0)
            {
                Obtained = true;
                //App.logger.Record(Name + " obtained");
            }

            if (Level < data[0])
            {
                Level = data[0];
                if (App.logger != null)
                    App.logger.Record(Name + " level " + Level.ToString() + " obtained");
            }

            return null;
        }
    }
}