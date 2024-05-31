namespace Project_MVC_.Models.DB
{
    public class Member
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int GroupId { get; set; }
        public virtual Group Group { get; set; }
        public string Role { get; set; }
        public string CanDo { get; set; }
    }
}
