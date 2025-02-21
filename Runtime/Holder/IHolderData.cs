using System.Collections;

namespace WhiteArrow.Incremental
{
    public interface IHolderData
    {
        int MaxCapacity { get; set; }
        IList Items { get; }
    }
}