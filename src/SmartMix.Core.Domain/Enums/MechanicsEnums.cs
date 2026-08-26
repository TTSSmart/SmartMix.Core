namespace SmartMix.Core.Domain.Enums;

/// <summary>
/// Тип механизма
/// </summary>
public enum MechanicsType
{
    Unknown = 0,
    Doser = 1,           // Дозатор
    Mixer = 2,           // Смеситель
    Skip = 3,            // Скип
    Accumulator = 4,     // Накопитель
    Bunker = 5,          // Бункер
    Viber = 6,           // Вибратор/Аэратор
    Valve = 7,           // Клапан
    TransportLine = 8,   // Транспортная линия
    Compressor = 9,      // Компрессор
    Switch = 10,         // Переключатель/Распределитель
    Kubel = 11,          // Кюбель
    Sensor = 12,         // Датчик
    Panel = 13,          // Панель
    Flowmeter = 14,      // Расходомер
    Lubricator = 15      // Смазчик
}

/// <summary>
/// Статус дозатора
/// </summary>
public enum DoserStatus
{
    Unknown = 0,
    Ready = 1,           // Готов
    Dosing = 2,          // Дозирование
    Unloading = 3,       // Выгрузка
    Waiting = 4,         // Ожидание
    Error = 5,           // Ошибка
    Calibrating = 6,     // Калибровка
    Washing = 7,         // Промывка
    Maintenance = 8      // Обслуживание
}

/// <summary>
/// Статус смесителя
/// </summary>
public enum MixerStatus
{
    Unknown = 0,
    Ready = 1,           // Готов
    Loading = 2,         // Загрузка
    Mixing = 3,          // Перемешивание
    Unloading = 4,       // Выгрузка
    Waiting = 5,         // Ожидание команды
    Error = 6,           // Ошибка
    Washing = 7,         // Промывка
    Paused = 8,          // Пауза
    Maintenance = 9      // Обслуживание
}

/// <summary>
/// Статус скипа
/// </summary>
public enum SkipStatus
{
    Unknown = 0,
    Ready = 1,           // Готов
    Loading = 2,         // Загрузка
    MovingUp = 3,        // Подъем
    MovingDown = 4,      // Опускание
    Unloading = 5,       // Выгрузка
    Waiting = 6,         // Ожидание
    Error = 7            // Ошибка
}

/// <summary>
/// Статус накопителя
/// </summary>
public enum AccumulatorStatus
{
    Unknown = 0,
    Ready = 1,           // Готов
    Loading = 2,         // Загрузка
    Unloading = 3,       // Выгрузка
    Waiting = 4,         // Ожидание
    Error = 5            // Ошибка
}

/// <summary>
/// Состояние клапана
/// </summary>
public enum ValveStatus
{
    Unknown = 0,
    Closed = 1,          // Закрыт
    Open = 2,            // Открыт
    Opening = 3,         // Открывается
    Closing = 4,         // Закрывается
    Error = 5,           // Ошибка
    Alarm = 6            // Авария (не открылся/не закрылся)
}

/// <summary>
/// Режим выгрузки клапана
/// </summary>
public enum ValveUnloadMode
{
    Automatic = 0,       // Автоматический
    Impulse = 1,         // Импульсный
    Permanent = 2,       // Постоянный
    Manual = 3           // Ручной
}

/// <summary>
/// Режим выгрузки смесителя
/// </summary>
public enum MixerUnloadMode
{
    Automatic = 0,       // Автоматический (как в авто)
    Impulse = 1,         // Импульсный
    ByCommand = 2        // По команде
}

/// <summary>
/// Режим выгрузки дозатора
/// </summary>
public enum DoserUnloadMode
{
    Automatic = 0,       // Автоматический
    Impulse = 1,         // Импульсный
    Permanent = 2,       // Постоянный
    Manual = 3           // Ручной
}

/// <summary>
/// Статус заявки
/// </summary>
public enum ApplicationStatus
{
    Unknown = 0,
    Created = 1,         // Создана
    ReadingData = 2,     // Чтение данных
    Dosing = 3,          // Дозирование
    WaitingUnload = 4,   // Ожидание выгрузки
    Unloading = 5,       // Выгрузка
    Complete = 6,        // Завершена
    Cancelled = 7,       // Отменена
    Error = 8            // Ошибка
}

/// <summary>
/// Тип компонента
/// </summary>
public enum ComponentType
{
    Unknown = 0,
    Cement = 1,          // Цемент
    Sand = 2,            // Песок
    Gravel = 3,          // Щебень
    Water = 4,           // Вода
    Additive = 5,        // Добавка
    Filler = 6,          // Заполнитель
    Fiber = 7            // Волокно
}

/// <summary>
/// Уровень влажности для калибровки
/// </summary>
public enum LevelHumidity
{
    Min = 1,             // Минимальный
    Middle = 2,          // Средний
    Max = 3              // Максимальный
}

/// <summary>
/// Тип завода
/// </summary>
public enum FactoryType
{
    Unknown = 0,
    Stationary = 1,      // Стационарный
    Mobile = 2,          // Мобильный
    Compact = 3          // Компактный
}

/// <summary>
/// Тип ПЛК
/// </summary>
public enum PlcType
{
    Unknown = 0,
    Crevis = 1,          // CREVIS
    Modbus = 2,          // Modbus TCP
    Siemens = 3,         // Siemens
    Ovation = 4          // Ovation
}

/// <summary>
/// Уровень события
/// </summary>
public enum EventLevel
{
    Info = 1,            // Информация
    Warning = 2,         // Предупреждение
    Critical = 3,        // Критическое
    Emergency = 4        // Аварийное
}

/// <summary>
/// Тип отчета
/// </summary>
public enum ReportType
{
    Unknown = 0,
    Application = 1,     // По заявке
    Consumption = 2,     // Потребление
    Bunker = 3,          // По бункерам
    Shift = 4,           // Сменный
    Daily = 5,           // Суточный
    Monthly = 6          // Месячный
}

/// <summary>
/// Режим работы программы
/// </summary>
public enum WorkType
{
    Recipe = 1,          // По рецепту
    Product = 2,         // По продукции
    Manual = 3,          // Ручной
    Auto = 4             // Автоматический
}

/// <summary>
/// Статус слоя заявки
/// </summary>
public enum LayerStatus
{
    Wait = 0,            // Ожидание
    Active = 1,          // Активный
    Complete = 2,        // Завершен
    Error = 3            // Ошибка
}

/// <summary>
/// Статус ворот/затворов
/// </summary>
public enum GateStatus
{
    Unknown = 0,
    Closed = 1,
    Open = 2,
    Opening = 3,
    Closing = 4,
    Error = 5
}

/// <summary>
/// Режим работы миксера (перемешивание)
/// </summary>
public enum MixTimeMode
{
    Common = 0,          // Общее время для всех
    PerRecipe = 1        // Из рецепта
}

/// <summary>
/// Тип загрузки в смеситель
/// </summary>
public enum MixerUploadType
{
    Unknown = 0,
    Single = 1,          // Одностадийная
    TwoStage = 2         // Двухстадийная
}

/// <summary>
/// Тип моточасов
/// </summary>
public enum MotoHoursType
{
    Unknown = 0,
    Mixer = 1,           // Смеситель
    Valve = 2,           // Клапан
    Viber = 3,           // Вибратор
    Start = 4,           // Пуск
    Compressor = 5,      // Компрессор
    Transport = 6,       // Транспорт
    Skip = 7,            // Скип
    Accumulator = 8      // Накопитель
}

/// <summary>
/// Результат аутентификации
/// </summary>
public enum AuthenticationResult
{
    Success = 0,         // Успешно
    InvalidCredentials = 1, // Неверные учетные данные
    UserDisabled = 2,    // Пользователь отключен
    LicenseExpired = 3,  // Лицензия истекла
    Error = 4            // Ошибка
}

/// <summary>
/// Идентификатор программы
/// </summary>
public enum ProgramIdentifier
{
    Unknown = 0,
    Server = 1,          // Сервер БСУ
    Client = 2,          // Клиент (оператор)
    Stock = 3,           // Склад
    Configurator = 4,    // Конфигуратор
    Panel = 5,           // Панель
    Lab = 6,             // Лаборатория
    Reporter = 7         // Репортер
}