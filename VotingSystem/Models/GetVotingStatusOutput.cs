using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Collections.Generic;
using System.Numerics;

[FunctionOutput]
internal class GetVotingStatusOutput
{
    [Parameter("string", 1)]
    public string? status { get; set; }
    [Parameter("uint256", 2)]
    public long StartTime { get; set; }
    [Parameter("uint256", 3)]
    public long EndTime { get; set; }

}
