using System;

namespace BackOffice.Authorizer.Management.Domains
{
    public class Question
    {
        public int Id { get; set; }
        public Company Company {get; set; }
        public string Text { get; set; }

        public Answer[] Answers { get; set; }
    }
}
