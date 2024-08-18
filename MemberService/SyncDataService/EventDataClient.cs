using Grpc.Net.Client;
using MemberService.Protos;

namespace MemberService.SyncDataService
{
    public class EventDataClient : IEventDataClient
    {
        private readonly IConfiguration _configuration;

        public EventDataClient(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> Signup(string memberToken, int conferenceId)
        {
            Console.WriteLine("Calling gRPC Service: " + _configuration["GrpcPlatform"]);

            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            var channel = GrpcChannel.ForAddress(_configuration["GrpcPlatform"], new GrpcChannelOptions { HttpClient = new HttpClient(handler) });
            var client = new SignupService.SignupServiceClient(channel);

            var request = new Signup
            {
                MemberJwtToken = memberToken,
                ConferenceId = conferenceId.ToString(),
            };

            Console.WriteLine("Sending gRPC Request: " + request);

            var response = await client.SignupMemberAsync(request);

            Console.WriteLine("Recieved gRPC Response: " + response);

            return response.Approved;
        }
    }
}
