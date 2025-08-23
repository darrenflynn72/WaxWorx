using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaxWorx.Data.Entities
{
    public class UserSetting : AuditBaseEntity
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } = 1;

        [Required]
        [MaxLength(20)]
        public string Symbol { get; set; }

        [MaxLength(256)]
        public string? DisplayName { get; set; }
    }
}
