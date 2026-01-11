using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromoCodeFactory.UnitTests.WebHost.DataBuilder
{
    public class PartnerBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _name = "Test Partner";
        private bool _isActive = true;
        private int _numberIssuedPromoCodes = 0;
        private List<PartnerPromoCodeLimit> _partnerLimits = new List<PartnerPromoCodeLimit>();

        public PartnerBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public PartnerBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public PartnerBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public PartnerBuilder WithNumberIssuedPromoCodes(int number)
        {
            _numberIssuedPromoCodes = number;
            return this;
        }

        public PartnerBuilder WithPartnerLimits(List<PartnerPromoCodeLimit> limits)
        {
            _partnerLimits = limits;
            return this;
        }

        public Partner Build()
        {
            return new Partner
            {
                Id = _id,
                Name = _name,
                IsActive = _isActive,
                NumberIssuedPromoCodes = _numberIssuedPromoCodes,
                PartnerLimits = _partnerLimits
            };
        }
    }
}
