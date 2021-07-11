using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Update;

namespace Library.API.Entities
{
    public class Book
    {
        [Key]       
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
        [Required]
        [MaxLength(13)]
        public string ISBN { get; set; }

        public string Edition { get; set; }
        public string Subject { get; set; }
        public string ISSN { get; set; }
        public string CallNumber { get; set; }
        public DateTime? DueTime { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime? ReserveDate { get; set; }
        public Guid ReaderId { get; set; }
        public Reader Reader { get; set; }

        [ForeignKey("AuthorId")]
        public Author Author { get; set; }

        public Guid AuthorId { get; set; }
    }

  
}
