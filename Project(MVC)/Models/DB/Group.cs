namespace Project_MVC_.Models.DB
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<Member> Members { get; set; }
        public virtual List<Invite> Invites { get; set; }
        public virtual List<Meeting> Meetings { get; set; }
        public virtual List<Deal> Deals { get; set; }
        public Group()
        {
            Members = new List<Member>();
            Invites = new List<Invite>();
            Meetings = new List<Meeting>();
            Deals = new List<Deal>();
        }
    }
}
