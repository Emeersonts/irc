namespace BackOffice.Authorizer.Management.Domains
{
    public class SearchParameters
    {
        private static readonly int DefaultStartIndex = 1;
        private static readonly int DefaultTotalPerPages = 10;

        public static SearchParameters CreateSearchParameters(int startIndex, int totalPerPages)
        {
            return new SearchParameters(startIndex, totalPerPages);
        }

        public int StartIndex { get; private set; }
        public int TotalPerPages { get; private set; }
        public string FilterText { get; set; }
        public int End { get { return StartIndex + TotalPerPages; } }

        public SearchParameters(int startIndex, int totalPerPages)
        {
            StartIndex = startIndex <= 0 ? DefaultStartIndex : startIndex;
            TotalPerPages = totalPerPages <= 0 ? DefaultTotalPerPages : totalPerPages;
        }
    }
}
