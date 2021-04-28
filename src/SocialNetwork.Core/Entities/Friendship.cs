using System;

namespace SocialNetwork.Core.Entities
{
    public class Friendship
    {
        public long RequesterId { get; set; }
        public long AddresseeId { get; set; }
        public DateTime Created { get; set; }
        public FriendshipStatus Status { get; set; }
    }

    public enum FriendshipStatus
    {
        RequestSent,
        RequestAccepted,
        RemovedByRequester,
        RemovedByAddressee
    }
}