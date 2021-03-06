﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RMarket.UnitTests
{
    public class SessionStateMock : HttpSessionStateBase
    {
        Hashtable m_SessionStorage = new Hashtable();

        public override object this[string name]
        {
            get { return m_SessionStorage[name]; }
            set { m_SessionStorage[name] = value; }
        }
    }
}
