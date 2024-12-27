using Robust.Shared.Prototypes;

namespace Content.Shared.Imperial.Radio;

// Copyright (c) 2024 Requiem4soul (https://github.com/Requiem4soul). Licensed under MIT License. This copyright notice must not be removed. This file

/// <summary>
/// Прототип для хранения данных о должностях и их цветах.
/// </summary>
[Prototype("idColor")]
public sealed class IdColorAndJobPrototype : IPrototype
{
    /// <summary>
    /// Уникальный идентификатор для должности.
    /// </summary>
    [IdDataField]
    public string ID { get; private set; } = "Неизвестно";

    /// <summary>
    /// Цвет, соответствующий должности.
    /// </summary>
    [DataField("color", required: true)]
    public string Color { get; private set; } = "lime";
}