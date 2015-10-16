﻿using System;
using System.Threading.Tasks;
using StarryEyes.Anomaly.TwitterApi;
using StarryEyes.Anomaly.TwitterApi.DataModels;
using StarryEyes.Anomaly.TwitterApi.Rest;
using StarryEyes.Models.Accounting;
using StarryEyes.Models.Backstages.NotificationEvents.PostEvents;
using StarryEyes.Settings;

namespace StarryEyes.Models.Requests
{
    public sealed class TweetPostingRequest : RequestBase<TwitterStatus>
    {
        public override int RetryCount
        {
            get { return 0; }
        }

        public override double RetryDelaySec
        {
            get { return 0; }
        }

        private const string LimitMessage = "over daily status update limit";
        private readonly string _status;
        private readonly long? _inReplyTo;
        private readonly GeoLocationInfo _geoInfo;
        private readonly byte[] _attachedImageBin;

        public TweetPostingRequest(string status,
            TwitterStatus inReplyTo, GeoLocationInfo geoInfo,
            byte[] attachedImageBytes)
            : this(status, inReplyTo == null ? (long?)null : inReplyTo.Id,
                geoInfo, attachedImageBytes)
        {
        }

        public TweetPostingRequest(string status, long? inReplyTo,
            GeoLocationInfo geoInfo, byte[] attachedImageBytes)
        {
            _status = status;
            _inReplyTo = inReplyTo;
            _geoInfo = geoInfo;
            _attachedImageBin = attachedImageBytes;
        }

        public override async Task<TwitterStatus> Send(TwitterAccount account)
        {
            var latlong = _geoInfo == null ? null : Tuple.Create(_geoInfo.Latitude, _geoInfo.Longitude);
            Exception lastThrown;
            // make retweet
            var acc = account;
            do
            {
                try
                {
                    TwitterStatus result;
                    if (_attachedImageBin != null)
                    {
                        result = await acc.UpdateWithMediaAsync(
                            _status,
                            new[] { _attachedImageBin },
                            account.MarkMediaAsPossiblySensitive ? true : (bool?)null, // Inherit property
                            _inReplyTo,
                            latlong).ConfigureAwait(false);
                    }
                    else
                    {
                        result = await acc.UpdateAsync(
                            _status,
                            _inReplyTo,
                            latlong).ConfigureAwait(false);
                    }
                    BackstageModel.NotifyFallbackState(acc, false);
                    return result;
                }
                catch (TwitterApiException tae)
                {
                    if (tae.Message.Contains(LimitMessage))
                    {
                        BackstageModel.NotifyFallbackState(acc, true);
                        if (acc.FallbackAccountId != null)
                        {
                            // reached post limit, fallback
                            var prev = acc;
                            acc = Setting.Accounts.Get(acc.FallbackAccountId.Value);
                            BackstageModel.RegisterEvent(new FallbackedEvent(prev, acc));
                            lastThrown = tae;
                            continue;
                        }
                    }
                    throw;
                }
            } while (acc != null && acc.Id != account.Id); // fallback
            throw lastThrown;
        }
    }

    public sealed class GeoLocationInfo
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public override string ToString()
        {
            return "lat:" + Latitude.ToString("0.000") + ", long:" + Longitude.ToString("0.000");
        }
    }
}
