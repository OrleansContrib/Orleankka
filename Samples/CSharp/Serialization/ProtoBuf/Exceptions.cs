using System;
using System.Runtime.Serialization;

namespace Example
{
    [Serializable]
    public class InventoryItemException : Exception
    {
        public InventoryItemException(string message)
            : base(message)
        {}

        InventoryItemException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }
}
