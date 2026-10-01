# Lab1MAUI: Генератор Розкладу

Цей проєкт є частиною лабораторної роботи з розробки мобільного застосунку на базі **.NET MAUI** з використанням архітектурного патерну **MVVM**. Основний функціонал застосунку — автоматична генерація навчального розкладу на основі заданих вхідних параметрів та обмежень.

## Особливості архітектури шару Model

Шар Model відповідає виключно за бізнес-логіку предметної області.

- **Предметна область (Навчальний план)**: Введено поняття дисциплін (`Discipline`). Тип заняття (Лекція, Практика, Лабораторна) більше не є окремим полем (`LessonType` видалено). Замість цього створюються окремі дисципліни з відповідною приставкою (наприклад, "Математика [Лк]", "Математика [Пр]"). Групи мають вимоги щодо кількості годин на дисципліну (`GroupRequirement`), а викладачі — свої компетенції (`TeacherCompetence`). Дисципліни, як і викладачі та групи, можуть мати власні обмеження (`Constraints`), які враховуються розкладом.
- **Генерація замість видалення**: Уроки не передаються ззовні як готовий список. Замість цього метод `Schedule.InitializeData()` на основі вимог груп та компетенцій викладачів автоматично генерує (`спавнить`) об'єкти `Lesson`. Жодне обмеження не додає і не видаляє уроки — вони лише змінюють властивості (наприклад, призначають викладача чи змінюють час).
- **Композиція замість наслідування**: Викладачі та Групи володіють компонентом `TimeGrid`, який інкапсулює управління часовими комірками.
- **Гнучка система правил**: Усі вимоги щодо розкладу реалізовані через інтерфейс `IConstraint`. При виборі дисципліни користувач не вписує її ID вручну — використовується випадаючий список з пошуком, реалізований через окрему ViewModel.

---

## UML Діаграма (Шар Model)

Нижче наведена діаграма класів алгоритмічного ядра з урахуванням дисциплін та навчальних вимог:

```mermaid
%%{init: {'useMaxWidth': false}}%%
classDiagram
    direction LR
    
    %% Головний контейнер та оркестратор
    class Schedule {
        +ScheduleConfig Config
        +List~Teacher~ Teachers
        +List~StudentGroup~ Groups
        +List~Lesson~ Lessons
        +List~Discipline~ Disciplines
        +ConstraintRegistry Registry
        +Generate()
        +InitializeData()
        +CollectAllConstraints()
        +ApplyConstraintsMainLoop()
        +ValidateConsistency()
    }

    class ScheduleConfig {
        +int Days
        +int CellsPerDay
        +int TotalCells
    }

    %% Предметна область: Дисципліни та вимоги
    class Discipline {
        +Guid Id
        +string Name
        +List~IConstraint~ Constraints
        +GetPersonalConstraints(int totalCells) List~IConstraint~
    }

    class GroupRequirement {
        +Guid DisciplineId
        +int RequiredHours
    }

    class TeacherCompetence {
        +Guid DisciplineId
    }

    %% Компонент розкладу (Композиція)
    class TimeGrid {
        -Guid?[] _cells
        +int TargetHours
        +IReadOnlyList~Guid?~ Cells
        +Initialize(int totalCells)
        +IsFree(int cellIndex) bool
        +Place(Guid itemId, int cellIndex)
    }

    %% Учасники
    class Teacher {
        +Guid Id
        +string Name
        +TimeGrid Grid
        +List~TeacherCompetence~ Competences
        +GetPersonalConstraints(int totalCells) List~IConstraint~
    }

    class StudentGroup {
        +Guid Id
        +string Name
        +TimeGrid Grid
        +List~GroupRequirement~ Requirements
        +GetPersonalConstraints(int totalCells) List~IConstraint~
    }

    class Lesson {
        +Guid Id
        +Guid TeacherId
        +Guid StudentGroupId
        +Guid DisciplineId
    }

    %% Система обмежень
    class IConstraint {
        <<interface>>
        +int Priority
        +Guid OwnerId
        +Apply(Schedule schedule) void
    }

    class ConstraintRegistry {
        +Register(IConstraint constraint)
        +GetOrdered() List~IConstraint~
    }

    class LessonPlacementConstraint {
        +int Priority
        +Guid OwnerId
        -Lesson _lesson
        +Apply(Schedule schedule) void
    }

    class TeacherAssignmentConstraint {
        +int Priority
        +Guid OwnerId
        +Apply(Schedule schedule) void
    }

    class DailyLimitConstraint {
        +int Priority
        +Guid OwnerId
        +Apply(Schedule schedule) void
    }

    class MaxConsecutiveLessonsConstraint {
        +int Priority
        +Guid OwnerId
        +Apply(Schedule schedule) void
    }

    %% Взаємозв'язки
    Schedule *-- ScheduleConfig : має
    Schedule *-- Teacher : має
    Schedule *-- StudentGroup : має
    Schedule *-- Lesson : має
    Schedule *-- Discipline : має
    Schedule *-- ConstraintRegistry : керує

    Teacher *-- TimeGrid : володіє
    Teacher *-- TeacherCompetence : володіє
    StudentGroup *-- TimeGrid : володіє
    StudentGroup *-- GroupRequirement : володіє

    TeacherCompetence --> Discipline : посилається
    GroupRequirement --> Discipline : посилається
    
    Lesson --> Discipline : посилається
    
    ConstraintRegistry o-- IConstraint : впорядковує
    LessonPlacementConstraint ..|> IConstraint
    TeacherAssignmentConstraint ..|> IConstraint
    DailyLimitConstraint ..|> IConstraint
    MaxConsecutiveLessonsConstraint ..|> IConstraint
```

## Сценарії використання (Use Cases)

Застосунок орієнтований на Адміністратора розкладу, який налаштовує вхідні дані та генерує розклад.

```mermaid
flowchart LR
    Admin((Адміністратор))

    subgraph Налаштування Бази
        UC1([UC-1: Налаштування параметрів сітки])
        UC2([UC-2: Управління дисциплінами])
    end

    subgraph Управління Учасниками
        UC3([UC-3: Налаштування Викладачів та Компетенцій])
        UC4([UC-4: Налаштування Груп та Вимог])
        UC5([UC-5: Встановлення Обмежень])
    end

    subgraph Генерація та Аналіз
        UC6([UC-6: Запуск генерації розкладу])
        UC7([UC-7: Перегляд розкладу])
        UC8([UC-8: Аналіз нерозподілених уроків])
    end

    Admin --> UC1
    Admin --> UC2
    Admin --> UC3
    Admin --> UC4
    Admin --> UC5
    Admin --> UC6
    Admin --> UC7
    Admin --> UC8
```

## Архітектура Інтерфейсу (UI Flow)

Для мобільного/десктопного застосунку на базі **.NET MAUI** ми використовуємо паттерн `AppShell` з бічним меню (Flyout) або вкладками (TabBar) для швидкого доступу до основних розділів, та стекову навігацію (Push/Pop) для сторінок редагування.

```mermaid
flowchart TD
    %% Головне меню
    Shell["AppShell / Головне Меню"]

    %% Основні сторінки (Вкладки/Пункти меню)
    Shell --> Dashboard["DashboardPage"]
    Shell --> Settings["SettingsPage"]
    Shell --> Disciplines["DisciplinesPage"]
    Shell --> Teachers["TeachersPage"]
    Shell --> Groups["StudentGroupsPage"]
    
    %% Сторінки редагування (Push Navigation)
    Disciplines -- "Редагувати" --> DisciplineEdit["DisciplineEditPage"]
    Teachers -- "Редагувати" --> TeacherEdit["TeacherEditPage"]
    Groups -- "Редагувати" --> GroupEdit["GroupEditPage"]
    
    %% Модальне вікно вибору дисциплін
    TeacherEdit -. "Відкриває" .-> SelectionModal["DisciplineSelectionPage (Модальне вікно)"]
    GroupEdit -. "Відкриває" .-> SelectionModal
    
    %% Логіка генерації
    Dashboard -- "Генерація" --> ScheduleResult["ScheduleResultPage"]
```

### Деталі взаємодії (UI Flow)
1. **Головне меню (Flyout/Tabs)**: Швидка навігація між списками сутностей та налаштуваннями.
2. **DisciplineEditPage**: Дозволяє задати назву дисципліни (включаючи приставки `[Лк/Пр/Лб]`) та налаштувати специфічні для неї обмеження (через міні-меню на цій же сторінці).
3. **TeacherEditPage / GroupEditPage**: 
   - Налаштовує параметри викладача/групи.
   - Для прив'язки компетенцій чи вимог користувач не вводить ID вручну. Замість цього відкривається **DisciplineSelectionPage**.
   - **DisciplineSelectionPage**: Окреме модальне вікно або випадаючий компонент із рядком пошуку. Користувач вводить назву, бачить відфільтрований список і клікає на потрібну дисципліну, після чого вона автоматично прив'язується.
   - Має міні-меню (ConstraintForm) для додавання обмежень (з ручним сортуванням за пріоритетом).

## Архітектура ViewModels (MVVM)

Тут наведено структуру класів ViewModel, яка відповідає всім вимогам лабораторної роботи (використання `INotifyPropertyChanged`, `ObservableCollection`, `ICommand` та `Data Binding`).

```mermaid
classDiagram
    direction LR
    
    class INotifyPropertyChanged {
        <<interface>>
        +event PropertyChanged
    }

    class BaseViewModel {
        <<abstract>>
        +event PropertyChanged
        #OnPropertyChanged(string propertyName)
        #SetProperty~T~(ref T backingStore, T value, string propertyName) bool
    }

    class DashboardViewModel {
        +int TotalTeachers
        +int TotalGroups
        +int TotalDisciplines
        +ICommand GenerateScheduleCommand
    }
    
    class SettingsViewModel {
        +int Days
        +int CellsPerDay
        +int TotalCells
    }
    
    class DisciplinesViewModel {
        +Collection~Discipline~ Disciplines
        +ICommand AddDisciplineCommand
        +ICommand DeleteDisciplineCommand
    }
    
    class DisciplineEditViewModel {
        +string Name
        +Collection~IConstraint~ Constraints
        +ICommand SaveCommand
        +ICommand DeleteConstraintCommand
    }

    class TeachersViewModel {
        +Collection~Teacher~ Teachers
        +ICommand AddTeacherCommand
        +ICommand EditTeacherCommand
        +ICommand DeleteTeacherCommand
        +ICommand MoveUpTeacherCommand
        +ICommand MoveDownTeacherCommand
    }
    
    class TeacherEditViewModel {
        +string FirstName
        +string LastName
        +string Patronymic
        +int TargetHours
        +Collection~TeacherCompetence~ Competences
        +Collection~IConstraint~ Constraints
        +ICommand SaveCommand
        +ICommand AddCompetenceCommand
        +ICommand DeleteCompetenceCommand
    }

    class StudentGroupsViewModel {
        +Collection~StudentGroup~ Groups
        +ICommand AddGroupCommand
        +ICommand EditGroupCommand
        +ICommand DeleteGroupCommand
        +ICommand MoveUpGroupCommand
        +ICommand MoveDownGroupCommand
    }
    
    class GroupEditViewModel {
        +string GroupName
        +int TargetHours
        +Collection~GroupRequirement~ Requirements
        +Collection~IConstraint~ Constraints
        +ICommand SaveCommand
        +ICommand AddRequirementCommand
        +ICommand DeleteRequirementCommand
    }

    class ConstraintFormViewModel {
        +Collection~string~ ConstraintTypes
        +string SelectedConstraintType
        +int DailyLimitValue
        +int MaxConsecutiveValue
        +int TimeOffCellIndex
        +string AssignmentTeacherId
        +ICommand AddConstraintCommand
    }

    class DisciplineSelectionViewModel {
        +Collection~Discipline~ FilteredDisciplines
        +string SearchQuery
        +Discipline SelectedDiscipline
        +ICommand SelectCommand
    }

    class ScheduleResultViewModel {
        +Collection~Lesson~ UnassignedLessons
        +Collection~TimeSlot~ GridCells
        +ICommand ExportCommand
    }

    %% Наслідування від BaseViewModel
    INotifyPropertyChanged <|.. BaseViewModel
    BaseViewModel <|-- DashboardViewModel
    BaseViewModel <|-- SettingsViewModel
    BaseViewModel <|-- DisciplinesViewModel
    BaseViewModel <|-- DisciplineEditViewModel
    BaseViewModel <|-- TeachersViewModel
    BaseViewModel <|-- TeacherEditViewModel
    BaseViewModel <|-- StudentGroupsViewModel
    BaseViewModel <|-- GroupEditViewModel
    BaseViewModel <|-- ScheduleResultViewModel
    BaseViewModel <|-- ConstraintFormViewModel
    BaseViewModel <|-- DisciplineSelectionViewModel

    %% Композиція та використання компонентів (дочірні ViewModels)
    DisciplineEditViewModel *-- ConstraintFormViewModel : використовує
    
    TeacherEditViewModel *-- ConstraintFormViewModel : використовує
    TeacherEditViewModel *-- DisciplineSelectionViewModel : використовує
    
    GroupEditViewModel *-- ConstraintFormViewModel : використовує
    GroupEditViewModel *-- DisciplineSelectionViewModel : використовує
```

---

## Концепція Нової Транзакційної Архітектури (V2)

Оновлена архітектура орієнтована на **транзакційність** та **маніпуляцію булевими масками** для швидкого і надійного пошуку вільних слотів (комірок). Вона вирішує проблему «важковаговитості» поточної ітеративної моделі з пріоритетами фаз.

### Ключові принципи:
1. **Єдиний інтерфейс учасника (`IParticipant`)**: Всі сутності (Група, Викладач, Дисципліна) реалізують цей інтерфейс. Вони мають спільну булеву маску вільного часу (`AvailabilityMask`), де `true` — слот вільний, `false` — слот заблокований або зайнятий.
2. **Цільові години Викладача (`TargetHours`)**: Кожен викладач має визначену цільову кількість годин (тижневе навантаження), яка обов'язково враховується при генерації розкладу.
3. **Єдина централізована Сітка Часу (`TimeGrid`) у `Schedule`**: Сітка часу більше не розпорошена всередині кожного викладача чи групи. Тепер саме `Schedule` володіє `TimeGrid`, що містить розміщені уроки (`Lesson`), тоді як учасники оперують лише легкими булевими масками доступності.
4. **Незалежна Попередня Фільтрація (Pre-calculation)**: Застосування обмежень відбувається *незалежно перед генерацією розкладу*. Оскільки кожне обмеження працює незалежно від інших учасників, воно має доступ тільки до інформації свого власника і модифікує виключно його персональну маску.
5. **Транзакційність через Злиття (Bitwise AND)**: Під час відбору до уроку ми просто робимо побітове `AND` масок учасників (`Group.Mask & Discipline.Mask & Teacher.Mask`), щоб дізнатися, чи є вільний перетин. Якщо результат має хоча б одне `true`, ми "коммітимо" транзакцію: створюємо урок, записуємо його у `Schedule.Grid` на знайдений слот і позначаємо цей слот як `false` в масках учасників. Якщо `true` немає — виконується відкат (Backtrack).

### UML Діаграма Класів (V2)

```mermaid
%%{init: {'useMaxWidth': false}}%%
classDiagram
    direction LR

    %% --- СУТНОСТІ ДАНИХ ТА КОНФІГУРАЦІЯ ---
    class ScheduleConfig {
        +int Days
        +int CellsPerDay
        +int MinLessonsPerDay
        +int LessonDurationMinutes
        +int TotalCells
    }

    class Schedule {
        +ScheduleConfig Config
        -[getter] _grid: IReadOnlyList~IReadOnlyList~Lesson~~
        -[getter] _teachers: List~Teacher~
        -[getter] _groups: List~StudentGroup~
        -[getter] _disciplines: List~Discipline~
        -[getter] _rooms: List~Room~
        
        +Generate() void
        -InitializeMasks() void
        -FilterTeachers(Discipline disc, StudentGroup group) IEnumerable~Teacher~
        -FilterRooms(Discipline disc, StudentGroup group) IEnumerable~Room~
        -FindCommonFreeSlot(IParticipant g, IParticipant t, IParticipant r, IParticipant d) int
        -CommitTransaction(StudentGroup g, Teacher t, Room r, Discipline d, int slot) Lesson
        -RollbackTransaction(Lesson lesson) void
    }

    class Lesson {
        +Guid Id
        +Guid DisciplineId
        +Guid TeacherId
        +Guid StudentGroupId
        +Guid RoomId
        +int CellIndex
    }

    %% --- УЧАСНИКИ ТА ЇХ СТАН ---
    class IParticipant {
        <<interface>>
        +Guid Id
        +string Name
        +GetAvailabilityMask() bool[]
        +IsBooked(int cellIndex) bool
    }

    class StudentGroup {
        +[get, set] StudentsCount: int
        -[getter] _requirements: List~GroupRequirement~
        -[getter] _constraints: List~IConstraint~
        +GetUnfulfilledRequirements() List~GroupRequirement~
        +BookSlot(int cellIndex) void
        +ReleaseSlot(int cellIndex) void
    }

    class Teacher {
        +int TargetHours
        -[getter] _assignedHours: int
        -[getter] _competences: List~TeacherCompetence~
        -[getter] _constraints: List~IConstraint~
        +CanTakeMoreHours()
        +CanTeach(Discipline disc) bool
        +BookSlot(int cellIndex) void
        +ReleaseSlot(int cellIndex) void
    }

    class Discipline {
        -[getter] _constraints: List~IConstraint~
    }

    class Room {
        +int Capacity
        -[getter] _reservedSeatsPerSlot: int[]
        +CanAccommodate(int studentsCount, int cellIndex) bool
        +ReserveSeats(int studentsCount, int cellIndex) void
        +FreeSeats(int studentsCount, int cellIndex) void
    }

    class GroupRequirement {
        +Guid DisciplineId
        +int RequiredHours
        +int FulfilledHours
        +bool IsFulfilled()
    }

    class TeacherCompetence {
        +Guid DisciplineId
    }

    %% --- ОБМЕЖЕННЯ ---
    class IConstraint {
        <<interface>>
        +Apply(bool[] mask) void
    }

    class DailyLimitConstraint {
        -IParticipant _owner
        -int _maxLessonsPerDay
        -int _cellsPerDay
    }

    class TimeOffConstraint {
        -int[] _unavailableCells
    }

    class MaxConsecutiveLessonsConstraint {
        -IParticipant _owner
        -int _maxConsecutive
        -int _cellsPerDay
    }

    class SpecificDayConstraint {
        -int[] _allowedDays
        -int _cellsPerDay
    }

    class SpecificTimeConstraint {
        -int[] _allowedCellsInDay
        -int _cellsPerDay
    }

    %% --- ВІДНОШЕННЯ ТА ВИКОРИСТАННЯ (Залежності) ---
    IParticipant <|.. StudentGroup
    IParticipant <|.. Teacher
    IParticipant <|.. Discipline
    IParticipant <|.. Room

    StudentGroup *-- GroupRequirement
    Teacher *-- TeacherCompetence
    
    Schedule *-- ScheduleConfig
    Schedule *-- Lesson : зберігає масив (Grid)
    Schedule o-- Teacher
    Schedule o-- StudentGroup
    Schedule o-- Discipline
    Schedule o-- Room

    %% Як використовуються інтерфейси обмежень
    StudentGroup o-- IConstraint : містить
    Teacher o-- IConstraint : містить
    Discipline o-- IConstraint : містить
    
    IParticipant ..> IConstraint : Інкапсулює виклик Apply() всередині GetAvailabilityMask()

    IConstraint <|.. DailyLimitConstraint
    IConstraint <|.. TimeOffConstraint
    IConstraint <|.. MaxConsecutiveLessonsConstraint
    IConstraint <|.. SpecificDayConstraint
    IConstraint <|.. SpecificTimeConstraint
```

### Алгоритм Генерації (Flowchart V2)

```mermaid
flowchart TD
    Start["Початок Генерації"] --> InitMasks["Незалежна ініціалізація:<br/>Кожен IParticipant застосовує свої IConstraint<br/>для генерації базової AvailabilityMask"]
    
    InitMasks --> ForGroup["Для кожної Групи"]
    ForGroup --> CheckReq["Перевірка вимог <br/>(Чи є предмети з годинами > 0?)"]
    
    CheckReq -->|Немає вимог| NextGroup["Наступна Група"]
    NextGroup -.-> ForGroup
    
    CheckReq -->|Є вимоги| PickDiscipline["Вибір Дисципліни"]
    PickDiscipline --> FilterTeachers["Фільтрація Викладачів <br/> за компетенціями"]
    
    FilterTeachers --> PickTeacher["Вибір Викладача"]
    
    PickTeacher --> FilterRooms["Фільтрація Аудиторій <br/> за місткістю та вимогами"]
    FilterRooms --> PickRoom["Вибір Аудиторії"]
    
    PickRoom --> CombineMasks["Злиття Масок (Bitwise AND)<br/>CombinedMask = Group.Mask & Disc.Mask & Teacher.Mask & Room.Mask"]
    
    CombineMasks --> CheckTrue{"CombinedMask <br/> містить хоч один TRUE?"}
    
    CheckTrue -->|Так, Слот знайдено| PlaceLesson["Підтвердження Транзакції:<br/>1. Записати Урок у перший знайдений Слот<br/>2. Зменшити RequiredHours<br/>3. mask[i] = false для всіх учасників"]
    PlaceLesson --> CheckReq
    
    CheckTrue -->|Ні, Колізія| BacktrackRoom["Відкат (Backtrack):<br/>Спробувати наступну Аудиторію"]
    BacktrackRoom --> CheckOtherRoom{"Є ще аудиторії?"}
    CheckOtherRoom -->|Так| PickRoom
    CheckOtherRoom -->|Ні| BacktrackTeacher["Відкат:<br/>Спробувати наступного Викладача"]
    BacktrackTeacher --> CheckOtherTeacher{"Є ще вчителі?"}
    CheckOtherTeacher -->|Так| PickTeacher
    CheckOtherTeacher -->|Ні| BacktrackDisc["Спробувати наступну Дисципліну"]
    BacktrackDisc --> PickDiscipline
```

