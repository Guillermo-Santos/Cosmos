﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Net
{
    [Plug(Target = typeof(IPEndPoint))]
    public static class IPEndPointImpl
    {
        public static void CCtor(IPEndPoint aThis)
        {
            Cosmos.HAL.Global.debugger.Send("IPEndPoint - cctor.");
        }

        public static void Ctor(IPEndPoint aThis, long address, int port, [FieldAccess(Name = "System.Net.IPAddress System.Net.IPEndPoint._address")] ref IPAddress _address,
            [FieldAccess(Name = "System.Int32 System.Net.IPEndPoint._port")] ref int _port)
        {
            Cosmos.HAL.Global.debugger.Send("IPEndPoint - start 1.");

            _address = new IPAddress(address);
            _port = port;

            Cosmos.HAL.Global.debugger.Send("IPEndPoint - ok.");
        }

        public static void Ctor(IPEndPoint aThis, IPAddress address, int port, [FieldAccess(Name = "System.Net.IPAddress System.Net.IPEndPoint._address")] ref IPAddress _address,
            [FieldAccess(Name = "System.Int32 System.Net.IPEndPoint._port")] ref int _port)
        {
            Cosmos.HAL.Global.debugger.Send("IPEndPoint - start. 2");

            _address = address;
            _port = port;

            Cosmos.HAL.Global.debugger.Send("IPEndPoint - ok.");
        }
    }
}
