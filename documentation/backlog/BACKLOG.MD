# Product Backlog - Intelligent Currency Tracker

**Загальний обсяг роботи:** 376 годин
**Кількість спринтів:** 5 (по 2 тижні)
**Орієнтовна тривалість:** 10 тижнів

## Product Vision
Система для моніторингу та прогнозування валютних курсів, що допомагає користувачам приймати обґрунтовані фінансові рішення через аналіз історичних даних та AI-прогнозування.

---

## Product Backlog Items

### Epic 1: Моніторинг валютних курсів

**US-001: Відображення актуальних курсів валют**
- **As a** користувач
- **I want** бачити поточні курси основних валют (USD, EUR, GBP, PLN)
- **So that** я можу швидко оцінити поточну ситуацію на ринку
- **Acceptance Criteria:**
    - Курси оновлюються кожні 15 хвилин
    - Показується час останнього оновлення
    - Відображаються мінімум 5 валютних пар
- **Estimate:** 20 годин
- **Priority:** High

**US-002: Інтеграція з зовнішніми API для отримання даних**
- **As a** система
- **I want** автоматично отримувати дані про курси з надійних джерел
- **So that** користувачі мають доступ до актуальної інформації
- **Acceptance Criteria:**
    - Інтеграція джерелами даних (fallback)
    - Обробка помилок при недоступності API
    - Логування всіх запитів
- **Estimate:** 24 години
- **Priority:** High

**US-003: Історичні дані валютних курсів**
- **As a** користувач
- **I want** переглядати історичні дані курсів за різні періоди
- **So that** я можу аналізувати тренди та приймати рішення
- **Acceptance Criteria:**
    - Доступні періоди: 1 день, 1 тиждень, 1 місяць, 3 місяці, 1 рік
    - Дані зберігаються в базі даних
    - Швидке завантаження (< 2 секунд)
- **Estimate:** 20 годин
- **Priority:** Medium

### Epic 2: Візуалізація та аналітика

**US-004: Інтерактивні графіки курсів валют**
- **As a** користувач
- **I want** бачити графіки зміни курсів у часі
- **So that** я можу візуально аналізувати тренди
- **Acceptance Criteria:**
    - Лінійні графіки та стовпчикові діаграми
    - Можливість вибору періоду відображення
- **Estimate:** 24 години
- **Priority:** High

**US-005: Dashboard з основними метриками**
- **As a** користувач
- **I want** бачити зведену інформацію на головній сторінці
- **So that** я швидко оцінюю ключові показники
- **Acceptance Criteria:**
    - Швидкий доступ до обраних валют
    - Відображення змін за останню добу/тиждень
    - Показ основних новин, що впливають на ринок
- **Estimate:** 20 годин
- **Priority:** Medium

### Epic 3: Прогнозування та AI

**US-006: Базове прогнозування на основі LSTM**
- **As a** користувач
- **I want** бачити прогноз курсу на найближчі дні
- **So that** я можу планувати валютні операції
- **Acceptance Criteria:**
    - Прогноз на 7, 14, 30 днів
    - Відображення довірчого інтервалу
    - Точність > 70% на тестових даних
- **Estimate:** 52 години
- **Priority:** High

**US-007: Покращення моделі прогнозування**
- **As a** Data Scientist
- **I want** експериментувати з різними ML-моделями
- **So that** підвищити точність прогнозів
- **Acceptance Criteria:**
    - Тестування мінімум 3 моделей (ARIMA, Prophet, LSTM)
    - Порівняльний аналіз результатів
    - Вибір найкращої моделі
- **Estimate:** 32 години
- **Priority:** Medium

### Epic 4: Сповіщення та алерти

**US-008: Email-сповіщення про зміни курсів**
- **As a** користувач
- **I want** отримувати email про значні зміни курсів
- **So that** я не пропущу важливі події
- **Acceptance Criteria:**
    - Налаштування порогових значень (наприклад, зміна > 2%)
    - Можливість вибору валют для відстеження
    - Відправка не частіше 1 разу на годину
- **Estimate:** 32 години
- **Priority:** Medium

**US-009: Розширені можливості системи**
- **As a** користувач
- **I want** мати доступ до додаткових функцій системи
- **So that** я можу використовувати повний функціонал платформи
- **Acceptance Criteria:**
    - OAuth авторизація через Google
    - Завантаження та зберігання файлів
    - Система постів та новин
- **Estimate:** 32 години
- **Priority:** Medium

### Epic 5: Користувацький профіль

**US-010: Реєстрація та авторизація**
- **As a** новий користувач
- **I want** створити обліковий запис
- **So that** я можу зберігати персональні налаштування
- **Acceptance Criteria:**
    - Email + пароль авторизація
    - JWT токени для безпеки
    - Можливість відновлення паролю
- **Estimate:** 32 години
- **Priority:** High

**US-011: Персоналізовані налаштування**
- **As a** користувач
- **I want** налаштовувати список відстежуваних валют
- **So that** бачити тільки релевантну інформацію
- **Acceptance Criteria:**
    - Список обраних валют
    - Налаштування сповіщень
    - Збереження в профілі користувача
- **Estimate:** 20 годин
- **Priority:** Medium

### Epic 6: DevOps та продуктивність

**US-012: CI/CD pipeline**
- **As a** розробник
- **I want** автоматизувати процес деплою
- **So that** прискорити випуск нових версій
- **Acceptance Criteria:**
    - Docker контейнеризація
    - Автоматичне тестування перед деплоєм
    - Деплой на staging та production
- **Estimate:** 28 годин
- **Priority:** Medium

---

## Sprint 1 Backlog (2 тижні) - COMPLETED

**Sprint Goal:** Створити базову інфраструктуру та відображення актуальних валютних курсів

**Team Capacity:** 80 годин

### Selected User Stories (Total: 64 години)

#### US-001: Відображення актуальних курсів валют (20 годин) - High Priority
**Tasks:**
- [x] Створення RateController.cs з API endpoints для курсів (6 годин)
- [x] Створення Domain entities: Rate.cs, Currency.cs (4 години)
- [x] Frontend компонент Dashboard.razor для відображення курсів (6 годин)
- [x] Стилізація Dashboard.razor.css (2 години)
- [x] Інтеграція та тестування RateServiceTests.cs (2 години)

#### US-002: Інтеграція з зовнішніми API (24 години) - High Priority
**Tasks:**
- [x] Дослідження та вибір API (PrivatBank API, NBU API) (8 годин)
- [x] Реалізація RateService.cs для інтеграції з NBU API (10 годин)
- [x] ExchangeRateSyncService.cs - background service для синхронізації (4 години)
- [x] Unit тести RateRepositoryTests.cs (2 години)

#### US-003: Історичні дані валютних курсів (20 годин) - Medium Priority
**Tasks:**
- [x] ApplicationDbContext.cs та схема БД з міграціями (6 годин)
- [x] RateController API для отримання історичних даних (8 годин)
- [x] Оптимізація запитів через Repositories (3 години)
- [x] CurrencyDetails.razor для відображення історії (3 години)

### Sprint Definition of Done
- [x] Код пройшов code review
- [x] Написані unit тести (coverage > 70%)
- [x] Задеплоєно на staging середовище (Docker)
- [x] Прийнято Product Owner
- [x] Немає критичних багів

---

## Sprint 2 Backlog (2 тижні) - COMPLETED

**Sprint Goal:** Реалізувати візуалізацію даних та інтерактивні графіки

**Team Capacity:** 80 годин

### Selected User Stories (Total: 64 години)

#### US-004: Інтерактивні графіки курсів валют (24 години) - High Priority
**Tasks:**
- [x] Компонент Chart/ для відображення графіків (10 годин)
- [x] CurrencyComparison.razor для порівняння валют (8 годин)
- [x] TrackedCharts.razor для відстеження обраних валют (4 години)
- [x] Стилізація CurrencyComparison.razor.css, TrackedCharts.razor.css (2 години)

#### US-005: Dashboard з основними метриками (20 годин) - Medium Priority
**Tasks:**
- [x] Dashboard.razor з зведеною інформацією (8 годин)
- [x] Home.razor головна сторінка з ключовими показниками (6 годин)
- [x] Компоненти для швидкого доступу до валют (4 години)
- [x] Стилізація Home.razor.css, Dashboard.razor.css (2 години)

#### US-010: Реєстрація та авторизація (20 годин - частина 1) - High Priority
**Tasks:**
- [x] ApplicationUser.cs модель користувача (4 години)
- [x] IdentityController.cs з базовими endpoints (8 годин)
- [x] Auth.razor сторінка авторизації (6 години)
- [x] Auth.razor.cs логіка авторизації (2 години)

### Sprint Definition of Done
- [x] Код пройшов code review
- [x] Написані unit тести
- [x] Графіки працюють коректно
- [x] Задеплоєно на staging середовище
- [x] Прийнято Product Owner
- [x] Немає критичних багів

---

## Sprint 3 Backlog (2 тижні) - COMPLETED

**Sprint Goal:** Реалізувати AI-прогнозування та завершити систему авторизації

**Team Capacity:** 80 годин

### Selected User Stories (Total: 84 години)

#### US-006: Базове прогнозування на основі LSTM (52 години) - High Priority
**Tasks:**
- [x] Python Model сервіс (main.py, model_controller.py) (16 годин)
- [x] LSTM модель model_service.py для прогнозування (20 годин)
- [x] AiModelController.cs для інтеграції з .NET API (8 годин)
- [x] AiModelService.cs для виклику Python сервісу (4 години)
- [x] Forecast.razor сторінка прогнозів (4 години)

#### US-010: Реєстрація та авторизація (12 годин - частина 2) - High Priority
**Tasks:**
- [x] JWT токени (Jwt/ folder) для безпеки (4 години)
- [x] RefreshToken.cs механізм оновлення токенів (4 години)
- [x] ChangePassword.razor для відновлення паролю (4 години)

#### US-012: CI/CD pipeline (20 годин - частина 1) - Medium Priority
**Tasks:**
- [x] docker-compose.yml налаштування (8 годин)
- [x] Dockerfile для WebApi, UI, Model (8 годин)
- [x] Налаштування PostgreSQL та MinIO (4 години)

### Sprint Definition of Done
- [x] Код пройшов code review
- [x] ML модель тестована
- [x] Docker контейнери працюють
- [x] Задеплоєно на staging середовище
- [x] Прийнято Product Owner
- [x] Немає критичних багів

---

## Sprint 4 Backlog (2 тижні) - COMPLETED

**Sprint Goal:** Реалізувати сповіщення, покращити ML-модель та профіль користувача

**Team Capacity:** 80 годин

### Selected User Stories (Total: 84 години)

#### US-007: Покращення моделі прогнозування (32 години) - Medium Priority
**Tasks:**
- [x] Тестування моделей та збереження станів (models/*.npy, *.txt) (12 годин)
- [x] AiModelUpdateService.cs для автоматичного оновлення (8 годин)
- [x] Порівняльний аналіз та оптимізація (8 годин)
- [x] Інтеграція з Forecast.razor.cs (4 години)

#### US-008: Email-сповіщення про зміни курсів (32 години) - Medium Priority
**Tasks:**
- [x] EmailSender.cs сервіс відправки email (12 годин)
- [x] AlertSenderService.cs для алертів про зміни курсів (10 годин)
- [x] Налаштування GMAIL інтеграції (6 годин)
- [x] SummarySenderService.cs для періодичних звітів (4 години)

#### US-011: Персоналізовані налаштування (20 годин) - Medium Priority
**Tasks:**
- [x] UserSettings.cs модель налаштувань (4 години)
- [x] Settings.razor сторінка налаштувань (6 годин)
- [x] Profile.razor сторінка профілю (6 години)
- [x] UserRateController.cs для обраних валют (4 години)

### Sprint Definition of Done
- [x] Код пройшов code review
- [x] Email сповіщення працюють
- [x] Налаштування зберігаються
- [x] Задеплоєно на staging середовище
- [x] Прийнято Product Owner
- [x] Немає критичних багів

---

## Sprint 5 Backlog (2 тижні) - COMPLETED

**Sprint Goal:** Завершити функціонал, додати додаткові можливості та фіналізувати проєкт

**Team Capacity:** 80 годин

### Selected User Stories (Total: 80 годин)

#### US-009: Розширені можливості системи (32 години) - Medium Priority
**Tasks:**
- [x] GoogleAuthController.cs та GoogleAuthService.cs для OAuth (12 годин)
- [x] MinioController.cs для роботи з файлами (8 годин)
- [x] PostController.cs та PostService.cs для постів/новин (8 годин)
- [x] Posts.razor, PostDetails.razor, CreatePost.razor сторінки (4 години)

#### US-012: CI/CD pipeline (8 годин - частина 2) - Medium Priority
**Tasks:**
- [x] Dockerfile.nginx та nginx.conf для продакшн (4 години)
- [x] Backup сервіс в docker-compose.yml (4 години)

#### Додатковий функціонал (40 годин)
**Tasks:**
- [x] ReportController.cs та ReportService.cs для звітів (10 годин)
- [x] Report.cs entity та Report.razor для звітності (8 годин)
- [x] Summary/ сервіси для аналітики (8 годин)
- [x] Contacts.razor сторінка контактів (4 години)
- [x] CsvService.cs експорт даних (6 годин)
- [x] Фінальне тестування та документація (4 години)

### Sprint Definition of Done
- [x] Код пройшов code review
- [x] Всі функції працюють коректно
- [x] Документація повна
- [x] Задеплоєно на production середовище
- [x] Прийнято Product Owner
- [x] Проєкт готовий до релізу
