﻿#region Copyright (C) 2007-2017 Team MediaPortal

/*
    Copyright (C) 2007-2017 Team MediaPortal
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
using System.Collections.Generic;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.ResourceAccess;
using MediaPortal.Plugins.MP2Extended.TAS.Tv;
using MediaPortal.Plugins.SlimTv.Interfaces.Aspects;
using MediaPortal.Utilities;
using MP2Extended.Extensions;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.TAS.Recording.BaseClasses
{
  class BaseRecordingBasic
  {
    internal WebRecordingBasic RecordingBasic(MediaItem item)
    {
      MediaItemAspect recordingAspect = item.GetAspect(RecordingAspect.Metadata);
      string path = "";
      if (item.PrimaryResources.Count > 0)
      {
        ResourcePath resourcePath = ResourcePath.Deserialize(item.PrimaryProviderResourcePath());
        path = resourcePath.PathSegments.Count > 0 ? StringUtils.RemovePrefixIfPresent(resourcePath.LastPathSegment.Path, "/") : string.Empty;
      }
      string genre = string.Empty;
      if (MediaItemAspect.TryGetAttribute(item.Aspects, GenreAspect.ATTR_GENRE, out List<string> genres))
        genre = string.Join(", ", genres);

      return new WebRecordingBasic
      {
        Id = item.MediaItemId.ToString(),
        Title = (string)item.GetAspect(MediaAspect.Metadata).GetAttributeValue(MediaAspect.ATTR_TITLE),
        ChannelName = (string)recordingAspect.GetAttributeValue(RecordingAspect.ATTR_CHANNEL),
        Description = (string)item.GetAspect(VideoAspect.Metadata).GetAttributeValue(VideoAspect.ATTR_STORYPLOT),
        StartTime = (DateTime) (recordingAspect.GetAttributeValue(RecordingAspect.ATTR_STARTTIME) ?? DateTime.Now),
        EndTime = (DateTime) (recordingAspect.GetAttributeValue(RecordingAspect.ATTR_ENDTIME) ?? DateTime.Now),
        Genre = genre,
        TimesWatched = (int)(item.GetAspect(MediaAspect.Metadata)[MediaAspect.ATTR_PLAYCOUNT] ?? 0),
        FileName = path,
      };
    }
  }
}
