using Amib.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace stitch
{
    public class MySmartThreadPool
    {
        static SmartThreadPool Pool = new SmartThreadPool() { MaxThreads = 25 };
        public static SmartThreadPool Instance()
        {
            return Pool;
        }
    }
}
