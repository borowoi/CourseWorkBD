using Microsoft.EntityFrameworkCore;
using SchoolJournal.DataAccess.Data;
using SchoolJournal.DataAccess.Entities;
using SchoolJournal.DataAccess.Interfaces;

namespace SchoolJournal.DataAccess.Repositories;

public class JournalRepository : Repository<Journal>, IJournalRepository
{
    public JournalRepository(SchoolJournalDbContext context) : base(context)
    {
    }

    public IQueryable<Journal> QueryWithReferences()
        => Context.Set<Journal>()
            .AsNoTracking()
            .Include(j => j.Student)
            .Include(j => j.Lesson).ThenInclude(l => l.Class)
            .Include(j => j.Lesson).ThenInclude(l => l.Subject);

    public async Task<bool> EntryExistsAsync(int studentId, int lessonId, int? exceptId = null,
        CancellationToken cancellationToken = default)
        => await Context.Set<Journal>().AnyAsync(
            j => j.StudentId == studentId && j.LessonId == lessonId
                 && (exceptId == null || j.JournalId != exceptId),
            cancellationToken);
}

public class LessonRepository : Repository<Lesson>, ILessonRepository
{
    public LessonRepository(SchoolJournalDbContext context) : base(context)
    {
    }

    public IQueryable<Lesson> QueryWithReferences()
        => Context.Set<Lesson>()
            .AsNoTracking()
            .Include(l => l.Class)
            .Include(l => l.Subject)
            .Include(l => l.Teacher);

    public async Task<Lesson?> GetWithJournalAsync(int id, CancellationToken cancellationToken = default)
        => await Context.Set<Lesson>()
            .Include(l => l.Class)
            .Include(l => l.Subject)
            .Include(l => l.Teacher)
            .Include(l => l.Journals).ThenInclude(j => j.Student)
            .FirstOrDefaultAsync(l => l.LessonId == id, cancellationToken);
}

public class StudentRepository : Repository<Student>, IStudentRepository
{
    public StudentRepository(SchoolJournalDbContext context) : base(context)
    {
    }

    public IQueryable<Student> QueryWithClass()
        => Context.Set<Student>().AsNoTracking().Include(s => s.CurrentClass);
}

public class SubjectHourRepository : Repository<SubjectHour>, ISubjectHourRepository
{
    public SubjectHourRepository(SchoolJournalDbContext context) : base(context)
    {
    }

    public async Task<SubjectHour?> GetAsync(int subjectId, int gradeLevel,
        CancellationToken cancellationToken = default)
        => await Context.Set<SubjectHour>().FindAsync([subjectId, gradeLevel], cancellationToken);
}
