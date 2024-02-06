using IEC60870_5_104_simulator.Infrastructure.Interfaces;
using lib60870.CS101;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IEC60870_5_104_simulator.Infrastructure
{
    internal class RandomSimulatorFactory : IValueSimulatorFactory
    {
        public void SimulateValues(IEnumerable<InformationObject> myList)
        {
            IEnumerable<SinglePointInformation> spiObjects = myList.OfType<SinglePointInformation>();
            SimulateValues(spiObjects);
        }
        public void SimulateValues(IEnumerable<SinglePointInformation> spilist)
        {
            Random random = new();
            foreach (var item in spilist)
            {
                item.Value = random.NextDouble() >= 0.5;
            }
        }
    }
}
