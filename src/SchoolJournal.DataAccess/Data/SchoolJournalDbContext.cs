using Microsoft.EntityFrameworkCore;
using SchoolJournal.DataAccess.Entities;

namespace SchoolJournal.DataAccess.Data;

public class SchoolJournalDbContext : DbContext
{
    public SchoolJournalDbContext(DbContextOptions<SchoolJournalDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Class> Classes { get; set; } = null!;
    public virtual DbSet<Journal> Journals { get; set; } = null!;
    public virtual DbSet<Lesson> Lessons { get; set; } = null!;
    public virtual DbSet<Student> Students { get; set; } = null!;
    public virtual DbSet<Subject> Subjects { get; set; } = null!;
    public virtual DbSet<SubjectHour> SubjectHours { get; set; } = null!;
    public virtual DbSet<Teacher> Teachers { get; set; } = null!;
    public virtual DbSet<VFinalGrade> VFinalGrades { get; set; } = null!;
}