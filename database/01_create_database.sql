-- =====================================================================
-- SchoolJournalDB: полный скрипт создания базы данных (таблицы, представление, индексы)
-- Запускать целиком в SSMS / VS (SQL Server Object Explorer).
-- =====================================================================

-- 1. Создание базы данных
USE master;
GO

IF DB_ID(N'SchoolJournalDB') IS NOT NULL
BEGIN
    RAISERROR(N'База SchoolJournalDB уже существует. Удалите или переименуйте её.', 16, 1);
    SET NOEXEC ON;
END
GO

DECLARE @dataPath NVARCHAR(500) = CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS NVARCHAR(500));
DECLARE @logPath  NVARCHAR(500) = CAST(SERVERPROPERTY('InstanceDefaultLogPath')  AS NVARCHAR(500));
DECLARE @sql NVARCHAR(MAX);

IF @dataPath IS NULL OR @logPath IS NULL
    -- запасной вариант: пути по умолчанию определит сам сервер
    SET @sql = N'CREATE DATABASE SchoolJournalDB';
ELSE
    SET @sql =
        N'CREATE DATABASE SchoolJournalDB ON PRIMARY
          (NAME = N''SchoolJournalCourse_Data'', FILENAME = N''' + @dataPath + N'SchoolJournalCourse.mdf'')
          LOG ON
          (NAME = N''SchoolJournalCourse_Log'',  FILENAME = N''' + @logPath  + N'SchoolJournalCourse_log.ldf'')';
EXEC (@sql);
GO

USE SchoolJournalDB;
GO

-- 2. Таблица "Учителя"
CREATE TABLE dbo.Teachers (
    TeacherID INT IDENTITY(1,1) CONSTRAINT PK_Teachers PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    QualificationCategory NVARCHAR(50) NOT NULL,
    WorkloadHours INT NOT NULL CONSTRAINT CHK_Teachers_Workload CHECK (WorkloadHours >= 0)
);
GO

-- 3. Таблица "Учебные предметы"
CREATE TABLE dbo.Subjects (
    SubjectID INT IDENTITY(1,1) CONSTRAINT PK_Subjects PRIMARY KEY,
    SubjectName NVARCHAR(100) NOT NULL,
    ExamType NVARCHAR(50) NOT NULL
        CONSTRAINT CHK_Subjects_ExamType CHECK (ExamType IN (N'Экзамен', N'Зачет', N'Дифференцированный зачет'))
);
GO

-- 4. Таблица "Нагрузка по часам"
CREATE TABLE dbo.SubjectHours (
    SubjectID INT NOT NULL
        CONSTRAINT FK_SubjectHours_Subjects FOREIGN KEY REFERENCES dbo.Subjects(SubjectID) ON DELETE CASCADE,
    GradeLevel INT NOT NULL CONSTRAINT CHK_SubjectHours_Grade CHECK (GradeLevel BETWEEN 1 AND 11),
    HoursPerWeek INT NOT NULL CONSTRAINT CHK_SubjectHours_Hours CHECK (HoursPerWeek BETWEEN 1 AND 10),
    CONSTRAINT PK_SubjectHours PRIMARY KEY CLUSTERED (SubjectID, GradeLevel)
);
GO

-- 5. Таблица "Классы"
CREATE TABLE dbo.Classes (
    ClassID INT IDENTITY(1,1) CONSTRAINT PK_Classes PRIMARY KEY,
    GradeLevel INT NOT NULL CONSTRAINT CHK_Classes_Grade CHECK (GradeLevel BETWEEN 1 AND 11),
    ClassLetter NCHAR(1) NOT NULL,
    HomeroomTeacherID INT NULL
        CONSTRAINT FK_Classes_Teachers FOREIGN KEY REFERENCES dbo.Teachers(TeacherID),
    AcademicYear NVARCHAR(9) NOT NULL
        CONSTRAINT CHK_Classes_Year CHECK (AcademicYear LIKE '[0-9][0-9][0-9][0-9]/[0-9][0-9][0-9][0-9]')
);
GO

-- 6. Таблица "Учащиеся"
CREATE TABLE dbo.Students (
    StudentID INT IDENTITY(1,1) CONSTRAINT PK_Students PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    BirthDate DATE NOT NULL,
    CurrentClassID INT NOT NULL
        CONSTRAINT FK_Students_Classes FOREIGN KEY REFERENCES dbo.Classes(ClassID),
    ContactInfo NVARCHAR(250) NULL,
    EnrollmentDate DATE NOT NULL
);
GO

-- 7. Таблица "Уроки"
CREATE TABLE dbo.Lessons (
    LessonID INT IDENTITY(1,1) CONSTRAINT PK_Lessons PRIMARY KEY,
    ClassID INT NOT NULL CONSTRAINT FK_Lessons_Classes FOREIGN KEY REFERENCES dbo.Classes(ClassID),
    SubjectID INT NOT NULL CONSTRAINT FK_Lessons_Subjects FOREIGN KEY REFERENCES dbo.Subjects(SubjectID),
    TeacherID INT NULL CONSTRAINT FK_Lessons_Teachers FOREIGN KEY REFERENCES dbo.Teachers(TeacherID),
    LessonDate DATE NOT NULL,
    LessonNumber INT NOT NULL CONSTRAINT CHK_Lessons_Number CHECK (LessonNumber BETWEEN 1 AND 8),
    Topic NVARCHAR(250) NOT NULL
);
GO

-- 8. Таблица "Журнал" (оценки и пропуски)
CREATE TABLE dbo.Journal (
    JournalID INT IDENTITY(1,1) CONSTRAINT PK_Journal PRIMARY KEY,
    StudentID INT NOT NULL CONSTRAINT FK_Journal_Students FOREIGN KEY REFERENCES dbo.Students(StudentID),
    LessonID INT NOT NULL CONSTRAINT FK_Journal_Lessons FOREIGN KEY REFERENCES dbo.Lessons(LessonID),
    IsPresent BIT NOT NULL, -- 1 = был, 0 = пропустил
    AbsenceReason NVARCHAR(50) NULL
        CONSTRAINT CHK_Journal_Reason CHECK (AbsenceReason IN (N'Болезнь', N'Уважительная', N'Без уважительной причины')),
    GradeValue INT NULL CONSTRAINT CHK_Journal_Grade CHECK (GradeValue BETWEEN 2 AND 5),
    WorkType NVARCHAR(50) NULL
        CONSTRAINT CHK_Journal_Work CHECK (WorkType IN (N'Устный ответ', N'Самостоятельная', N'Контрольная', N'Лабораторная')),
    CONSTRAINT CHK_Journal_Logical_Integrity CHECK (
        (IsPresent = 1 AND AbsenceReason IS NULL) OR
        (IsPresent = 0 AND AbsenceReason IS NOT NULL AND GradeValue IS NULL AND WorkType IS NULL)
    )
);
GO

-- 9. Индексы
CREATE UNIQUE INDEX UX_Journal_Student_Lesson ON dbo.Journal (StudentID, LessonID);
CREATE INDEX IX_Lessons_Class_Subject_Date ON dbo.Lessons (ClassID, SubjectID, LessonDate);
CREATE INDEX IX_Journal_Lesson ON dbo.Journal (LessonID) INCLUDE (StudentID, IsPresent, GradeValue);
GO

-- 10. Представление для расчёта итоговых отметок (четверти и годовая)
CREATE VIEW dbo.v_FinalGrades AS
WITH QuarterAvg AS (
    SELECT
        j.StudentID, l.SubjectID, c.AcademicYear,
        CASE
            WHEN MONTH(l.LessonDate) IN (9, 10)    THEN N'1 Четверть'
            WHEN MONTH(l.LessonDate) IN (11, 12)   THEN N'2 Четверть'
            WHEN MONTH(l.LessonDate) IN (1, 2)     THEN N'3 Четверть'
            WHEN MONTH(l.LessonDate) IN (3, 4, 5)  THEN N'4 Четверть'
        END AS PeriodType,
        AVG(CAST(j.GradeValue AS FLOAT)) AS AvgGrade
    FROM dbo.Journal j
    JOIN dbo.Lessons l ON j.LessonID = l.LessonID
    JOIN dbo.Classes c ON l.ClassID = c.ClassID
    WHERE j.GradeValue IS NOT NULL
      AND MONTH(l.LessonDate) IN (1, 2, 3, 4, 5, 9, 10, 11, 12)
    GROUP BY j.StudentID, l.SubjectID, c.AcademicYear,
        CASE
            WHEN MONTH(l.LessonDate) IN (9, 10)    THEN N'1 Четверть'
            WHEN MONTH(l.LessonDate) IN (11, 12)   THEN N'2 Четверть'
            WHEN MONTH(l.LessonDate) IN (1, 2)     THEN N'3 Четверть'
            WHEN MONTH(l.LessonDate) IN (3, 4, 5)  THEN N'4 Четверть'
        END
),
Combined AS (
    SELECT StudentID, SubjectID, AcademicYear, PeriodType,
           ROUND(AvgGrade, 0) AS FinalGrade
    FROM QuarterAvg
    UNION ALL
    SELECT StudentID, SubjectID, AcademicYear, N'Годовая',
           ROUND(AVG(ROUND(AvgGrade, 0)), 0)
    FROM QuarterAvg
    GROUP BY StudentID, SubjectID, AcademicYear
)
SELECT
    cb.StudentID,
    s.FullName    AS StudentName,
    cb.SubjectID,
    sub.SubjectName,
    cb.AcademicYear,
    cb.PeriodType,
    cb.FinalGrade
FROM Combined cb
JOIN dbo.Students s   ON s.StudentID = cb.StudentID
JOIN dbo.Subjects sub ON sub.SubjectID = cb.SubjectID;
GO

SET NOEXEC OFF;
GO
