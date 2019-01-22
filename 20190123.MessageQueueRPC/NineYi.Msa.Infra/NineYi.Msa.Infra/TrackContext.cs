using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NineYi.Msa.Infra
{
    public class TrackContext
    {
        #region shop context
        public int ShopId { get; set; }
        #endregion


        #region session context
        public string SessionId { get; set; }

        public string MemberId { get; set; }

        public string ChannelId { get; set; }

        #endregion


        #region tracking context
        public string RequestId { get; set; }
        #endregion


        //public static bool TryToContext(Dictionary<string, string> headers, TrackContext track)
        //{
        //}

        //public static bool TryToHeaders(TrackContext track, Dictionary<string, string> headers)
        //{
        //}

        public static bool TryToContext(IServiceProvider ioc, Dictionary<string, string> headers)
        {
            if (ioc == null) return false;

            var track = ioc.GetService<TrackContext>();

            if (headers == null) return false;
            if (track == null) return false;

            string value = null;

            if (headers.TryGetValue("x-shop-id", out value)) track.ShopId = int.Parse(value);
            if (headers.TryGetValue("x-member-id", out value)) track.MemberId = value;
            if (headers.TryGetValue("x-session-id", out value)) track.SessionId = value;
            if (headers.TryGetValue("x-channel-id", out value)) track.ChannelId = value;
            if (headers.TryGetValue("x-request-id", out value)) track.RequestId = value;

            return true;
        }

        public static bool TryToHeaders(IServiceProvider ioc, Dictionary<string, string> headers)
        {
            if (ioc == null) return false;

            var track = ioc.GetService<TrackContext>();

            if (headers == null) return false;
            if (track == null) return false;

            headers["x-shop-id"] = track.ShopId.ToString();
            headers["x-member-id"] = track.MemberId;
            headers["x-session-id"] = track.SessionId;
            headers["x-channel-id"] = track.ChannelId;
            headers["x-request-id"] = track.RequestId;

            return true;
        }
    }
}
