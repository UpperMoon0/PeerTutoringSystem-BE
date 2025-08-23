using System.ComponentModel.DataAnnotations;

namespace PeerTutoringSystem.Domain.Common
{
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
    }
}