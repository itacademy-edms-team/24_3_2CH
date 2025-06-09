class QuizRenderer {
    constructor() {
        this.initializeQuizzes();
    }

    initializeQuizzes() {
        // Находим все контейнеры с данными опросов
        document.querySelectorAll('[data-quiz]').forEach(container => {
            const quizData = JSON.parse(container.dataset.quiz);
            this.renderQuiz(container, quizData);
        });
    }

    renderQuiz(container, quiz) {
        // Создаем основной контейнер опроса
        const quizElement = document.createElement('div');
        quizElement.className = 'quiz-container mt-3 p-3 border rounded';
        
        // Добавляем вопрос
        const questionElement = document.createElement('h5');
        questionElement.className = 'quiz-question mb-3';
        questionElement.textContent = quiz.question;
        quizElement.appendChild(questionElement);

        // Создаем форму для опций
        const form = document.createElement('form');
        form.className = 'quiz-options';
        form.dataset.quizId = quiz.id;

        // Добавляем опции
        quiz.options.forEach(option => {
            const optionContainer = document.createElement('div');
            optionContainer.className = 'form-check mb-2';

            const input = document.createElement('input');
            input.className = 'form-check-input';
            input.type = quiz.isMultiple ? 'checkbox' : 'radio';
            input.name = `quiz-${quiz.id}`;
            input.id = `option-${option.id}`;
            input.value = option.id;
            input.disabled = quiz.hasUserVoted;

            const label = document.createElement('label');
            label.className = 'form-check-label';
            label.htmlFor = `option-${option.id}`;
            label.textContent = option.text;

            // Добавляем прогресс-бар если есть результаты
            if (quiz.hasUserVoted) {
                const totalVotes = quiz.options.reduce((sum, opt) => sum + opt.votesCount, 0);
                const percentage = totalVotes > 0 ? (option.votesCount / totalVotes * 100).toFixed(1) : 0;
                
                const progress = document.createElement('div');
                progress.className = 'progress mt-1';
                progress.style.height = '20px';
                
                const progressBar = document.createElement('div');
                progressBar.className = 'progress-bar';
                progressBar.style.width = `${percentage}%`;
                progressBar.textContent = `${percentage}% (${option.votesCount} голосов)`;
                
                progress.appendChild(progressBar);
                optionContainer.appendChild(progress);
            }

            optionContainer.appendChild(input);
            optionContainer.appendChild(label);
            form.appendChild(optionContainer);
        });

        // Добавляем кнопку голосования если пользователь еще не голосовал
        if (!quiz.hasUserVoted) {
            const submitButton = document.createElement('button');
            submitButton.type = 'submit';
            submitButton.className = 'btn btn-primary mt-2';
            submitButton.textContent = 'Проголосовать';
            form.appendChild(submitButton);

            // Добавляем обработчик отправки формы
            form.addEventListener('submit', async (e) => {
                e.preventDefault();
                const formData = new FormData(form);
                const selectedOptions = Array.from(formData.getAll(`quiz-${quiz.id}`)).map(Number);
                
                if (selectedOptions.length === 0) {
                    alert('Выберите хотя бы один вариант ответа');
                    return;
                }

                try {
                    const response = await fetch('/api/Quiz/Vote', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                            optionIds: selectedOptions
                        })
                    });

                    if (response.ok) {
                        // Перезагружаем страницу чтобы обновить результаты
                        window.location.reload();
                    } else {
                        const error = await response.text();
                        alert(`Ошибка: ${error}`);
                    }
                } catch (error) {
                    alert('Произошла ошибка при отправке голоса');
                    console.error(error);
                }
            });
        }

        quizElement.appendChild(form);
        container.appendChild(quizElement);
    }
}

// Инициализируем рендерер при загрузке страницы
document.addEventListener('DOMContentLoaded', () => {
    new QuizRenderer();
}); 