using System.Text.RegularExpressions;
using ForumProject.Data;
using ForumProject.Data.Models;
using ForumProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ForumProject.Configuration;

namespace ForumProject.Services
{
    public class QuizService : IQuizService
    {
        private readonly ApplicationDbContext _context;

        public QuizService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Quiz?> CreateQuizAsync(int threadId, string question, List<string> options, bool isMultiple)
        {
            if (string.IsNullOrWhiteSpace(question) || !options.Any())
                return null;

            // Проверяем существование треда
            var thread = await _context.Threads.FindAsync(threadId);
            if (thread == null)
                throw new ArgumentException($"Тред с ID {threadId} не найден");

            // Создаем опрос
            var quiz = new Quiz
            {
                ThreadId = threadId,
                Question = question,
                IsMultiple = isMultiple,
                Options = options.Select(o => new QuizOption { Text = o }).ToList()
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return quiz;
        }

        public async Task<bool> HasUserVotedAsync(int quizId, int fingerprintId)
        {
            return await _context.QuizVotes
                .AnyAsync(v => v.QuizOption.QuizId == quizId && v.UserFingerprintId == fingerprintId);
        }

        public async Task AddVoteAsync(int[] optionIds, int fingerprintId)
        {
            if (!optionIds.Any())
                throw new ArgumentException("Необходимо выбрать хотя бы один вариант ответа");

            // Проверяем, что все опции принадлежат одному опросу
            var firstOption = await _context.QuizOptions
                .Include(o => o.Quiz)
                .FirstOrDefaultAsync(o => optionIds.Contains(o.Id));

            if (firstOption == null)
                throw new ArgumentException("Вариант ответа не найден");

            var quiz = firstOption.Quiz;
            if (!quiz.IsMultiple && optionIds.Length > 1)
                throw new InvalidOperationException("В этом опросе можно выбрать только один вариант");

            var quizId = firstOption.QuizId;

            // Проверяем, что пользователь еще не голосовал
            if (await HasUserVotedAsync(quizId, fingerprintId))
                throw new InvalidOperationException("Вы уже голосовали в этом опросе");

            // Создаем голоса
            var votes = optionIds.Select(optionId => new QuizVote
            {
                QuizOptionId = optionId,
                UserFingerprintId = fingerprintId
            });

            _context.QuizVotes.AddRange(votes);

            // Обновляем счетчики голосов
            var options = await _context.QuizOptions
                .Where(o => optionIds.Contains(o.Id))
                .ToListAsync();

            foreach (var option in options)
            {
                option.VotesCount++;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Dictionary<int, int>> GetQuizStatisticsAsync(int quizId)
        {
            var options = await _context.QuizOptions
                .Where(o => o.QuizId == quizId)
                .Select(o => new { o.Id, o.VotesCount })
                .ToDictionaryAsync(o => o.Id, o => o.VotesCount);

            return options;
        }

        public async Task<int> GetThreadIdByQuizIdAsync(int quizId)
        {
            var quiz = await _context.Quizzes
                .Where(q => q.Id == quizId)
                .Select(q => new { q.ThreadId })
                .FirstOrDefaultAsync();

            if (quiz == null)
                throw new ArgumentException("Опрос не найден");

            return quiz.ThreadId;
        }

        public async Task<int> GetQuizIdByOptionIdAsync(int optionId)
        {
            var option = await _context.QuizOptions
                .Where(o => o.Id == optionId)
                .Select(o => new { o.QuizId })
                .FirstOrDefaultAsync();

            if (option == null)
                throw new ArgumentException("Вариант ответа не найден");

            return option.QuizId;
        }
    }
} 