using SFA.DAS.Encoding;
using System;

namespace SFA.DAS.EmployerFeedback.Web.Attributes
{
    public class AutoDecodeAttribute : Attribute
    {
        public string Source { get; }
        public EncodingType EncodingType { get; }

        public AutoDecodeAttribute(string source, EncodingType encodingType)
        {
            Source = source;
            EncodingType = encodingType;
        }
    }
}