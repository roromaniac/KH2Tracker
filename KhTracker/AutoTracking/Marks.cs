using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace KhTracker
{
    class Marks : ImportantCheck
    {
        private int count;
        public int Count
        {
            get { return count; }
            set
            {
                count = value;
                OnPropertyChanged("Count");
            }
        }

        public Marks(MemoryReader mem, int address, int offset, string name) : base(mem, address, offset, name) 
        {
        
        }

        public override byte[] UpdateMemory()
        {
            byte[] data = base.UpdateMemory();

            if (Count < data[0])
            {
                Count = data[0];
                App.logger?.Record("Mark " + Count.ToString() + " obtained");
            }

            return null;
        }
    }
}