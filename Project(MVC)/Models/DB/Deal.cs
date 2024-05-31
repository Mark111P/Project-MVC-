namespace Project_MVC_.Models.DB
{
    public class Deal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int GroupId { get; set; }
        public virtual Group Group { get; set; }
        public DateTime Time { get; set; }
        public int Price { get; set; }
        public string Status { get; set; }
    }
}
