using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.API.Models
{
    public class EmployeeDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        /// <summary>
        /// Category of document e.g. NationalID, Passport, Certificate, Resume, Other
        /// </summary>
        [Required]
        [StringLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        /// <summary>Original filename supplied by the uploader.</summary>
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        /// <summary>Unique filename used to store the file on disk.</summary>
        [Required]
        [StringLength(500)]
        public string StoredFileName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ContentType { get; set; }

        public long FileSizeBytes { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
