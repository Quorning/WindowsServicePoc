using System;
using Domain.Serivces;

namespace WindowsService
{
    public class Worker
    {
        private readonly ITestLogningService _testLogningService;
        public Worker(ITestLogningService testLogningService)
        {
            _testLogningService = testLogningService ?? throw new ArgumentNullException(nameof(testLogningService));
        }

        public void Run()
        {
            _testLogningService.TestLog();
        }
    }
}
