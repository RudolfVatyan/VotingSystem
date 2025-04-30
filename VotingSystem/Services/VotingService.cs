using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;
using Nethereum.Accounts;
using System;
using System.Threading.Tasks;
using Nethereum.Model;
using System.Numerics;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections;
using Microsoft.AspNetCore.Mvc;
using VotingSystem.Models;
using VotingSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Security.Claims;
using Nethereum.BlockchainProcessing.BlockStorage.Entities;
using Nethereum.ABI.FunctionEncoding;


namespace VotingSystem.Services
{
    public class VotingService
    {
        private readonly string? _infuraUrl = Environment.GetEnvironmentVariable("INFURAURL"); 
        private readonly string? _contractAddress = Environment.GetEnvironmentVariable("CONTRACTADDRESS");
        private readonly string? _privateKey = Environment.GetEnvironmentVariable("PRIVATE_KEY");
        private readonly string? _privateKeyForUsers = "d76dcfea714eb89f4e9d31e5babb2d1b013c01db31f42e6d379a28444091cebc";
        private Web3 _web3;
        private Web3 _web3WithAccount;
        private Nethereum.Contracts.Contract _contract;
        private Nethereum.Contracts.Contract _contractForUsers;
        private string _abi;
        private readonly VotingContext _context;
        private Nethereum.Web3.Accounts.Account _accountForUsers;
        private Nethereum.Web3.Accounts.Account _account;

        public VotingService(VotingContext context)
        {
            _abi = File.ReadAllText("contractABI.json");
            _account = new Nethereum.Web3.Accounts.Account(_privateKey);
            _web3 = new Web3(_account, _infuraUrl);
            _contract = _web3.Eth.GetContract(_abi, _contractAddress);
            _context = context;
            _accountForUsers = new Nethereum.Web3.Accounts.Account(_privateKeyForUsers); 
            _web3WithAccount = new Web3(_accountForUsers, _infuraUrl);
            _contractForUsers = _web3WithAccount.Eth.GetContract(_abi, _contractAddress);
        }

        // Example: Get total votes for a candidate
        public async Task<int> GetTotalVotesFor(string candidateName)
        {
            var totalVotesFunction = _contract.GetFunction("totalVotesFor");

            try
            {
                Console.WriteLine("Calling totalVotesFor function with candidate: " + candidateName);

                // Check if contract method is properly fetched
                var totalVotes = await totalVotesFunction.CallAsync<int>(candidateName);

                Console.WriteLine("Total votes fetched: " + totalVotes);

                return totalVotes;
            }
            catch (Exception ex)
            {
                // Log more details about the exception
                Console.WriteLine("Error occurred while fetching total votes: " + ex.Message);
                Console.WriteLine("Stack trace: " + ex.StackTrace);
                throw;
            }
        }

        public async Task<bool> CandidateExists(string candidateName)
        {
            var candidatesFunction = _contract.GetFunction("candidates");

            try
            {
                var candidateExists = await candidatesFunction.CallAsync<bool>(candidateName);
                return candidateExists;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking candidate existence: " + ex.Message);
                return false;
            }
        }


        public async Task<string> VoteForCandidate(string username, string candidate)
        {
            try
            {
                
                var voteFunction = _contractForUsers.GetFunction("voteForCandidate");

                var gas = new HexBigInteger(300000);
                var gasPrice = Web3.Convert.ToWei(20, UnitConversion.EthUnit.Gwei);

                var transactionHash = await voteFunction.SendTransactionAsync(
                    _accountForUsers.Address,
                    gas,
                    new HexBigInteger(gasPrice),
                    null,
                    username,
                    candidate
                );

                return transactionHash;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while casting the vote.", ex);
            }
        }




        // Example: Add a candidate
        public async Task AddCandidate(string candidate, string username)
        {
            // Check if user is an admin
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user?.Role != "admin")
            {
                Console.WriteLine("Only admins can add candidates.");
                return;  // Reject if not an admin
            }

            try
            {
                // Log candidate name for debugging purposes
                Console.WriteLine("Adding candidate: " + candidate);

                // Get the function and prepare the transaction
                var addCandidateFunction = _contract.GetFunction("addCandidate");

                var gasPrice = Web3.Convert.ToWei(20, UnitConversion.EthUnit.Gwei);
                var gasLimit = new HexBigInteger(100000);

                // Call the smart contract function with proper encoding
                var transactionHash = await addCandidateFunction.SendTransactionAsync(
                    from: _account.Address,
                    gas: gasLimit,
                    gasPrice: new HexBigInteger(gasPrice),
                    value: null,
                    functionInput: candidate // Ensure the string is correctly encoded
                );

                Console.WriteLine("Add Candidate Transaction Hash: " + transactionHash);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding candidate: " + ex.Message);
            }
        }

        public async Task ResetVoting()
        {

            var ResetVotingFunction = _contract.GetFunction("ResetVoting");
            var gasPrice = Web3.Convert.ToWei(20, UnitConversion.EthUnit.Gwei);
            var gasLimit = new HexBigInteger(100000);
            try
            {
                var transactionHash = await ResetVotingFunction.SendTransactionAsync(
                    from: _account.Address,
                    gas: gasLimit,
                    gasPrice: new HexBigInteger(gasPrice),
                    value: null
                );
                Console.WriteLine("Add Candidate Transaction Hash: " + transactionHash);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Ending vote: " + ex.Message);
            }
        }

        public async Task startVoting(long startTime, long endTime)
        {
            var startVotingFunction = _contract.GetFunction("startVoting");
            var gasPrice = Web3.Convert.ToWei(20, UnitConversion.EthUnit.Gwei);
            var gasLimit = new HexBigInteger(100000);

            Console.WriteLine("startTime: " + startTime);
            Console.WriteLine("endTime: " + endTime);

            try
            {
                // Convert directly to object parameters instead of HexBigInteger
                var transactionHash = await startVotingFunction.SendTransactionAsync(
                    from: _account.Address,
                    gas: gasLimit,
                    gasPrice: new HexBigInteger(gasPrice),
                    value: null,
                    functionInput: new object[] { startTime, endTime } // Pass as object array
                );
                Console.WriteLine("Starting vote Transaction Hash: " + transactionHash);
            }
            catch (SmartContractRevertException rex)
            {
                Console.WriteLine("REVERT Reason: " + rex.RevertMessage);
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Starting vote: " + ex.Message);
                throw;
            }
        }




        // In VotingService

        public async Task<List<object>> GetVotingStatus()
        {
            try
            {
                var votingStatusFunction = _contract.GetFunction("VotingStatus");
                // This should match your Solidity return type: (uint256, uint256)
                var result = await votingStatusFunction
                    .CallDeserializingToObjectAsync<GetVotingStatusOutput>();
                // Ensure result.StartTime and result.EndTime are not null before accessing them
                var startTime = DateTimeOffset.FromUnixTimeSeconds((long)result.StartTime).UtcDateTime;
                var endTime = DateTimeOffset.FromUnixTimeSeconds((long)result.EndTime).UtcDateTime;


                var output = new List<object>
                {
                    new {status = result.status, startTime = startTime, endTime = endTime }
                };
                Console.WriteLine("status: " + result.status);
                Console.WriteLine("start time: " + startTime);
                Console.WriteLine("end time: " + endTime);
                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching voting status: " + ex.Message);
                throw;
            }
        }




        public async Task<List<object>> GetCandidatesVotes(string privateKey)
        {
            try
            {

                var candidatesFunction = _contractForUsers.GetFunction("getAllCandidates");

                // This should match your Solidity return type: (string[], uint[])
                var result = await candidatesFunction
                    .CallDeserializingToObjectAsync<GetAllCandidatesOutput>();

                // Ensure result.Names and result.Votes are not null before accessing them
                if (result.Names == null || result.Votes == null)
                {
                    throw new InvalidOperationException("The result from the smart contract is null.");
                }

                var output = new List<object>();

                for (int i = 0; i < result.Names.Count; i++)
                {
                    output.Add(new CandidateResult
                    {
                        Name = result.Names[i],
                        Votes = (long)result.Votes[i]
                    });
                }

                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching candidate data: " + ex.Message);
                throw;
            }
        }

    }
}

