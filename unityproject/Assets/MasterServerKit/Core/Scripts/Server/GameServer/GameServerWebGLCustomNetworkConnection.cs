// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// Custom network connection used to support WebGL in the game server.
    /// </summary>
    public class GameServerWebGLCustomNetworkConnection : NetworkConnection
    {
        private int offset;

        public override void Initialize(string networkAddress, int networkHostId, int networkConnectionId, HostTopology hostTopology)
        {
            offset = 10;
            base.Initialize(networkAddress, networkHostId, networkConnectionId + offset, hostTopology);
        }

        public override bool TransportSend(byte[] bytes, int numBytes, int channelId, out byte error)
        {
            return NetworkTransport.Send(GameServer.webGLHostId, connectionId - offset, channelId, bytes, numBytes, out error);
        }
    }
}