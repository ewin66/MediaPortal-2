﻿#region Copyright (C) 2007-2011 Team MediaPortal

/*
    Copyright (C) 2007-2011 Team MediaPortal
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

using System;
using MediaPortal.Core;
using MediaPortal.Core.MediaManagement;
using MediaPortal.Core.MediaManagement.DefaultItemAspects;
using MediaPortal.Plugins.SlimTv.Providers;
using MediaPortal.Plugins.SlimTvClient.Interfaces;
using MediaPortal.Plugins.SlimTvClient.Interfaces.Items;
using MediaPortal.Plugins.SlimTvClient.Interfaces.LiveTvMediaItem;
using MediaPortal.UI.Presentation.Players;
using MediaPortal.UiComponents.Media.Models;

namespace MediaPortal.Plugins.SlimTvClient
{
  public class SlimTvHandler: ITvHandler
  {
    private struct TVSlotContext
    {
      public string AccessorPath;
      public IChannel Channel;
    }
    private ITvProvider _tvProvider;
    private readonly TVSlotContext[] _slotContexes = new TVSlotContext[2];


    public SlimTvHandler()
    {
      _tvProvider = new SlimTv4HomeProvider();
      _tvProvider.Init();
    }

    public ITimeshiftControl TimeshiftControl
    {
      get { return _tvProvider as ITimeshiftControl; }
    }

    public IChannelAndGroupInfo ChannelAndGroupInfo
    {
      get { return _tvProvider as IChannelAndGroupInfo; }
    }
    
    public IProgramInfo ProgramInfo
    {
      get { return _tvProvider as IProgramInfo; }
    }

    public IProgram CurrentProgram
    {
      get { return GetCurrentProgram(GetChannel(PlayerManagerConsts.PRIMARY_SLOT)); }
    }

    public IProgram NextProgram
    {
      get { return GetNextProgram(GetChannel(PlayerManagerConsts.PRIMARY_SLOT)); }
    }

    public IProgram GetCurrentProgram(IChannel channel)
    {
      IProgram currentProgram;
      if (ProgramInfo != null && ProgramInfo.GetCurrentProgram(channel, out currentProgram))
        return currentProgram;

      return null;
    }

    public IProgram GetNextProgram(IChannel channel)
    {
      IProgram nextProgram;
      if (ProgramInfo != null && ProgramInfo.GetNextProgram(channel, out nextProgram))
        return nextProgram;

      return null;
    }

    /// <summary>
    /// Gets a value how many slots are currently used for timeshifting (0..2).
    /// </summary>
    public int NumberOfActiveSlots
    {
      get { return (_slotContexes[0].Channel == null ? 0 : 1) + (_slotContexes[1].Channel == null ? 0 : 1); }
    }

    private int GetMatchingSlotIndex(int requestedSlotIndex)
    {
      if (requestedSlotIndex == 0)
        return 0;

      if (requestedSlotIndex == 1)
      {
        // if MasterStream in PiP mode was stopped, but PiP Stream still active, we fill the 0 slotindex.
        if (_slotContexes[0].Channel == null && _slotContexes[1].Channel != null)
          return 0;

        if (_slotContexes[0].Channel != null)
          return 1;

      }
      return 0;
    }

    private PlayerContextConcurrencyMode GetMatchingPlayMode()
    {
      // no tv slots active? the stop all and play.
      if (NumberOfActiveSlots == 0)
        return PlayerContextConcurrencyMode.None;

      return PlayerContextConcurrencyMode.ConcurrentVideo;
    }

    public IChannel GetChannel(int slotIndex)
    {
      if (TimeshiftControl == null)
        return null;

      return TimeshiftControl.GetChannel(GetMatchingSlotIndex(slotIndex));
    }

    public bool StartTimeshift(int slotIndex, IChannel channel)
    {
      if (TimeshiftControl == null || channel == null)
        return false;

      int newSlotIndex = GetMatchingSlotIndex(slotIndex);
      MediaItem timeshiftMediaItem;
      bool result = TimeshiftControl.StartTimeshift(newSlotIndex, channel, out timeshiftMediaItem);
      if (result && timeshiftMediaItem != null)
      {
        string newAccessorPath =
          (string) timeshiftMediaItem.Aspects[ProviderResourceAspect.ASPECT_ID].GetAttributeValue(
            ProviderResourceAspect.ATTR_RESOURCE_ACCESSOR_PATH);
        
        if (_slotContexes[slotIndex].AccessorPath != newAccessorPath)
        {
          AddTimeshiftContext(timeshiftMediaItem as LiveTvMediaItem, channel);
          PlayerContextConcurrencyMode playMode = GetMatchingPlayMode();
          PlayItemsModel.PlayOrEnqueueItem(timeshiftMediaItem, true, playMode);
        }
        else
        {
          UpdateExistingMediaItem(timeshiftMediaItem);
        }
        _slotContexes[slotIndex].AccessorPath = newAccessorPath;
        _slotContexes[slotIndex].Channel = channel;
      }

      return result;
    }

    private void AddTimeshiftContext(LiveTvMediaItem timeshiftMediaItem, IChannel channel)
    {
      IProgram program = GetCurrentProgram(channel);
      TimeshiftContext tsContext = new TimeshiftContext
                                     {
                                       Channel = channel,
                                       Program = program,
                                       TuneInTime = DateTime.Now
                                     };

      int tc = timeshiftMediaItem.TimeshiftContexes.Count;
      if (tc > 0)
      {
        ITimeshiftContext lastContext = timeshiftMediaItem.TimeshiftContexes[tc - 1];
        lastContext.TimeshiftDuration = DateTime.Now - lastContext.TuneInTime;
      }
      timeshiftMediaItem.TimeshiftContexes.Add(tsContext);
    }

    private void UpdateExistingMediaItem(MediaItem timeshiftMediaItem)
    {
      IPlayerContextManager playerContextManager = ServiceRegistration.Get<IPlayerContextManager>();
      for (int index = 0; index < playerContextManager.NumActivePlayerContexts; index++)
      {
        IPlayerContext playerContext = playerContextManager.GetPlayerContext(index);
        if (playerContext != null)
        {
          LiveTvMediaItem liveTvMediaItem = playerContext.CurrentMediaItem as LiveTvMediaItem;
          LiveTvMediaItem newLiveTvMediaItem = timeshiftMediaItem as LiveTvMediaItem;
          if (liveTvMediaItem != null && newLiveTvMediaItem != null)
          {
            // check if this is "our" media item to update.
            if (liveTvMediaItem.Aspects[ProviderResourceAspect.ASPECT_ID].GetAttributeValue(ProviderResourceAspect.ATTR_RESOURCE_ACCESSOR_PATH).ToString() !=
              newLiveTvMediaItem.Aspects[ProviderResourceAspect.ASPECT_ID].GetAttributeValue(ProviderResourceAspect.ATTR_RESOURCE_ACCESSOR_PATH).ToString())
              continue;

            AddTimeshiftContext(liveTvMediaItem, newLiveTvMediaItem.AdditionalProperties[LiveTvMediaItem.CHANNEL] as IChannel);
          }
        }
      }
    }


    public bool StopTimeshift(int slotIndex)
    {
      if (TimeshiftControl == null)
        return false;

      _slotContexes[slotIndex].AccessorPath = null;
      _slotContexes[slotIndex].Channel = null;

      return TimeshiftControl.StopTimeshift(slotIndex);
    }

    #region IDisposable Member

    public void Dispose()
    {
      if (_tvProvider != null)
      {
        _tvProvider.DeInit();
        _tvProvider = null;
      }
    }

    #endregion
  }
}