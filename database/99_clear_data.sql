-- =====================================================================
-- Очистка всех данных SchoolJournalDB (структура и view сохраняются).
-- Нужна, чтобы повторно выполнить 02_seed_data.sql.
-- =====================================================================
USE SchoolJournalDB;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

DELETE FROM dbo.Journal;
DELETE FROM dbo.Lessons;
DELETE FROM dbo.Students;
DELETE FROM dbo.SubjectHours;
DELETE FROM dbo.Classes;
DELETE FROM dbo.Subjects;
DELETE FROM dbo.Teachers;

-- Сброс счётчиков IDENTITY: следующая запись снова получит номер 1
DBCC CHECKIDENT (N'dbo.Journal',  RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT (N'dbo.Lessons',  RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT (N'dbo.Students', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT (N'dbo.Classes',  RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT (N'dbo.Subjects', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT (N'dbo.Teachers', RESEED, 0) WITH NO_INFOMSGS;

COMMIT TRANSACTION;
GO
