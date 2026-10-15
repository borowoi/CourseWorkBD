USE SchoolJournalDB;
GO
SELECT N'Teachers'     AS [Таблица], COUNT(*) AS [Записей] FROM dbo.Teachers
UNION ALL SELECT N'Subjects',     COUNT(*) FROM dbo.Subjects
UNION ALL SELECT N'SubjectHours', COUNT(*) FROM dbo.SubjectHours
UNION ALL SELECT N'Classes',      COUNT(*) FROM dbo.Classes
UNION ALL SELECT N'Students',     COUNT(*) FROM dbo.Students
UNION ALL SELECT N'Lessons',      COUNT(*) FROM dbo.Lessons
UNION ALL SELECT N'Journal',      COUNT(*) FROM dbo.Journal;
GO
