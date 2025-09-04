using SFA.DAS.EmployerFeedback.Web.Attributes;
using SFA.DAS.Encoding;
using System;

namespace SFA.DAS.EmployerFeedback.Web.Models;

public class StartFeedbackRequest
{
    public string EncodedAccountId { get; set; }

    [AutoDecode(nameof(EncodedAccountId), EncodingType.AccountId)]
    public long AccountId { get; set; }
    public Guid UniqueCode { get; set; }
}
