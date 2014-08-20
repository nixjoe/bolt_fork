﻿namespace UdpKit {
    internal enum UdpCommandType : byte {
        Connect = 2,
        Accepted = 4,
        Refused = 6,
        Disconnected = 8,
        Ping = 10,
        Refused_HandshakeSize = 12,
        Refused_HandshakeValue = 14
    }
}
