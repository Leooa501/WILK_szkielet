namespace WILK.Models
{
    /// <summary>
    /// Represents a component reservation in the production system
    /// </summary>
    public class Reservation
    {
        public int Id { get; set; }
        public int ComponentId { get; set; }
        public int Quantity { get; set; }
        public int? ListId { get; set; } 

        public Reservation(int id, int componentId, int quantity, int? listId = null)
        {
            Id = id;
            ComponentId = componentId;
            Quantity = quantity;
            ListId = listId;
        }
    }
}