using System;
using Xunit;
using ProseTutorial;

namespace ProseTutorial.Tests
{
    public class PrimeService_IsPrimeShould
    {
        private readonly PrimeService _primeService;

        public PrimeService_IsPrimeShould()
        {
            _primeService = new PrimeService();
        }

        [Fact]
        public void ReturnFAlseGivenValueOf1()
        {
            var result = _primeService.IsPrime(1);

            Assert.False(result, "1 shoud not be prime");
        }
    }
}