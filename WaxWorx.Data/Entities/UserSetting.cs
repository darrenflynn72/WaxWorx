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
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Value { get; set; }
    }
}
