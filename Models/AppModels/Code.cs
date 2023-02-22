using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimesheetBE.Models.IdentityModels;

namespace TimesheetBE.Models.AppModels
{
    public class Code : BaseModel
    {
        [Required]
        [MaxLength(50)]
        [Column(TypeName = "varchar")]
        public string CodeString { get; set; }
        [MaxLength(50)]
        [Column(TypeName = "varchar")]
        public string Key { get; set; }
        public Guid? UserId { get; set; }
        public User User { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsExpired { get; set; }
        public string Token { get; set; }
    }
}