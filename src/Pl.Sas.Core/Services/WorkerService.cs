using Pl.Sas.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pl.Sas.Core.Services
{
    /// <summary>
    /// các nghiệp vụ được xử lý bởi worker
    /// </summary>
    public class WorkerService
    {
        private readonly IMarketData _marketData;
        private readonly ICrawlerData _crawlerData;

        public WorkerService(
            ICrawlerData crawlerData,
            IMarketData marketData)
        {
            _marketData = marketData;
            _crawlerData = crawlerData;
        }


    }
}
