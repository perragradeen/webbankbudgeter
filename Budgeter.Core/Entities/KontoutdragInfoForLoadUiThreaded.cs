using System.Threading;

namespace Budgeter.Core.Entities
{
    public class KontoutdragInfoForLoadUiThreaded : KontoutdragInfoForLoad
    {
        public Thread MainThread { get; set; }
        public Thread WorkerThread { get; set; }
    }
}