namespace Project_MVC_.Models.DB
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Login {  get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Adress { get; set; }
        public virtual List<Member> Members { get; set; }
        public virtual List<Invite> Invites { get; set; }
        public User() 
        { 
            Members = new List<Member>();
            Invites = new List<Invite>();
        }
    }
}
