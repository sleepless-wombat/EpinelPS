using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event;

[GameRequest("/event/login/get")]
public class EventLoginGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqLoginEventData req = await ReadData<ReqLoginEventData>();
        User user = GetUser();
        int evId = req.EventId;
        ResLoginEventData response = new()
        {
            EndDate = DateTime.Now.AddDays(13).Ticks,
            DisableDate = DateTime.Now.AddDays(13).Ticks,
            LastAttendance = new LoginEventAttendance()
        }; // fields "EndDate", "DisableDate", "RewardHistories", "LastAttendance"
        // Check if event exists
        if (!user.LoginEventInfo.TryGetValue(evId, out var loginEventData))
        {
            loginEventData = new LoginEventData();
            loginEventData.LastDay++;
            loginEventData.LastDate = DateTime.Now.Ticks;
            user.LoginEventInfo.Add(evId, loginEventData);
            JsonDb.Save();
        }

        // Increment the login count if needed.
        if (loginEventData.LastDate == 0)
        {
            loginEventData.LastDay++;
            loginEventData.LastDate = DateTime.Now.Ticks;
        }

        // Populate response with event data
        GameData.Instance.LoginEventTable.Values.Where(ev => ev.EventId == evId).ToList().ForEach(ev =>
        {
            response.RewardHistories.Add(new LoginEventRewardHistory() { IsReceived = loginEventData.Days.Contains(ev.Day), Day = ev.Day });
        });

        response.LastAttendance.Day = loginEventData.LastDay;
        response.LastAttendance.AttendanceDate = loginEventData.LastDate;

        await WriteDataAsync(response);
    }
}
