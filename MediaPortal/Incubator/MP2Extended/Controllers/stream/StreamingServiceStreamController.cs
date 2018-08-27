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
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.Services.ResourceAccess;
using MediaPortal.Plugins.MP2Extended.Common;
using MediaPortal.Plugins.MP2Extended.MAS.OnlineVideos;
using MediaPortal.Plugins.MP2Extended.ResourceAccess.WSS.stream.Control;
using MediaPortal.Plugins.MP2Extended.ResourceAccess.WSS.stream.General;
using MediaPortal.Plugins.MP2Extended.ResourceAccess.WSS.stream.Images;

namespace MediaPortal.Plugins.MP2Extended.Controllers.stream
{
  [RoutePrefix("MPExtended/StreamingService/stream")]
  [Route("{action}")]
  [MediaPortalAuthorize]
  public class StreamingServiceStreamController : ApiController
  {
    #region General

    [HttpGet]
    [ApiExplorerSettings]
    public async Task GetMediaItem(Guid itemId, long? startPosition)
    {
      Logger.Debug("WSS Request: {0}", Request.GetOwinContext().Request.Uri);
      await new GetMediaItem().ProcessAsync(Request.GetOwinContext(), itemId);
    }

    [HttpGet]
    [ApiExplorerSettings]
    public async Task GetHTMLResource(string path)
    {
      Logger.Debug("WSS Request: {0}", Request.GetOwinContext().Request.Uri);
      await new GetHtmlResource().ProcessAsync(Request.GetOwinContext(), path);
    }

    #endregion

    #region Control

    [HttpGet]
    [ApiExplorerSettings]
    public async Task RetrieveStream(string identifier, string file, string hls)
    {
      Logger.Debug("WSS Request: {0}", Request.GetOwinContext().Request.Uri);
      await new RetrieveStream().ProcessAsync(Request.GetOwinContext(), identifier, file, hls);
    }

    [HttpGet]
    [ApiExplorerSettings]
    public Task DoStream(WebMediaType type, int? provider, string itemId, string clientDescription, string profileName, long startPosition, int? idleTimeout)
    {
      Logger.Debug("WSS Request: {0}", Request.GetOwinContext().Request.Uri);
      throw new NotImplementedException();
    }

    [HttpGet]
    [ApiExplorerSettings]
    public Task CustomTranscoderData(string identifier, string action, string parameters)
    {
      Logger.Debug("WSS Request: {0}", Request.GetOwinContext().Request.Uri);
      throw new NotImplementedException();
    }

    #endregion

    #region Images

    [HttpGet]
    [ApiExplorerSettings]
    public async Task ExtractImage(WebMediaType type, string itemId)
    {
      Logger.Debug("WSS Request: {0}", Request.GetOwinContext().Request.Uri);
      await new ExtractImage().ProcessAsync(Request.GetOwinContext(), type, itemId);
    }

    [HttpGet]
    [ApiExplorerSettings]
    public async Task ExtractImageResized(WebMediaType type, string itemId, int maxWidth, int maxHeight, string borders = null)
    {
      Logger.Debug("WSS Request: {0}", Request.GetOwinContext().Request.Uri);
      await new ExtractImageResized().ProcessAsync(Request.GetOwinContext(), type, itemId, maxWidth, maxHeight, borders);
    }

    [HttpGet]
    [ApiExplorerSettings]
    public async Task GetImage(WebMediaType type, string id)
    {
      Logger.Debug("WSS Request: {0}", Request.GetOwinContext().Request.Uri);
      await new GetImage().ProcessAsync(Request.GetOwinContext(), type, id);
    }

    [HttpGet]
    [ApiExplorerSettings]
    public async Task GetImageResized(WebMediaType type, string id, int maxWidth, int maxHeight, string borders = null)
    {
      Logger.Debug("WSS Request: {0}", Request.GetOwinContext().Request.Uri);
      await new GetImageResized().ProcessAsync(Request.GetOwinContext(), type, id, maxWidth, maxHeight, borders);
    }

    [HttpGet]
    [ApiExplorerSettings]
    public async Task GetArtwork(WebMediaType mediatype, string id, WebFileType artworktype, int offset = 0)
    {
      Logger.Debug("WSS Request: {0}", Request.GetOwinContext().Request.Uri);
      await new GetArtwork().ProcessAsync(Request.GetOwinContext(), mediatype, id, artworktype, offset);
    }

    [HttpGet]
    [ApiExplorerSettings]
    public async Task GetArtworkResized(WebMediaType mediatype, string id, WebFileType artworktype, int maxWidth, int maxHeight, int offset = 0, string borders = null)
    {
      Logger.Debug("WSS Request: {0}", Request.GetOwinContext().Request.Uri);
      await new GetArtworkResized().ProcessAsync(Request.GetOwinContext(), mediatype, id, artworktype, offset, maxWidth, maxHeight, borders);
    }

    [HttpGet]
    [ApiExplorerSettings]
    public async Task GetOnlineVideosArtwork(WebOnlineVideosMediaType mediatype, string id)
    {
      Logger.Debug("WSS Request: {0}", Request.GetOwinContext().Request.Uri);
      await new GetOnlineVideosArtwork().ProcessAsync(Request.GetOwinContext(), mediatype, id);
    }

    [HttpGet]
    [ApiExplorerSettings]
    public async Task GetOnlineVideosArtworkResized(WebOnlineVideosMediaType mediatype, string id, int maxWidth, int maxHeight, string borders = null)
    {
      Logger.Debug("WSS Request: {0}", Request.GetOwinContext().Request.Uri);
      await new GetOnlineVideosArtworkResized().ProcessAsync(Request.GetOwinContext(), mediatype, id, maxWidth, maxHeight, borders);
    }

    #endregion

    internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }
  }
}
