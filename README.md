# Fitness Tracker API

## Описание

Необходимо разработать backend-приложение для трекинга фитнес-активностей, которое позволит пользователям записывать тренировки, отслеживать прогресс и анализировать показатели.

## Функционал Web API

- Получение списка всех тренировок пользователя;
- Получение конкретной тренировки по ID;
- Добавление новой тренировки;
- Обновление данных о тренировке;
- Удаление тренировки;
- Хранение и обработка медиа-материалов (фото прогресса).

## Стек

- **C#** & **.NET CORE 8** - язык и фреймворк для создания кроссплатформенных серверных приложений;
- **PostgreSQL** - реляционная база данных для хранения и управления данными;
- **ASP.NET** - фреймворк для построения RESTful API на платформе .NET;
- **EntityFramework Core** - ORM для работы с базой данных через C#-код;
- **xUnit** - инструмент для написания и запуска модульных тестов;
- **Docker** - платформа для контейнеризации и удобного развертывания приложений.

## Дополнительный функционал

- **JWT** аутентификация;
- Валидация всех входных данных через **DTO**;
- Расширенный поиск тренировок с:
    - Фильтрацией по типу, дате, продолжительности;
    - Сортировкой по дате, потраченным калориям;
    - Пагинацией результатов;

## Модели

```C#
public interface IDocument
{
    string Id { get; set; }
    DateTime CreatedAt { get; set; }
}

public class Workout : IDocument
{
    public string UserId { get; set; }
    public string Title { get; set; }
    public WorkoutType Type { get; set; }
    public List<Exercise> Exercises { get; set; }
    public TimeSpan Duration { get; set; }
    public int CaloriesBurned { get; set; }
    public List<string> ProgressPhotos { get; set; }
    public DateTime WorkoutDate { get; set; }
}

public class Exercise
{
    public string Name { get; set; }
    public List<Set> Sets { get; set; }
}

public class Set
{
    public int Reps { get; set; }
    public double Weight { get; set; }
}

public enum WorkoutType
{
    Strength,
    Cardio,
    Flexibility,
    HIIT,
    CrossFit
}
```

## Так же проект предполагает
- Строгое соответствие принципам **RESTful API**;
- Глобальная обработка ошибок через **middleware**;
- Следование **GitFlow** в процессе разработки;
- Следование **Conventional Commits** в процессе разработки; 
- Использование **Docker** и **Docker-compose**;
- Cоблюдение принципов **SOLID**.

**ВАЖНО!** Реализация должна находиться на **приватном** репозитории, на который необходимо добавить <code>SU-MCC</code> аккаунт.

## Полезные источники
- [C#](https://learn.microsoft.com/ru-ru/dotnet/csharp/)
- [ASP.NET](https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core?view=aspnetcore-9.0)
- [PostgreSQL](https://www.postgresql.org/docs/)
- [EntityFramework](https://learn.microsoft.com/ru-ru/ef/)
- [JWT](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-9.0)
- [xUnit](https://learn.microsoft.com/ru-ru/dotnet/core/testing/unit-testing-csharp-with-xunit)
- [Docker](https://www.docker.com/)
- [GitFlow](https://www.atlassian.com/ru/git/tutorials/comparing-workflows/gitflow-workflow)
- [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/)
