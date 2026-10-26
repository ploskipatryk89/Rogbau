namespace Rogbau.Api.Features.EquipmentTransfer.Dtos
{
    public class EditPzItemDto
    {
        public Guid EquipmentId { get; set; }
        public int Quantity { get; set; }
        public int ScrappedQuantity { get; set; }
        public int? ReceivedQuantity { get; set; }
        public int? ReceivedScrappedQuantity { get; set; }
    }
}
