using LimpidusMongoDB.Application.Data.Entities;

namespace LimpidusMongoDB.Application.Contracts.Responses
{
    public class ItemHistoryResponse
    {
        public ItemHistoryResponse(string legacyId, string name, string endDate, bool performed, string description, int orderBy)
        {
            LegacyId = legacyId;
            Name = name;
            Description = description;
            OrderBy = orderBy;
            EndDate = endDate;
            Performed = performed;
        }

        public static implicit operator ItemHistoryResponse(ItemHistoryEntity itemHistoryEntity)
        {
            return new ItemHistoryResponse(itemHistoryEntity.LegacyId, itemHistoryEntity.Name, itemHistoryEntity.EndDate, itemHistoryEntity.Performed, itemHistoryEntity.Description, itemHistoryEntity.OrderBy);
        }

        public string LegacyId { get; set; }
        public string Name { get; set; }
        public string EndDate { get; set; }
        public bool Performed { get; set; }
        public string Description { get; set; }
        public int OrderBy { get; set; }
    }
}