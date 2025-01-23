namespace ToDo_API.Models
{
    public class ReminderModel
    {
        public int? ReminderID { get; set; }
        public int TaskID { get; set; }
        public DateTime ReminderTime { get; set; }
        public bool IsSent { get; set; }
    }
}
