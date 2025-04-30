using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Collections.Generic;
using System.Numerics;

[FunctionOutput]
public class GetAllCandidatesOutput : IFunctionOutputDTO
{
    [Parameter("string[]", 1)]
    public List<string>? Names { get; set; }

    [Parameter("uint256[]", 2)]
    public List<BigInteger>? Votes { get; set; }
}

