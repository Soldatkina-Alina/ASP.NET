using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Controllers;
using PromoCodeFactory.WebHost.Models;
using Xunit;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    public class SetPartnerPromoCodeLimitAsyncAutoFixtureTests
    {
        private readonly Mock<IRepository<Partner>> _partnersRepositoryMock;
        private readonly PartnersController _partnersController;

        public SetPartnerPromoCodeLimitAsyncAutoFixtureTests()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _partnersRepositoryMock = fixture.Freeze<Mock<IRepository<Partner>>>();
            _partnersController = fixture.Build<PartnersController>().OmitAutoProperties().Create();
        }

        /// <summary>
        /// При не найденном партнере метод должен возвращать NotFound (AutoFixture)
        /// </summary>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerNotFound_ReturnsNotFound()
        {
            // Arrange
            var fixture = new Fixture();
            var partnerId = fixture.Create<Guid>();
            var request = fixture.Create<SetPartnerPromoCodeLimitRequest>();
            Partner partner = null;

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);

            // Assert
            result.Should().BeAssignableTo<NotFoundResult>();
        }

        /// <summary>
        /// При неактивном партнере метод должен возвращать BadRequest с сообщением "Данный партнер не активен" (AutoFixture)
        /// </summary>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerIsNotActive_ReturnsBadRequest()
        {
            // Arrange
            var fixture = new Fixture();
            var partnerId = fixture.Create<Guid>();
            var partner = fixture.Build<Partner>()
                .With(p => p.Id, partnerId)
                .With(p => p.IsActive, false)
                .With(p => p.PartnerLimits, new List<PartnerPromoCodeLimit>())
                .Create();
            var request = fixture.Create<SetPartnerPromoCodeLimitRequest>();

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);

            // Assert
            result.Should().BeAssignableTo<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("Данный партнер не активен");
        }

        /// <summary>
        /// При валидном запросе с активным лимитом метод должен обнулить количество выданных промокодов и отменить предыдущий лимит (AutoFixture)
        /// </summary>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_ValidRequestWithActiveLimit_ResetsNumberIssuedPromoCodesAndCancelsPreviousLimit()
        {
            // Arrange
            var fixture = new Fixture();
            var partnerId = fixture.Create<Guid>();
            var activeLimit = new PartnerPromoCodeLimit
            {
                Id = Guid.NewGuid(),
                PartnerId = partnerId,
                Limit = 50,
                CreateDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now.AddDays(1),
                CancelDate = null
            };
            var partner = fixture.Build<Partner>()
                .With(p => p.Id, partnerId)
                .With(p => p.NumberIssuedPromoCodes, 10)
                .With(p => p.PartnerLimits, new List<PartnerPromoCodeLimit> { activeLimit })
                .Create();
            var request = fixture.Create<SetPartnerPromoCodeLimitRequest>();

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);
            _partnersRepositoryMock.Setup(repo => repo.UpdateAsync(partner))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);

            // Assert
            result.Should().BeAssignableTo<CreatedAtActionResult>();
            partner.NumberIssuedPromoCodes.Should().Be(0);
            activeLimit.CancelDate.Should().NotBeNull();
            partner.PartnerLimits.Should().HaveCount(2);
        }

        /// <summary>
        /// При валидном запросе метод должен сохранить новый лимит в базе данных (AutoFixture)
        /// </summary>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_ValidRequest_SavesNewLimitToDatabase()
        {
            // Arrange
            var fixture = new Fixture();
            var partnerId = fixture.Create<Guid>();
            var partner = fixture.Build<Partner>()
                .With(p => p.Id, partnerId)
                .With(p => p.PartnerLimits, new List<PartnerPromoCodeLimit>())
                .Create();
            var request = fixture.Create<SetPartnerPromoCodeLimitRequest>();

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);
            _partnersRepositoryMock.Setup(repo => repo.UpdateAsync(partner))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);

            // Assert
            result.Should().BeAssignableTo<CreatedAtActionResult>();
            partner.PartnerLimits.Should().HaveCount(1);
            _partnersRepositoryMock.Verify(repo => repo.UpdateAsync(partner), Times.Once);
        }
    }
}
