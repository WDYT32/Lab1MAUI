using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lab1MAUI.Models.Schedule;

public class Schedule
{
    public ScheduleConfig Config { get; set; } = new();
    public List<Teacher> Teachers { get; set; } = new();
    public List<StudentGroup> Groups { get; set; } = new();
    public List<Lesson> Lessons { get; set; } = new();
    public List<Discipline> Disciplines { get; set; } = new();
    public ConstraintRegistry Registry { get; set; } = new();

    public Schedule() { }

    public Schedule(ScheduleConfig config, List<Teacher> teachers, List<StudentGroup> groups, List<Lesson> lessons)
    {
        Config = config;
        Teachers = teachers;
        Groups = groups;
        Lessons = lessons;
    }

    public void Generate()
    {
        InitializeData();
        CollectAllConstraints();
        ApplyConstraintsMainLoop();
    }

    public void InitializeData()
    {
        Lessons.Clear();
        int total = Config.TotalCells;
        var rnd = new Random();

        foreach (var t in Teachers)
        {
            if (t.Grid.TargetHours == 0) t.Grid.TargetHours = rnd.Next(10, 20);
            t.Grid.Initialize(total);
        }

        foreach (var g in Groups)
        {
            if (g.Grid.TargetHours == 0) g.Grid.TargetHours = rnd.Next(15, 25);
            g.Grid.Initialize(total);
        }

        SpawnLessons();
    }

    private void SpawnLessons()
    {
        foreach (var group in Groups)
        {
            foreach (var req in group.Requirements)
            {
                var disciplineName = Disciplines.FirstOrDefault(d => d.Id == req.DisciplineId)?.Name ?? "Generated";
                
                for (int i = 0; i < req.RequiredHours; i++)
                {
                    var lesson = new Lesson
                    {
                        StudentGroupId = group.Id,
                        TeacherId = Guid.Empty, // Призначення відбувається у TeacherAssignmentConstraint
                        DisciplineId = req.DisciplineId,
                        SubjectName = disciplineName
                    };
                    
                    Lessons.Add(lesson);
                }
            }
            // НЕ ОЧИЩАЙМО ВИМОГИ ТУТ, ІНАКШЕ ДРУГЕ ГЕНЕРУВАННЯ ПУСТЕ!
            // group.Requirements.Clear(); 
        }
    }

    public void CollectAllConstraints()
    {
        Registry = new ConstraintRegistry();
        int total = Config.TotalCells;

        // Вчителі: позиція у списку - це множник пріоритету
        for (int i = 0; i < Teachers.Count; i++)
        {
            var t = Teachers[i];
            int entityMultiplier = Teachers.Count - i;
            
            var personalConstraints = t.GetPersonalConstraints(total);
            for (int j = 0; j < personalConstraints.Count; j++)
            {
                var c = personalConstraints[j];
                int constraintWeight = personalConstraints.Count - j;
                
                c.Priority += entityMultiplier * constraintWeight * 10;
                Registry.Register(c);
            }
        }

        // Групи: позиція у списку - це множник пріоритету
        for (int i = 0; i < Groups.Count; i++)
        {
            var g = Groups[i];
            int entityMultiplier = Groups.Count - i;
            
            var personalConstraints = g.GetPersonalConstraints(total);
            for (int j = 0; j < personalConstraints.Count; j++)
            {
                var c = personalConstraints[j];
                int constraintWeight = personalConstraints.Count - j;
                
                c.Priority += entityMultiplier * constraintWeight * 10;
                Registry.Register(c);
            }
        }
        
        // Дисципліни: позиція у списку - це множник пріоритету
        for (int i = 0; i < Disciplines.Count; i++)
        {
            var d = Disciplines[i];
            int entityMultiplier = Disciplines.Count - i;
            
            var personalConstraints = d.GetPersonalConstraints(total);
            for (int j = 0; j < personalConstraints.Count; j++)
            {
                var c = personalConstraints[j];
                int constraintWeight = personalConstraints.Count - j;
                
                c.Priority += entityMultiplier * constraintWeight * 10;
                Registry.Register(c);
            }
        }

        foreach (var l in Lessons)
        {
            foreach (var c in l.GetLessonConstraints()) Registry.Register(c);
        }
    }

    public void ApplyConstraintsMainLoop()
    {
        var processed = new HashSet<IConstraint>();
        
        while (true)
        {
            var ordered = Registry.GetOrdered();
            var next = ordered.FirstOrDefault(c => !processed.Contains(c));
            
            if (next == null) break; // Усі обмеження виконано
            
            next.Apply(this);
            processed.Add(next);
            
            ValidateConsistency();
        }
    }

    public void ValidateConsistency()
    {
        int total = Config.TotalCells;

        for (int i = 0; i < total; i++)
        {
            foreach (var lesson in Lessons)
            {
                var teacher = Teachers.FirstOrDefault(t => t.Id == lesson.TeacherId);
                var group = Groups.FirstOrDefault(g => g.Id == lesson.StudentGroupId);
                
                if (teacher == null || group == null) continue;

                bool teacherHasLesson = !teacher.Grid.IsFree(i) && teacher.Grid.Cells[i] == lesson.Id;
                bool groupHasLesson = !group.Grid.IsFree(i) && group.Grid.Cells[i] == lesson.Id;

                if (teacherHasLesson && !groupHasLesson)
                {
                    teacher.Grid.Remove(i);
                }
                if (groupHasLesson && !teacherHasLesson)
                {
                    group.Grid.Remove(i);
                }
            }
        }
    }
}
