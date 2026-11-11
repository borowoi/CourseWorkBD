using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolJournal.BusinessLogic.Common;
using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.BusinessLogic.Exceptions;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.Web.Helpers;
using SchoolJournal.Web.ViewModels;

namespace SchoolJournal.Web.Controllers;

public class SubjectsController(ISubjectService service) : Controller
{
    private const string FilterKey = "filter.subjects";

    public async Task<IActionResult> Index(SubjectFilterDto filter, bool reset = false, CancellationToken ct = default)
    {
        filter = this.ResolveFilter(filter, FilterKey, reset);
        var model = new SubjectIndexViewModel
        {
            Result = await service.GetPagedAsync(filter, ct),
            Filter = filter,
            ExamTypes = new SelectList(ExamTypes.All, filter.ExamType)
        };
        return View(model);
    }

    public IActionResult Create()
        => View(new SubjectEditDto { ExamType = ExamTypes.Exam });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SubjectEditDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            var id = await service.CreateAsync(dto, ct);
            TempData["Success"] = "Предмет добавлен. Теперь можно указать нагрузку по параллелям.";
            return RedirectToAction(nameof(Hours), new { id });
        }
        catch (BusinessRuleException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var dto = await service.GetForEditAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SubjectEditDto dto, CancellationToken ct)
    {
        if (id != dto.Id)
            return NotFound();
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await service.UpdateAsync(dto, ct);
            TempData["Success"] = "Изменения сохранены.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(dto);
        }
    }

    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var dto = await service.GetForEditAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        try
        {
            await service.DeleteAsync(id, ct);
            TempData["Success"] = "Предмет удалён.";
        }
        catch (BusinessRuleException exception)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    // ---------- нагрузка по часам в неделю по параллелям ----------
    public async Task<IActionResult> Hours(int id, CancellationToken ct)
    {
        var subject = await service.GetForEditAsync(id, ct);
        if (subject is null)
            return NotFound();

        return View(new SubjectHoursViewModel
        {
            SubjectId = id,
            SubjectName = subject.SubjectName,
            Hours = await service.GetHoursAsync(id, ct)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetHours(int id, int gradeLevel, int hoursPerWeek, CancellationToken ct)
    {
        try
        {
            await service.SetHoursAsync(id, gradeLevel, hoursPerWeek, ct);
            TempData["Success"] = "Нагрузка сохранена.";
        }
        catch (BusinessRuleException exception)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Hours), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveHours(int id, int gradeLevel, CancellationToken ct)
    {
        try
        {
            await service.RemoveHoursAsync(id, gradeLevel, ct);
            TempData["Success"] = "Запись о нагрузке удалена.";
        }
        catch (BusinessRuleException exception)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Hours), new { id });
    }
}
