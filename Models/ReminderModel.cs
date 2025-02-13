namespace To_Do_UI.Models
{
    public class ReminderModel
    {
        public int? ReminderID { get; set; }
        public string? Title { get; set; }

        public int TaskID { get; set; }
        public DateTime ReminderTime { get; set; }
        public bool IsSent { get; set; }
    }
    public class TaskDropDownByUser
    {
        public int TaskID { get; set; }
        public string Title { get; set; }
    }
}
