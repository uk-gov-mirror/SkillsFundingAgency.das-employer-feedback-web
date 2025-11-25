using System;

namespace SFA.DAS.EmployerFeedback.Web.Exceptions
{
    public class EmployerFeedbackException : Exception
    {
        public EmployerFeedbackException()
            : base()
        { 
        }

        public EmployerFeedbackException(string message)
            : base(message)
        {
        }

        public EmployerFeedbackException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
