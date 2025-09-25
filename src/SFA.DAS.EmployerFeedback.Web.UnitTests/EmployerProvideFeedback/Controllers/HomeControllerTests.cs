using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.EmployerFeedback.Web.Models;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.Encoding;
using SFA.DAS.GovUK.Auth.Services;

namespace UnitTests.EmployerProvideFeedback.Controllers
{
    [TestFixture]
    public class HomeControllerTests
    {
        private HomeController _controller;
        private Mock<ISessionStorageService> _sessionServiceMock;
        private Mock<IEncodingService> _encodingServiceMock;
        private Mock<ILogger<HomeController>> _loggerMock;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<IStubAuthenticationService> _stubAuthenticationServiceMock;
        private Mock<IEmployerFeedbackOuterApi> _employerFeedbackOuterApiMock;
        private List<SFA.DAS.EmployerFeedback.Web.Models.Shared.ProviderAttributeModel> _providerAttributes;
        private IFixture _fixture;
        
        private EmployerSurveyInvite _employerEmailDetail;
        private SurveyModel _surveyModel;

        

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employerEmailDetail = _fixture.Create<EmployerSurveyInvite>();
            
            _surveyModel = new SurveyModel
            {
                UserRef = Guid.NewGuid(),
                ProviderName = _employerEmailDetail.ProviderName,
            };
            
            _providerAttributes = _fixture.Build<SFA.DAS.EmployerFeedback.Web.Models.Shared.ProviderAttributeModel>()
                .With(x => x.Good, false)
                .With(x => x.Bad, false)
                .CreateMany(10)
                .ToList();

            _employerFeedbackOuterApiMock = new Mock<IEmployerFeedbackOuterApi>();

            _sessionServiceMock = new Mock<ISessionStorageService>();
            _sessionServiceMock.Setup(m => m.Get<SurveyModel>(It.IsAny<string>())).Returns(Task.FromResult(_surveyModel));

            _encodingServiceMock = new Mock<IEncodingService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _stubAuthenticationServiceMock = new Mock<IStubAuthenticationService>();
            _configurationMock = new Mock<IConfiguration>();

            _loggerMock = new Mock<ILogger<HomeController>>();

            _controller = new HomeController(_sessionServiceMock.Object, _encodingServiceMock.Object, _loggerMock.Object, _configurationMock.Object, _stubAuthenticationServiceMock.Object, _httpContextAccessorMock.Object, _employerFeedbackOuterApiMock.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                    new Claim(EmployerClaims.UserId, _surveyModel.UserRef.ToString()),
                }))
                }
            };
        }

        [Test]
        public async Task SessionSurvey_DoesNotExist_ShouldPopulateProviderName_OnViewData()
        {
            // Arrange
            var request = new Parameters();

            // Act
            await _controller.Index(request);

            // Assert
            _controller.ViewData.Should().ContainKey("ProviderName");
            _controller.ViewData["ProviderName"].Should().Be(_employerEmailDetail.ProviderName);
        }

        [TearDown]
        public void DisposeController()
        {
            _controller.Dispose();
        }   
    }
}
