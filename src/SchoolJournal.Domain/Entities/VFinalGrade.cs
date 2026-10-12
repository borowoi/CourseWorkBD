using System;
using System.Collections.Generic;

namespace SchoolJournal.Domain.Entities;

public partial class VFinalGrade
{
    public int StudentId { get; set; }

    public string StudentName { get; set; } = null!;

    public int SubjectId { get; set; }

    public string SubjectName { get; set; } = null!;

    public string AcademicYear { get; set; } = null!;

    public string? PeriodType { get; set; }

    public double? FinalGrade { get; set; }
}
