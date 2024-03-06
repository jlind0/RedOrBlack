using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace RedOrBlack.Contracts.Wheel.ContractDefinition
{
    public partial class Bet : BetBase { }

    public class BetBase 
    {
        [Parameter("address", "account", 1)]
        public virtual string Account { get; set; }
        [Parameter("uint256", "spinId", 2)]
        public virtual BigInteger SpinId { get; set; }
        [Parameter("uint256", "amount", 3)]
        public virtual BigInteger Amount { get; set; }
        [Parameter("uint8", "betType", 4)]
        public virtual byte BetType { get; set; }
        [Parameter("uint256", "number", 5)]
        public virtual BigInteger Number { get; set; }
        [Parameter("uint8", "row", 6)]
        public virtual byte Row { get; set; }
        [Parameter("uint8", "column", 7)]
        public virtual byte Column { get; set; }
        [Parameter("uint8", "color", 8)]
        public virtual byte Color { get; set; }
        [Parameter("uint8", "parity", 9)]
        public virtual byte Parity { get; set; }
        [Parameter("uint256[]", "numbers", 10)]
        public virtual List<BigInteger> Numbers { get; set; }
        [Parameter("uint8", "byTheDozen", 11)]
        public virtual byte ByTheDozen { get; set; }
        [Parameter("uint8", "byThe18", 12)]
        public virtual byte ByThe18 { get; set; }
    }
}
