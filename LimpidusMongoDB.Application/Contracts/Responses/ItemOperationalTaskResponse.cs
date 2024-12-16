using LimpidusMongoDB.Application.Data.Entities;

namespace LimpidusMongoDB.Application.Contracts.Responses
{
    public class ItemOperationalTaskResponse
    {
        public ItemOperationalTaskResponse(string legacyId, string name, int orderBy)
        {
            LegacyId = legacyId;
            Name = name;
            OrderBy = orderBy;
        }

        public static implicit operator ItemOperationalTaskResponse(ItemOperationalTaskEntity itemOperationalTaskEntity)
        {
            return new ItemOperationalTaskResponse(itemOperationalTaskEntity.LegacyId, itemOperationalTaskEntity.Name, itemOperationalTaskEntity.OrderBy);
        }

        public string LegacyId { get; set; }
        public string Name { get; set; }
        public int OrderBy { get; set; }
    }
}