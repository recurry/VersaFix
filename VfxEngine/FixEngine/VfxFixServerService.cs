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
using VfxEngine.FixParser;
using VfxEngine.FixDatabase;
using VfxEngine.FixVxRegistry;
using VfxEngine.FixDxRegistry;
using VfxEngine.FixVxMatcher;
using VfxEngine.FixSessions;
using VfxEngine.Settings;
using VfxEngine.Services;
using VfxEngine.Ipc;
using VfxEngine.Ipc.Endpoints;

namespace VfxEngine.FixEngine
{
    /// <summary>
    /// The VfxFixServerService class implements a FIX service
    /// that acts as the server side of a FIX session.
    /// </summary>
    public class VfxFixServerService : IVfxFixService, IVfxFixSessionHandler
    {
        /// <summary>
        /// The IPC endpoint assigned to the service.
        /// </summary>
        private IVfxEndpoint _endpoint;

        /// <summary>
        /// The IVfxFixApp implementation that the service will
        /// route FIX session events to.
        /// </summary>
        private IVfxFixApp _application;

        /// <summary>
        /// The local services that are provided to the
        /// sessions that are created by the server.
        /// </summary>
        private IVfxServices _localServices;

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

        // REC: The major version number of the session layer
        // of the FIX protocol being used by the service.
        private int _fixMajor;

        // REC: The minor version number of the session layer
        // of the FIX protocol being used by the service.
        private int _fixMinor;

        /// <summary>
        /// The map of all IVfxFixSession instances that an instance
        /// of the server service is managing, keyed by their id.
        /// </summary>
        private Dictionary<Guid, IVfxFixSession> _mapFixSessions = new Dictionary<Guid, IVfxFixSession>();

        /// <summary>
        /// The map of all IPC session identifiers, keyed by the session
        /// identifier of the FIX session they are bound to.
        /// </summary>
        private Dictionary<Guid, string> _mapFixToIpc = new Dictionary<Guid, string>();

        /// <summary>
        /// The map of all FIX session identifiers, keyed by the session
        /// identifier of the IPC session they are bound to.
        /// </summary>
        private Dictionary<string, Guid> _mapIpcToFix = new Dictionary<string, Guid>();

        /// <summary>
        /// The map of all IVfxAppSession instances that an instance
        /// of the server service is managing, keyed by the unique id
        /// of the IVfxFixSession instance they are associated with.
        /// </summary>
        private Dictionary<Guid, IVfxFixAppSession> _mapAppSessions = new Dictionary<Guid, IVfxFixAppSession>();

        /// <summary>
        /// The service's current state.
        /// </summary>
        private VfxFixServiceStates _serviceState = VfxFixServiceStates.Service_State_Closed;

        /// <summary>
        /// The service's current status.
        /// </summary>
        private VfxFixServiceStatus _serviceStatus = VfxFixServiceStatus.Service_Status_Closed;

        #region IVfxService Members

        /// <summary>
        /// The EventDispatch event is used to provide subscribers with
        /// notifications of different events that are occurring over an
        /// instance of a service.
        /// </summary>
        public event EventHandler<VfxFixServiceEventArgs> EventDispatch;

        /// <summary>
        /// The Activate method starts the service by activating
        /// the IPC endpoint that is assigned to it. The endpoint
        /// will start accepting connections from peer systems and
        /// route the IO from those connections to the service.
        /// </summary>
        public void Activate()
        {
            ThreadPool.QueueUserWorkItem(Activate_Entrypoint, null);
        }

        private void Activate_Entrypoint(object state)
        {
            if (_serviceState == VfxFixServiceStates.Service_State_Closed)
            {
                _serviceState = VfxFixServiceStates.Service_State_Opened;

                _serviceStatus = VfxFixServiceStatus.Service_Status_Opened;

                // REC: Dispatch the service started event:
                EventHandler<VfxFixServiceEventArgs> tmpDispatch_Started = EventDispatch;
                if (tmpDispatch_Started != null)
                {
                    VfxFixServiceEventArgs tmpArgs = new VfxFixServiceEventArgs(VfxFixServiceEventTypes.Event_Service_Started, _serviceStatus);
                    tmpDispatch_Started(this, tmpArgs);
                }

                _endpoint.Activate();
            }
        }

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
            _application = services.GetService(typeof(IVfxFixApp)) as IVfxFixApp;
            if (_application == null)
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
            if(vxRecord == null)
            {
                throw new ArgumentException("The specified application layer could not be resolved.");
            }

            // REC: If the session layer version of the protocol
            // has not been specified, this may indicate that the
            // application version is FIX 4.0-4.4. If that is the
            // case, then we can default the session layer to the
            // same version as the application layer:
            if(string.IsNullOrEmpty(this._sxVersion))
            {
                // REC: The layer "combined" is set on version definitions
                // to indicate that both the session layer and application
                // layer are defined within the same dictionary:
                if(vxRecord.Layer.ToLower().CompareTo("combined") == 0)
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

                this._endpoint = VfxEndpointFactory.Create(epxConfiguration);
            }

            this._endpoint.EventDispatch += HandleIpcDispatch;

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
        /// The Status method returns the text string for the
        /// service's current status.
        /// </summary>
        /// <returns></returns>
        public string Status()
        {
            return VfxFixServiceStatusConverter.ToString(_serviceStatus);
        }

        /// <summary>
        /// The Shutdown method is invoked to request that the
        /// server service shutdown all of the sessions that it
        /// is currently managing.
        /// </summary>
        public void Shutdown()
        {
            ThreadPool.QueueUserWorkItem(Shutdown_Entrypoint);
        }

        private void Shutdown_Entrypoint(object state)
        {
            if (_serviceState == VfxFixServiceStates.Service_State_Opened)
            {
                // REC: Adjust the service's state:
                _serviceState = VfxFixServiceStates.Service_State_Closing;
                // REC: Adjust the service's status:
                _serviceStatus = VfxFixServiceStatus.Service_Status_Closing;

                // REC: Shutdown the IPC endpoint first:
                _endpoint.Shutdown();

                // REC: If there are any active FIX sessions, then they
                // need to be shutdown before the service can be shutdown.
                if (_mapFixSessions.Count > 0)
                {
                    // REC: Iterate over all of the sessions and instruct
                    // each one to shut itself down:
                    foreach (Guid key in _mapFixSessions.Keys)
                    {
                        _mapFixSessions[key].HandleShutdown();
                    }
                }
                else
                {
                    // REC: Adjust the service's current state:
                    _serviceState = VfxFixServiceStates.Service_State_Closed;
                    // REC: Adjust the service's current status:
                    _serviceStatus = VfxFixServiceStatus.Service_Status_Closed;
                    // REC: Dispatch the service stopped notification:
                    EventHandler<VfxFixServiceEventArgs> tmpDispatch = EventDispatch;
                    if (tmpDispatch != null)
                    {
                        VfxFixServiceEventArgs tmpArgs = new VfxFixServiceEventArgs(VfxFixServiceEventTypes.Event_Service_Stopped, _serviceStatus);
                        tmpDispatch(this, tmpArgs);
                    }

                }
            }
        }

        #endregion

        /// <summary>
        /// The HandleIpcDispatch event handler is invoked in response to
        /// an IPC session raising an event. The server service examines
        /// the token/sessionId that is contained in the event arguments
        /// and uses that information to demultiplex the details of the
        /// event to the corresponding FIX session.
        /// </summary>
        /// <param name="sender">
        /// The endpoint session that generated the event.
        /// </param>
        /// <param name="args">
        /// The details of the event that was generated.
        /// </param>
        private void HandleIpcDispatch(object sender, VfxIpcEventArgs args)
        {
            switch (args.EventType)
            {
                case VfxIpcEventTypes.Event_Session_Opened:
                    HandleIpcDispatch_EventOpened(sender, args);
                    break;
                case VfxIpcEventTypes.Event_Session_Closed:
                    HandleIpcDispatch_EventClosed(sender, args);
                    break;
                case VfxIpcEventTypes.Event_Session_Message:
                    HandleIpcDispatch_EventMessage(sender, args);
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// The HandleIpcDispatch_EventOpened event handler is invoked in
        /// response to a new session being established between a peer
        /// system and the endpoint associated with the service.
        /// </summary>
        /// <param name="sender">
        /// The IPC endpoint that dispatched the event.
        /// </param>
        /// <param name="args">
        /// The details of the event being dispatched.
        /// </param>
        private void HandleIpcDispatch_EventOpened(object sender, VfxIpcEventArgs args)
        {
            // REC: Create a new instance of a FIX session to handle
            // the communication between the server and the peer:
            IVfxFixSession fixSession = null;
            
            // REC: Retrieve the version definition from the version
            // definition registry and determine what kind of session
            // needs to be created in order to handle the connection:
            IVfxFixVxRegistry vxRegistry = this._localServices.GetService(typeof(IVfxFixVxRegistry)) as IVfxFixVxRegistry;
            
            // REC: Check the version definition in order to tell if
            // this is a FIX 4.x or FIX 5.x service:
            VfxFixVxRecord vxRecord = vxRegistry.Get(this._sxVersion);
            if (vxRecord.Layer.ToLower().CompareTo("combined") == 0)
            {
                fixSession = new VfxFix4xServerSession();
            }
            else
            {
                fixSession = new VfxFix5xServerSession();
            }


            // REC: Initialize the session:
            fixSession.Init(_localServices, this);

            // REC: Create an entry for the session in the
            // local session map:
            _mapFixSessions.Add(fixSession.InstanceId, fixSession);

            // REC: Bind the FIX session to the IPC session:
            _mapFixToIpc.Add(fixSession.InstanceId, args.Token);
            // REC: Bind the IPC session to the FIX session:
            _mapIpcToFix.Add(args.Token, fixSession.InstanceId);
            
            // REC: Create the application session container
            // that will be passed to the app implementation
            // when events are generated on the session:
            VfxFixServerSession appSession = new VfxFixServerSession(fixSession);

            // REC: Create a binding between the FIX session
            // and the application session so that events from
            // the FIX session can be correlated to the correct
            // application session when events are dispatched to
            // the user's FIX application instance:
            _mapAppSessions.Add(fixSession.InstanceId, appSession);

            // REC: Inform the session that a peer system
            // has been connected to it:
            fixSession.HandleConnect();
        }

        private void HandleIpcDispatch_EventMessage(object sender, VfxIpcEventArgs args)
        {
            if (_mapIpcToFix.ContainsKey(args.Token))
            {
                _mapFixSessions[_mapIpcToFix[args.Token]].HandleRxMessage(args.EventData);
            }
        }

        /// <summary>
        /// The HandleIpcDispatch_EventClosed event handler is invoked
        /// when an IPC session disconnects from a peer system.
        /// </summary>
        /// <param name="sender">
        /// The IPC session that dispatched the event.
        /// </param>
        /// <param name="args">
        /// The details of the event being dispatched.
        /// </param>
        private void HandleIpcDispatch_EventClosed(object sender, VfxIpcEventArgs args)
        {
            // REC: Determine if there is currently an instance of
            // a FIX session associated with the IPC session:
            if (_mapIpcToFix.ContainsKey(args.Token))
            {
                // REC: Remove the binding between the FIX session
                // and the IPC session, since the IPC session has been
                // disconnected and can not be used by the FIX session
                // for any other operations:
                if (_mapFixToIpc.ContainsKey(_mapIpcToFix[args.Token]))
                {
                    _mapFixToIpc.Remove(_mapIpcToFix[args.Token]);
                }

                // REC: Notify the associated FIX session that the
                // connection has been broken:
                _mapFixSessions[_mapIpcToFix[args.Token]].HandleDisconnect();

            }
        }

        #region IVfxFixSessionHandler Members

        /// <summary>
        /// The OnSessionStarted callback method is invoked by an
        /// instance of a session when it is activated.
        /// </summary>
        /// <param name="session">
        /// The FIX session that the event relates to.
        /// </param>
        public void OnSessionOpened(IVfxFixSession session)
        {
            if (_mapAppSessions.ContainsKey(session.InstanceId))
            {
                IVfxFixAppSession appSession = _mapAppSessions[session.InstanceId];
                _application.OnSessionOpened(appSession);
            }
        }

        /// <summary>
        /// The OnSessionLogon callback method is invoked by an
        /// instance of a session when it completes a logon with
        /// the peer system it's communicating with.
        /// </summary>
        /// <param name="session">
        /// The IVfxFixSession implementation that is issuing
        /// the callback to the handler.
        /// </param>
        /// <param name="msg">
        /// The FIX logon message (request or response) that was
        /// received from the peer session.
        /// </param>
        public void OnSessionLogon(IVfxFixSession session, FixMessage msg)
        {
            // REC: Forward the callback to the appropriate method
            // on the application's callback interface:
            if (_mapAppSessions.ContainsKey(session.InstanceId))
            {
                IVfxFixAppSession appSession = _mapAppSessions[session.InstanceId];
                _application.OnSessionLogon(appSession, msg);
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
            //throw new NotImplementedException();
        }


        /// <summary>
        /// The OnSessionLogout callback method is invoked by an
        /// instance of a session when it completes a logout with
        /// the peer system it's communicating with.
        /// </summary>
        /// <param name="session">
        /// The IVfxFixSession implementation that is issuing
        /// the callback to the handler.
        /// </param>
        /// <param name="msg">
        /// The FIX logout message (request or response) that was
        /// received from the peer session.
        /// </param>
        public void OnSessionLogout(IVfxFixSession session, FixMessage msg)
        {
            if (_mapAppSessions.ContainsKey(session.InstanceId))
            {
                IVfxFixAppSession appSession = _mapAppSessions[session.InstanceId];
                _application.OnSessionLogout(appSession, msg);
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
            if (_mapAppSessions.ContainsKey(session.InstanceId))
            {
                IVfxFixAppSession appSession = _mapAppSessions[session.InstanceId];
                _application.OnSessionRxAdmMessage(appSession, msg);
            }
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
            if (_mapAppSessions.ContainsKey(session.InstanceId))
            {
                IVfxFixAppSession appSession = _mapAppSessions[session.InstanceId];
                _application.OnSessionRxAppMessage(appSession, msg);
            }
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
            if (_mapAppSessions.ContainsKey(session.InstanceId))
            {
                IVfxFixAppSession appSession = _mapAppSessions[session.InstanceId];
                _application.OnSessionTxAdmMessage(appSession, msg);

                // REC: Locate the IPC session identifier of the IPC session
                // that is associated with the service that is generating the
                // event and use that to send the message to the peer:
                if (_mapFixToIpc.ContainsKey(session.InstanceId))
                {
                    string ipcIdentifier = _mapFixToIpc[session.InstanceId];
                    _endpoint.Send(ipcIdentifier, new VfxMsgBlock(msg.ToString()));
                }
            }
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
            if (_mapAppSessions.ContainsKey(session.InstanceId))
            {
                IVfxFixAppSession appSession = _mapAppSessions[session.InstanceId];
                _application.OnSessionTxAppMessage(appSession, msg);

                // REC: Locate the IPC session identifier of the IPC session
                // that is associated with the service that is generating the
                // event and use that to send the message to the peer:
                if (_mapFixToIpc.ContainsKey(session.InstanceId))
                {
                    string ipcIdentifier = _mapFixToIpc[session.InstanceId];
                    _endpoint.Send(ipcIdentifier, new VfxMsgBlock(msg.ToString()));
                }
            }
        }

        /// <summary>
        /// The OnSessionStopped callback method is invoked in response
        /// to an instance of a session being shutdown.
        /// </summary>
        /// <param name="session">
        /// The FIX session that the event relates to.
        /// </param>
        public void OnSessionClosed(IVfxFixSession session)
        {
            // REC: Determine which application session corresponds to
            // the FIX session that the event is coming from and route
            // the appropriate notification to it.
            if (_mapAppSessions.ContainsKey(session.InstanceId))
            {
                IVfxFixAppSession appSession = _mapAppSessions[session.InstanceId];
                _application.OnSessionClosed(appSession);
            }


            // REC: If there is currently an IPC session in place
            // that corresponds to the FIX session, it needs to be
            // disconnected at this point:
            if (_mapFixToIpc.ContainsKey(session.InstanceId))
            {
                _endpoint.Shutdown(_mapFixToIpc[session.InstanceId]);
            }
            else
            {
                // REC: Since the FIX session has been closed, it can
                // be removed from the active sessions map:
                if (_mapFixSessions.ContainsKey(session.InstanceId))
                {
                    _mapFixSessions.Remove(session.InstanceId);
                }
            }

            if (_serviceState == VfxFixServiceStates.Service_State_Closing)
            {
                if (_mapFixSessions.Count == 0)
                {
                    // REC: Adjust the service's state:
                    _serviceState = VfxFixServiceStates.Service_State_Closed;
                    // REC: Adjust the service's status:
                    _serviceStatus = VfxFixServiceStatus.Service_Status_Closed;
                    // REC: Dispatch the closed event:
                    EventHandler<VfxFixServiceEventArgs> tmpDispatch = EventDispatch;
                    if (tmpDispatch != null)
                    {
                        VfxFixServiceEventArgs args = new VfxFixServiceEventArgs(VfxFixServiceEventTypes.Event_Service_Stopped, _serviceStatus);
                        tmpDispatch(this, args);
                    }
                }
            }
        }

        #endregion
    }
}
