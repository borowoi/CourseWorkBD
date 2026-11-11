using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.BusinessLogic.Exceptions;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.Web.Helpers;
using SchoolJournal.Web.ViewModels;

namespace SchoolJournal.Web.Controllers;

public class ClassesController(IClassService service, ILookupService lookup) : Controller
{
    private const string FilterKey = "filter.classes";

    public async Task<IActionResult> Index(ClassFilterDto filter, bool reset = false, CancellationToken ct = default)
    {
        filter = this.ResolveFilter(filter, FilterKey, reset);
        var model = new ClassIndexViewModel
        {
            Result = await service.GetPagedAsync(filter, ct),
            Filter = filter,
            Grades = new SelectList(Enumerable.Range(1, 11), filter.GradeLevel),
            Years = new SelectList(await lookup.GetAcademicYearsAsync(ct), filter.AcademicYear),
            Teachers = new SelectList(await lookup.GetTeachersAsync(ct), "Id", "Name", filter.HomeroomTeacherId)
        };
        return View(model);
    }

    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await PopulateAsync(ct);
        return View(new ClassEditDto { GradeLevel = 1, AcademicYear = "2026/2027" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClassEditDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAsync(ct);
            return View(dto);
        }

        try
        {
            await service.CreateAsync(dto, ct);
            TempData["Success"] = "Класс добавлен.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await PopulateAsync(ct);
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var dto = await service.GetForEditAsync(id, ct);
        if (dto is null)
            return NotFound();
        await PopulateAsync(ct);
        return View(dto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClassEditDto dto, CancellationToken ct)
    {
        if (id != dto.Id)
            return NotFound();
        if (!ModelState.IsValid)
        {
            await PopulateAsync(ct);
            return View(dto);
        }

        try
        {
            await service.UpdateAsync(dto, ct);
            TempData["Success"] = "Изменения сохранены.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await PopulateAsync(ct);
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
            TempData["Success"] = "Класс удалён.";
        }
        catch (BusinessRuleException exception)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateAsync(CancellationToken ct)
    {
        ViewBag.Teachers = new SelectList(await lookup.GetTeachersAsync(ct), "Id", "Name");
    }
}
