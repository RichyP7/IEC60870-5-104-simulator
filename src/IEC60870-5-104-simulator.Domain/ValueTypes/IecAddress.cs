namespace IEC60870_5_104_simulator.Domain.ValueTypes
{
    public record IecAddress
    {
        public IecAddress(int stationaryAddress, int objectAddress)
        {
            StationaryAddress = stationaryAddress;
            ObjectAddress = objectAddress;
        }

        public int StationaryAddress { get; set; }
        public int ObjectAddress { get; set; }
    }
}
