-- =====================================================================
-- SchoolJournalDB: начальное заполнение тестовыми данными
-- Выполняется ПОСЛЕ 01_create_database.sql, один раз, целиком (F5 / Выполнить).
--
-- Итоговые объёмы (одна транзакция, при ошибке всё откатывается):
--   Teachers 120, Subjects 104, SubjectHours 1 144, Classes 110, Students ~2 100,
--   Lessons ~22 000, Journal ~180 000.
-- Данные детерминированы (псевдослучайность на основе CHECKSUM), результат повторяем.
-- Если данные уже есть, скрипт остановится: сначала выполните 99_clear_data.sql.
-- =====================================================================
USE SchoolJournalDB;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF EXISTS (SELECT 1 FROM dbo.Teachers) OR EXISTS (SELECT 1 FROM dbo.Classes)
    THROW 50001, N'В базе уже есть данные. Для повторного заполнения сначала выполните 99_clear_data.sql.', 1;

BEGIN TRANSACTION;

DECLARE @TeacherCount INT = 120;
DECLARE @Monday   DATE = '2026-08-31';   -- понедельник первой учебной недели
DECLARE @FirstDay DATE = '2026-09-01';   -- первый учебный день

-- ---------------------------------------------------------------- вспомогательные таблицы
DECLARE @Nums TABLE (n INT PRIMARY KEY);
INSERT INTO @Nums (n)
SELECT TOP (3000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL))
FROM sys.all_objects AS a CROSS JOIN sys.all_objects AS b;

DECLARE @Surnames    TABLE (id INT PRIMARY KEY, v NVARCHAR(50));
DECLARE @MaleNames   TABLE (id INT PRIMARY KEY, v NVARCHAR(50));
DECLARE @FemaleNames TABLE (id INT PRIMARY KEY, v NVARCHAR(50));
DECLARE @Patronymics TABLE (id INT PRIMARY KEY, v NVARCHAR(50));
DECLARE @BaseSubjects TABLE (id INT PRIMARY KEY, v NVARCHAR(80));
DECLARE @Variants    TABLE (id INT PRIMARY KEY, v NVARCHAR(40));
DECLARE @Letters     TABLE (id INT PRIMARY KEY, v NCHAR(1));
DECLARE @Ops         TABLE (id INT PRIMARY KEY, v NVARCHAR(5));
DECLARE @Streets     TABLE (id INT PRIMARY KEY, v NVARCHAR(50));

INSERT INTO @Surnames (id, v) VALUES
    (0, N'Иванов'),
    (1, N'Петров'),
    (2, N'Сидоров'),
    (3, N'Козлов'),
    (4, N'Новиков'),
    (5, N'Морозов'),
    (6, N'Волков'),
    (7, N'Соловьёв'),
    (8, N'Васильев'),
    (9, N'Зайцев'),
    (10, N'Павлов'),
    (11, N'Семёнов'),
    (12, N'Голубев'),
    (13, N'Виноградов'),
    (14, N'Богданов'),
    (15, N'Воробьёв'),
    (16, N'Фёдоров'),
    (17, N'Михайлов'),
    (18, N'Беляев'),
    (19, N'Тарасов'),
    (20, N'Белов'),
    (21, N'Комаров'),
    (22, N'Орлов'),
    (23, N'Киселёв'),
    (24, N'Макаров'),
    (25, N'Андреев'),
    (26, N'Ковалёв'),
    (27, N'Ильин'),
    (28, N'Гусев'),
    (29, N'Титов'),
    (30, N'Кузьмин'),
    (31, N'Кудрявцев'),
    (32, N'Баранов'),
    (33, N'Куликов'),
    (34, N'Алексеев'),
    (35, N'Степанов'),
    (36, N'Яковлев'),
    (37, N'Сорокин'),
    (38, N'Сергеев'),
    (39, N'Романов'),
    (40, N'Захаров'),
    (41, N'Борисов'),
    (42, N'Королёв'),
    (43, N'Герасимов'),
    (44, N'Пономарёв'),
    (45, N'Григорьев'),
    (46, N'Лазарев'),
    (47, N'Медведев'),
    (48, N'Ершов'),
    (49, N'Никитин'),
    (50, N'Ковальский'),
    (51, N'Войтеховский'),
    (52, N'Дубровский'),
    (53, N'Ковальчук'),
    (54, N'Мельник'),
    (55, N'Шевченко'),
    (56, N'Лукашевич'),
    (57, N'Кравец'),
    (58, N'Радзивилл'),
    (59, N'Жук');
INSERT INTO @MaleNames (id, v) VALUES
    (0, N'Александр'),
    (1, N'Алексей'),
    (2, N'Андрей'),
    (3, N'Артём'),
    (4, N'Виктор'),
    (5, N'Владимир'),
    (6, N'Дмитрий'),
    (7, N'Евгений'),
    (8, N'Иван'),
    (9, N'Игорь'),
    (10, N'Максим'),
    (11, N'Михаил'),
    (12, N'Николай'),
    (13, N'Сергей'),
    (14, N'Павел'),
    (15, N'Роман'),
    (16, N'Кирилл'),
    (17, N'Денис'),
    (18, N'Егор'),
    (19, N'Илья');
INSERT INTO @FemaleNames (id, v) VALUES
    (0, N'Анна'),
    (1, N'Мария'),
    (2, N'Елена'),
    (3, N'Ольга'),
    (4, N'Татьяна'),
    (5, N'Наталья'),
    (6, N'Ирина'),
    (7, N'Екатерина'),
    (8, N'Светлана'),
    (9, N'Юлия'),
    (10, N'Дарья'),
    (11, N'Алина'),
    (12, N'Виктория'),
    (13, N'Полина'),
    (14, N'София'),
    (15, N'Ксения'),
    (16, N'Валерия'),
    (17, N'Алиса'),
    (18, N'Ангелина'),
    (19, N'Вероника');
INSERT INTO @Patronymics (id, v) VALUES
    (0, N'Александрович'),
    (1, N'Алексеевич'),
    (2, N'Андреевич'),
    (3, N'Викторович'),
    (4, N'Владимирович'),
    (5, N'Дмитриевич'),
    (6, N'Евгеньевич'),
    (7, N'Иванович'),
    (8, N'Игоревич'),
    (9, N'Максимович'),
    (10, N'Михайлович'),
    (11, N'Николаевич'),
    (12, N'Сергеевич'),
    (13, N'Павлович'),
    (14, N'Романович'),
    (15, N'Кириллович'),
    (16, N'Денисович'),
    (17, N'Олегович'),
    (18, N'Петрович'),
    (19, N'Анатольевич');
INSERT INTO @BaseSubjects (id, v) VALUES
    (0, N'Математика'),
    (1, N'Алгебра'),
    (2, N'Геометрия'),
    (3, N'Русский язык'),
    (4, N'Русская литература'),
    (5, N'Белорусский язык'),
    (6, N'Белорусская литература'),
    (7, N'Английский язык'),
    (8, N'Немецкий язык'),
    (9, N'История'),
    (10, N'Обществоведение'),
    (11, N'География'),
    (12, N'Биология'),
    (13, N'Химия'),
    (14, N'Физика'),
    (15, N'Информатика'),
    (16, N'Физическая культура'),
    (17, N'Музыка'),
    (18, N'Изобразительное искусство'),
    (19, N'Трудовое обучение'),
    (20, N'Черчение'),
    (21, N'Астрономия'),
    (22, N'Основы безопасности жизнедеятельности'),
    (23, N'Всемирная история'),
    (24, N'История Беларуси'),
    (25, N'Человек и мир');
INSERT INTO @Variants (id, v) VALUES
    (0, N''),
    (1, N' (углублённый курс)'),
    (2, N' (факультатив)'),
    (3, N' (практикум)');
INSERT INTO @Letters (id, v) VALUES
    (0, N'А'),
    (1, N'Б'),
    (2, N'В'),
    (3, N'Г'),
    (4, N'Д'),
    (5, N'Е'),
    (6, N'Ж'),
    (7, N'З'),
    (8, N'И'),
    (9, N'К');
INSERT INTO @Ops (id, v) VALUES
    (0, N'29'),
    (1, N'33'),
    (2, N'44'),
    (3, N'25');
INSERT INTO @Streets (id, v) VALUES
    (0, N'Советская'),
    (1, N'Ленина'),
    (2, N'Мира'),
    (3, N'Победы'),
    (4, N'Кирова'),
    (5, N'Гагарина'),
    (6, N'Интернациональная'),
    (7, N'Космонавтов'),
    (8, N'Барыкина'),
    (9, N'Речицкое шоссе');

-- ---------------------------------------------------------------- ФИО (3000 вариантов)
-- Номера 1..120 используются для учителей (75 % женщин), остальные для учащихся (50 %).
CREATE TABLE #Persons (n INT PRIMARY KEY, FullName NVARCHAR(150) NOT NULL);

INSERT INTO #Persons (n, FullName)
SELECT n.n,
       CASE WHEN r.Fem = 1
            THEN CASE WHEN su.v LIKE N'%кий' THEN LEFT(su.v, LEN(su.v) - 2) + N'ая'
                      WHEN su.v LIKE N'%ов' OR su.v LIKE N'%ев' OR su.v LIKE N'%ёв' OR su.v LIKE N'%ин'
                           THEN su.v + N'а'
                      ELSE su.v END
            ELSE su.v END
       + N' ' + CASE WHEN r.Fem = 1 THEN fn.v ELSE mn.v END
       + N' ' + CASE WHEN r.Fem = 1 THEN LEFT(pa.v, LEN(pa.v) - 2) + N'на' ELSE pa.v END
FROM @Nums AS n
CROSS APPLY (SELECT CASE WHEN n.n <= @TeacherCount
                         THEN CASE WHEN ABS(CHECKSUM(n.n, 100) % 100) < 75 THEN 1 ELSE 0 END
                         ELSE CASE WHEN ABS(CHECKSUM(n.n, 100) % 2) = 0 THEN 1 ELSE 0 END END AS Fem,
                    ABS(CHECKSUM(n.n, 1) % 60) AS Si,
                    ABS(CHECKSUM(n.n, 2) % 20) AS Ni,
                    ABS(CHECKSUM(n.n, 3) % 20) AS Pi) AS r
JOIN @Surnames    AS su ON su.id = r.Si
JOIN @MaleNames   AS mn ON mn.id = r.Ni
JOIN @FemaleNames AS fn ON fn.id = r.Ni
JOIN @Patronymics AS pa ON pa.id = r.Pi;

-- ---------------------------------------------------------------- 1. Учителя (120)
INSERT INTO dbo.Teachers (FullName, QualificationCategory, WorkloadHours)
SELECT p.FullName,
       CASE WHEN c.R < 35 THEN N'Высшая'
            WHEN c.R < 75 THEN N'Первая'
            WHEN c.R < 92 THEN N'Вторая'
            ELSE N'Без категории' END,
       18 + ABS(CHECKSUM(p.n, 5) % 19)
FROM #Persons AS p
CROSS APPLY (SELECT ABS(CHECKSUM(p.n, 4) % 100) AS R) AS c
WHERE p.n <= @TeacherCount
ORDER BY p.n;

-- ---------------------------------------------------------------- 2. Предметы (26 x 4 = 104)
INSERT INTO dbo.Subjects (SubjectName, ExamType)
SELECT b.v + vr.v,
       CASE (b.id * 4 + vr.id) % 3
            WHEN 0 THEN N'Экзамен'
            WHEN 1 THEN N'Зачет'
            ELSE N'Дифференцированный зачет' END
FROM @BaseSubjects AS b
CROSS JOIN @Variants AS vr
ORDER BY b.id, vr.id;

-- ---------------------------------------------------------------- 3. Нагрузка по часам (104 x 11 = 1 144)
INSERT INTO dbo.SubjectHours (SubjectID, GradeLevel, HoursPerWeek)
SELECT s.SubjectID, g.n, 1 + ((s.SubjectID + g.n) % 3)
FROM dbo.Subjects AS s
CROSS JOIN (SELECT n FROM @Nums WHERE n <= 11) AS g;

-- ---------------------------------------------------------------- 4. Классы (11 параллелей x 10 литер = 110)
;WITH T AS (
    SELECT TeacherID, ROW_NUMBER() OVER (ORDER BY TeacherID) - 1 AS N
    FROM dbo.Teachers
),
C AS (
    SELECT g.n AS GradeLevel, l.v AS Letter, (g.n - 1) * 10 + l.id AS Idx
    FROM @Nums AS g
    CROSS JOIN @Letters AS l
    WHERE g.n <= 11
)
INSERT INTO dbo.Classes (GradeLevel, ClassLetter, HomeroomTeacherID, AcademicYear)
SELECT C.GradeLevel, C.Letter, T.TeacherID, N'2026/2027'
FROM C
JOIN T ON T.N = C.Idx % @TeacherCount
ORDER BY C.Idx;

-- ---------------------------------------------------------------- 5. Учащиеся (16-22 в классе, ~2 100)
;WITH Roster AS (
    SELECT c.ClassID, c.GradeLevel,
           ROW_NUMBER() OVER (ORDER BY c.ClassID, n.n) + @TeacherCount AS PN
    FROM dbo.Classes AS c
    JOIN @Nums AS n ON n.n <= 16 + c.ClassID % 7
)
INSERT INTO dbo.Students (FullName, BirthDate, CurrentClassID, ContactInfo, EnrollmentDate)
SELECT p.FullName,
       DATEFROMPARTS(2026 - (r.GradeLevel + 6) - ABS(CHECKSUM(r.PN, 7) % 2),
                     1 + ABS(CHECKSUM(r.PN, 8) % 12),
                     1 + ABS(CHECKSUM(r.PN, 9) % 28)),
       r.ClassID,
       N'+375 ' + op.v + N' ' + CAST(1000000 + ABS(CHECKSUM(r.PN, 10) % 9000000) AS NVARCHAR(10))
         + N'; г. Гомель, ул. ' + st.v
         + N', д. ' + CAST(1 + ABS(CHECKSUM(r.PN, 12) % 119) AS NVARCHAR(10))
         + N', кв. ' + CAST(1 + ABS(CHECKSUM(r.PN, 13) % 199) AS NVARCHAR(10)),
       DATEFROMPARTS(2026 - r.GradeLevel + 1, 9, 1)
FROM Roster AS r
JOIN #Persons AS p  ON p.n = r.PN
JOIN @Ops     AS op ON op.id = ABS(CHECKSUM(r.PN, 14) % 4)
JOIN @Streets AS st ON st.id = ABS(CHECKSUM(r.PN, 11) % 10)
ORDER BY r.ClassID, r.PN;

-- ---------------------------------------------------------------- 6. Уроки (~22 000)
-- У каждого класса по 6-7 предметов (SubjectID + ClassID делится на 17), 1-3 урока в неделю,
-- 17 недель (с 01.09.2026 по 25.12.2026: 1-я и 2-я четверти). Слот недели уникален внутри класса,
-- учитель назначается так, чтобы у него не было двух уроков одновременно.
;WITH ClassNum AS (
    SELECT ClassID, GradeLevel, CAST(ROW_NUMBER() OVER (ORDER BY ClassID) - 1 AS INT) AS Ord
    FROM dbo.Classes
),
Pairs AS (
    SELECT cn.ClassID, cn.GradeLevel, cn.Ord, s.SubjectID, s.SubjectName, sh.HoursPerWeek,
           CAST(ROW_NUMBER() OVER (PARTITION BY cn.ClassID ORDER BY s.SubjectID) - 1 AS INT) AS Rnk
    FROM ClassNum AS cn
    JOIN dbo.Subjects AS s ON (s.SubjectID + cn.ClassID) % 17 = 0
    JOIN dbo.SubjectHours AS sh ON sh.SubjectID = s.SubjectID AND sh.GradeLevel = cn.GradeLevel
),
TeacherNum AS (
    SELECT TeacherID, CAST(ROW_NUMBER() OVER (ORDER BY TeacherID) - 1 AS INT) AS N
    FROM dbo.Teachers
),
Weeks AS (
    SELECT n - 1 AS W FROM @Nums WHERE n <= 17
),
Ks AS (
    SELECT 1 AS K UNION ALL SELECT 2 UNION ALL SELECT 3
)
INSERT INTO dbo.Lessons (ClassID, SubjectID, TeacherID, LessonDate, LessonNumber, Topic)
SELECT p.ClassID, p.SubjectID, t.TeacherID, d.LessonDate, d.LessonNumber,
       p.SubjectName + N'. Урок № ' + CAST(w.W * 3 + k.K AS NVARCHAR(10))
FROM Pairs AS p
JOIN Ks AS k ON k.K <= p.HoursPerWeek
CROSS JOIN Weeks AS w
CROSS APPLY (SELECT p.Rnk * 3 + k.K - 1 AS Slot) AS sl
CROSS APPLY (SELECT DATEADD(DAY, w.W * 7 + (sl.Slot % 5), @Monday) AS LessonDate,
                    sl.Slot / 5 + 1 AS LessonNumber) AS d
JOIN TeacherNum AS t ON t.N = (p.Ord + p.Rnk * 17) % @TeacherCount
WHERE d.LessonDate >= @FirstDay;

-- ---------------------------------------------------------------- 7. Журнал (~180 000)
-- На каждом уроке у каждого ученика: 7 % пропуск (с причиной), 35 % отметка, остальное без записи.
-- Ученики с StudentID, кратным 13, учатся заметно хуже (чтобы в отчётах были неуспевающие).
INSERT INTO dbo.Journal (StudentID, LessonID, IsPresent, AbsenceReason, GradeValue, WorkType)
SELECT st.StudentID, l.LessonID,
       CASE WHEN x.R < 7 THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END,
       CASE WHEN x.R < 3 THEN N'Болезнь'
            WHEN x.R < 5 THEN N'Уважительная'
            WHEN x.R < 7 THEN N'Без уважительной причины' END,
       CASE WHEN x.R >= 7 THEN
            CASE WHEN st.StudentID % 13 = 0
                 THEN CASE WHEN x.G < 35 THEN 2 WHEN x.G < 70 THEN 3 WHEN x.G < 92 THEN 4 ELSE 5 END
                 ELSE CASE WHEN x.G < 4 THEN 2 WHEN x.G < 20 THEN 3 WHEN x.G < 60 THEN 4 ELSE 5 END
            END
       END,
       CASE WHEN x.R >= 7 THEN
            CASE WHEN x.W < 50 THEN N'Устный ответ'
                 WHEN x.W < 70 THEN N'Самостоятельная'
                 WHEN x.W < 90 THEN N'Контрольная'
                 ELSE N'Лабораторная' END
       END
FROM dbo.Lessons AS l
JOIN dbo.Students AS st ON st.CurrentClassID = l.ClassID
CROSS APPLY (SELECT ABS(CHECKSUM(l.LessonID, st.StudentID) % 100)     AS R,
                    ABS(CHECKSUM(st.StudentID, l.LessonID, 17) % 100) AS G,
                    ABS(CHECKSUM(l.LessonID, 31, st.StudentID) % 100) AS W) AS x
WHERE x.R < 42;

COMMIT TRANSACTION;

DROP TABLE #Persons;

-- ---------------------------------------------------------------- Контроль результата
SELECT N'Teachers'     AS [Таблица], COUNT(*) AS [Записей] FROM dbo.Teachers
UNION ALL SELECT N'Subjects',     COUNT(*) FROM dbo.Subjects
UNION ALL SELECT N'SubjectHours', COUNT(*) FROM dbo.SubjectHours
UNION ALL SELECT N'Classes',      COUNT(*) FROM dbo.Classes
UNION ALL SELECT N'Students',     COUNT(*) FROM dbo.Students
UNION ALL SELECT N'Lessons',      COUNT(*) FROM dbo.Lessons
UNION ALL SELECT N'Journal',      COUNT(*) FROM dbo.Journal;
GO
