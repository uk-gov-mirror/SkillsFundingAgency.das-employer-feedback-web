using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;

namespace SFA.DAS.EmployerFeedback.Application.Commands.SubmitEmployerRequest
{
    public class SubmitEmployerFeedbackCommandHandler : IRequestHandler<SubmitEmployerFeedbackCommand, bool>
    {
        private readonly IEmployerFeedbackOuterApi _outerApi;
        private readonly ILogger<SubmitEmployerFeedbackCommandHandler> _logger;

        public SubmitEmployerFeedbackCommandHandler(IEmployerFeedbackOuterApi outerApi, ILogger<SubmitEmployerFeedbackCommandHandler> logger)
        {
            _outerApi = outerApi;
            _logger = logger;
        }

        public async Task<bool> Handle(SubmitEmployerFeedbackCommand command, CancellationToken cancellationToken)
        {
            try
            {
                await _outerApi.SubmitEmployerFeedback(new EmployerFeedbackResult
                {
                    Ukprn = command.Ukprn,
                    AccountId = command.AccountId,
                    ProviderRating = command.Rating,
                    FeedbackSource = command.FeedbackSource,
                    ProviderAttributes = command.Attributes,
                    UserRef = command.UserRef
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to submit feedback");
                return false;
            }

            return true;
        }
    }
}
