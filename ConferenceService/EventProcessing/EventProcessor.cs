using ConferenceService.DTOs;
using ConferenceService.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace ConferenceService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        public EventProcessor(IServiceScopeFactory scopeFactory,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);

            switch (eventType)
            {
                case EventType.CheckConference:
                    CheckConference(message);
                    break;

                default:
                    break;
            }
        }

        private EventType DetermineEvent(string notificationMessage)
        {
            Console.WriteLine("Determining event...");

            var eventType = JsonSerializer.Deserialize<GenericEventDTO>(notificationMessage);
            Console.WriteLine("Event received: " + eventType.eventMessage + " \nFrom this message: " + notificationMessage);

            switch (eventType.eventMessage)
            {
                case "check_conference":
                    Console.WriteLine("check_conference event detected.");
                    return EventType.CheckConference;

                default:
                    Console.WriteLine($"Unknown event: {eventType.eventMessage}");
                    return EventType.Unknown;
            }
        }

        private async void CheckConference(string checkConferenceMessage)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<IConferenceRepository>();

                var checkConferenceDTO = JsonSerializer.Deserialize<CheckConferenceDTO>(checkConferenceMessage);

                if (VerifyToken(checkConferenceDTO.token))
                {
                    var response = await repo.GetByIdAsync(checkConferenceDTO.ConferenceId);

                    if (response != null) {
                        Console.WriteLine("Conference found: " + response.Name);
                    }
                    else
                    {
                        Console.WriteLine("Conference not found.");
                    }
                }
                else
                {
                    Console.WriteLine("Ivalid Token");
                }
            }
        }

        private bool VerifyToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudiences = _configuration.GetSection("Jwt:Audiences").Get<List<string>>(),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!))
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                Console.WriteLine("The passed in token successfully authenticated.");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in validating token: " + ex.Message);
                return false;
            }
        }
    }

        enum EventType
        {
            Unknown,
            CheckConference
        }
    }
