using BackOffice.Authorizer.Management.Domains.Validators;
using System;

namespace BackOffice.Authorizer.Management.Domains
{
    public class Approval
    {
        public int Id { get; private set; }
		public int AcquirerCode { get; private set; }
		public string RequestType { get; private set; }
		public int RequestId { get; private set; }
		public string Status { get; private set; }
		public string RequestInfo { get; private set; }
        public DateTime RecInsTime { get; private set; }
        public string RecInsUser { get; private set; }
        public DateTime RecUpdTime { get; private set; }
        public string RecUpdUser { get; private set; }

        public Approval(int id, int acquirerCode, string requestType, int requestId, string status, string requestInfo, DateTime recInsTime, string recInsUser, DateTime recUpdTime, string recUpdUser)
        {
            Id = id;
            AcquirerCode = acquirerCode;
            RequestType = requestType ?? throw new ArgumentNullException(nameof(requestType));
            RequestId = requestId;
            Status = status ?? throw new ArgumentNullException(nameof(status));
            RequestInfo = requestInfo ?? throw new ArgumentNullException(nameof(requestInfo));
            RecInsTime = recInsTime;
            RecInsUser = recInsUser ?? throw new ArgumentNullException(nameof(recInsUser));
            RecUpdTime = recUpdTime;
            RecUpdUser = recUpdUser ?? throw new ArgumentNullException(nameof(recUpdUser));
        }

        public void Decline(string user, string description)
        {
            this.RecUpdUser = user;
            this.RecUpdTime = DateTime.Now;
            this.RequestInfo = description;
            this.Status = "R";
        }

        public void Approve(string user)
        {
            this.RecUpdUser = user;
            this.RecUpdTime = DateTime.Now;
            this.Status = "A";
        }
    }
}
