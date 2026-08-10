using LimpidusMongoDB.Application.Data.Entities;
using LimpidusMongoDB.Application.Helpers;

namespace LimpidusMongoDB.Tests.Helpers;

public class HistoryDeduperTests
{
    [Fact]
    public void Deduplicate_keeps_oldest_of_identical_submissions()
    {
        var end = new DateTime(2026, 5, 1, 12, 30, 15, DateTimeKind.Utc);
        var older = MakeHistory("aaaaaaaaaaaaaaaaaaaaaaaa", end, "area-1", "emp-1");
        var newer = MakeHistory("bbbbbbbbbbbbbbbbbbbbbbbb", end, "area-1", "emp-1");
        var other = MakeHistory("cccccccccccccccccccccccc", end, "area-2", "emp-1");

        var result = HistoryDeduper.Deduplicate(new[] { newer, older, other });

        Assert.Equal(2, result.Count);
        Assert.Contains(result, h => h.Id == older.Id);
        Assert.Contains(result, h => h.Id == other.Id);
        Assert.DoesNotContain(result, h => h.Id == newer.Id);
    }

    [Fact]
    public void PreferCanonical_picks_highest_level()
    {
        var n1 = new ProjectEntity { LegacyId = 4700, Name = "BACKUP", Level = 1 };
        var n2 = new ProjectEntity { LegacyId = 4700, Name = "CC N2", Level = 2 };

        var picked = ProjectLegacyResolver.PreferCanonical(new[] { n1, n2 });

        Assert.Same(n2, picked);
    }

    private static HistoryEntity MakeHistory(string idHex, DateTime end, string areaId, string employeeId)
    {
        var entity = new HistoryEntity
        {
            ProjectId = 4698,
            EmployeeId = employeeId,
            AreaTaskId = areaId,
            AreaTaskName = "WC",
            EndDate = end,
            Items = Array.Empty<HistoryItemEntity>()
        };
        entity.SetObjectId(idHex);
        return entity;
    }
}
