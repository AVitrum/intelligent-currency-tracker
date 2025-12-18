# Test Cases

Цей документ описує всі тест-кейси для проекту Intelligent Currency Tracker.

## Огляд

| № | Тест-кейс | Компонент | Тип тесту |
|---|-----------|-----------|-----------|
| 1 | GetLastByCurrencyIdAsync_ShouldReturnLatestRate_WhenRatesExist | RateRepository | Integration |
| 2 | GetLastByCurrencyIdAsync_ShouldReturnNull_WhenNoRatesExist | RateRepository | Integration |
| 3 | GetRangeAsync_ShouldReturnRatesInDateRange_WithPagination | RateRepository | Integration |
| 4 | AddRangeAsync_ShouldAddMultipleRates_WhenValidRatesProvided | RateRepository | Integration |
| 5 | RemoveByDateAsync_ShouldRemoveRates_WhenRatesExistForDate | RateRepository | Integration |
| 6 | RemoveByDateAsync_ShouldReturnFalse_WhenNoRatesExistForDate | RateRepository | Integration |
| 7 | GetAllCurrenciesAsync_ShouldReturnCurrencies_WhenCurrenciesExist | RateService | Unit |
| 8 | GetAllCurrenciesAsync_ShouldThrowException_WhenNoCurrenciesExist | RateService | Unit |
| 9 | GetDetailsAsync_ShouldReturnFailure_WhenNoRatesFound | RateService | Unit |
| 10 | GetDetailsAsync_ShouldReturnSuccess_WhenRatesExist | RateService | Unit |

---

## RateRepository Tests

### TC-001: GetLastByCurrencyIdAsync_ShouldReturnLatestRate_WhenRatesExist

**Опис:** Перевіряє, що метод `GetLastByCurrencyIdAsync` повертає останній курс валюти за датою.

**Передумови:**
- Створена in-memory база даних
- Додана тестова валюта (USD)
- Додані два курси з різними датами

**Кроки:**
1. Створити курс з датою T-2 дні
2. Створити курс з датою T-1 день
3. Викликати `GetLastByCurrencyIdAsync` з ID валюти

**Очікуваний результат:**
- Повертається курс з датою T-1 день
- Значення курсу відповідає очікуваному

---

### TC-002: GetLastByCurrencyIdAsync_ShouldReturnNull_WhenNoRatesExist

**Опис:** Перевіряє, що метод `GetLastByCurrencyIdAsync` повертає null, коли курси відсутні.

**Передумови:**
- Створена in-memory база даних
- Курси відсутні для вказаного ID валюти

**Кроки:**
1. Викликати `GetLastByCurrencyIdAsync` з неіснуючим ID валюти

**Очікуваний результат:**
- Повертається `null`

---

### TC-003: GetRangeAsync_ShouldReturnRatesInDateRange_WithPagination

**Опис:** Перевіряє роботу пагінації при отриманні курсів за діапазоном дат.

**Передумови:**
- Створена in-memory база даних
- Додана тестова валюта
- Додано 15 курсів за 15 днів

**Кроки:**
1. Викликати `GetRangeAsync` з діапазоном дат, page=1, pageSize=5

**Очікуваний результат:**
- Повертається рівно 5 записів
- Перший запис має найменшу дату
- Останній запис має п'яту за порядком дату

---

### TC-004: AddRangeAsync_ShouldAddMultipleRates_WhenValidRatesProvided

**Опис:** Перевіряє додавання кількох курсів одночасно.

**Передумови:**
- Створена in-memory база даних
- Додана тестова валюта

**Кроки:**
1. Створити список з 2 курсів
2. Викликати `AddRangeAsync`
3. Перевірити вміст бази даних

**Очікуваний результат:**
- Обидва курси збережені в базі даних
- Значення курсів відповідають очікуваним

---

### TC-005: RemoveByDateAsync_ShouldRemoveRates_WhenRatesExistForDate

**Опис:** Перевіряє видалення курсів за вказаною датою.

**Передумови:**
- Створена in-memory база даних
- Додана тестова валюта
- Доданий курс на сьогоднішню дату

**Кроки:**
1. Викликати `RemoveByDateAsync` з сьогоднішньою датою
2. Перевірити вміст бази даних

**Очікуваний результат:**
- Метод повертає `true`
- Курси за вказаною датою відсутні в базі даних

---

### TC-006: RemoveByDateAsync_ShouldReturnFalse_WhenNoRatesExistForDate

**Опис:** Перевіряє, що метод повертає false, коли курси за датою відсутні.

**Передумови:**
- Створена in-memory база даних
- Курси за вказаною датою відсутні

**Кроки:**
1. Викликати `RemoveByDateAsync` з датою 10 років тому

**Очікуваний результат:**
- Метод повертає `false`

---

## RateService Tests

### TC-007: GetAllCurrenciesAsync_ShouldReturnCurrencies_WhenCurrenciesExist

**Опис:** Перевіряє отримання списку всіх валют.

**Передумови:**
- Замокований `ICurrencyRepository` повертає список з 2 валют (USD, EUR)
- Замокований `IMapper` конвертує валюти в DTO

**Кроки:**
1. Викликати `GetAllCurrenciesAsync`

**Очікуваний результат:**
- Результат успішний (`Success = true`)
- Повернуто список з 2 валют
- Список містить USD та EUR

---

### TC-008: GetAllCurrenciesAsync_ShouldThrowException_WhenNoCurrenciesExist

**Опис:** Перевіряє, що виникає виключення при відсутності валют.

**Передумови:**
- Замокований `ICurrencyRepository` повертає порожній список

**Кроки:**
1. Викликати `GetAllCurrenciesAsync`

**Очікуваний результат:**
- Виникає виключення `EntityNotFoundException<Currency>`

---

### TC-009: GetDetailsAsync_ShouldReturnFailure_WhenNoRatesFound

**Опис:** Перевіряє повернення помилки при відсутності курсів для валюти.

**Передумови:**
- Замокований `ICurrencyRepository` повертає валюту USD
- Замокований `IRateRepository` повертає порожній список курсів

**Кроки:**
1. Викликати `GetDetailsAsync("USD", startDate, endDate)`

**Очікуваний результат:**
- Результат неуспішний (`Success = false`)
- Список помилок містить повідомлення про відсутність курсів

---

### TC-010: GetDetailsAsync_ShouldReturnSuccess_WhenRatesExist

**Опис:** Перевіряє успішне отримання деталей курсів для валюти.

**Передумови:**
- Замокований `ICurrencyRepository` повертає валюту USD
- Замокований `IRateRepository` повертає список з 2 курсів
- Замокований `IMapper` конвертує курси в DTO

**Кроки:**
1. Викликати `GetDetailsAsync("USD", startDate, endDate)`

**Очікуваний результат:**
- Результат успішний (`Success = true`)
- Метод `GetByCodeAsync` викликаний один раз
- Метод `GetRangeAsync` викликаний один раз

---

## Запуск тестів

```bash
# Запуск всіх тестів
dotnet test

# Запуск тестів з детальним виводом
dotnet test --logger "console;verbosity=detailed"

# Запуск тестів конкретного класу
dotnet test --filter "FullyQualifiedName~RateRepositoryTests"
```

## Структура тестів

```
tests/
└── Application.Tests/
    ├── RateRepositoryTests.cs  # Інтеграційні тести репозиторію курсів
    └── RateServiceTests.cs     # Unit-тести сервісу курсів
```

## Використані фреймворки

- **xUnit** - тестовий фреймворк
- **FluentAssertions** - бібліотека для читабельних assertions
- **Moq** - бібліотека для створення mock-об'єктів
- **Microsoft.EntityFrameworkCore.InMemory** - in-memory провайдер для EF Core

