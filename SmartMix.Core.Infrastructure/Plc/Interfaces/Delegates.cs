namespace SmartMix.Core.Infrastructure.Plc.Interfaces
{
    public class Delegates
    {
        public delegate void VoidEvent();
        public delegate void BoolEvent(bool status);
        public delegate void LogMessage(string message);
    }
}
