using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SchoolJournal.DataAccess.Entities;

// Таблица называется Journal, а свойство контекста Journals
[Table("Journal")]
public partial class Journal
{
}

// Составной первичный ключ (по соглашению EF Core его не определить)
[PrimaryKey("SubjectId", "GradeLevel")]
public partial class SubjectHour
{
}

// Представление: без ключа и с именем v_FinalGrades
[Keyless]
[Table("v_FinalGrades")]
public partial class VFinalGrade
{
}