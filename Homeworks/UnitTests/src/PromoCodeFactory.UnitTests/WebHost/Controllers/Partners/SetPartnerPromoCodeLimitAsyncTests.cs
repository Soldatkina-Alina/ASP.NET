using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.UnitTests.WebHost.DataBuilder;
using PromoCodeFactory.WebHost.Controllers;
using PromoCodeFactory.WebHost.Models;
using Xunit;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    public class SetPartnerPromoCodeLimitAsyncTests
    {
        private readonly Mock<IRepository<Partner>> _partnersRepositoryMock;
        private readonly PartnersController _partnersController;

        public SetPartnerPromoCodeLimitAsyncTests()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _partnersRepositoryMock = fixture.Freeze<Mock<IRepository<Partner>>>();
            _partnersController = fixture.Build<PartnersController>().OmitAutoProperties().Create();
        }

        /// <summary>
        /// При не найденном партнере метод должен возвращать NotFound
        /// </summary>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerNotFound_ReturnsNotFound()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            var request = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();
            Partner partner = null;

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);

            // Assert
            result.Should().BeAssignableTo<NotFoundResult>();
        }

        /// <summary>
        /// При неактивном партнере метод должен возвращать BadRequest с сообщением "Данный партнер не активен"
        /// </summary>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerIsNotActive_ReturnsBadRequest()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            var partner = new PartnerBuilder()
                .WithId(partnerId)
                .WithIsActive(false)
                .Build();
            var request = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();

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
        /// При валидном запросе с активным лимитом метод должен обнулить количество выданных промокодов
        /// </summary>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_WithActiveLimit_ResetsNumberIssuedPromoCodes()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            var activeLimit = new PartnerPromoCodeLimit
            {
                Id = Guid.NewGuid(),
                PartnerId = partnerId,
                Limit = 50,
                CreateDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now.AddDays(1),
                CancelDate = null
            };
            var partner = new PartnerBuilder()
                .WithId(partnerId)
                .WithNumberIssuedPromoCodes(10)
                .WithPartnerLimits(new List<PartnerPromoCodeLimit> { activeLimit })
                .Build();
            var request = new SetPartnerPromoCodeLimitRequest
            {
                Limit = 100,
                EndDate = DateTime.Now.AddDays(10)
            };

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);
            _partnersRepositoryMock.Setup(repo => repo.UpdateAsync(partner))
                .Returns(Task.CompletedTask);

            // Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);

            // Assert
            partner.NumberIssuedPromoCodes.Should().Be(0);
        }

        /// <summary>
        /// При валидном запросе с активным лимитом метод должен отменить предыдущий лимит
        /// </summary>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_WithActiveLimit_CancelsPreviousLimit()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            var activeLimit = new PartnerPromoCodeLimit
            {
                Id = Guid.NewGuid(),
                PartnerId = partnerId,
                Limit = 50,
                CreateDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now.AddDays(1),
                CancelDate = null
            };
            var partner = new PartnerBuilder()
                .WithId(partnerId)
                .WithPartnerLimits(new List<PartnerPromoCodeLimit> { activeLimit })
                .Build();
            var request = new SetPartnerPromoCodeLimitRequest
            {
                Limit = 100,
                EndDate = DateTime.Now.AddDays(10)
            };

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);
            _partnersRepositoryMock.Setup(repo => repo.UpdateAsync(partner))
                .Returns(Task.CompletedTask);

            // Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);

            // Assert
            activeLimit.CancelDate.Should().NotBeNull();
        }

        /// <summary>
        /// При валидном запросе метод должен добавить новый лимит партнеру
        /// </summary>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_AddsNewLimit()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            var partner = new PartnerBuilder()
                .WithId(partnerId)
                .Build();
            var request = new SetPartnerPromoCodeLimitRequest
            {
                Limit = 100,
                EndDate = DateTime.Now.AddDays(10)
            };

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);
            _partnersRepositoryMock.Setup(repo => repo.UpdateAsync(partner))
                .Returns(Task.CompletedTask);

            // Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);

            // Assert
            partner.PartnerLimits.Should().HaveCount(1);
            var newLimit = partner.PartnerLimits.First();
            newLimit.Limit.Should().Be(100);
            newLimit.PartnerId.Should().Be(partnerId);
            newLimit.EndDate.Should().Be(request.EndDate);
            newLimit.CreateDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// При валидном запросе метод должен вернуть CreatedAtActionResult
        /// </summary>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_ReturnsCreatedAtAction()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            var partner = new PartnerBuilder()
                .WithId(partnerId)
                .Build();
            var request = new SetPartnerPromoCodeLimitRequest
            {
                Limit = 100,
                EndDate = DateTime.Now.AddDays(10)
            };

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);
            _partnersRepositoryMock.Setup(repo => repo.UpdateAsync(partner))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);

            // Assert
            result.Should().BeAssignableTo<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult.ActionName.Should().Be(nameof(_partnersController.GetPartnerLimitAsync));
        }

        /// <summary>
        /// При валидном запросе метод должен вызвать UpdateAsync один раз
        /// </summary>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_CallsUpdateAsync()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            var partner = new PartnerBuilder()
                .WithId(partnerId)
                .Build();
            var request = new SetPartnerPromoCodeLimitRequest
            {
                Limit = 100,
                EndDate = DateTime.Now.AddDays(10)
            };

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);
            _partnersRepositoryMock.Setup(repo => repo.UpdateAsync(partner))
                .Returns(Task.CompletedTask);

            // Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);

            // Assert
            _partnersRepositoryMock.Verify(repo => repo.UpdateAsync(partner), Times.Once);
        }

        /// <summary>
        /// При валидном запросе без активного лимита метод не должен обнулять количество выданных промокодов
        /// </summary>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_ValidRequestWithoutActiveLimit_DoesNotResetNumberIssuedPromoCodes()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            var cancelledLimit = new PartnerPromoCodeLimit
            {
                Id = Guid.NewGuid(),
                PartnerId = partnerId,
                Limit = 50,
                CreateDate = DateTime.Now.AddDays(-2),
                EndDate = DateTime.Now.AddDays(-1),
                CancelDate = DateTime.Now.AddDays(-1)
            };
            var partner = new PartnerBuilder()
                .WithId(partnerId)
                .WithNumberIssuedPromoCodes(20)
                .WithPartnerLimits(new List<PartnerPromoCodeLimit> { cancelledLimit })
                .Build();
            var request = new SetPartnerPromoCodeLimitRequest
            {
                Limit = 100,
                EndDate = DateTime.Now.AddDays(10)
            };

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);
            _partnersRepositoryMock.Setup(repo => repo.UpdateAsync(partner))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);

            // Assert
            result.Should().BeAssignableTo<CreatedAtActionResult>();
            partner.NumberIssuedPromoCodes.Should().Be(20); // Not reset
            partner.PartnerLimits.Should().HaveCount(2);
            var newLimit = partner.PartnerLimits.Last();
            newLimit.Limit.Should().Be(100);
            _partnersRepositoryMock.Verify(repo => repo.UpdateAsync(partner), Times.Once);
        }

        /// <summary>
        /// При невалидном лимите (0 или отрицательном) метод должен возвращать BadRequest с сообщением "Лимит должен быть больше 0"
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task SetPartnerPromoCodeLimitAsync_InvalidLimit_ReturnsBadRequest(int invalidLimit)
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            var partner = new PartnerBuilder()
                .WithId(partnerId)
                .Build();
            var request = new SetPartnerPromoCodeLimitRequest
            {
                Limit = invalidLimit,
                EndDate = DateTime.Now.AddDays(10)
            };

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);

            // Assert
            result.Should().BeAssignableTo<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("Лимит должен быть больше 0");
        }

        /// <summary>
        /// При валидном запросе метод должен сохранить новый лимит в базе данных
        /// </summary>
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_ValidRequest_SavesNewLimitToDatabase()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            var partner = new PartnerBuilder()
                .WithId(partnerId)
                .Build();
            var request = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);
            _partnersRepositoryMock.Setup(repo => repo.UpdateAsync(partner))
                .Returns(Task.CompletedTask)
                .Callback<Partner>(p =>
                {
                    // Verify that new limit is added
                    p.PartnerLimits.Should().HaveCount(1);
                    var newLimit = p.PartnerLimits.First();
                    newLimit.Limit.Should().Be(request.Limit);
                    newLimit.EndDate.Should().Be(request.EndDate);
                    newLimit.PartnerId.Should().Be(partnerId);
                    newLimit.CreateDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
                });

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);

            // Assert
            result.Should().BeAssignableTo<CreatedAtActionResult>();
            _partnersRepositoryMock.Verify(repo => repo.UpdateAsync(partner), Times.Once);
        }

    }
}
