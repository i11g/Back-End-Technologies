﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetGreeting
{
    public class TimeProvider : ITimeProvider
    {
        public DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }
    }

    public class FakeTiemProvider : ITimeProvider
    {    
        private DateTime _fakeTime;

        public FakeTiemProvider(DateTime fakeTime)
        {
            this._fakeTime = fakeTime;
        }
        public DateTime GetCurrentTime()
        {
            return _fakeTime;
        }
    }
}
