using Domain.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Utils.Seeders;

public static class PostSeeder
{
    public static async Task SeedPostsAsync(IApplicationBuilder applicationBuilder)
    {
        using IServiceScope serviceScope = applicationBuilder.ApplicationServices.CreateScope();
        ApplicationDbContext dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await dbContext.Posts.AnyAsync())
        {
            return;
        }

        var adminUser = await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == "admin");
        string userId = adminUser?.Id ?? "system";

        ICollection<Post> posts =
        [
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Долар США досяг річного максимуму на тлі рішення ФРС",
                Content = "Федеральна резервна система США оголосила про підвищення процентної ставки на 0.25%, що спричинило зміцнення долара до найвищого рівня за останній рік. Аналітики прогнозують подальше зростання курсу USD відносно основних світових валют. Інвестори активно скуповують американську валюту, очікуючи стабільного зростання економіки США у 2026 році.",
                Category = PostCategory.News,
                UserId = userId,
                TimeStamp = new DateTimeOffset(2025, 12, 18, 10, 30, 0, TimeSpan.Zero),
                LastModifiedAt = new DateTimeOffset(2025, 12, 18, 10, 30, 0, TimeSpan.Zero)
            },
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Євро під тиском: ЄЦБ знижує прогноз зростання економіки",
                Content = "Європейський центральний банк переглянув прогноз зростання економіки єврозони у бік зниження, що негативно вплинуло на курс євро. Експерти вказують на енергетичну кризу та геополітичну нестабільність як основні фактори тиску на європейську валюту. Курс EUR/USD знизився до 1.05, найнижчого рівня за останні шість місяців.",
                Category = PostCategory.News,
                UserId = userId,
                TimeStamp = new DateTimeOffset(2025, 12, 17, 14, 15, 0, TimeSpan.Zero),
                LastModifiedAt = new DateTimeOffset(2025, 12, 17, 14, 15, 0, TimeSpan.Zero)
            },
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Гривня стабілізувалася: НБУ вжив додаткових заходів",
                Content = "Національний банк України повідомив про стабілізацію курсу гривні після впровадження нових монетарних заходів. Офіційний курс гривні до долара США встановлено на рівні 41.50 UAH/USD. Експерти НБУ прогнозують збереження стабільності до кінця року за умови продовження міжнародної фінансової підтримки.",
                Category = PostCategory.News,
                UserId = userId,
                TimeStamp = new DateTimeOffset(2025, 12, 16, 9, 0, 0, TimeSpan.Zero),
                LastModifiedAt = new DateTimeOffset(2025, 12, 16, 9, 0, 0, TimeSpan.Zero)
            },
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Британський фунт зміцнився після позитивних економічних даних",
                Content = "Курс британського фунта до долара США зріс на 1.2% після публікації даних про зростання ВВП Великобританії. Банк Англії зберіг процентну ставку на рівні 5.25%, що підтримало валюту. Аналітики очікують подальшого зміцнення GBP у разі продовження позитивної економічної динаміки.",
                Category = PostCategory.News,
                UserId = userId,
                TimeStamp = new DateTimeOffset(2025, 12, 15, 16, 45, 0, TimeSpan.Zero),
                LastModifiedAt = new DateTimeOffset(2025, 12, 15, 16, 45, 0, TimeSpan.Zero)
            },
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Прогноз: Що чекає на основні валюти у 2026 році",
                Content = "Експерти провідних інвестиційних банків оприлюднили прогнози щодо курсів валют на 2026 рік. Очікується, що долар США збереже лідерство серед основних валют, євро може ослабнути до 1.02 USD, а швейцарський франк залишиться стабільним. Ризики для прогнозів включають геополітичну напруженість та можливі зміни монетарної політики центробанків.",
                Category = PostCategory.Predictions,
                UserId = userId,
                TimeStamp = new DateTimeOffset(2025, 12, 14, 11, 0, 0, TimeSpan.Zero),
                LastModifiedAt = new DateTimeOffset(2025, 12, 14, 11, 0, 0, TimeSpan.Zero)
            },
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Японська єна впала до мінімуму: Банк Японії не змінює політику",
                Content = "Курс японської єни до долара США знизився до 155 JPY/USD після того, як Банк Японії вирішив зберегти ультрам'яку монетарну політику. Це найнижчий рівень єни за останні 30 років. Уряд Японії розглядає можливість валютних інтервенцій для підтримки національної валюти.",
                Category = PostCategory.News,
                UserId = userId,
                TimeStamp = new DateTimeOffset(2025, 12, 13, 8, 30, 0, TimeSpan.Zero),
                LastModifiedAt = new DateTimeOffset(2025, 12, 13, 8, 30, 0, TimeSpan.Zero)
            },
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Аналітика: Вплив криптовалют на традиційні валютні ринки",
                Content = "Дослідження показують, що зростання популярності криптовалют починає впливати на традиційні валютні ринки. Bitcoin досяг нового історичного максимуму в $105,000, що спричинило підвищену волатильність на Forex. Центральні банки багатьох країн прискорюють розробку власних цифрових валют (CBDC) у відповідь на ці тенденції.",
                Category = PostCategory.Analytics,
                UserId = userId,
                TimeStamp = new DateTimeOffset(2025, 12, 12, 13, 20, 0, TimeSpan.Zero),
                LastModifiedAt = new DateTimeOffset(2025, 12, 12, 13, 20, 0, TimeSpan.Zero)
            },
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Китайський юань зміцнюється на тлі торговельних переговорів",
                Content = "Курс китайського юаня зріс після оголошення про відновлення торговельних переговорів між Китаєм та США. Народний банк Китаю встановив довідковий курс на рівні 7.10 CNY/USD. Експерти вважають, що позитивна динаміка торговельних відносин може підтримати юань у середньостроковій перспективі.",
                Category = PostCategory.News,
                UserId = userId,
                TimeStamp = new DateTimeOffset(2025, 12, 11, 10, 0, 0, TimeSpan.Zero),
                LastModifiedAt = new DateTimeOffset(2025, 12, 11, 10, 0, 0, TimeSpan.Zero)
            },
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Оновлення системи: Нові функції прогнозування курсів",
                Content = "Ми раді повідомити про оновлення нашої системи прогнозування валютних курсів. Тепер алгоритми машинного навчання враховують більше факторів, включаючи геополітичні події та настрої ринку. Точність прогнозів покращилася на 15% порівняно з попередньою версією. Запрошуємо вас випробувати нові можливості!",
                Category = PostCategory.Updates,
                UserId = userId,
                TimeStamp = new DateTimeOffset(2025, 12, 10, 9, 0, 0, TimeSpan.Zero),
                LastModifiedAt = new DateTimeOffset(2025, 12, 10, 9, 0, 0, TimeSpan.Zero)
            },
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Швейцарський франк: Безпечна гавань в умовах невизначеності",
                Content = "На тлі глобальної економічної невизначеності швейцарський франк продовжує залучати інвесторів як «безпечна гавань». Курс CHF/USD зріс на 2% за останній тиждень. Швейцарський національний банк заявив про готовність вжити заходів для запобігання надмірному зміцненню франка.",
                Category = PostCategory.Analytics,
                UserId = userId,
                TimeStamp = new DateTimeOffset(2025, 12, 9, 15, 30, 0, TimeSpan.Zero),
                LastModifiedAt = new DateTimeOffset(2025, 12, 9, 15, 30, 0, TimeSpan.Zero)
            },
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Важливе оголошення: Технічні роботи на платформі",
                Content = "Шановні користувачі! Повідомляємо, що 20 грудня 2025 року з 02:00 до 06:00 за київським часом будуть проводитися планові технічні роботи. Під час цього періоду доступ до платформи може бути обмежений. Дякуємо за розуміння та вибачаємося за можливі незручності.",
                Category = PostCategory.Announcements,
                UserId = userId,
                TimeStamp = new DateTimeOffset(2025, 12, 8, 12, 0, 0, TimeSpan.Zero),
                LastModifiedAt = new DateTimeOffset(2025, 12, 8, 12, 0, 0, TimeSpan.Zero)
            },
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Турецька ліра під тиском: Інфляція залишається високою",
                Content = "Курс турецької ліри продовжує знижуватися на тлі високої інфляції, яка перевищила 60% річних. Центральний банк Туреччини підвищив процентну ставку до 45%, проте це не зупинило падіння валюти. Аналітики прогнозують подальше ослаблення TRY у разі збереження поточної економічної політики.",
                Category = PostCategory.News,
                UserId = userId,
                TimeStamp = new DateTimeOffset(2025, 12, 7, 11, 15, 0, TimeSpan.Zero),
                LastModifiedAt = new DateTimeOffset(2025, 12, 7, 11, 15, 0, TimeSpan.Zero)
            },
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Канадський долар реагує на ціни на нафту",
                Content = "Курс канадського долара до USD зріс після підвищення цін на нафту. Brent досягла $82 за барель, що підтримало валюти країн-експортерів енергоносіїв. Банк Канади зберіг процентну ставку без змін, що відповідало очікуванням ринку.",
                Category = PostCategory.News,
                UserId = userId,
                TimeStamp = new DateTimeOffset(2025, 12, 6, 14, 0, 0, TimeSpan.Zero),
                LastModifiedAt = new DateTimeOffset(2025, 12, 6, 14, 0, 0, TimeSpan.Zero)
            },
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Прогноз на тиждень: Основні валютні пари",
                Content = "Аналітичний огляд на тиждень 16-22 грудня 2025. EUR/USD: очікується торгівля в діапазоні 1.04-1.07. GBP/USD: можливе зростання до 1.28. USD/JPY: тиск на єну збережеться, ціль 156. Ключові події тижня: засідання ФРС, дані по інфляції в єврозоні, індекс споживчих настроїв США.",
                Category = PostCategory.Predictions,
                UserId = userId,
                TimeStamp = new DateTimeOffset(2025, 12, 5, 8, 0, 0, TimeSpan.Zero),
                LastModifiedAt = new DateTimeOffset(2025, 12, 5, 8, 0, 0, TimeSpan.Zero)
            },
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Австралійський долар відновлюється після падіння",
                Content = "Курс австралійського долара зріс на 0.8% після публікації позитивних даних про зайнятість в Австралії. Резервний банк Австралії натякнув на можливе підвищення ставки у першому кварталі 2026 року. Попит на сировинні товари з боку Китаю також підтримує AUD.",
                Category = PostCategory.News,
                UserId = userId,
                TimeStamp = new DateTimeOffset(2025, 12, 4, 10, 30, 0, TimeSpan.Zero),
                LastModifiedAt = new DateTimeOffset(2025, 12, 4, 10, 30, 0, TimeSpan.Zero)
            }
        ];

        await dbContext.Posts.AddRangeAsync(posts);
        await dbContext.SaveChangesAsync();
    }
}

