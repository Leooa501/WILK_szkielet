using System;

namespace WILK.Models
{
    /// <summary>
    /// Data transfer object for reservation list items displayed in UI
    /// </summary>
    public class ReservationItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsList { get; set; }
        public string Status { get; set; } = string.Empty;

        public override string ToString() => Name;
    }
}
