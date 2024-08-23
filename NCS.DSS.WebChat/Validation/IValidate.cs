using NCS.DSS.WebChat.Models;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.WebChat.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IWebChat resource, bool validateModelForPost);
    }
}