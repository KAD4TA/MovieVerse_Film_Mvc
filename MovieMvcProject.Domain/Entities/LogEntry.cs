
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieMvcProject.Domain.Entities
{
    
    [Table("Logs", Schema = "dbo")]
    public class LogEntry
    {
        [Key]
        public int Id { get; set; }

        public string? Message { get; set; }

        public string? MessageTemplate { get; set; }

        [MaxLength(128)]
        public string? Level { get; set; }

       
        public DateTime TimeStamp { get; set; }

        public string? Exception { get; set; }

        public string? Properties { get; set; }

        public string? Environment { get; set; }

        public string? Application { get; set; }

        public string? DeviceType { get; set; }
    }
}