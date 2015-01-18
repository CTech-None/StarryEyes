﻿using System;
using System.Threading.Tasks;
using StarryEyes.Anomaly.TwitterApi.DataModels;
using StarryEyes.Anomaly.TwitterApi.Rest;
using StarryEyes.Anomaly.TwitterApi.Rest.Parameter;
using StarryEyes.Models.Accounting;

namespace StarryEyes.Models.Requests
{
    public sealed class UpdateRelationRequest : RequestBase<TwitterUser>
    {
        private readonly long _userId;
        private readonly RelationKind _kind;

        public UpdateRelationRequest(TwitterUser target, RelationKind kind)
            : this(target.Id, kind)
        {
        }

        public UpdateRelationRequest(long userId, RelationKind kind)
        {
            _userId = userId;
            _kind = kind;
        }

        public override Task<TwitterUser> Send(TwitterAccount account)
        {
            switch (_kind)
            {
                case RelationKind.Follow:
                    return account.CreateFriendshipAsync(new UserParameter(_userId));
                case RelationKind.Unfollow:
                    return account.DestroyFriendshipAsync(new UserParameter(_userId));
                case RelationKind.Block:
                    return account.CreateBlockAsync(new UserParameter(_userId));
                case RelationKind.ReportAsSpam:
                    return account.ReportSpamAsync(new UserParameter(_userId));
                case RelationKind.Unblock:
                    return account.DestroyBlockAsync(new UserParameter(_userId));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum RelationKind
    {
        Follow,
        Unfollow,
        Block,
        ReportAsSpam,
        Unblock
    }
}
