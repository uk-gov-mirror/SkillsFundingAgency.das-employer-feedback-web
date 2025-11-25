using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFeedback.Web.Services;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Services
{
    [TestFixture]
    public class AccountsLinkServiceTests
    {
        private AccountsLinkService _sut;

        [SetUp]
        public void Setup()
        {
            // Uses the real UrlBuilder; “LOCAL” ensures it builds safe URLs.
            var realUrlBuilder = new UrlBuilder("LOCAL");
            _sut = new AccountsLinkService(realUrlBuilder);
        }

        [Test]
        public void AccountsHome_Should_Return_A_Valid_Url_String()
        {
            // Arrange
            var encodedAccountId = "ABC123";

            // Act
            var result = _sut.AccountsHome(encodedAccountId);

            // Assert
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Contain(encodedAccountId);
        }
    }
}
