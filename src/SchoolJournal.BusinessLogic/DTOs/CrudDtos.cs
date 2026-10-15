using System.ComponentModel.DataAnnotations;

namespace SchoolJournal.BusinessLogic.DTOs;

/// <summary>Общие параметры постраничного вывода и сортировки.</summary>
public class PagingFilter
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortOrder { get; set; }
}

/// <summary>Элемент выпадающего списка.</summary>
public sealed record LookupDto(int Id, string Name);

// =============================== Учителя ===============================
public class TeacherDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string QualificationCategory { get; set; } = string.Empty;
    public int WorkloadHours { get; set; }
    public int ClassesCount { get; set; }
    public int LessonsCount { get; set; }
}

public class TeacherFilterDto : PagingFilter
{
    public string? Search { get; set; }
    public string? Category { get; set; }
}

public class TeacherEditDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите ФИО")]
    [StringLength(150, ErrorMessage = "ФИО не должно быть длиннее 150 символов")]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите квалификационную категорию")]
    [StringLength(50)]
    [Display(Name = "Квалификационная категория")]
    public string QualificationCategory { get; set; } = string.Empty;

    [Range(0, 60, ErrorMessage = "Нагрузка должна быть от 0 до 60 часов")]
    [Display(Name = "Нагрузка, часов")]
    public int WorkloadHours { get; set; }
}

// =============================== Предметы ===============================
public class SubjectDto
{
    public int Id { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public int LessonsCount { get; set; }
    public int GradesWithHours { get; set; }
}

public class SubjectFilterDto : PagingFilter
{
    public string? Search { get; set; }
    public string? ExamType { get; set; }
}

public class SubjectEditDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите наименование предмета")]
    [StringLength(100, ErrorMessage = "Наименование не должно быть длиннее 100 символов")]
    [Display(Name = "Наименование")]
    public string SubjectName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите форму итоговой аттестации")]
    [RegularExpression("^(Экзамен|Зачет|Дифференцированный зачет)$",
        ErrorMessage = "Допустимые значения: Экзамен, Зачет, Дифференцированный зачет")]
    [Display(Name = "Форма итоговой аттестации")]
    public string ExamType { get; set; } = string.Empty;
}

/// <summary>Количество часов в неделю по параллели.</summary>
public sealed record SubjectHourDto(int GradeLevel, int HoursPerWeek);

// =============================== Классы ===============================
public class ClassDto
{
    public int Id { get; set; }
    public int GradeLevel { get; set; }
    public string ClassLetter { get; set; } = string.Empty;
    public string Name => $"{GradeLevel}{ClassLetter}";
    public string AcademicYear { get; set; } = string.Empty;
    public int? HomeroomTeacherId { get; set; }
    public string? HomeroomTeacherName { get; set; }
    public int StudentsCount { get; set; }
}

public class ClassFilterDto : PagingFilter
{
    public int? GradeLevel { get; set; }
    public string? AcademicYear { get; set; }
    public int? HomeroomTeacherId { get; set; }
}

public class ClassEditDto
{
    public int Id { get; set; }

    [Range(1, 11, ErrorMessage = "Параллель должна быть от 1 до 11")]
    [Display(Name = "Параллель")]
    public int GradeLevel { get; set; }

    [Required(ErrorMessage = "Укажите литеру")]
    [RegularExpression(@"^\p{L}$", ErrorMessage = "Литера — одна буква")]
    [Display(Name = "Литера")]
    public string ClassLetter { get; set; } = string.Empty;

    [Display(Name = "Классный руководитель")]
    public int? HomeroomTeacherId { get; set; }

    [Required(ErrorMessage = "Укажите учебный год")]
    [RegularExpression(@"^\d{4}/\d{4}$", ErrorMessage = "Формат учебного года: 2026/2027")]
    [Display(Name = "Учебный год")]
    public string AcademicYear { get; set; } = string.Empty;
}

// =============================== Учащиеся ===============================
public class StudentDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public int ClassId { get; set; }
    public int GradeLevel { get; set; }
    public string ClassLetter { get; set; } = string.Empty;
    public string ClassName => $"{GradeLevel}{ClassLetter}";
    public string? ContactInfo { get; set; }
    public DateOnly EnrollmentDate { get; set; }
}

public class StudentFilterDto : PagingFilter
{
    public string? Search { get; set; }
    public int? ClassId { get; set; }
    public int? GradeLevel { get; set; }
}

public class StudentEditDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите ФИО")]
    [StringLength(150, ErrorMessage = "ФИО не должно быть длиннее 150 символов")]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Дата рождения")]
    [DataType(DataType.Date)]
    public DateOnly BirthDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите класс")]
    [Display(Name = "Класс")]
    public int CurrentClassId { get; set; }

    [StringLength(250, ErrorMessage = "Контактные данные не должны быть длиннее 250 символов")]
    [Display(Name = "Контактные данные")]
    public string? ContactInfo { get; set; }

    [Display(Name = "Дата зачисления")]
    [DataType(DataType.Date)]
    public DateOnly EnrollmentDate { get; set; }
}

// =============================== Уроки ===============================
public class LessonDto
{
    public int Id { get; set; }
    public int ClassId { get; set; }
    public int GradeLevel { get; set; }
    public string ClassLetter { get; set; } = string.Empty;
    public string ClassName => $"{GradeLevel}{ClassLetter}";
    public string SubjectName { get; set; } = string.Empty;
    public string? TeacherName { get; set; }
    public DateOnly LessonDate { get; set; }
    public int LessonNumber { get; set; }
    public string Topic { get; set; } = string.Empty;
    public int EntriesCount { get; set; }
}

public class LessonFilterDto : PagingFilter
{
    public int? ClassId { get; set; }
    public int? GradeLevel { get; set; }
    public int? SubjectId { get; set; }
    public int? TeacherId { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
}

public class LessonEditDto
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите класс")]
    [Display(Name = "Класс")]
    public int ClassId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите предмет")]
    [Display(Name = "Предмет")]
    public int SubjectId { get; set; }

    [Display(Name = "Учитель")]
    public int? TeacherId { get; set; }

    [Display(Name = "Дата")]
    [DataType(DataType.Date)]
    public DateOnly LessonDate { get; set; }

    [Range(1, 8, ErrorMessage = "Номер урока должен быть от 1 до 8")]
    [Display(Name = "Номер урока")]
    public int LessonNumber { get; set; } = 1;

    [Required(ErrorMessage = "Укажите тему урока")]
    [StringLength(250, ErrorMessage = "Тема не должна быть длиннее 250 символов")]
    [Display(Name = "Тема")]
    public string Topic { get; set; } = string.Empty;
}

// =============================== Журнал (отметки и пропуски) ===============================
public class JournalEntryDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int GradeLevel { get; set; }
    public string ClassLetter { get; set; } = string.Empty;
    public string ClassName => $"{GradeLevel}{ClassLetter}";
    public string SubjectName { get; set; } = string.Empty;
    public int LessonId { get; set; }
    public DateOnly LessonDate { get; set; }
    public int LessonNumber { get; set; }
    public bool IsPresent { get; set; }
    public string? AbsenceReason { get; set; }
    public int? GradeValue { get; set; }
    public string? WorkType { get; set; }
}

public class JournalFilterDto : PagingFilter
{
    public int? ClassId { get; set; }
    public int? SubjectId { get; set; }
    public int? StudentId { get; set; }
    public int? LessonId { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public bool OnlyGraded { get; set; }
    public bool OnlyAbsences { get; set; }
}

public class JournalEntryEditDto
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите учащегося")]
    [Display(Name = "Учащийся")]
    public int StudentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите урок")]
    [Display(Name = "Урок")]
    public int LessonId { get; set; }

    [Display(Name = "Присутствовал")]
    public bool IsPresent { get; set; } = true;

    [Display(Name = "Причина пропуска")]
    public string? AbsenceReason { get; set; }

    [Range(2, 5, ErrorMessage = "Отметка должна быть от 2 до 5")]
    [Display(Name = "Отметка")]
    public int? GradeValue { get; set; }

    [Display(Name = "Вид работы")]
    public string? WorkType { get; set; }
}
