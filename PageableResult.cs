namespace BackOffice.Authorizer.Management.Domains
{
    public class PageableResult
    {
        public static PageableResult From(int start, int totalPerPages, long total, object[] data)
        {
            return new PageableResult(start, totalPerPages, total, data);
        }

        public int Start { get; private set; }
        public int TotalPerPages { get; private set; }
        public long Total { get; private set; }
        public object[] Data { get; private set; }

        public PageableResult(int start, int totalPerPages, long total, object[] data)
        {
            Start = start;
            TotalPerPages = totalPerPages;
            Total = total;
            Data = data;
        }
    }
}