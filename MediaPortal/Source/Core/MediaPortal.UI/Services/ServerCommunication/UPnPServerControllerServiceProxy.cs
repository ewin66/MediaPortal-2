#region Copyright (C) 2007-2012 Team MediaPortal

/*
    Copyright (C) 2007-2012 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System.Collections.Generic;
using MediaPortal.Common.ClientCommunication;
using MediaPortal.Common.General;
using MediaPortal.Common.UPnP;
using MediaPortal.UI.ServerCommunication;
using MediaPortal.Utilities.UPnP;
using UPnP.Infrastructure.CP.DeviceTree;

namespace MediaPortal.UI.Services.ServerCommunication
{
  /// <summary>
  /// Encapsulates the MediaPortal 2 UPnP client's proxy for the ServerController service.
  /// </summary>
  public class UPnPServerControllerServiceProxy : UPnPServiceProxyBase, IServerController
  {
    protected const string SV_ATTACHED_CLIENTS = "AttachedClients";
    protected const string SV_CONNECTED_CLIENTS = "ConnectedClients";

    public UPnPServerControllerServiceProxy(CpService serviceStub) : base(serviceStub, "ServerController")
    {
      serviceStub.StateVariableChanged += OnStateVariableChanged;
      serviceStub.SubscribeStateVariables();
    }

    private void OnStateVariableChanged(CpStateVariable statevariable)
    {
      if (statevariable.Name == SV_ATTACHED_CLIENTS)
        FireAttachedClientsChanged();
      else if (statevariable.Name == SV_CONNECTED_CLIENTS)
        FireConnectedClientsChanged();
    }

    protected void FireAttachedClientsChanged()
    {
      ParameterlessMethod dlgt = AttachedClientsChanged;
      if (dlgt != null)
        dlgt();
    }

    protected void FireConnectedClientsChanged()
    {
      ParameterlessMethod dlgt = ConnectedClientsChanged;
      if (dlgt != null)
        dlgt();
    }

    #region State variables

    // We don't make those events available via the public interface because .net event registrations are not allowed between MP2 modules.
    // It is the job of the class which instanciates this class to publicize those events.

    public event ParameterlessMethod AttachedClientsChanged;
    public event ParameterlessMethod ConnectedClientsChanged;

    #endregion

    public ICollection<MPClientMetadata> AttachedClients
    {
      get
      {
        CpStateVariable variable = GetStateVariable(SV_ATTACHED_CLIENTS);
        return (ICollection<MPClientMetadata>) variable.Value;
      }
    }

    public ICollection<string> ConnectedClients
    {
      get
      {
        CpStateVariable variable = GetStateVariable(SV_CONNECTED_CLIENTS);
        return MarshallingHelper.ParseCsvStringCollection((string) variable.Value);
      }
    }

    public void AttachClient(string clientSystemId)
    {
      CpAction action = GetAction("AttachClient");
      action.InvokeAction(new List<object> {clientSystemId});
    }

    public void DetachClient(string clientSystemId)
    {
      CpAction action = GetAction("DetachClient");
      action.InvokeAction(new List<object> {clientSystemId});
    }

    public SystemName GetSystemNameForSystemId(string systemId)
    {
      CpAction action = GetAction("GetSystemNameForSystemId");
      IList<object> outParams = action.InvokeAction(new List<object> {systemId});
      string hostName = (string) outParams[0];
      if (string.IsNullOrEmpty(hostName))
        return null;
      return new SystemName(hostName);
    }

    // TODO: State variables, if present
  }
}
