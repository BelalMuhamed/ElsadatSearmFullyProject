using Domain.Entities.Commonitems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Users;
public class Notification:BaseEntity
{
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Type { get; set; } = string.Empty;
    public int? RelatedEntityId { get; set; }

    public string UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
