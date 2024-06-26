﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KhTracker
{
    class TornPage : ImportantCheck
    {
        private int current;
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

        public TornPage(MemoryReader mem, int address, int offset, string name) : base(mem, address, offset, name)
        {

        }

        public override byte[] UpdateMemory()
        {
            byte[] data = base.UpdateMemory();
            int used = (App.Current.MainWindow as MainWindow).GetUsedPages(Address);
            int total = used + data[0];

            if (Quantity < total)
            {
                // add the difference incase of getting multiple at the same time
                Quantity += total - current;
                if (App.logger != null)
                    App.logger.Record(Quantity.ToString() + " torn pages obtained");
            }
            current = total;
            return null;

            //if (current < data[0])
            //{
            //    // add the difference incase of getting multiple at the same time
            //    Quantity += data[0] - current;
            //    if (App.logger != null)
            //        App.logger.Record(Quantity.ToString() + " torn pages obtained");
            //}
            //else if (current > data[0])
            //{
            //    // reduce quantity so when you regrab a torn page after dying the quantity goes back to where it should be
            //    if ((App.Current.MainWindow as MainWindow).GetWorld() != "HundredAcreWood")
            //        Quantity -= current - data[0];
            //    else
            //        (App.Current.MainWindow as MainWindow).UpdateUsedPages();
            //}
        }
    }
}