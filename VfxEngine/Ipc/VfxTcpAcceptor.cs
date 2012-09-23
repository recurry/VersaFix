//************************************************************************
// The VersaFix FIX Engine. 
// Copyright (c) 2009 Assimilate Technology, Inc.
//------------------------------------------------------------------------
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
//************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace VfxEngine.Ipc
{
    /// <summary>
    /// The VfxTcpAcceptor class implements an acceptor that uses
    /// a socket for accepting connections from peer systems.
    /// </summary>
    public class VfxTcpAcceptor
    {
        /// <summary>
        /// The IoContext class encapsulates information about an
        /// asynchronous IO operation the acceptor is performing:
        /// </summary>
        private class IoContext
        {
            private Socket _ipcSocket;

            public Socket IpcSocket
            {
                get { return _ipcSocket; }
            }

            private IPEndPoint _ipcEndPoint;

            public IPEndPoint IpcEndPoint
            {
                get { return _ipcEndPoint; }
            }

            public IoContext(Socket ipcSocket, IPEndPoint ipcEndPoint)
            {
                _ipcSocket = ipcSocket;
                _ipcEndPoint = ipcEndPoint;
            }

        }
        
        // REC: Map of the IoContexts associated with each endpoint
        // that the acceptor is accepting on:
        private Dictionary<IPEndPoint, IoContext> _mapEndpoints = new Dictionary<IPEndPoint,IoContext>() ; 

        // REC: Map of the session factories associated with
        // each endpoint that the acceptor is listening on:
        private Dictionary<IPEndPoint, IVfxIpcSessionFactory> _mapFactories = new Dictionary<IPEndPoint,IVfxIpcSessionFactory>() ;

        public bool RxBuffering { get; set; }

        /// <summary>
        /// Initializes a new instance of the class with the
        /// appropriate default settings.
        /// </summary>
        public VfxTcpAcceptor() 
        {
            this.RxBuffering = false;
        }

        /// <summary>
        /// The acceptor's activate method is called to start
        /// accepting incoming connections. The acceptor initiates
        /// an asynchronous accept operation, which is then handled
        /// in the OnAccept method when a connection is established.
        /// </summary>
        /// <param name="ipcEndPoint">
        /// The IP endpoint that the acceptor is to accept incoming
        /// connections on.
        /// </param>
        /// <param name="factory">
        /// The session factory that the acceptor is to use when 
        /// creating new sessions to handle the incoming connections.
        /// </param>
        public void Activate(IPEndPoint ipcEndPoint, IVfxIpcSessionFactory factory)
        {
            Socket ipcSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            IoContext context = new IoContext(ipcSocket, ipcEndPoint);
            
            _mapEndpoints.Add(ipcEndPoint, context) ; 
            _mapFactories.Add(ipcEndPoint, factory);

            context.IpcSocket.Bind(context.IpcEndPoint);
            context.IpcSocket.Listen(100);
            context.IpcSocket.BeginAccept(CompleteAccept, context);
        }

        /// <summary>
        /// The Shutdown method is invoked to request that the
        /// acceptor shutdown a specific endpoint. The acceptor
        /// will respond to this method being invoked by shutting
        /// down the specified endpoint so that connections are no
        /// longer accepted on it.
        /// </summary>
        /// <param name="ipcEndPoint">
        /// The endpoint that is to be shutdown.
        /// </param>
        public void Shutdown(IPEndPoint ipcEndPoint)
        {
            if (_mapEndpoints.ContainsKey(ipcEndPoint))
            {
                IoContext context = _mapEndpoints[ipcEndPoint];
                context.IpcSocket.Close();

                _mapEndpoints.Remove(ipcEndPoint); 
            }

            if (_mapFactories.ContainsKey(ipcEndPoint))
            {
                _mapFactories.Remove(ipcEndPoint);
            }
        }


        private void CompleteAccept(IAsyncResult ar)
        {
            IoContext context = ar.AsyncState as IoContext;
            if (context != null)
            {
                try
                {
                    Socket peer = context.IpcSocket.EndAccept(ar);

                    // REC: Locate the factory associated with this endpoint
                    // and create the appropriate type of session instance:
                    if (_mapFactories.ContainsKey(context.IpcEndPoint))
                    {
                        IPEndPoint peerEP = peer.RemoteEndPoint as IPEndPoint;
                        if (ValidatePeer(peerEP))
                        {
                            VfxTcpModule tcpModule = new VfxTcpModule();
                            tcpModule.Init(peer, this.RxBuffering);

                            // REC: Create a new instance of the session handler that
                            // is configured for the endpoint and associated the 
                            // client IO stream with it:
                            IVfxIpcSession session = _mapFactories[context.IpcEndPoint].CreateSession();
                            session.Init(tcpModule);
                        }
                        else
                        {
                            peer.Close();
                        }
                    }

                    // REC: Initiate another asynchronous connection:
                    context.IpcSocket.BeginAccept(CompleteAccept, context);
                }
                catch (System.ObjectDisposedException)
                {
                    // REC: This exception gets thrown when an asynchronous
                    // accept fails due to the socket it was initiated from
                    // being closed. This is part of the shutdown procedure
                    // and should just be caught and ignored.
                }
            }
        }

        /// <summary>
        /// The ValidatePeer method is invoked by the acceptor as new
        /// connections are accepted. This method can be overriden in
        /// a derived class to allow for validating the IP address of
        /// an incoming connection before allowing it to complete.
        /// </summary>
        /// <param name="peerEP">
        /// The IPEndPoint for the peer system that is being validated.
        /// </param>
        /// <returns></returns>
        public virtual bool ValidatePeer(IPEndPoint peerEP)
        {
            // REC: This method is intended to be overriden, so the
            // default behavior is to always allow connections.
            return true;
        }
    }
}
