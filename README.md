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
        -GenerateRecursive(int placedCount) bool
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
        +Guid Id
        +string Name
        +int StudentsCount
        +List~GroupRequirement~ Requirements
        +List~IConstraint~ Constraints
        +GetUnfulfilledRequirements() List~GroupRequirement~
        +BookSlot(int cellIndex) void
        +ReleaseSlot(int cellIndex) void
        +ClearBookings() void
    }

    class Teacher {
        +Guid Id
        +string Name
        +int TargetLessons
        +int AssignedLessons
        +List~TeacherCompetence~ Competences
        +List~IConstraint~ Constraints
        +CanTakeMoreLessons() bool
        +CanTeach(Discipline disc) bool
        +BookSlot(int cellIndex) void
        +ReleaseSlot(int cellIndex) void
        +ClearBookings() void
    }

    class Discipline {
        +Guid Id
        +string Name
        +List~IConstraint~ Constraints
        +SetContext(Guid groupId) void
        +BookSlot(int cellIndex, Guid groupId) void
        +ReleaseSlot(int cellIndex, Guid groupId) void
        +ClearBookings() void
        +GetAvailabilityMask() bool[]
        +IsBooked(int cellIndex) bool
    }

    class Room {
        +Guid Id
        +string Name
        +int Capacity
        -int[] _reservedSeatsPerSlot
        -int _currentRequestStudents
        +SetContext(int studentsCount) void
        +CanAccommodate(int studentsCount, int cellIndex) bool
        +ReserveSeats(int studentsCount, int cellIndex) void
        +FreeSeats(int studentsCount, int cellIndex) void
        +ClearBookings() void
    }

    class GroupRequirement {
        +Guid DisciplineId
        +int RequiredLessons
        +int FulfilledLessons
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
        +DailyLimitConstraint(IParticipant owner, int maxLessonsPerDay, int cellsPerDay)
    }

    class TimeOffConstraint {
        -int[] _unavailableCells
        +TimeOffConstraint(int[] unavailableCells)
    }

    class MaxConsecutiveLessonsConstraint {
        -IParticipant _owner
        -int _maxConsecutive
        -int _cellsPerDay
        +MaxConsecutiveLessonsConstraint(IParticipant owner, int maxConsecutive, int cellsPerDay)
    }

    class SpecificDayConstraint {
        -int[] _allowedDays
        -int _cellsPerDay
        +SpecificDayConstraint(int[] allowedDays, int cellsPerDay)
    }

    class SpecificTimeConstraint {
        -int[] _allowedCellsInDay
        -int _cellsPerDay
        +SpecificTimeConstraint(int[] allowedCellsInDay, int cellsPerDay)
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

### UML Діаграма (Ієрархія UI Models (Presentation Layer))

```mermaid
%%{init: {'useMaxWidth': false}}%%
classDiagram
    direction LR

    %% --- ПАРАЛЕЛЬНА ІЄРАРХІЯ UI MODELS ---
    class IParticipantUI {
        <<interface>>
        +Guid Id
        +string Name
    }

    class ScheduleUI {
        +ScheduleConfig Config
        +ObservableCollection~TeacherUI~ Teachers
        +ObservableCollection~StudentGroupUI~ Groups
        +ObservableCollection~RoomUI~ Rooms
        +ObservableCollection~DisciplineUI~ Disciplines
        +Schedule LastResult
        +MapToModel() Schedule
        +Generate() Schedule
    }

    class TeacherUI {
        +Guid Id
        +string FirstName
        +string LastName
        +string Patronymic
        +string Name
        +int TargetLessons
        +ObservableCollection~TeacherCompetenceUI~ Competences
        +ObservableCollection~IConstraintUI~ Constraints
        +MapToModel(ScheduleConfig config) Teacher
    }
    class StudentGroupUI {
        +Guid Id
        +string Name
        +int StudentsCount
        +ObservableCollection~GroupRequirementUI~ Requirements
        +ObservableCollection~IConstraintUI~ Constraints
        +MapToModel(ScheduleConfig config) StudentGroup
    }
    class RoomUI {
        +Guid Id
        +string Name
        +int Capacity
        +MapToModel(ScheduleConfig config) Room
    }
    class DisciplineUI {
        +Guid Id
        +string Name
        +ObservableCollection~IConstraintUI~ Constraints
        +MapToModel(ScheduleConfig config) Discipline
    }

    IParticipantUI <|.. RoomUI
    IParticipantUI <|.. TeacherUI
    IParticipantUI <|.. StudentGroupUI
    IParticipantUI <|.. DisciplineUI
    class IConstraintUI {
        <<interface>>
        +string DisplayName
        +MapToModel(IParticipant owner, ScheduleConfig config) IConstraint
    }
    class DailyLimitConstraintUI {
        +int MaxLessonsPerDay
        +MapToModel(IParticipant owner, ScheduleConfig config) IConstraint
    }
    class TimeOffConstraintUI {
        +int[] UnavailableCells
        +MapToModel(IParticipant owner, ScheduleConfig config) IConstraint
    }
    class MaxConsecutiveLessonsConstraintUI {
        +int MaxConsecutive
        +MapToModel(IParticipant owner, ScheduleConfig config) IConstraint
    }

    class GroupRequirementUI {
        +Guid DisciplineId
        +int RequiredLessons
        +MapToModel() GroupRequirement
    }
    class TeacherCompetenceUI {
        +Guid DisciplineId
        +MapToModel() TeacherCompetence
    }
    ScheduleUI ..> Schedule : MapToModel() розгортає
    ScheduleUI *-- TeacherUI
    ScheduleUI *-- StudentGroupUI
    ScheduleUI *-- RoomUI
    ScheduleUI *-- DisciplineUI
    TeacherUI *-- IConstraintUI
    StudentGroupUI *-- IConstraintUI
    DisciplineUI *-- IConstraintUI
    StudentGroupUI *-- GroupRequirementUI
    TeacherUI *-- TeacherCompetenceUI
    GroupRequirementUI ..> GroupRequirement : MapToModel() створює
    TeacherCompetenceUI ..> TeacherCompetence : MapToModel() створює
    IConstraintUI <|.. DailyLimitConstraintUI
    IConstraintUI <|.. TimeOffConstraintUI
    IConstraintUI <|.. MaxConsecutiveLessonsConstraintUI

    TeacherUI ..> Teacher : MapToModel() створює
    StudentGroupUI ..> StudentGroup : MapToModel() створює
    RoomUI ..> Room : MapToModel() створює
    DisciplineUI ..> Discipline : MapToModel() створює
    IConstraintUI ..> IConstraint : MapToModel() створює

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
    
    CheckTrue -->|Так, Слот знайдено| PlaceLesson["Підтвердження Транзакції:<br/>1. Записати Урок у Слот<br/>2. Збільшити FulfilledLessons<br/>3. Заблокувати слот для учасників"]
    PlaceLesson --> UpdateBestState{"Чи розміщено <br/> більше уроків ніж раніше?"}
    UpdateBestState -->|Так| SaveBest["Зберегти поточний <br/> стан (_bestLessons)"]
    UpdateBestState -->|Ні| CheckReq
    SaveBest --> CheckReq
    
    CheckTrue -->|Ні, Колізія| BacktrackRoom["Відкат (Backtrack):<br/>Спробувати наступну Аудиторію"]
    BacktrackRoom --> CheckOtherRoom{"Є ще аудиторії?"}
    CheckOtherRoom -->|Так| PickRoom
    CheckOtherRoom -->|Ні| BacktrackTeacher["Відкат:<br/>Спробувати наступного Викладача"]
    BacktrackTeacher --> CheckOtherTeacher{"Є ще вчителі?"}
    CheckOtherTeacher -->|Так| PickTeacher
    CheckOtherTeacher -->|Ні| BacktrackDisc["Спробувати наступну Дисципліну"]
    BacktrackDisc --> PickDiscipline
    
    Start -.->|Якщо генерація<br/>завершилась невдачею| RestorePartial["Відновлення найкращого<br/>часткового розкладу<br/>з _bestLessons"]
```

### Часткова Генерація (Best Partial Schedule)
Замість того, щоб переривати роботу помилкою при неможливості виконати всі 100% обмежень, алгоритм використовує евристику збереження найкращого проміжного результату. 
Під час рекурсивного пошуку він відстежує максимальну кількість успішно розміщених уроків. Якщо ідеальний розклад не знайдено (наприклад, через конфлікт `Teacher Daily Limit` та `Group Specific Day`), алгоритм **відновлює найкращий збережений частковий стан** і відображає його користувачу в UI, де чітко вказує нерозподілені години.

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

Для застосунку на базі **.NET MAUI** використовується патерн `AppShell` з бічним меню (Flyout) для швидкого доступу до основних розділів, та стекова навігація (Push/Pop через Shell Routes) для сторінок редагування.

```mermaid
flowchart TD
    %% Головне меню
    Shell["AppShell / Бічне Меню (Flyout)"]

    %% Основні сторінки (Пункти FlyoutItem)
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
    
    %% Вбудований компонент вибору дисциплін
    TeacherEdit -. "Вбудований компонент" .-> SelectionModal["DisciplineSelection (Modal Popup)"]
    GroupEdit -. "Вбудований компонент" .-> SelectionModal
    
    %% Логіка генерації
    Dashboard -- "Генерація" --> ScheduleResult["ScheduleResultPage"]
```

### Деталі взаємодії (UI Flow)
1. **Головне меню (Flyout)**: Швидка навігація бічним меню між списками сутностей та налаштуваннями.
2. **DisciplineEditPage**: Дозволяє задати назву дисципліни та налаштувати обмеження (через вбудовану форму `ConstraintForm`).
3. **RoomEditPage**: Дозволяє задати назву (номер) аудиторії та її місткість (`Capacity`).
4. **TeacherEditPage / GroupEditPage**: 
   - Налаштовує параметри викладача/групи.
   - Для додавання компетенцій чи вимог містить вбудований компонент **DisciplineSelection** (`Frame`), який відкривається прямо на сторінці з рядком живого пошуку.
   - Містить форму **ConstraintForm** для додавання обмежень (з ручним упорядкуванням drag-and-drop).

## Архітектура ViewModels (MVVM)

У цій версії архітектури **ViewModels працюють виключно з UI моделями (Presentation Models)**, діючи як "губка", що накопичує налаштування користувача. 
Класи доменної моделі (`Teacher`, `Schedule` тощо) не існують у пам'яті постійно. Вони конструюються динамічно з цих UI моделей **лише в момент натискання кнопки Генерації**, отримуючи актуальні розміри сітки, після чого відпрацьовують алгоритм і знищуються.

### Уніфікація та усунення дублювання (Шаблони проектування)

Для усунення надлишкового бойлерплейту архітектуру ViewModels оптимізовано за допомогою узагальнених базових класів та компонентного підходу:

1. **Уніфікований шар спискових моделей (`BaseListViewModel<TEntity>`)**:
   - Реалізує шаблонний життєвий цикл CRUD для сутностей, що реалізують `IParticipantUI`:
     - Навігаційні команди створення (`AddCommand`) та редагування (`EditCommand`) з переходом через Shell (`GoToAsync`).
     - Команда видалення (`DeleteCommand`).
     - Повноцінна підтримка Drag-and-Drop із реордерингом елементів списку (`DragStartingCommand`, `DropCommand` та `Items.Move`).
   - Спискові моделі учасників (`TeachersViewModel`, `RoomsViewModel`, `DisciplinesViewModel`, `StudentGroupsViewModel`) винесені в окрему підпапку `ViewModels/Participants/` (простір імен `Lab1MAUI.ViewModels.Participants`) за аналогією до `Models/UI/Participants/`. Вони є лаконічними типізованими спадкоємцями `BaseListViewModel<TEntity>`, що конфігурують джерело даних і роут для DI та сторінок.

2. **Інкапсуляція управління обмеженнями (`ConstraintsEditorViewModel`)**:
   - Відокремлений компонент, який інкапсулює колекцію обмежень `Constraints`, форму вибору `ConstraintFormViewModel`, команди видалення та перетягування обмежень.
   - Забезпечує стандартизоване копіювання правил при завантаженні та збереженні через методи `LoadFrom(...)` та `ApplyTo(...)`.

3. **Уніфікований шар редагування (`BaseEditViewModel<TEntity>`)**:
   - `BaseEditViewModel<TEntity>`: автоматизує обробку Shell `[QueryProperty]` для завантаження за ID або створення нового екземпляра (`"new"`), керує станом `IsNew`, надає гачок асинхронної валідації `ValidateBeforeSaveAsync()`, синхронізацію в колекції `ScheduleUI` та зворотну навігацію `..`. 
   - Моделі редагування (`TeacherEditViewModel`, `GroupEditViewModel`, `DisciplineEditViewModel`, `RoomEditViewModel`) напряму наслідують `BaseEditViewModel<TEntity>`, утворюючи просту пласку ієрархію без проміжних класів, та використовують композицію з дочірніми компонентами `ConstraintsEditorViewModel` і `DisciplineSelectionViewModel`.
   - **Структура папок**: Усі класи редагування, базові моделі та форми винесені в окрему підпапку `ViewModels/Edit/` (простір імен `Lab1MAUI.ViewModels.Edit`), а відповідні XAML-сторінки редагування — у підпапку `Views/Edit/` (простір імен `Lab1MAUI.Views.Edit`). Спискові сторінки учасників згруповані у підпапку `Views/Participants/` (простір імен `Lab1MAUI.Views.Participants`) за аналогією до `ViewModels/Participants/` та `Models/UI/Participants/`, тоді як у коренях `ViewModels/` та `Views/` залишаються головні сторінкові компоненти.

4. **Чітке структурування учасників розкладу (`Participants`) у Domain та Presentation (UI) шарах**:
   - **`Models/Schedule/Participants/`**: підпапка містить виключно сутності, що реалізують доменний інтерфейс `IParticipant` (`Teacher`, `StudentGroup`, `Discipline`, `Room`, `IParticipant`). Допоміжні класи зв'язків та вимог (`GroupRequirement`, `TeacherCompetence`), які не наслідують `IParticipant`, винесені безпосередньо в корінь `Models/Schedule/`.
   - **`Models/UI/Participants/`**: дзеркальна структура в шарі представлення (UI). Містить інтерфейс `IParticipantUI` (що містить `Guid Id` та `string Name`) та всі сутності-учасники: `TeacherUI`, `StudentGroupUI`, `DisciplineUI`, `RoomUI`. Загальні або допоміжні класи (`GroupRequirementUI`, `TeacherCompetenceUI`, `ScheduleUI`) розташовані на рівні `Models/UI/`.


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

    class BaseListViewModel~TEntity~ {
        <<abstract>>
        +ObservableCollection~TEntity~ Items
        +string EditPageRoute
        +ICommand AddCommand
        +ICommand EditCommand
        +ICommand DeleteCommand
        +ICommand DragStartingCommand
        +ICommand DropCommand
    }

    class BaseEditViewModel~TEntity~ {
        <<abstract>>
        #ScheduleUI ScheduleUI
        #ObservableCollection~TEntity~ EntityCollection
        +TEntity Entity
        +bool IsNew
        +ICommand SaveCommand
        +LoadEntityById(string value) void
        #LoadData(TEntity entity)*
        #ApplyData(TEntity entity)*
        #ValidateBeforeSaveAsync()* Task~bool~
        #SaveAsync() Task
    }

    class ConstraintsEditorViewModel {
        +ObservableCollection~IConstraintUI~ Constraints
        +ConstraintFormViewModel ConstraintForm
        +ICommand DeleteConstraintCommand
        +ICommand DragStartingConstraintCommand
        +ICommand DropConstraintCommand
        +LoadFrom(IEnumerable~IConstraintUI~) void
        +ApplyTo(ICollection~IConstraintUI~) void
    }

    class DashboardViewModel {
        +int TotalTeachers
        +int TotalGroups
        +int TotalDisciplines
        +bool IsGenerating
        +ICommand GenerateScheduleCommand
    }
    
    class SettingsViewModel {
        +int Days
        +int CellsPerDay
        +int MinLessonsPerDay
        +int LessonDurationMinutes
        +int TotalCells
    }
    
    class DisciplinesViewModel
    
    class DisciplineEditViewModel {
        +string Name
        +string DisciplineId
    }

    class RoomsViewModel
    
    class RoomEditViewModel {
        +string Name
        +int Capacity
        +string RoomId
    }

    class TeachersViewModel
    
    class TeacherEditViewModel {
        +string FirstName
        +string LastName
        +string Patronymic
        +int TargetLessons
        +string TeacherId
        +ObservableCollection~TeacherCompetenceItem~ Competences
        +ICommand AddCompetenceCommand
        +ICommand DeleteCompetenceCommand
    }

    class StudentGroupsViewModel
    
    class GroupEditViewModel {
        +string Name
        +int StudentsCount
        +string GroupId
        +ObservableCollection~GroupRequirementItem~ Requirements
        +ICommand AddRequirementCommand
        +ICommand DeleteRequirementCommand
    }

    class DisciplineSelectionViewModel {
        +ObservableCollection~DisciplineUI~ FilteredDisciplines
        +string SearchQuery
        +bool IsVisible
        +ICommand SelectCommand
        +ICommand CancelCommand
        +Open() void
    }

    class ConstraintFormViewModel {
        +ObservableCollection~string~ ConstraintTypes
        +string SelectedConstraintType
        +int DailyLimitValue
        +int MaxConsecutiveValue
        +int TimeOffCellIndex
        +ICommand AddConstraintCommand
    }

    class ScheduleResultViewModel {
        +ObservableCollection~UnassignedDisplay~ UnassignedLessons
        +ObservableCollection~GroupScheduleDisplay~ GroupSchedules
        +bool HasUnassignedLessons
    }

    %% Наслідування від BaseViewModel
    INotifyPropertyChanged <|.. BaseViewModel
    BaseViewModel <|-- BaseListViewModel
    BaseViewModel <|-- BaseEditViewModel
    BaseViewModel <|-- ConstraintsEditorViewModel
    BaseViewModel <|-- DashboardViewModel
    BaseViewModel <|-- SettingsViewModel
    BaseViewModel <|-- ScheduleResultViewModel
    BaseViewModel <|-- ConstraintFormViewModel
    BaseViewModel <|-- DisciplineSelectionViewModel

    %% Наслідування спискових моделей
    BaseListViewModel <|-- DisciplinesViewModel
    BaseListViewModel <|-- RoomsViewModel
    BaseListViewModel <|-- TeachersViewModel
    BaseListViewModel <|-- StudentGroupsViewModel

    %% Наслідування форм редагування
    BaseEditViewModel <|-- RoomEditViewModel
    BaseEditViewModel <|-- DisciplineEditViewModel
    BaseEditViewModel <|-- TeacherEditViewModel
    BaseEditViewModel <|-- GroupEditViewModel
    
    %% Композиція (дочірні ViewModels)
    DisciplineEditViewModel *-- ConstraintsEditorViewModel
    TeacherEditViewModel *-- ConstraintsEditorViewModel
    GroupEditViewModel *-- ConstraintsEditorViewModel
    ConstraintsEditorViewModel *-- ConstraintFormViewModel
    TeacherEditViewModel *-- DisciplineSelectionViewModel
    GroupEditViewModel *-- DisciplineSelectionViewModel
```

## Покриття Тестами (Testing Suite)

Проєкт містить 51 автоматизований xUnit тест, що гарантує надійність алгоритму та архітектури.

1. **Базові тести алгоритму (`FunctionalAlgorithmTests`, `ScheduleGenerationTests`, `ScheduleTests`)**: Перевіряють здатність алгоритму заповнювати сітку без конфліктів, розпізнавати нестачу аудиторій, перевіряти місткість та обробляти базові прив'язки й розмірності.
2. **Ізольовані обмеження (`IndividualConstraintTests`, `ConstraintTests`)**: Тестують поведінку `DailyLimit`, `MaxConsecutive`, `SpecificDay`, `SpecificTime` та `TimeOff` незалежно для кожного типу учасника.
3. **Комбінаторні тести та граничні випадки (`CombinatorialScheduleTests`, `ComplexScheduleTests`, `EdgeCaseTests`)**: Набір тестів для перевірки складних багатовимірних перетинів та екстремальних умов:
   - Конфлікти між обмеженнями Групи та Викладача (напр. *Teacher TimeOff vs Group Specific Day*).
   - Динаміка "пляшкових горлечок" (bottlenecks) через нестачу кімнат чи годин викладача.
   - Екстремальні значення та граничні випадки (напр. ліміт 0 уроків на день, заборона ставити підряд в умовах короткого дня, неможливі вимоги).
   - Масштабні стрес-тести (100+ уроків, 10 груп, 10 викладачів).
4. **Тести інфраструктури UI Models (`ViewModelIntegrationTests`, `MultipleRegenerationTests`)**: Перевіряють, що багаторазова регенерація розкладу через UI-прошарок не залишає "брудного стану" (сміттєві лічильники годин, хибні маски) між натисканнями кнопки.
