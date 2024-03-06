using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace RedOrBlack.Contracts.Wheel.ContractDefinition
{
    public partial class Number : NumberBase { }

    public class NumberBase 
    {
        [Parameter("uint256", "id", 1)]
        public virtual BigInteger Id { get; set; }
        [Parameter("string", "name", 2)]
        public virtual string Name { get; set; }
        [Parameter("uint8", "parity", 3)]
        public virtual byte Parity { get; set; }
        [Parameter("uint8", "color", 4)]
        public virtual byte Color { get; set; }
        [Parameter("uint8", "row", 5)]
        public virtual byte Row { get; set; }
        [Parameter("uint8", "column", 6)]
        public virtual byte Column { get; set; }
        [Parameter("uint8", "which18", 7)]
        public virtual byte Which18 { get; set; }
        [Parameter("uint8", "whichDozen", 8)]
        public virtual byte WhichDozen { get; set; }
    }
}
