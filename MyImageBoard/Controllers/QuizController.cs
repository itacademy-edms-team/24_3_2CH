using Microsoft.AspNetCore.Mvc;
using ForumProject.Services.Interfaces;
using System.Threading.Tasks;

namespace ForumProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly IUserFingerprintService _fingerprintService;

        public class VoteRequest
        {
            public int[] OptionIds { get; set; }
        }

        public class QuizCreateRequest
        {
            public string Question { get; set; }
            public List<string> Options { get; set; }
            public bool IsMultiple { get; set; }
        }

        public class BatchQuizCreateRequest
        {
            public int ThreadId { get; set; }
            public List<QuizCreateRequest> Quizzes { get; set; }
        }

        public QuizController(IQuizService quizService, IUserFingerprintService fingerprintService)
        {
            _quizService = quizService;
            _fingerprintService = fingerprintService;
        }

        [HttpGet("HasVoted/{quizId}")]
        public async Task<IActionResult> HasVoted(int quizId)
        {
            try
            {
                var fingerprint = await _fingerprintService.GetOrCreateFingerprintAsync(HttpContext);
                var hasVoted = await _quizService.HasUserVotedAsync(quizId, fingerprint.Id);
                return Ok(new { hasVoted });
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Vote")]
        public async Task<IActionResult> Vote([FromBody] VoteRequest request)
        {
            try
            {
                var fingerprint = await _fingerprintService.GetOrCreateFingerprintAsync(HttpContext);
                await _quizService.AddVoteAsync(request.OptionIds, fingerprint.Id);
                return Ok();
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreateBatch")]
        public async Task<IActionResult> CreateBatch([FromBody] BatchQuizCreateRequest request)
        {
            try
            {
                var createdQuizzes = new List<object>();
                foreach (var quizRequest in request.Quizzes)
                {
                    var quiz = await _quizService.CreateQuizAsync(
                        request.ThreadId,
                        quizRequest.Question,
                        quizRequest.Options,
                        quizRequest.IsMultiple
                    );

                    if (quiz != null)
                    {
                        createdQuizzes.Add(new
                        {
                            id = quiz.Id,
                            question = quiz.Question,
                            isMultiple = quiz.IsMultiple,
                            options = quiz.Options.Select(o => new
                            {
                                id = o.Id,
                                text = o.Text,
                                votesCount = o.VotesCount
                            })
                        });
                    }
                }

                return Ok(createdQuizzes);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
} 