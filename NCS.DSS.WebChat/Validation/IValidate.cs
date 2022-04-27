using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.WebChat.Models;

namespace NCS.DSS.WebChat.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IWebChat resource, bool validateModelForPost);
    }
}
