using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KhTracker
{
    class TornPageNew : ImportantCheck
    {
        private int quantity;
        public int Quantity
        {
            get { return quantity; }
            set
            {
                quantity = value;
                OnPropertyChanged("Quantity");
            }
        }

        public TornPageNew(MemoryReader mem, int address, int offset, string name) : base(mem, address, offset, name){}

        public override byte[] UpdateMemory()
        {
            byte[] data = base.UpdateMemory();
            int quant = data[0] + window.GetUsedPages();
            string world = window.GetWorld();

            if (world == "HundredAcreWood" && Quantity > quant)
            {
                window.UpdateUsedPages();
            }

            int total = window.GetUsedPages() + data[0];
            if (Quantity < total)
            {
                Quantity = total;
                App.logger?.Record(Quantity.ToString() + " torn pages obtained");
            }

            return null;
        }
    }
}