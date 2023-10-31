namespace BackOffice.Authorizer.Management.Domains
{
    // Classe criada por conta do retorno do procedimento GetUserList
    public class UserGetListReturn
    {
        public int start { get; set; }
        public int end { get; set; }
        public long totalItems { get; set; }
        public User[] items { get; set; }
    }
}
