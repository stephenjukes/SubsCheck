using SubsCheck.Constants.Enums;

namespace SubsCheck.Models
{
    public class ColumnProtectionStatus
    {
        public ColumnProtectionStatus(int columnNumber, ProtectionStatus protectionStatus)
        {
            ColumnNumber = columnNumber;
            ProtectionStatus = protectionStatus;
        }

        public int ColumnNumber { get; set; }

        public ProtectionStatus ProtectionStatus { get; set; }
    }
}
