using IEC60870_5_104_simulator.Domain.Service;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.Domain.UnitTests
{
    public class DatapointRegistryTest
    {
        private static readonly IecAddress Address = new(1, 100);

        private static Iec104DataPoint SinglePoint(string initValue = "")
        {
            return new Iec104DataPoint(Address, Iec104DataTypes.M_SP_NA_1) { InitString = initValue };
        }

        private static Iec104DataPoint StepPosition(int initValue)
        {
            return new Iec104DataPoint(Address, Iec104DataTypes.M_ST_NA_1)
            {
                InitString = initValue.ToString()
            };
        }

        [Fact]
        public void Add_RejectsSecondDatapointAtSameAddress()
        {
            var registry = new DatapointRegistry();

            Assert.True(registry.Add(SinglePoint()));
            Assert.False(registry.Add(SinglePoint()));
            Assert.Single(registry.Snapshot());
        }

        [Fact]
        public void Add_SeedsValueFromInitString()
        {
            var registry = new DatapointRegistry();
            registry.Add(SinglePoint("true"));

            Assert.Equal(new IecSinglePointValueObject(true), registry.Get(Address).Value);
        }

        [Fact]
        public void Add_KeepsAlreadyPresentValue()
        {
            var registry = new DatapointRegistry();
            var datapoint = SinglePoint("true");
            datapoint.Value = new IecSinglePointValueObject(false);

            registry.Add(datapoint);

            Assert.Equal(new IecSinglePointValueObject(false), registry.Get(Address).Value);
        }

        [Fact]
        public void Get_ThrowsForUnknownAddress()
        {
            var registry = new DatapointRegistry();

            Assert.Throws<KeyNotFoundException>(() => registry.Get(Address));
        }

        [Fact]
        public void AdjustStep_IncrementsWithinRange()
        {
            var registry = new DatapointRegistry();
            registry.Add(StepPosition(10));

            Assert.Equal(11, registry.AdjustStep(Address, 1));
            Assert.Equal(11, ((IecIntValueObject)registry.Get(Address).Value).Value);
        }

        [Fact]
        public void AdjustStep_ClampsAtUpperBound()
        {
            var registry = new DatapointRegistry();
            registry.Add(StepPosition(StepPositionConst.STEP_POSITION_MAX));

            Assert.Equal(StepPositionConst.STEP_POSITION_MAX, registry.AdjustStep(Address, 1));
        }

        [Fact]
        public void AdjustStep_ClampsAtLowerBound()
        {
            var registry = new DatapointRegistry();
            registry.Add(StepPosition(StepPositionConst.STEP_POSITION_MIN));

            Assert.Equal(StepPositionConst.STEP_POSITION_MIN, registry.AdjustStep(Address, -1));
        }

        [Fact]
        public void AdjustStep_RejectsNonStepValue()
        {
            var registry = new DatapointRegistry();
            registry.Add(SinglePoint());

            Assert.Throws<InvalidCastException>(() => registry.AdjustStep(Address, 1));
        }

        [Fact]
        public void Snapshot_IsStableAcrossConcurrentAdd()
        {
            var registry = new DatapointRegistry();
            registry.Add(SinglePoint());
            var snapshot = registry.Snapshot();

            for (int i = 0; i < 50; i++)
                registry.Add(new Iec104DataPoint(new IecAddress(1, 200 + i), Iec104DataTypes.M_SP_NA_1));

            Assert.Single(snapshot);
            Assert.Equal(51, registry.Snapshot().Count);
        }
    }
}
