using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace RedOrBlack.Contracts.Wheel.ContractDefinition
{
    public partial class Spin : SpinBase { }

    public class SpinBase 
    {
        [Parameter("uint256", "spinId", 1)]
        public virtual BigInteger SpinId { get; set; }
        [Parameter("uint256", "startTime", 2)]
        public virtual BigInteger StartTime { get; set; }
        [Parameter("bool", "hasSpun", 3)]
        public virtual bool HasSpun { get; set; }
        [Parameter("uint256", "spunNumberId", 4)]
        public virtual BigInteger SpunNumberId { get; set; }
        [Parameter("uint256", "spunTime", 5)]
        public virtual BigInteger SpunTime { get; set; }
    }
}
