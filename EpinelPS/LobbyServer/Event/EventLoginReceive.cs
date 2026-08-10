using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event;

[GameRequest("/event/login/receive")]
public class EventLoginReceive : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        // ReqObtainLoginEventReward Fields: EventId, Day
        ReqObtainLoginEventReward req = await ReadData<ReqObtainLoginEventReward>();
        User user = GetUser();

        ResObtainLoginEventReward response = new();

        if (!user.LoginEventInfo.TryGetValue(req.EventId, out var loginEventData))
        {
            loginEventData = new LoginEventData();
            loginEventData.LastDay++;
            loginEventData.LastDate = DateTime.Now.Ticks;
            user.LoginEventInfo.Add(req.EventId, loginEventData);
        }

        GameData.Instance.LoginEventTable.Values.Where(ev => ev.EventId == req.EventId && ev.Day == req.Day).ToList().ForEach(ev =>
        {
            loginEventData.Days.Add(req.Day);
            response.Reward = RewardUtils.RegisterRewardsForUser(user, ev.RewardId);
        });
        
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}