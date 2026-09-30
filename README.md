# Lab1MAUI: Генератор Розкладу

Цей проєкт є частиною лабораторної роботи з розробки мобільного застосунку на базі **.NET MAUI** з використанням архітектурного патерну **MVVM**. Основний функціонал застосунку — автоматична генерація навчального розкладу на основі заданих вхідних параметрів та обмежень.

## Особливості архітектури шару Model

Шар Model відповідає виключно за бізнес-логіку предметної області.

- **Предметна область (Навчальний план)**: Введено поняття дисциплін (`Discipline`) та типів занять (`LessonType`: Лекція, Практика, Лабораторна). Групи мають вимоги щодо кількості годин (`GroupRequirement`), а викладачі — свої компетенції (`TeacherCompetence`) та цільове навантаження.
- **Генерація замість видалення**: Уроки не передаються ззовні як готовий список. Замість цього метод `Schedule.InitializeData()` на основі вимог груп та компетенцій викладачів автоматично генерує (`спавнить`) об'єкти `Lesson`. Жодне обмеження не додає і не видаляє уроки — вони лише змінюють властивості (наприклад, призначають викладача чи змінюють час).
- **Композиція замість наслідування**: Викладачі та Групи володіють компонентом `TimeGrid`, який інкапсулює управління часовими комірками.
- **Гнучка система правил**: Усі вимоги щодо розкладу реалізовані через інтерфейс `IConstraint`.

---

## UML Діаграма (Шар Model)

Нижче наведена діаграма класів алгоритмічного ядра з урахуванням дисциплін та навчальних вимог:

```mermaid
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
    }

    class LessonType {
        <<enumeration>>
        Lecture
        Practice
        Lab
    }

    class GroupRequirement {
        +Guid DisciplineId
        +LessonType Type
        +int RequiredHours
    }

    class TeacherCompetence {
        +Guid DisciplineId
        +LessonType Type
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
        +LessonType Type
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
    Shell --> Dashboard["DashboardPage (Головна)"]
    Shell --> Settings["SettingsPage (Параметри)"]
    Shell --> Disciplines["DisciplinesPage (Дисципліни)"]
    Shell --> Teachers["TeachersPage (Викладачі)"]
    Shell --> Groups["StudentGroupsPage (Групи)"]
    Shell --> ScheduleResult["ScheduleResultPage (Розклад)"]

    %% Внутрішня стекова навігація
    Disciplines -- "Додати / Редагувати" --> DisciplineEdit["DisciplineEditPage"]
    
    Teachers -- "Додати / Редагувати" --> TeacherEdit["TeacherEditPage (+Компетенції)"]
    TeacherEdit -- "Налаштувати обмеження" --> ConstraintsEdit["ConstraintsEditPage"]
    
    Groups -- "Додати / Редагувати" --> GroupEdit["StudentGroupEditPage (+Вимоги)"]
    GroupEdit -- "Налаштувати обмеження" --> ConstraintsEdit
    
    %% Логіка генерації
    Dashboard -- "Згенерувати" --> GenerateAction{"Процес Генерації"}
    GenerateAction -- "Успіх" --> ScheduleResult
```

### Короткий опис сторінок
1. **DashboardPage**: Керуючий центр. Показує статистику (кількість груп, викладачів) і має головну кнопку "Згенерувати розклад".
2. **SettingsPage**: Форма для введення `Days` та `CellsPerDay`.
3. **DisciplinesPage / EditPage**: CRUD для предметів.
4. **TeachersPage / EditPage**: Список викладачів. На сторінці редагування можна додавати `TeacherCompetence` та переходити до списку персональних обмежень (`ConstraintsEditPage`).
5. **StudentGroupsPage / EditPage**: Список груп. На сторінці редагування можна додавати `GroupRequirement` (Скільки годин якого предмету треба) та переходити до їхніх обмежень.
6. **ScheduleResultPage**: Сторінка з `Picker` (вибір викладача або групи). Нижче відображається двовимірна сітка (CollectionView або Grid), де видно розклад обраного учасника. Також тут виводиться список нерозподілених уроків.

## Архітектура ViewModels (MVVM)

Тут наведено структуру класів ViewModel, яка відповідає всім вимогам лабораторної роботи (використання `INotifyPropertyChanged`, `ObservableCollection`, `ICommand` та `Data Binding`).

```mermaid
classDiagram
    direction TB
    
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
        +ICommand GenerateScheduleCommand
    }

    class TeachersViewModel {
        +ObservableCollection~Teacher~ Teachers
        +string FirstName
        +string LastName
        +string Patronymic
        +string FullNamePreview
        +ICommand AddTeacherCommand
        -bool CanAddTeacher()
    }

    class StudentGroupsViewModel {
        +ObservableCollection~StudentGroup~ Groups
        +string GroupName
        +ICommand AddGroupCommand
    }

    class ScheduleResultViewModel {
        +ObservableCollection~Lesson~ UnassignedLessons
        +ObservableCollection~TimeSlot~ GridCells
        +ICommand ExportCommand
    }

    INotifyPropertyChanged <|.. BaseViewModel : реалізує
    BaseViewModel <|-- DashboardViewModel : наслідує
    BaseViewModel <|-- TeachersViewModel : наслідує
    BaseViewModel <|-- StudentGroupsViewModel : наслідує
    BaseViewModel <|-- ScheduleResultViewModel : наслідує
```
