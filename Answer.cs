namespace BackOffice.Authorizer.Management.Domains
{
    public class Answer
    {
        public Question Question { get; set; }
        public int Id { get; set; }
        public string Text { get; set; }
        public bool AllowFreeText { get; set; }

        public bool Default { get; set; }

        public int Weight { get; set; }
    }
}
