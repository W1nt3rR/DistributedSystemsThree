using MemberService.DTOs;

namespace MemberService.AsyncDataServices
{
    public interface IMessageBusClient
    {
        Task<bool> CheckConference(CheckConferenceDTO checkConferenceDTO);
    }
}
