using LimpidusMongoDB.Application.Data.Entities;

namespace LimpidusMongoDB.Application.Contracts.Responses
{
    public class JustificationResponse
    {
        public JustificationResponse(string information, string reason)
        {
            Information = information;
            Reason = reason;
        }

        public static implicit operator JustificationResponse(HistoryJustificationEntity justifyEntity)
        {
            return new JustificationResponse(justifyEntity.Information, justifyEntity.Reason);
        }

        public string Information { get; set; }
        public string Reason { get; set; }       
    }
}