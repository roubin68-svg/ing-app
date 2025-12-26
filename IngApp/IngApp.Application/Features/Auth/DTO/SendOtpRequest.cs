using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngApp.Application.Features.Auth.DTO;

public class SendOtpRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
}
