using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Models
{
    public abstract class BookForManipulationDto
    {

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        [MaxLength(500)]
        public virtual string Description { get; set; }
    }
}
