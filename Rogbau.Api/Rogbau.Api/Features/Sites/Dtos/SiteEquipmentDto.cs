namespace Rogbau.Api.Features.Sites.Dtos
{
    public record SiteEquipmentDto(
     Guid EquipmentId,
     string EquipmentName,
     string Unit,
     decimal AllocatedQuantity,
     decimal PurchaseTotalPrice,
     
decimal WeightKg

 // Ile tego sprzętu jest obecnie na tej budowie
 );
}
