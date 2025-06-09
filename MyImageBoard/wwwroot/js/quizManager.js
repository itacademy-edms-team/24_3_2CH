const QuizConfig = {
    MAX_OPTIONS: 42
};

class QuizManager {
    static initializeCreateQuizModal() {
        // Проверяем, существует ли уже модальное окно
        if (document.getElementById('createQuizModal')) {
            return;
        }

        const modalHtml = `
            <div class="modal fade" id="createQuizModal" tabindex="-1">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">Создать опрос</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <form id="quizForm">
                                <div class="mb-3">
                                    <label for="quizQuestion" class="form-label">Вопрос</label>
                                    <input type="text" class="form-control" id="quizQuestion" required>
                                </div>
                                
                                <div class="mb-3">
                                    <label class="form-label">Тип опроса</label>
                                    <select class="form-select" id="quizType">
                                        <option value="single">Выбор одного ответа</option>
                                        <option value="multiple">Множественный выбор</option>
                                    </select>
                                </div>

                                <div id="optionsContainer">
                                    <div class="mb-3">
                                        <label class="form-label">Вариант 1</label>
                                        <div class="input-group">
                                            <input type="text" class="form-control quiz-option" required>
                                        </div>
                                    </div>
                                    <div class="mb-3">
                                        <label class="form-label">Вариант 2</label>
                                        <div class="input-group">
                                            <input type="text" class="form-control quiz-option" required>
                                        </div>
                                    </div>
                                </div>

                                <button type="button" class="btn btn-outline-primary btn-sm" id="addOptionBtn">
                                    <i class="bi bi-plus"></i> Добавить вариант
                                </button>
                            </form>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Отмена</button>
                            <button type="button" class="btn btn-primary" id="createQuizBtn">Создать опрос</button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Добавляем модальное окно в DOM
        document.body.insertAdjacentHTML('beforeend', modalHtml);

        // Инициализируем обработчики событий
        this.initializeEventHandlers();
    }

    static initializeEventHandlers() {
        const addOptionBtn = document.getElementById('addOptionBtn');
        const createQuizBtn = document.getElementById('createQuizBtn');
        const optionsContainer = document.getElementById('optionsContainer');

        addOptionBtn.addEventListener('click', () => {
            const optionsCount = optionsContainer.children.length;
            
            if (optionsCount >= QuizConfig.MAX_OPTIONS) {
                alert(`Максимальное количество вариантов: ${QuizConfig.MAX_OPTIONS}`);
                return;
            }

            const newOption = document.createElement('div');
            newOption.className = 'mb-3';
            newOption.innerHTML = `
                <label class="form-label">Вариант ${optionsCount + 1}</label>
                <div class="input-group">
                    <input type="text" class="form-control quiz-option" required>
                    <button type="button" class="btn btn-outline-danger" onclick="QuizManager.removeOption(this)">
                        <i class="bi bi-x"></i>
                    </button>
                </div>`;

            optionsContainer.appendChild(newOption);
        });

        createQuizBtn.addEventListener('click', () => this.handleQuizCreation());
    }

    static removeOption(button) {
        const optionContainer = button.closest('.mb-3');
        optionContainer.remove();
        this.renumberOptions();
    }

    static renumberOptions() {
        const optionsContainer = document.getElementById('optionsContainer');
        const options = optionsContainer.children;
        
        for (let i = 0; i < options.length; i++) {
            const label = options[i].querySelector('.form-label');
            label.textContent = `Вариант ${i + 1}`;
        }
    }

    static async handleQuizCreation() {
        const form = document.getElementById('quizForm');
        const question = document.getElementById('quizQuestion').value.trim();
        const isMultiple = document.getElementById('quizType').value === 'multiple';
        const options = Array.from(form.querySelectorAll('.quiz-option'))
            .map(input => input.value.trim())
            .filter(value => value);

        if (!question) {
            alert('Введите вопрос');
            return;
        }

        if (options.length < 2) {
            alert('Добавьте как минимум два варианта ответа');
            return;
        }

        const quiz = {
            Question: question,
            Options: options,
            IsMultiple: isMultiple
        };

        // Добавляем опрос в скрытое поле формы создания треда
        const quizzesInput = document.getElementById('threadQuizzes');
        const currentQuizzes = quizzesInput.value ? JSON.parse(quizzesInput.value) : [];
        currentQuizzes.push(quiz);
        quizzesInput.value = JSON.stringify(currentQuizzes);

        // Обновляем превью опросов
        this.updateQuizzesPreviews(currentQuizzes);

        // Закрываем модальное окно
        const modal = bootstrap.Modal.getInstance(document.getElementById('createQuizModal'));
        modal.hide();

        // Очищаем форму
        form.reset();
        const optionsContainer = document.getElementById('optionsContainer');
        while (optionsContainer.children.length > 2) {
            optionsContainer.lastChild.remove();
        }
    }

    static updateQuizzesPreviews(quizzes) {
        const previewContainer = document.getElementById('quizzesPreviews');
        previewContainer.innerHTML = quizzes.map((quiz, index) => `
            <div class="card mb-3">
                <div class="card-body">
                    <h5 class="card-title">${quiz.Question}</h5>
                    <p class="card-text text-muted">
                        ${quiz.IsMultiple ? 'Множественный выбор' : 'Выбор одного варианта'}
                    </p>
                    <ul class="list-group list-group-flush">
                        ${quiz.Options.map(option => `
                            <li class="list-group-item">${option}</li>
                        `).join('')}
                    </ul>
                    <button type="button" class="btn btn-outline-danger btn-sm mt-2" 
                            onclick="QuizManager.removeQuiz(${index})">
                        <i class="bi bi-x"></i> Удалить опрос
                    </button>
                </div>
            </div>
        `).join('');
    }

    static removeQuiz(index) {
        const quizzesInput = document.getElementById('threadQuizzes');
        const quizzes = JSON.parse(quizzesInput.value);
        quizzes.splice(index, 1);
        quizzesInput.value = JSON.stringify(quizzes);
        this.updateQuizzesPreviews(quizzes);
    }

    static renderQuiz(quiz) {
        const inputType = quiz.isMultiple ? 'checkbox' : 'radio';
        const optionsHtml = quiz.options.map((option, index) => `
            <div class="form-check mb-2">
                <input class="form-check-input" type="${inputType}" 
                       name="optionIds_${quiz.id}" 
                       value="${option.id}" 
                       id="option_${option.id}"
                       ${option.hasVoted ? 'checked disabled' : ''}>
                <label class="form-check-label" for="option_${option.id}">
                    ${option.text}
                </label>
                <div class="progress mt-1" style="height: 4px;">
                    <div class="progress-bar" role="progressbar" 
                         style="width: ${option.percentage}%"></div>
                </div>
                <small class="text-muted">
                    ${option.votesCount} голосов (${option.percentage.toFixed(1)}%)
                </small>
            </div>
        `).join('');

        return `
            <div class="quiz-container border rounded p-3 my-3" data-quiz-id="${quiz.id}">
                <form class="quiz-form" onsubmit="return QuizManager.handleVote(event)">
                    <input type="hidden" name="quizId" value="${quiz.id}">
                    <div class="quiz-question mb-3">
                        <h5>${quiz.question}</h5>
                    </div>
                    <div class="quiz-options">
                        ${optionsHtml}
                    </div>
                    ${!quiz.hasVoted ? `
                        <div class="quiz-submit mt-3">
                            <button type="submit" class="btn btn-primary">Проголосовать</button>
                        </div>
                    ` : ''}
                </form>
            </div>
        `;
    }
} 