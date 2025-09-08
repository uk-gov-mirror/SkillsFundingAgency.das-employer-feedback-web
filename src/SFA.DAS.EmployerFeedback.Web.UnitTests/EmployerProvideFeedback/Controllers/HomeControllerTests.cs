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
        private List<ProviderAttributeModel> _providerAttributes;
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
            _providerAttributes = _fixture.Build<ProviderAttributeModel>()
                .With(x => x.Good, false)
                .With(x => x.Bad, false)
                .CreateMany(10)
                .ToList();

            _sessionServiceMock = new Mock<ISessionStorageService>();
            _sessionServiceMock.Setup(m => m.Get<SurveyModel>(It.IsAny<string>())).Returns(Task.FromResult(_surveyModel));
            _encodingServiceMock = new Mock<IEncodingService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _loggerMock = new Mock<ILogger<HomeController>>();

            _controller = new HomeController(_sessionServiceMock.Object, _encodingServiceMock.Object, _loggerMock.Object, _configurationMock.Object, _stubAuthenticationServiceMock.Object, _httpContextAccessorMock.Object);

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
        public async Task UniqueCode_Invalid_ShouldRedirect_ToError()
        {
            //FIXME - Replace repository code with outer API
            // Arrange
            //var uniqueCode = Guid.NewGuid();
            //_employerEmailDetailsRepoMock.Setup(m => m.GetEmployerInviteForUniqueCode(uniqueCode))
            //    .ReturnsAsync((EmployerSurveyInvite)null);

            //// Act
            //var result = await _controller.Index(uniqueCode);

            //// Assert
            //result.Should().BeOfType<NotFoundResult>();
            Assert.Fail();
        }

        [Test]
        public async Task SessionSurvey_DoesNotExist_ShouldPopulateProviderName_OnViewData()
        {
            // Arrange
            var request = new StartFeedbackRequest();

            // Act
            await _controller.Index(request);

            // Assert
            _controller.ViewData.Should().ContainKey("ProviderName");
            _controller.ViewData["ProviderName"].Should().Be(_employerEmailDetail.ProviderName);
        }

        [Test]
        public async Task SessionSurvey_DoesNotExist_ShouldCreateNewSurveyInSession()
        {
            // Arrange
            var uniqueEmailCode = Guid.NewGuid();

            // Act
            await _controller.Index(uniqueEmailCode);

            // Assert
            _sessionServiceMock.Verify(m => m.Set(_surveyModel.UserRef.ToString(), It.IsAny<SurveyModel>()), Times.Once);
        }

        [Test]
        public async Task EmailEntryPoint_Should_Create_AccountId()
        {
            //FIXME - Replace repository code with outer API
            // Arrange
            var uniqueCode = Guid.NewGuid();

            // Act
            await _controller.Index(uniqueCode);

            // Assert
            Assert.Fail();
            //_employerEmailDetailsRepoMock.Verify(m => m.GetEmployerInviteForUniqueCode(uniqueCode), Times.Once);
        }

        [Test]
        public async Task SessionSurvey_Exists_ShouldNotCreateNewSurvey()
        {
            // Arrange
            var existingSurvey = _fixture.Create<SurveyModel>();
            var uniqueCode = Guid.NewGuid();
            _sessionServiceMock.Setup(m => m.Get<SurveyModel>(uniqueCode.ToString()))
                .ReturnsAsync(existingSurvey);

            // Act
            await _controller.Index(uniqueCode);

            // Assert
            _sessionServiceMock.Verify(m => m.Set(uniqueCode.ToString(), It.IsAny<SurveyModel>()), Times.Never);
        }

        [TearDown]
        public void DisposeController()
        {
            _controller.Dispose();
        }   
    }
}
