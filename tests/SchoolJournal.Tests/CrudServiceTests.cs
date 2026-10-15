using Microsoft.EntityFrameworkCore;
using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.BusinessLogic.Exceptions;
using SchoolJournal.BusinessLogic.Services;
using SchoolJournal.DataAccess.Data;
using SchoolJournal.DataAccess.Entities;
using SchoolJournal.DataAccess.Interfaces;
using SchoolJournal.DataAccess.Repositories;
using Xunit;

namespace SchoolJournal.Tests;

/// <summary>Вспомогательные методы: чистая БД в памяти и стартовые данные.</summary>
internal static class TestDb
{
    public static (SchoolJournalDbContext Context, IUnitOfWork Uow) Create()
    {
        var options = new DbContextOptionsBuilder<SchoolJournalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new SchoolJournalDbContext(options);
        return (context, new UnitOfWork(context));
    }

    public static Class AddClass(SchoolJournalDbContext ctx, int grade = 5, string letter = "А")
    {
        var cls = new Class { GradeLevel = grade, ClassLetter = letter, AcademicYear = "2026/2027" };
        ctx.Set<Class>().Add(cls);
        ctx.SaveChanges();
        return cls;
    }

    public static Teacher AddTeacher(SchoolJournalDbContext ctx, string name = "Иванова Анна Петровна")
    {
        var teacher = new Teacher { FullName = name, QualificationCategory = "Высшая", WorkloadHours = 18 };
        ctx.Set<Teacher>().Add(teacher);
        ctx.SaveChanges();
        return teacher;
    }

    public static Subject AddSubject(SchoolJournalDbContext ctx, string name = "Математика")
    {
        var subject = new Subject { SubjectName = name, ExamType = "Экзамен" };
        ctx.Set<Subject>().Add(subject);
        ctx.SaveChanges();
        return subject;
    }

    public static Student AddStudent(SchoolJournalDbContext ctx, int classId, string name = "Петров Пётр Петрович")
    {
        var student = new Student
        {
            FullName = name,
            BirthDate = new DateOnly(2015, 3, 3),
            CurrentClassId = classId,
            EnrollmentDate = new DateOnly(2021, 9, 1)
        };
        ctx.Set<Student>().Add(student);
        ctx.SaveChanges();
        return student;
    }

    public static Lesson AddLesson(SchoolJournalDbContext ctx, int classId, int subjectId, int? teacherId,
        DateOnly? date = null, int number = 2)
    {
        var lesson = new Lesson
        {
            ClassId = classId,
            SubjectId = subjectId,
            TeacherId = teacherId,
            LessonDate = date ?? new DateOnly(2026, 9, 7),
            LessonNumber = number,
            Topic = "Тема урока"
        };
        ctx.Set<Lesson>().Add(lesson);
        ctx.SaveChanges();
        return lesson;
    }
}

public class JournalServiceTests
{
    private static (JournalService Service, SchoolJournalDbContext Ctx, Lesson Lesson, Student Student) Arrange()
    {
        var (ctx, uow) = TestDb.Create();
        var cls = TestDb.AddClass(ctx);
        var subject = TestDb.AddSubject(ctx);
        var teacher = TestDb.AddTeacher(ctx);
        var lesson = TestDb.AddLesson(ctx, cls.ClassId, subject.SubjectId, teacher.TeacherId);
        var student = TestDb.AddStudent(ctx, cls.ClassId);
        return (new JournalService(uow), ctx, lesson, student);
    }

    [Fact]
    public async Task Create_AbsentWithoutReason_Throws()
    {
        var (service, _, lesson, student) = Arrange();
        var dto = new JournalEntryEditDto { StudentId = student.StudentId, LessonId = lesson.LessonId, IsPresent = false };
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task Create_AbsentWithGrade_Throws()
    {
        var (service, _, lesson, student) = Arrange();
        var dto = new JournalEntryEditDto
        {
            StudentId = student.StudentId, LessonId = lesson.LessonId, IsPresent = false,
            AbsenceReason = "Болезнь", GradeValue = 4, WorkType = "Устный ответ"
        };
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task Create_GradeWithoutWorkType_Throws()
    {
        var (service, _, lesson, student) = Arrange();
        var dto = new JournalEntryEditDto
        {
            StudentId = student.StudentId, LessonId = lesson.LessonId, IsPresent = true, GradeValue = 5
        };
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task Create_InvalidGrade_Throws()
    {
        var (service, _, lesson, student) = Arrange();
        var dto = new JournalEntryEditDto
        {
            StudentId = student.StudentId, LessonId = lesson.LessonId, IsPresent = true,
            GradeValue = 7, WorkType = "Устный ответ"
        };
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task Create_Valid_SavesEntry()
    {
        var (service, ctx, lesson, student) = Arrange();
        var dto = new JournalEntryEditDto
        {
            StudentId = student.StudentId, LessonId = lesson.LessonId, IsPresent = true,
            GradeValue = 5, WorkType = "Контрольная"
        };

        var id = await service.CreateAsync(dto);

        var saved = ctx.Set<Journal>().Single();
        Assert.Equal(id, saved.JournalId);
        Assert.Equal(5, saved.GradeValue);
        Assert.Null(saved.AbsenceReason);
    }

    [Fact]
    public async Task Create_Duplicate_Throws()
    {
        var (service, _, lesson, student) = Arrange();
        var dto = new JournalEntryEditDto
        {
            StudentId = student.StudentId, LessonId = lesson.LessonId, IsPresent = false, AbsenceReason = "Болезнь"
        };
        await service.CreateAsync(dto);
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task Create_StudentFromAnotherClass_Throws()
    {
        var (service, ctx, lesson, _) = Arrange();
        var otherClass = TestDb.AddClass(ctx, 6, "Б");
        var stranger = TestDb.AddStudent(ctx, otherClass.ClassId, "Сидоров Сидор Сидорович");
        var dto = new JournalEntryEditDto
        {
            StudentId = stranger.StudentId, LessonId = lesson.LessonId, IsPresent = false, AbsenceReason = "Болезнь"
        };
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task Update_ChangesAbsenceToGrade()
    {
        var (service, ctx, lesson, student) = Arrange();
        var id = await service.CreateAsync(new JournalEntryEditDto
        {
            StudentId = student.StudentId, LessonId = lesson.LessonId, IsPresent = false, AbsenceReason = "Болезнь"
        });

        await service.UpdateAsync(new JournalEntryEditDto
        {
            Id = id, StudentId = student.StudentId, LessonId = lesson.LessonId, IsPresent = true,
            GradeValue = 4, WorkType = "Самостоятельная"
        });

        var saved = ctx.Set<Journal>().Single();
        Assert.True(saved.IsPresent);
        Assert.Null(saved.AbsenceReason);
        Assert.Equal(4, saved.GradeValue);
    }
}

public class LessonServiceTests
{
    [Fact]
    public async Task Create_SameClassSameTime_Throws()
    {
        var (ctx, uow) = TestDb.Create();
        var cls = TestDb.AddClass(ctx);
        var subject = TestDb.AddSubject(ctx);
        var teacher = TestDb.AddTeacher(ctx);
        var other = TestDb.AddTeacher(ctx, "Сидорова Мария Ивановна");
        TestDb.AddLesson(ctx, cls.ClassId, subject.SubjectId, teacher.TeacherId, new DateOnly(2026, 9, 7), 2);

        var service = new LessonService(uow);
        var dto = new LessonEditDto
        {
            ClassId = cls.ClassId, SubjectId = subject.SubjectId, TeacherId = other.TeacherId,
            LessonDate = new DateOnly(2026, 9, 7), LessonNumber = 2, Topic = "Другая тема"
        };
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task Create_TeacherBusyInAnotherClass_Throws()
    {
        var (ctx, uow) = TestDb.Create();
        var first = TestDb.AddClass(ctx, 5, "А");
        var second = TestDb.AddClass(ctx, 5, "Б");
        var subject = TestDb.AddSubject(ctx);
        var teacher = TestDb.AddTeacher(ctx);
        TestDb.AddLesson(ctx, first.ClassId, subject.SubjectId, teacher.TeacherId, new DateOnly(2026, 9, 7), 3);

        var service = new LessonService(uow);
        var dto = new LessonEditDto
        {
            ClassId = second.ClassId, SubjectId = subject.SubjectId, TeacherId = teacher.TeacherId,
            LessonDate = new DateOnly(2026, 9, 7), LessonNumber = 3, Topic = "Тема"
        };
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task Create_OnSunday_Throws()
    {
        var (ctx, uow) = TestDb.Create();
        var cls = TestDb.AddClass(ctx);
        var subject = TestDb.AddSubject(ctx);
        var service = new LessonService(uow);
        var dto = new LessonEditDto
        {
            ClassId = cls.ClassId, SubjectId = subject.SubjectId,
            LessonDate = new DateOnly(2026, 9, 6), LessonNumber = 1, Topic = "Тема"   // воскресенье
        };
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task Delete_WithEntries_RequiresConfirmation()
    {
        var (ctx, uow) = TestDb.Create();
        var cls = TestDb.AddClass(ctx);
        var subject = TestDb.AddSubject(ctx);
        var teacher = TestDb.AddTeacher(ctx);
        var lesson = TestDb.AddLesson(ctx, cls.ClassId, subject.SubjectId, teacher.TeacherId);
        var student = TestDb.AddStudent(ctx, cls.ClassId);
        ctx.Set<Journal>().Add(new Journal { StudentId = student.StudentId, LessonId = lesson.LessonId, IsPresent = true });
        ctx.SaveChanges();

        var service = new LessonService(uow);
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.DeleteAsync(lesson.LessonId));

        await service.DeleteAsync(lesson.LessonId, withEntries: true);
        Assert.Empty(ctx.Set<Lesson>());
        Assert.Empty(ctx.Set<Journal>());
    }
}

public class ClassAndStudentServiceTests
{
    [Fact]
    public async Task Class_Create_NonConsecutiveYears_Throws()
    {
        var (_, uow) = TestDb.Create();
        var service = new ClassService(uow);
        var dto = new ClassEditDto { GradeLevel = 5, ClassLetter = "А", AcademicYear = "2026/2028" };
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task Class_Create_Duplicate_Throws()
    {
        var (_, uow) = TestDb.Create();
        var service = new ClassService(uow);
        var dto = new ClassEditDto { GradeLevel = 5, ClassLetter = "а", AcademicYear = "2026/2027" };
        await service.CreateAsync(dto);
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task Class_Delete_WithStudents_Throws()
    {
        var (ctx, uow) = TestDb.Create();
        var cls = TestDb.AddClass(ctx);
        TestDb.AddStudent(ctx, cls.ClassId);
        var service = new ClassService(uow);
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.DeleteAsync(cls.ClassId));
    }

    [Fact]
    public async Task Student_Create_EnrolledTooYoung_Throws()
    {
        var (ctx, uow) = TestDb.Create();
        var cls = TestDb.AddClass(ctx);
        var service = new StudentService(uow);
        var dto = new StudentEditDto
        {
            FullName = "Новый Ученик Иванович", BirthDate = new DateOnly(2020, 1, 1),
            CurrentClassId = cls.ClassId, EnrollmentDate = new DateOnly(2021, 9, 1)
        };
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task Student_Create_Valid_ThenFilterByName()
    {
        var (ctx, uow) = TestDb.Create();
        var cls = TestDb.AddClass(ctx);
        var service = new StudentService(uow);
        await service.CreateAsync(new StudentEditDto
        {
            FullName = "Орлов Олег Олегович", BirthDate = new DateOnly(2015, 5, 5),
            CurrentClassId = cls.ClassId, EnrollmentDate = new DateOnly(2021, 9, 1)
        });
        await service.CreateAsync(new StudentEditDto
        {
            FullName = "Зайцев Захар Захарович", BirthDate = new DateOnly(2015, 6, 6),
            CurrentClassId = cls.ClassId, EnrollmentDate = new DateOnly(2021, 9, 1)
        });

        var page = await service.GetPagedAsync(new StudentFilterDto { Search = "Орлов" });

        var item = Assert.Single(page.Items);
        Assert.Equal("5А", item.ClassName);
        Assert.Equal(1, page.TotalCount);
    }
}

public class TeacherAndSubjectServiceTests
{
    [Fact]
    public async Task Teacher_GetPaged_ReturnsRequestedPage()
    {
        var (ctx, uow) = TestDb.Create();
        for (var i = 1; i <= 25; i++)
            TestDb.AddTeacher(ctx, $"Учитель {i:D2}");

        var page = await new TeacherService(uow).GetPagedAsync(new TeacherFilterDto { PageIndex = 3, PageSize = 10 });

        Assert.Equal(5, page.Items.Count);
        Assert.Equal(25, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
        Assert.False(page.HasNextPage);
        Assert.True(page.HasPreviousPage);
    }

    [Fact]
    public async Task Teacher_Delete_WithLessons_Throws()
    {
        var (ctx, uow) = TestDb.Create();
        var cls = TestDb.AddClass(ctx);
        var subject = TestDb.AddSubject(ctx);
        var teacher = TestDb.AddTeacher(ctx);
        TestDb.AddLesson(ctx, cls.ClassId, subject.SubjectId, teacher.TeacherId);

        await Assert.ThrowsAsync<BusinessRuleException>(() => new TeacherService(uow).DeleteAsync(teacher.TeacherId));
    }

    [Fact]
    public async Task Teacher_Create_InvalidCategory_Throws()
    {
        var (_, uow) = TestDb.Create();
        var dto = new TeacherEditDto { FullName = "Тест Тестович", QualificationCategory = "Магистр", WorkloadHours = 10 };
        await Assert.ThrowsAsync<BusinessRuleException>(() => new TeacherService(uow).CreateAsync(dto));
    }

    [Fact]
    public async Task Subject_SetHours_CreatesThenUpdatesSingleRecord()
    {
        var (ctx, uow) = TestDb.Create();
        var subject = TestDb.AddSubject(ctx);
        var service = new SubjectService(uow);

        await service.SetHoursAsync(subject.SubjectId, 5, 3);
        await service.SetHoursAsync(subject.SubjectId, 5, 4);

        var hours = await service.GetHoursAsync(subject.SubjectId);
        var single = Assert.Single(hours);
        Assert.Equal(5, single.GradeLevel);
        Assert.Equal(4, single.HoursPerWeek);
    }

    [Fact]
    public async Task Subject_Create_DuplicateName_Throws()
    {
        var (_, uow) = TestDb.Create();
        var service = new SubjectService(uow);
        var dto = new SubjectEditDto { SubjectName = "Физика", ExamType = "Зачет" };
        await service.CreateAsync(dto);
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(dto));
    }
}
