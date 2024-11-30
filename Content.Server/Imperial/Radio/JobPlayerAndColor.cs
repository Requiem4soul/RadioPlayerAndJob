using System.Text.RegularExpressions;
using Content.Server.Access.Systems;
using Robust.Shared.Utility;
using Robust.Shared.Prototypes;
using Content.Shared.Imperial.Radio;

namespace Content.Server.Imperial.Radio;

/// <summary>
/// Данный класс отвечает за отображения должностей в канале рации. То есть, если ошибка в Content.Radi.EnitySystem - вам сюда. Так же в самом оригинальном коде рации есть способ быстро востановить его оригинальную работу без моего класса
/// </summary>
public sealed class JobPlayerAndColor : EntitySystem
{
    // Для работы с id картой
    [Dependency] private readonly IdCardSystem _idCardSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    // Словарь для перевода должностей и для должностей состоящих не только из букв
    private static readonly Dictionary<string, string> JobTranslations = new()
    {
        { "Central Commander", "Центральное Командование" },
        { "Centcom Quarantine Officer", "РХБЗЗ" }
    };

    private string TranslateJob(string job) =>
        JobTranslations.TryGetValue(job, out var translated) ? translated : job;

    // FIX: Не надо описывать логику работы метода

	/// <summary>
    /// Получает id работы из сущности <paramref name="uid"/>
    /// </summary>
    /// <param name="uid"> uid сущности, у которого мы ищем карту</param>
    /// <returns>Возвращает id работы или "неизвестно", если таковая не обнаружена</returns>
    public string GetJobPlayer(EntityUid uid)
    {
        // FIX: Ревёрсим условие для уменьшения вложенности (если значение не найдено, то мы сразу же возвращаем и else не нужен)

        // Если id карта не найдена
        if (!_idCardSystem.TryFindIdCard(uid, out var id))
            return "Неизвестно";

        string? playerJob = id.Comp.LocalizedJobTitle;

        // Если работа не определена
        if (playerJob is null)
            return "Неизвестно";


        playerJob = char.ToUpper(playerJob[0]) + playerJob.Substring(1);
        // Убираем символы ----, которые ломают вывод в рацию заменяя их на пробелы
        playerJob = Regex.Replace(playerJob, @"[-–—−]", " ");
        // Убираем символы (типа ?!), которые ломают вывод в рацию
        playerJob = Regex.Replace(playerJob, @"[^a-zA-Zа-яА-ЯёЁ ]", "");
        playerJob = TranslateJob(playerJob);

        return playerJob.Trim();
    }

    // FIX: Выносить подобные значения всё равно стоит в константы,
    // т. к. незнающий человек возможно захочет поменять это значение,
    // а незнание C# ему может очень сильно помешать
    // Но если значение в константе это всё ещё легче понять.

    // А так же все константы могут находиться в одном месте для удобного изменения
    // без необходимости разобраться где что какой метод делает

    /// <summary>
    /// Стандартный цвет при неизвестной должности
    /// </summary>
    const string DefaultColor = "lime";

    /// <summary>
    /// Определяет цвет по id профессии (<see cref="DefaultColor"/> если цвет не сохранён)
    /// </summary>
    /// <param name="jobPlayer"> Нужно получить из метода GetJobPlayer. Или же вставляйте сюда свою string? работу, но нужно привести её к нижниму регистру для словаря. Метод ваш_стринг.ToLower()</param>
    public string GetColorPlayer(string jobPlayer)
    {
        string normalizedJob = jobPlayer.ToLower();

        // FIX: Index метод точно так же выполняется за O(1)
        // Если цвет хранится, то возвращаем его, иначе дефолтный
        return _prototypeManager.TryIndex<IdColorAndJobPrototype>(normalizedJob, out var formatInfo) ? formatInfo.Color : DefaultColor;
    }

    /// <summary>
    /// Данный метод, используя два прошлых метода, формирует уже необходимый name, который и будет отображён в рации. На самом деле, его можно применять и к ChatSystem.cs
    /// </summary>
    /// <param name="uid"> id отправителя. В оригинальном коде рации messageSource</param>
    /// <param name="name"> string?. Это по сути то, что будет указано в отправителе. Раньше там писалось просто имя</param>
    /// <returns></returns>
    public string CompletedJobAndPlayer(EntityUid uid, string? name)
    {
        string nameJobPlayer = GetJobPlayer(uid);
        string nameColorPlayer = GetColorPlayer(nameJobPlayer);

        // Ескейпим все символы форматирования, что бы игрок не мог форматировать своё имя
        var displayName = FormattedMessage.EscapeText($"[{nameJobPlayer}] {name}");

        return $"[bold][color={nameColorPlayer}]{displayName}[/bold][/color]";
    }
}

