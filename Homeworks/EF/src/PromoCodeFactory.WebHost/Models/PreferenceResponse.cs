using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using System;
using System.Collections.Generic;

namespace PromoCodeFactory.WebHost.Models
{
    public class PreferenceResponse
    {
        public Guid CustomerId { get; set; }
        public Guid PreferenceId { get; set; }
        public string PreferenceName { get; set; }
    }
}