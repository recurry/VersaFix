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
using System.Threading;

using System.Xml;
using System.Xml.XPath;

using VfxEngine.Fix;
using VfxEngine.FixApp;
using VfxEngine.FixDatabase;
using VfxEngine.FixSessions;
using VfxEngine.FixVxRegistry;
using VfxEngine.FixDxRegistry;
using VfxEngine.Ipc;
using VfxEngine.Ipc.Endpoints;
using VfxEngine.Settings;
using VfxEngine.Services;

namespace VfxEngine.FixEngine
{

    /// <summary>
    /// The VfxFixClientService implements the IVfxService interface
    /// for a FIX session that is acting as a client side session.
    /// </summary>
    public sealed class VfxFixClientService : IVfxFixService, IVfxFixSessionHandler
    {
        /// <summary>
        /// The IPC endpoint assigned to the service.
        /// </summary>
        private IVfxEndpoint _ipcEndpoint;

        /// <summary>
        /// The IVfxFixApp implementation that the service will
        /// route FIX session events to.
        /// </summary>
        private IVfxFixApp _fixApplication;

        /// <summary>
        /// The local services that are provided to the
        /// sessions that are created by the server.
        /// </summary>
        private IVfxServices _localServices;

        // REC: The major version number of the session layer
        // of the FIX protocol being used by the service.
        private int _fixMajor;

        // REC: The minor version number of the session layer
        // of the FIX protocol being used by the service.
        private int _fixMinor;

        /// <summary>
        /// The version of the session layer protocol that the
        /// server has been configured to use.
        /// </summary>
        private string _sxVersion { get; set; }

        /// <summary>
        /// The version of the application layer protocol that
        /// the server has been configured to use.
        /// </summary>
        private string _axVersion { get; set; }


        /// <summary>
        /// The FIX session implementation that manages the
        /// actual FIX session between the application and the
        /// peer system it is connected to. Note that the client
        /// service only manages one FIX session so there is no
        /// need to maintain maps and link sessions to handlers
        /// as there is in the server implementation...
        /// </summary>
        private IVfxFixSession _fixSession;

        /// <summary>
        /// The application session wrapper that is passed to
        /// the FIX application when FIX events occur. Note that
        /// client services only manage one FIX session so there
        /// is no need to maintain maps and link sessions to the
        /// handlers as there is in the server implementation...
        /// </summary>
        private VfxFixClientSession _appSession;

        /// <summary>
        /// The service's current state indication.
        /// </summary>
        private VfxFixServiceStates _serviceState = VfxFixServiceStates.Service_State_Closed;

        /// <summary>
        /// The service's current status indication.
        /// </summary>
        private VfxFixServiceStatus _serviceStatus = VfxFixServiceStatus.Service_Status_Closed;

        /// <summary>
        /// Synchronization object for state management.
        /// </summary>
        private object _synch = new object();

        /// <summary>
        /// Indicates whether or not the underlying FIX session
        /// has been established; used for shutdown logic.
        /// </summary>
        private bool _fixEstablished = false;

        /// <summary>
        /// Indicates whether or not the underlying IPC session
        /// has been established; used for shutdown logic.
        /// </summary>
        private bool _ipcEstablished = false;

        #region IVfxService Members

        /// <summary>
        /// The EventDispatch event is used to provide subscribers with
        /// notifications of different events that are occurring over an
        /// instance of a service.
        /// </summary>
        public event EventHandler<VfxFixServiceEventArgs> EventDispatch;

        /// <summary>
        /// The Activate method is invoked to activate an
        /// instance of the service. The service instance
        /// will immediately activate the client endpoint
        /// so that the connection can be established for
        /// interacting with the peer system.
        /// </summary>
        public void Activate()
        {
            // REC: Activation is initiated asynchronously:
            ThreadPool.QueueUserWorkItem(Activate_Entrypoint);    
        }

        private void Activate_Entrypoint(object state)
        {
            // REC: The service can only be activated if it is
            // currently in the closed state.
            if (_serviceState == VfxFixServiceStates.Service_State_Closed)
            {
                // REC: Adjust the service's current state:
                _serviceState = VfxFixServiceStates.Service_State_Opened;

                // REC: Adjust the service's current status:
                _serviceStatus = VfxFixServiceStatus.Service_Status_Opened;

                // REC: Dispatch the service started event:
                EventHandler<VfxFixServiceEventArgs> tmpDispatch_Started = EventDispatch;
                if (tmpDispatch_Started != null)
                {
                    VfxFixServiceEventArgs tmpArgs = new VfxFixServiceEventArgs(VfxFixServiceEventTypes.Event_Service_Started, _serviceStatus);
                    tmpDispatch_Started(this, tmpArgs);
                }

                // REC: The service doesn't support a timed wait right now, so the
                // connection attempts start immediately:
                _serviceStatus = VfxFixServiceStatus.Service_Status_Connecting;
                EventHandler<VfxFixServiceEventArgs> tmpDispatch_Connecting = EventDispatch;
                if (tmpDispatch_Connecting != null)
                {
                    VfxFixServiceEventArgs tmpArgs = new VfxFixServiceEventArgs(VfxFixServiceEventTypes.Event_Service_Started, _serviceStatus);
                    tmpDispatch_Connecting(this, tmpArgs);
                }

                // REC: Activate the IPC endpoint so that the
                // connection can be established to the peer:
                _ipcEndpoint.Activate();
            }
        }

        /// <summary>
        /// The Initialize method is invoked to initialize 
        /// an instance of the service with the services and
        /// configuration settings it requires in order to be
        /// successfully activated.
        /// </summary>
        /// <param name="services">
        /// The service container that contains the services
        /// provided by the instance's owner.
        /// </param>
        /// <param name="settings">
        /// The configuration settings for the service instance
        /// as found in a section of a configuration metabase.
        /// </param>
        public void Init(IVfxServices services, XmlDocument settings)
        {
            // REC: The client service creates its own instance of a
            // service container and populates it with references to
            // both application and local service references:
            _localServices = new VfxServices();

            // REC: Maintain a reference to the configuration that is
            // provided by the caller - this will be used to configure
            // the sessions that are created when clients connect to an
            // instance of the service:
            _localServices.AddService(typeof(IVfxSettings), new VfxSettings(settings));

            // REC: Retrieve the IVfxFixApp service and maintain a
            // reference to it so that events from the FIX sessions
            // can be routed to the user's application:
            _fixApplication = services.GetService(typeof(IVfxFixApp)) as IVfxFixApp;
            if (_fixApplication == null)
            {
                throw new ArgumentException("The IVfxFixApp service must be provided.");
            }

            // REC: Ensure that the FIX version registry has been provided
            // in the service container that was supplied by the caller:
            IVfxFixVxRegistry vxRegistry = services.GetService(typeof(IVfxFixVxRegistry)) as IVfxFixVxRegistry;
            if (vxRegistry == null)
            {
                throw new ArgumentException("The IVfxFixVxRegistry service is not available.");
            }

            // REC: Ensure that the FIX dictionary registry has been provided
            // in the service container that was supplied by the caller:
            IVfxFixDxRegistry dxRegistry = services.GetService(typeof(IVfxFixDxRegistry)) as IVfxFixDxRegistry;
            if (dxRegistry == null)
            {
                throw new ArgumentException("The IVfxFixDxRegistry service is not available.");
            }

            // REC: An XPathNavigator is used to retrieve all of the
            // relevant settings from the corresponding configuration
            // document that is provided by the settings instance:
            XPathNavigator xpn = settings.CreateNavigator();

            // REC: An XPathNodeIterator is used to retrieve all of
            // the relevant settings from the document. The instance
            // is created by calling the 'Select' method of the path
            // navigator that is created from the settings:
            XPathNodeIterator xpi = null;

            xpi = xpn.Select("/Session/Protocol/Settings/Setting[@name='Fix.Session.Sx.Version']");
            if ((xpi.Count > 0) && (xpi.MoveNext()))
            {
                this._sxVersion = xpi.Current.GetAttribute("content", "").Trim();
            }

            xpi = xpn.Select("/Session/Protocol/Settings/Setting[@name='Fix.Session.Ax.Version']");
            if ((xpi.Count > 0) && (xpi.MoveNext()))
            {
                this._axVersion = xpi.Current.GetAttribute("content", "").Trim();
            }

            // REC: Attempt to retrieve the version information that
            // corresponds to the configured application layer:
            VfxFixVxRecord vxRecord = vxRegistry.Get(this._axVersion);
            if (vxRecord == null)
            {
                throw new ArgumentException("The specified application layer could not be resolved.");
            }

            // REC: If the session layer version of the protocol
            // has not been specified, this may indicate that the
            // application version is FIX 4.0-4.4. If that is the
            // case, then we can default the session layer to the
            // same version as the application layer:
            if (string.IsNullOrEmpty(this._sxVersion))
            {
                // REC: The layer "combined" is set on version definitions
                // to indicate that both the session layer and application
                // layer are defined within the same dictionary:
                if (vxRecord.Layer.ToLower().CompareTo("combined") == 0)
                {
                    this._sxVersion = this._axVersion;
                }
                else
                {
                    // REC: If the application layer does not resolve to
                    // an instance of a version definition that represents
                    // a version of FIX earlier than 5.0, then the service
                    // cannot be configured based on the app layer:
                    throw new ArgumentException("The session layer must be specified...");
                }
            }

            // REC: Retrieve the endpoint configuration from the session
            // configuration and use it to create a new instance of that
            // type of endpoint:
            xpi = xpn.Select("/Session/Endpoint");
            if ((xpi.Count == 1) && (xpi.MoveNext()))
            {
                XmlDocument epxConfiguration = new XmlDocument();
                epxConfiguration.LoadXml(xpi.Current.OuterXml);

                this._ipcEndpoint = VfxEndpointFactory.Create(epxConfiguration);
            }

            this._ipcEndpoint.EventDispatch += HandleIpc_Dispatch;

            this._localServices.AddService(typeof(IVfxFixVxRegistry), vxRegistry);
            this._localServices.AddService(typeof(IVfxFixDxRegistry), dxRegistry);

            // REC: Retrieve the session database configuration from
            // the configuration settings:
            xpi = xpn.Select("/Session/Database");
            if ((xpi.Count == 1) && (xpi.MoveNext()))
            {
                XmlDocument dbxConfiguration = new XmlDocument();
                dbxConfiguration.LoadXml(xpi.Current.OuterXml);

                IVfxFixDatabase database = VfxFixDatabaseFactory.Create(services, dbxConfiguration);
                this._localServices.AddService(typeof(IVfxFixDatabase), database);
            }
        }

        /// <summary>
        /// The Shutdown method is invoked to request that an
        /// instance of the service shut down the client session
        /// that it is managing.
        /// </summary>
        public void Shutdown()
        {
            // REC: Shutdown is initiated asynchronously:
            ThreadPool.QueueUserWorkItem(Shutdown_Entrypoint, null);
        }

        /// <summary>
        /// The Shutdown_Entrypoint method is the asynchronous entrypoint
        /// for the shutdown operation. It is invoked by the thread pool.
        /// </summary>
        /// <param name="state"></param>
        private void Shutdown_Entrypoint(object state)
        {
            lock (_synch)
            {
                // REC: The service can only be shutdown if it
                // is currently in the opened state:
                if (_serviceState == VfxFixServiceStates.Service_State_Opened)
                {
                    // REC: Adjust the service's current state:
                    _serviceState = VfxFixServiceStates.Service_State_Closing;

                    // REC: Adjust the service's current status:
                    _serviceStatus = VfxFixServiceStatus.Service_Status_Closing;

                    // REC: Dispatch an update to the service's subscribers:
                    EventHandler<VfxFixServiceEventArgs> tmpDispatch_Update = EventDispatch;
                    if (tmpDispatch_Update != null)
                    {
                        VfxFixServiceEventArgs tmpArgs = new VfxFixServiceEventArgs(VfxFixServiceEventTypes.Event_Service_Updated, _serviceStatus);
                        tmpDispatch_Update(this, tmpArgs);
                    }
                    
                    // REC: If the FIX session is established, it needs
                    // to be shutdown before the service can stop:
                    if(_fixEstablished == true)
                    {
                        _fixSession.HandleShutdown();
                    }
                    else if(_ipcEstablished == true)
                    {
                        _ipcEndpoint.Shutdown();
                    }
                    else
                    {
                        // REC: Neither the IPC session nor the FIX session
                        // are currently established, so the service should
                        // just dispatch the shutdown event to its owner:
                        _serviceState = VfxFixServiceStates.Service_State_Closed;

                        // REC: Adjust the service's status to reflect the new state:
                        _serviceStatus = VfxFixServiceStatus.Service_Status_Closed;

                        EventHandler<VfxFixServiceEventArgs> tmpDispatch = EventDispatch;
                        if (tmpDispatch != null)
                        {
                            VfxFixServiceEventArgs tmpArgs = new VfxFixServiceEventArgs(VfxFixServiceEventTypes.Event_Service_Stopped, _serviceStatus);
                            tmpDispatch(this, tmpArgs);
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// The HandleIpc_Dispatch event handler is invoked in 
        /// response to the IPC endpoint dispatching an event to
        /// an instance of the service. The service demultiplexes
        /// the dispatched event to the appropriate handler.
        /// </summary>
        /// <param name="sender">
        /// The IPC endpoint that generated the event.
        /// </param>
        /// <param name="args">
        /// The IPC event details for the dispatched event.
        /// </param>
        private void HandleIpc_Dispatch(object sender, VfxIpcEventArgs args)
        {
            switch (args.EventType)
            {
                case VfxIpcEventTypes.Event_Session_Opened:
                    HandleIpc_EventOpened(sender, args);
                    break;
                case VfxIpcEventTypes.Event_Session_Message:
                    HandleIpc_EventMessage(sender, args);
                    break;
                case VfxIpcEventTypes.Event_Session_Closed:
                    HandleIpc_EventClosed(sender, args);
                    break;
            }
        }

        /// <summary>
        /// The HandleIpc_EventOpened method is invoked in response
        /// to the IPC endpoint establishing a connection to the peer
        /// system that the service is interacting with.
        /// </summary>
        /// <param name="sender">
        /// The VfxIpcEndpoint that dispatched the event.
        /// </param>
        /// <param name="args">
        /// The details associated with the event.
        /// </param>
        private void HandleIpc_EventOpened(object sender, VfxIpcEventArgs args)
        {
            lock (_synch)
            {
                // REC: The IPC session is now established.
                _ipcEstablished = true;

                // REC: Adjust the service's current status:
                _serviceStatus = VfxFixServiceStatus.Service_Status_Opened;
                // REC: Dispatch the update to the service's subscribers:
                EventHandler<VfxFixServiceEventArgs> tmpDispatch_Update = EventDispatch;
                if (tmpDispatch_Update != null)
                {
                    VfxFixServiceEventArgs tmpArgs = new VfxFixServiceEventArgs(VfxFixServiceEventTypes.Event_Service_Updated, _serviceStatus);
                    tmpDispatch_Update(this, tmpArgs);
                }

                // REC: Retrieve the version definition from the version
                // definition registry and determine what kind of session
                // needs to be created in order to handle the connection:
                IVfxFixVxRegistry vxRegistry = this._localServices.GetService(typeof(IVfxFixVxRegistry)) as IVfxFixVxRegistry;


                // REC: Create a new instance of a FIX session to handle
                // the communication between the server and the peer:
                IVfxFixSession fixSession = null;

                // REC: Check the version definition in order to tell if
                // this is a FIX 4.x or FIX 5.x service:
                VfxFixVxRecord vxRecord = vxRegistry.Get(this._sxVersion);
                if (vxRecord.Layer.ToLower().CompareTo("combined") == 0)
                {
                    fixSession = new VfxFix4xClientSession();
                }
                else
                {
                    fixSession = new VfxFix5xClientSession();
                }


                _fixSession = fixSession;

                // REC: Initialize the session:
                _fixSession.Init(this._localServices, this); 

                // REC: Construct an instance of the session wrapper
                // for the FIX application and bind it to the session
                // implementation that has been created:
                _appSession = new VfxFixClientSession(_fixSession);

                // REC: Notify the FIX session implementation that it
                // has been connected to a peer system:
                _fixSession.HandleConnect();
            }
        }

        /// <summary>
        /// The HandleIpc_EventMessage event handler is invoked in
        /// response to the service being notified that the IPC session
        /// has received some data from the peer system.
        /// </summary>
        /// <param name="sender">
        /// The IPC session that is dispatching the event.
        /// </param>
        /// <param name="args">
        /// The details of the IPC event being dispatched.
        /// </param>
        private void HandleIpc_EventMessage(object sender, VfxIpcEventArgs args)
        {
            lock (_synch)
            {
                _fixSession.HandleRxMessage(args.EventData);
            }
        }

        /// <summary>
        /// The HandleIpc_EventClosed event handler is invoked in
        /// response to the service being notified that the session
        /// has been disconnected from the peer system.
        /// </summary>
        /// <param name="sender">
        /// The IPC session that is dispatching the event.
        /// </param>
        /// <param name="args">
        /// The details of the IPC event being dispatched.
        /// </param>
        private void HandleIpc_EventClosed(object sender, VfxIpcEventArgs args)
        {
            lock (_synch)
            {
                // REC: The IPC session is no longer established:
                _ipcEstablished = false;

                // REC: Notify the FIX session that the peer system
                // is no longer connected:
                _fixSession.HandleDisconnect();

                // REC: Adjust the service's current status:
                _serviceStatus = VfxFixServiceStatus.Service_Status_Disconnected;
                EventHandler<VfxFixServiceEventArgs> tmpDispatch_Update = EventDispatch;
                if (tmpDispatch_Update != null)
                {
                    VfxFixServiceEventArgs tmpArgs = new VfxFixServiceEventArgs(VfxFixServiceEventTypes.Event_Service_Updated, _serviceStatus);
                    tmpDispatch_Update(this, tmpArgs);
                }

                // REC: If the service is closing, then the disconnection
                // of the IPC endpoint is the last thing that needs to be
                // completed before dispatching the stopped event back up
                // to the service's owner/subscribers:
                if (_serviceState == VfxFixServiceStates.Service_State_Closing)
                {
                    // REC: Adjust the service's current state:
                    _serviceState = VfxFixServiceStates.Service_State_Closed;
                    // REC: Adjust the service's current status:
                    _serviceStatus = VfxFixServiceStatus.Service_Status_Closed;

                    EventHandler<VfxFixServiceEventArgs> tmpDispatch_Stopped = EventDispatch;
                    if (tmpDispatch_Stopped != null)
                    {
                        VfxFixServiceEventArgs tmpArgs = new VfxFixServiceEventArgs(VfxFixServiceEventTypes.Event_Service_Stopped, _serviceStatus);
                        tmpDispatch_Stopped(this, tmpArgs);
                    }
                }
                else
                {
                    // REC: If the service is not configured to reconnect
                    // to the peer system, then it is now stopped:

                    // NOTE: Add reconnect logic here!

                    // REC: Adjust the service's current state:
                    _serviceState = VfxFixServiceStates.Service_State_Closed;
                    // REC: Adjust the service's current status:
                    _serviceStatus = VfxFixServiceStatus.Service_Status_Closed;

                    EventHandler<VfxFixServiceEventArgs> tmpDispatch_Stopped = EventDispatch;
                    if (tmpDispatch_Stopped != null)
                    {
                        VfxFixServiceEventArgs tmpArgs = new VfxFixServiceEventArgs(VfxFixServiceEventTypes.Event_Service_Stopped, _serviceStatus);
                        tmpDispatch_Stopped(this, tmpArgs);
                    }
                }
            }
        }

        #region IVfxFixSessionHandler Members

        /// <summary>
        /// the OnSessionOpened callback is invoked by the underlying
        /// instance of a FIX session to notify the service that it has
        /// been started. At this point, the session is merely connected
        /// to the peer system and no messages have been exchanged.
        /// </summary>
        /// <param name="session">
        /// The FIX session that the event relates to.
        /// </param>
        public void OnSessionOpened(IVfxFixSession session)
        {
            _fixApplication.OnSessionOpened(_appSession);
        }

        /// <summary>
        /// The OnSessionLogon callback is invoked by the underlying
        /// instance of a FIX session to notify the service that the
        /// session has completed a FIX logon with the peer.
        /// </summary>
        /// <param name="session">
        /// The FIX session that is generating the callback.
        /// </param>
        /// <param name="msg">
        /// The FIX logon message received from the peer.
        /// </param>
        public void OnSessionLogon(IVfxFixSession session, FixMessage msg)
        {
            lock (_synch)
            {
                // REC: The FIX session has been established.
                _fixEstablished = true;

                // REC: Forward the logon notification to the app:
                _fixApplication.OnSessionLogon(_appSession, msg);
            }
        }

        /// <summary>
        /// The OnSessionTimeout callback method is invoked by an
        /// instance of a session when it has not received any data
        /// from the peer during its configured timeout interval.
        /// </summary>
        /// <param name="session">
        /// The IVfxFixSession implementation that is issuing the
        /// callback to the handler.
        /// </param>
        public void OnSessionTimeout(IVfxFixSession session)
        {
            this._ipcEndpoint.Shutdown();
        }

        /// <summary>
        /// The OnSessionLogout callback is invoked by the underlying
        /// instance of a FIX session to notify the service that the
        /// session has logged out from the peer system.
        /// </summary>
        /// <param name="session">
        /// The FIX session that is generating the callback.
        /// </param>
        /// <param name="msg">
        /// The FIX logout message received from the peer.
        /// </param>
        public void OnSessionLogout(IVfxFixSession session, FixMessage msg)
        {
            lock (_synch)
            {
                // REC: The FIX session is no longer established.
                _fixEstablished = false;
             
                // REC: Forward the logout notification to the app:
                _fixApplication.OnSessionLogout(_appSession, msg);
            }
        }

        /// <summary>
        /// The OnSessionRxAdmMessage callback is invoked by a session
        /// when it receives an administrative (session) layer message
        /// from the peer session it's interacting with.
        /// </summary>
        /// <param name="session">
        /// The IVfxFixSession implementation that is issuing
        /// the callback to the handler.
        /// </param>
        /// <param name="msg">
        /// The session layer message that was received.
        /// </param>
        public void OnSessionRxAdmMessage(IVfxFixSession session, FixMessage msg)
        {
            // REC: Forward the admin message to the application:
            _fixApplication.OnSessionRxAdmMessage(_appSession, msg);
        }

        /// <summary>
        /// The OnSessionRxAppMessage callback is invoked by a session
        /// when it receives an application layer message from the peer
        /// session it's interacting with.
        /// </summary>
        /// <param name="session">
        /// The IVfxFixSession implementation that is issuing the
        /// callback to the handler.
        /// </param>
        /// <param name="msg">
        /// The application message that the session has received
        /// from its corresponding peer session.
        /// </param>
        public void OnSessionRxAppMessage(IVfxFixSession session, FixMessage msg)
        {
            // REC: Forward the app message to the application:
            _fixApplication.OnSessionRxAppMessage(_appSession, msg);
        }

        /// <summary>
        /// The OnSessionTxAdmMessage callback is invoked by a session
        /// when it is ready to dispatch an administrative message to the
        /// peer session it's interacting with.
        /// </summary>
        /// <param name="session">
        /// The IVfxFixSession implementation that is issuing the
        /// callback to the handler.
        /// </param>
        /// <param name="msg">
        /// The administrative message that the session wants to have
        /// sent to its corresponding peer session.
        /// </param>
        public void OnSessionTxAdmMessage(IVfxFixSession session, FixMessage msg)
        {
            // REC: Forward the admin message to the application:
            _fixApplication.OnSessionTxAdmMessage(_appSession, msg);
            // REC: Send the admin message to the peer session:
            _ipcEndpoint.Send(null, new VfxMsgBlock(msg.ToString()));
        }

        /// <summary>
        /// The OnSessionTxAppMessage callback is invoked by a session
        /// when it is ready to dispatch an application message to the
        /// peer session it's interacting with.
        /// </summary>
        /// <param name="session">
        /// The IVfxFixSession implementation that is issuing the
        /// callback to the handler.
        /// </param>
        /// <param name="msg">
        /// The application message that the session wants to have sent
        /// to its corresponding peer session.
        /// </param>
        public void OnSessionTxAppMessage(IVfxFixSession session, FixMessage msg)
        {
            // REC: Forward the app message to the application:
            _fixApplication.OnSessionTxAppMessage(_appSession, msg);
            // REC: Send the app message to the peer system:
            _ipcEndpoint.Send(null, new VfxMsgBlock(msg.ToString()));
        }

        /// <summary>
        /// The OnSessionStopped event handler is invoked in response
        /// to the underlying FIX session being shutdown.
        /// </summary>
        /// <param name="session">
        /// The FIX session that the event relates to.
        /// </param>
        public void OnSessionClosed(IVfxFixSession session)
        {
            lock (_synch)
            {
                _fixApplication.OnSessionClosed(_appSession);

                // REC: If the service is being closed and the FIX session
                // has just stopped, then the IPC handler can be shutdown:
                if (_serviceState == VfxFixServiceStates.Service_State_Closing)
                {
                    _ipcEndpoint.Shutdown();
                }
            }
        }

        #endregion
    }
}
