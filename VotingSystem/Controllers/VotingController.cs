using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VotingSystem.Services;
using System;
using VotingSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace VotingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VotingController : ControllerBase
    {
        private readonly VotingService _votingService;
        private readonly VotingContext _context;

        public VotingController(VotingContext context, VotingService votingService)
        {
            _context = context;
            _votingService = votingService;
        }

        // Endpoint to get total votes for a candidate
        [HttpGet("getTotalVotesFor")]
        public async Task<IActionResult> GetTotalVotesFor(string candidateName)
        {
            if (string.IsNullOrEmpty(candidateName))
            {
                return BadRequest(new { message = "Candidate name is required" });
            }

            try
            {
                var totalVotes = await _votingService.GetTotalVotesFor(candidateName);
                return Ok(new { candidate = candidateName, totalVotes });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error fetching total votes", details = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("vote")]
        public async Task<IActionResult> VoteForCandidate([FromBody] VoteRequest request)
        {
            var username = User.Identity?.Name; // gets it from token claims
            if (string.IsNullOrEmpty(username))
                return Unauthorized("User not authenticated.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return BadRequest("User not found.");

            if (user.HasVoted)
                return BadRequest("You have already voted.");

            var transactionHash = await _votingService.VoteForCandidate(username, request.Candidate);
            Console.WriteLine($"Vote TX: {transactionHash} for user {username}");

            user.HasVoted = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Vote successfully recorded." });
        }


        [Authorize(Roles = "admin")]  // Only admins can add candidates
        [HttpPost("addCandidate")]
        public async Task<IActionResult> AddCandidate([FromBody] AddCandidateRequest request)
        {
            if (string.IsNullOrEmpty(request.CandidateName))
            {
                return BadRequest(new { message = "Candidate name is required" });
            }

            try
            {
                var username = User.Identity?.Name;

                if (string.IsNullOrEmpty(username))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // Call the service method to add a candidate
                await _votingService.AddCandidate(request.CandidateName, username);

                return Ok(new { message = "Candidate successfully added!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error adding candidate", details = ex.Message });
            }
        }

        [Authorize(Roles = "admin")]  // Only admins can add candidates
        [HttpPost("startVoting")]
        public async Task<IActionResult> startVoting([FromBody] StartVotingRequest request)
        {
            try
            {
                await _votingService.startVoting(request.startTime, request.endTime);

                return Ok(new { message = "voting successfully started." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error starting vote", details = ex.Message });
            }
        }


        [Authorize(Roles = "admin")]  // Only admins can add candidates
        [HttpDelete("ResetVoting")]
        public async Task<IActionResult> ResetVoting()
        {

            try
            {
                // Call the service method to add a candidate
                await _votingService.ResetVoting();
                var users = _context.Users.ToList();

                foreach (var user in users)
                {
                    user.HasVoted = false;
                }

                _context.SaveChanges();


                return Ok(new { message = "The vote has ended successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error Ending vote", details = ex.Message });
            }
        }

        // Add a new model to capture candidate name
        public class AddCandidateRequest
        {
            public string? CandidateName { get; set; }
        }


        [HttpGet("status")]
        public async Task<IActionResult> GetVotingStatus()
        {
            try
            {
                // Implementation to fetch status from your service
                var status = await _votingService.GetVotingStatus();
                return Ok(status);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error getting voting status", details = ex.Message });
            }
        }


        [HttpGet("GetCandidatesVotes")]
        public async Task<IActionResult> GetCandidatesVotes()
        {
            try
            {
                // Fetch private key securely - do not expose it in logs
                var privateKey = Environment.GetEnvironmentVariable("PRIVATE_KEY");
                if (string.IsNullOrEmpty(privateKey))
                {
                    return BadRequest(new { message = "Private key not found or is invalid." });
                }

                var candidatesVotes = await _votingService.GetCandidatesVotes(privateKey);

                return Ok(candidatesVotes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        public class VoteRequest
        {
            public string? Candidate { get; set; }
        }

        public class StartVotingRequest
        {
            public long startTime { get; set; }
            public long endTime { get; set; }
        }
    }
    

    public class EndTimeRequest
    {
        public long EndTime { get; set; }
    }
}
