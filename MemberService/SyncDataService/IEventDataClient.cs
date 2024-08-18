namespace MemberService.SyncDataService
{
    public interface IEventDataClient
    {
        Task<bool> Signup(string memberToken, string conferenceId);
    }
}
