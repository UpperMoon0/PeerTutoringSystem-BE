using Microsoft.Extensions.Configuration;
using Moq;
using PeerTutoringSystem.Application.Services.Payment;
using PeerTutoringSystem.Domain.Entities.PaymentEntities;
using PeerTutoringSystem.Domain.Interfaces.Payment;
using PeerTutoringSystem.Domain.Interfaces.Booking;
using PeerTutoringSystem.Domain.Interfaces.Profile_Bio;
using PeerTutoringSystem.Application.DTOs.Payment;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using PeerTutoringSystem.Application.Services.Booking;
using Microsoft.Extensions.Logging;

namespace PeerTutoringSystem.Tests.Application.Services
{
    public class PaymentServiceTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }
    }
}