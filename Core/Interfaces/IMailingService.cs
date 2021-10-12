using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
   public interface IMailingService
    {
        string GetMailBody([Optional] string userName , string Email , string tokenLink,string TemplateNameView);
        Task SendEmailAsync(string mailTo, string subject, string body);
    }
}
