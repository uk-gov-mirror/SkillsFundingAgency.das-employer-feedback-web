using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace SFA.DAS.EmployerFeedback.Web.Validators.Attributes
{
    public class EnsureMaxThreeProviderAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var list = value as List<ProviderAttributeModel>;
            if (list != null)
            {
                
                return list.Where(att => att.Good).Count() <= 3
                    && list.Where(att => att.Bad).Count() <= 3;
            }

            return false;
        }
    }
}
