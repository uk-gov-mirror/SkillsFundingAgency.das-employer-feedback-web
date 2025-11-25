## ⛔Never push sensitive information such as client id's, secrets or keys into repositories including in the README file⛔

# Employer Feedback Web

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_apis/build/status/das-employer-feedback-web?branchName=main)](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_build/latest?definitionId=4189&branchName=main)
[![Quality gate](https://sonarcloud.io/api/project_badges/quality_gate?project=SkillsFundingAgency_das-employer-feedback-web)](https://sonarcloud.io/summary/new_code?id=SkillsFundingAgency_das-employer-feedback-web)
[![Confluence Page](https://img.shields.io/badge/Confluence-Project-blue)](https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/4932174087/Employer+feedback+-+New+email+engagement+Architecture)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

This web solution is part of Employer Feedback project. This web application enables employers to provide feedback about training providers they have used for apprenticeship training. Employers can search for training providers and submit structured feedback through a multi-step questionnaire covering strengths, areas for improvement, and overall ratings.

## How It Works
Employers register through the Employer portal and gain access to the feedback system. They can search for training providers, confirm the provider they wish to review, and complete a feedback survey about their experience with that provider's apprenticeship training services.

When running this locally, with stub sign-in enabled, the launch url should be `https://localhost:7701/`

## 🚀 Installation

### Pre-Requisites
* A clone of this repository
* Optionally an Azure Active Directory account with the appropriate roles.
* The Outer API [das-apim-endpoints](https://github.com/SkillsFundingAgency/das-apim-endpoints/tree/master/src/EmployerFeedback) should be available either running locally or accessible in an Azure tenancy.

### Config
You can find the latest config file in [das-employer-config repository](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-employer-feedback-web/SFA.DAS.EmployerFeedback.Web.json)

In the web project, if not exist already, add `AppSettings.Development.json` file with following content:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConfigurationStorageConnectionString": "UseDevelopmentStorage=true;",
  "ConfigNames": "SFA.DAS.EmployerFeedback.Web,SFA.DAS.Employer.Shared.UI,SFA.DAS.Encoding:EncodingConfig,SFA.DAS.Employer.GovSignIn",
  "EnvironmentName": "LOCAL",
  "ResourceEnvironmentName": "LOCAL",
  "cdn": {
    "url": "https://das-test-frnt-end.azureedge.net"
  },
  "StubEmail": "someemail",
  "StubId": "someid",
  "StubAuth": true
} 
```

## Technologies
* .NetCore 8.0
* NUnit
* Moq
* FluentAssertions
* RestEase
* MediatR
