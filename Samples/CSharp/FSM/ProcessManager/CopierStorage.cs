using System;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;
using Orleans.Storage;

namespace Example
{
    public class CopierData
    {
        public string CurrentState { get; set; }
        public string PreviousState { get; set; }
        public string LastCopiedLine  { get; set; }
        public string LastCompressedLine  { get; set; }
    }

    class CopierStorage : IGrainStorage
    {
        public Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            throw new NotImplementedException();
        }

        public Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            throw new NotImplementedException();
        }

        public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            throw new NotImplementedException();
        }
    }
}