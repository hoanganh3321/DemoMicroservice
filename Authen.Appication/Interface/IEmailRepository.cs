using Authen.Application.Common;
using Authen.Application.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authen.Application.Interface
{
    public interface IEmailRepository
    {
        Task<EmailConfirmResult> UserConfirmEmailAsync(
           string userId, string token,
           CancellationToken cancellationToken = default);       
    }
    public record EmailConfirmResult(string userId, string token);
}
