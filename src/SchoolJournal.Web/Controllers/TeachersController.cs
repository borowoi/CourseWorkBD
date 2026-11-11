using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolJournal.BusinessLogic.Common;
using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.BusinessLogic.Exceptions;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.Web.Helpers;
using SchoolJournal.Web.ViewModels;

namespace SchoolJournal.Web.Controllers;

public class TeachersController(ITeacherService service) : Controller
{
    private const string FilterKey = "filter.teachers";

    public async Task<IActionResult> Index(TeacherFilterDto filter, bool reset = false, CancellationToken ct = default)
    {
        filter = this.ResolveFilter(filter, FilterKey, reset);
        var model = new TeacherIndexViewModel
        {
            Result = await service.GetPagedAsync(filter, ct),
            Filter = filter,
            Categories = new SelectList(QualificationCategories.All, filter.Category)
        };
        return View(model);
    }

    public IActionResult Create()
        => View(new TeacherEditDto { QualificationCategory = QualificationCategories.Highest, WorkloadHours = 18 });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TeacherEditDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await service.CreateAsync(dto, ct);
            TempData["Success"] = "Учитель добавлен.";
            return RedirectToAction(nameof(Index));
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
    public async Task<IActionResult> Edit(int id, TeacherEditDto dto, CancellationToken ct)
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
            TempData["Success"] = "Учитель удалён.";
        }
        catch (BusinessRuleException exception)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
