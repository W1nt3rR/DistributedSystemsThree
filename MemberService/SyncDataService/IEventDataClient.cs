namespace MemberService.SyncDataService
{
    public interface IEventDataClient
    {
        Task<bool> Signup(string memberToken, int conferenceId);
    }
}
