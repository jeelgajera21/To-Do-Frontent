namespace ToDo_API.Models
{
    public class CategoryModel
    {
        public int? CategoryID { get; set; }
        public int UserID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
