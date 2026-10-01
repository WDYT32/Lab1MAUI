# Lab1MAUI: Генератор Розкладу

Цей проєкт є частиною лабораторної роботи з розробки мобільного застосунку на базі **.NET MAUI** з використанням архітектурного патерну **MVVM**. Основний функціонал застосунку — автоматична генерація навчального розкладу на основі заданих вхідних параметрів та обмежень.

## Особливості архітектури шару Model (Транзакційна Модель)

Шар Model відповідає виключно за бізнес-логіку предметної області. Оновлена архітектура орієнтована на **транзакційність** та **маніпуляцію булевими масками** для швидкого і надійного пошуку вільних слотів (комірок).

### Ключові принципи:
1. **Єдиний інтерфейс учасника (`IParticipant`)**: Всі сутності (Група, Викладач, Дисципліна, Аудиторія) реалізують цей інтерфейс. Вони мають спільну булеву маску вільного часу (`AvailabilityMask`), де `true` — слот вільний, `false` — слот заблокований або зайнятий.
2. **Єдина централізована Сітка Часу (`TimeGrid`) у `Schedule`**: Сітка часу більше не розпорошена всередині кожного викладача чи групи. Тепер саме `Schedule` володіє `Grid`, що містить розміщені уроки (`Lesson`), тоді як учасники оперують лише легкими булевими масками доступності.
3. **Незалежна Попередня Фільтрація (Pre-calculation)**: Застосування обмежень відбувається *незалежно перед генерацією розкладу*. Кожне обмеження має доступ тільки до інформації свого власника і модифікує виключно його персональну маску.
4. **Транзакційність через Злиття (Bitwise AND)**: Під час відбору до уроку ми робимо побітове `AND` масок учасників (`Group.Mask & Discipline.Mask & Teacher.Mask & Room.Mask`), щоб дізнатися, чи є вільний перетин. Якщо є хоча б одне `true`, ми "коммітимо" транзакцію: створюємо урок, записуємо його у `Schedule.Grid` і позначаємо цей слот як `false` в масках учасників. Якщо `true` немає — виконується відкат (Backtrack).

## UML Діаграма (Шар Model)

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
        +IReadOnlyList~IReadOnlyList~Lesson~~ Grid
        +List~Teacher~ Teachers
        +List~StudentGroup~ Groups
        +List~Discipline~ Disciplines
        +List~Room~ Rooms
        
        +Generate() void
        -GenerateRecursive() bool
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
        +GetAvailabilityMask() bool[]
        +IsBooked(int cellIndex) bool
    }

    class StudentGroup {
        +StudentGroupMTM Info
        +List~IConstraint~ Constraints
        +GetUnfulfilledRequirements() List~GroupRequirement~
        +BookSlot(int cellIndex) void
        +ReleaseSlot(int cellIndex) void
    }

    class Teacher {
        +TeacherMTM Info
        +int AssignedHours
        +List~IConstraint~ Constraints
        +CanTakeMoreHours() bool
        +CanTeach(Discipline disc) bool
        +BookSlot(int cellIndex) void
        +ReleaseSlot(int cellIndex) void
    }

    class Discipline {
        +DisciplineMTM Info
        +List~IConstraint~ Constraints
    }

    class Room {
        +RoomMTM Info
        -int[] _reservedSeatsPerSlot
        -int _currentRequestStudents
        +SetContext(int studentsCount) void
        +CanAccommodate(int studentsCount, int cellIndex) bool
        +ReserveSeats(int studentsCount, int cellIndex) void
        +FreeSeats(int studentsCount, int cellIndex) void
    }

    class GroupRequirement {
        +GroupRequirementMTM Info
        +int FulfilledHours
        +bool IsFulfilled()
    }

    class TeacherCompetence {
        +TeacherCompetenceMTM Info
    }

    %% --- ОБМЕЖЕННЯ ---
    class IConstraint {
        <<interface>>
        +Apply(bool[] mask) void
    }

    class DailyLimitConstraint {
        -IParticipant _owner
        +DailyLimitConstraintMTM Info
        -int _maxLessonsPerDay
        +DailyLimitConstraint(IParticipant owner, int maxLessonsPerDay)
    }

    class TimeOffConstraint {
        +TimeOffConstraintMTM Info
        -int[] _unavailableCells
        +TimeOffConstraint(int[] unavailableCells)
    }

    class MaxConsecutiveLessonsConstraint {
        -IParticipant _owner
        +MaxConsecutiveLessonsConstraintMTM Info
        -int _maxConsecutive
        +MaxConsecutiveLessonsConstraint(IParticipant owner, int maxConsecutive)
    }

    class SpecificDayConstraint {
        +SpecificDayConstraintMTM Info
        -int[] _allowedDays
        +SpecificDayConstraint(int[] allowedDays)
    }

    class SpecificTimeConstraint {
        +SpecificTimeConstraintMTM Info
        -int[] _allowedCellsInDay
        +SpecificTimeConstraint(int[] allowedCellsInDay)
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

    DailyLimitConstraint o-- IParticipant : використовує як _owner
    MaxConsecutiveLessonsConstraint o-- IParticipant : використовує як _owner

```

### UML Діаграма (Ієрархія MTM / DTO)

```mermaid
%%{init: {'useMaxWidth': false}}%%
classDiagram
    direction LR

    %% --- ПАРАЛЕЛЬНА ІЄРАРХІЯ MTM ---
    class ScheduleMTM {
        +ScheduleConfig Config
        +ObservableCollection~TeacherMTM~ Teachers
        +ObservableCollection~StudentGroupMTM~ Groups
        +ObservableCollection~RoomMTM~ Rooms
        +ObservableCollection~DisciplineMTM~ Disciplines
        +MapToModel() Schedule
    }

    class TeacherMTM {
        +Guid Id
        +string Name
        +int TargetHours
        +List~TeacherCompetenceMTM~ Competences
        +ObservableCollection~ConstraintMTM~ Constraints
        +MapToModel(ScheduleConfig config) Teacher
    }
    class StudentGroupMTM {
        +Guid Id
        +string Name
        +int StudentsCount
        +List~GroupRequirementMTM~ Requirements
        +ObservableCollection~ConstraintMTM~ Constraints
        +MapToModel(ScheduleConfig config) StudentGroup
    }
    class RoomMTM {
        +Guid Id
        +string Name
        +int Capacity
        +MapToModel(ScheduleConfig config) Room
    }
    class DisciplineMTM {
        +Guid Id
        +string Name
        +ObservableCollection~ConstraintMTM~ Constraints
        +MapToModel(ScheduleConfig config) Discipline
    }
    class ConstraintMTM {
        <<abstract>>
        +int CellsPerDay
        +MapToModel(object owner, ScheduleConfig config) IConstraint
    }
    class DailyLimitConstraintMTM {
        +int MaxLessonsPerDay
        +MapToModel(object owner, ScheduleConfig config) IConstraint
    }
    class TimeOffConstraintMTM {
        +int[] UnavailableCells
        +MapToModel(object owner, ScheduleConfig config) IConstraint
    }
    class MaxConsecutiveLessonsConstraintMTM {
        +int MaxConsecutive
        +MapToModel(object owner, ScheduleConfig config) IConstraint
    }
    class SpecificDayConstraintMTM {
        +int[] AllowedDays
        +MapToModel(object owner, ScheduleConfig config) IConstraint
    }
    class SpecificTimeConstraintMTM {
        +int[] AllowedCellsInDay
        +MapToModel(object owner, ScheduleConfig config) IConstraint
    }

    class GroupRequirementMTM {
        +Guid DisciplineId
        +int RequiredHours
        +MapToModel() GroupRequirement
    }
    class TeacherCompetenceMTM {
        +Guid DisciplineId
        +MapToModel() TeacherCompetence
    }
    ScheduleMTM ..> Schedule : MapToModel() розгортає
    ScheduleMTM *-- TeacherMTM
    ScheduleMTM *-- StudentGroupMTM
    ScheduleMTM *-- RoomMTM
    ScheduleMTM *-- DisciplineMTM
    TeacherMTM *-- ConstraintMTM
    StudentGroupMTM *-- ConstraintMTM
    DisciplineMTM *-- ConstraintMTM
    StudentGroupMTM *-- GroupRequirementMTM
    TeacherMTM *-- TeacherCompetenceMTM
    GroupRequirementMTM ..> GroupRequirement : MapToModel() створює
    TeacherCompetenceMTM ..> TeacherCompetence : MapToModel() створює
    ConstraintMTM <|-- DailyLimitConstraintMTM
    ConstraintMTM <|-- TimeOffConstraintMTM
    ConstraintMTM <|-- MaxConsecutiveLessonsConstraintMTM
    ConstraintMTM <|-- SpecificDayConstraintMTM
    ConstraintMTM <|-- SpecificTimeConstraintMTM

    TeacherMTM ..> Teacher : MapToModel() створює
    StudentGroupMTM ..> StudentGroup : MapToModel() створює
    RoomMTM ..> Room : MapToModel() створює
    DisciplineMTM ..> Discipline : MapToModel() створює
    ConstraintMTM ..> IConstraint : MapToModel() створює

```

### Алгоритм Генерації (Backtracking)

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

## Сценарії використання (Use Cases)

Застосунок орієнтований на Адміністратора розкладу, який налаштовує вхідні дані та генерує розклад.

```mermaid
flowchart LR
    Admin((Адміністратор))

    subgraph Налаштування Бази
        UC1([UC-1: Налаштування параметрів сітки])
        UC2([UC-2: Управління дисциплінами])
        UC9([UC-9: Управління аудиторіями])
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
    Admin --> UC9
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
    Shell --> Rooms["RoomsPage"]
    Shell --> Teachers["TeachersPage"]
    Shell --> Groups["StudentGroupsPage"]
    
    %% Сторінки редагування (Push Navigation)
    Disciplines -- "Редагувати" --> DisciplineEdit["DisciplineEditPage"]
    Rooms -- "Редагувати" --> RoomEdit["RoomEditPage"]
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
2. **DisciplineEditPage**: Дозволяє задати назву дисципліни та налаштувати специфічні для неї обмеження.
3. **RoomEditPage**: Дозволяє задати назву (номер) аудиторії та її місткість (`Capacity`).
4. **TeacherEditPage / GroupEditPage**: 
   - Налаштовує параметри викладача/групи.
   - Для прив'язки компетенцій чи вимог користувач відкриває **DisciplineSelectionPage**.
   - **DisciplineSelectionPage**: Окреме модальне вікно або випадаючий компонент із рядком пошуку.
   - Має міні-меню (ConstraintForm) для додавання обмежень (з ручним сортуванням за пріоритетом).

## Архітектура ViewModels (MVVM)

У цій версії архітектури **ViewModels працюють виключно з MTM (MapToModel)**, діючи як "губка", що накопичує налаштування користувача. 
Класи доменної моделі (`Teacher`, `Schedule` тощо) не існують у пам'яті постійно. Вони конструюються динамічно з цих MTM **лише в момент натискання кнопки Генерації**, отримуючи актуальні розміри сітки, після чого відпрацьовують алгоритм і знищуються.

```mermaid
%%{init: {'useMaxWidth': false}}%%
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
        +int MinLessonsPerDay
        +int LessonDurationMinutes
        +int TotalCells
    }
    
    class DisciplinesViewModel {
        +Collection~DisciplineMTM~ Disciplines
        +ICommand AddDisciplineCommand
        +ICommand EditDisciplineCommand
        +ICommand DeleteDisciplineCommand
        +ICommand DragStartingCommand
        +ICommand DropCommand
    }
    
    class DisciplineEditViewModel {
        +string Name
        +Collection~ConstraintMTM~ Constraints
        +ICommand SaveCommand
        +ICommand DeleteConstraintCommand
    }

    class RoomsViewModel {
        +Collection~RoomMTM~ Rooms
        +ICommand AddRoomCommand
        +ICommand EditRoomCommand
        +ICommand DeleteRoomCommand
    }
    
    class RoomEditViewModel {
        +string Name
        +int Capacity
        +ICommand SaveCommand
    }

    class TeachersViewModel {
        +Collection~TeacherMTM~ Teachers
        +ICommand AddTeacherCommand
        +ICommand EditTeacherCommand
        +ICommand DeleteTeacherCommand
    }
    
    class TeacherEditViewModel {
        +string FirstName
        +string LastName
        +int TargetHours
        +Collection~TeacherCompetenceMTM~ Competences
        +Collection~ConstraintMTM~ Constraints
        +ICommand SaveCommand
        +ICommand AddCompetenceCommand
        +ICommand DeleteCompetenceCommand
        +ICommand DeleteConstraintCommand
    }

    class StudentGroupsViewModel {
        +Collection~StudentGroupMTM~ Groups
        +ICommand AddGroupCommand
        +ICommand EditGroupCommand
        +ICommand DeleteGroupCommand
    }
    
    class GroupEditViewModel {
        +string GroupName
        +int StudentsCount
        +Collection~GroupRequirementMTM~ Requirements
        +Collection~ConstraintMTM~ Constraints
        +ICommand SaveCommand
        +ICommand AddRequirementCommand
        +ICommand DeleteRequirementCommand
        +ICommand DeleteConstraintCommand
    }

    class ConstraintFormViewModel {
        +Collection~string~ ConstraintTypes
        +string SelectedConstraintType
        +int DailyLimitValue
        +int MaxConsecutiveValue
        +int TimeOffCellIndex
        +ICommand AddConstraintCommand
    }

    class ScheduleResultViewModel {
        +Collection~UnassignedDisplay~ UnassignedLessons
        +Collection~GroupScheduleDisplay~ GroupSchedules
    }

    %% Наслідування від BaseViewModel
    INotifyPropertyChanged <|.. BaseViewModel
    BaseViewModel <|-- DashboardViewModel
    BaseViewModel <|-- SettingsViewModel
    BaseViewModel <|-- DisciplinesViewModel
    BaseViewModel <|-- DisciplineEditViewModel
    BaseViewModel <|-- RoomsViewModel
    BaseViewModel <|-- RoomEditViewModel
    BaseViewModel <|-- TeachersViewModel
    BaseViewModel <|-- TeacherEditViewModel
    BaseViewModel <|-- StudentGroupsViewModel
    BaseViewModel <|-- GroupEditViewModel
    BaseViewModel <|-- ScheduleResultViewModel
    BaseViewModel <|-- ConstraintFormViewModel
    
    %% Композиція (дочірні ViewModels)
    DisciplineEditViewModel *-- ConstraintFormViewModel
    TeacherEditViewModel *-- ConstraintFormViewModel
    GroupEditViewModel *-- ConstraintFormViewModel
```

