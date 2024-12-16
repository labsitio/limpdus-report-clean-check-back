using LimpidusMongoDB.Application.Data.Entities;

namespace LimpidusMongoDB.Application.Contracts.Responses
{
    public class HistoryItemResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public short? OrderBy { get; set; }
        public DateTime EndDate { get; set; }
        public bool Performed { get; set; }

        public static implicit operator HistoryItemResponse(HistoryItemEntity entity)
        {
            return new HistoryItemResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                OrderBy = entity.OrderBy,
                EndDate = entity.EndDate,
                Performed = entity.Performed
            };
        }
    }
}
