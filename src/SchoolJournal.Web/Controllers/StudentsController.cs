using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolJournal.BusinessLogic.DTOs;
using SchoolJournal.BusinessLogic.Exceptions;
using SchoolJournal.BusinessLogic.Interfaces;
using SchoolJournal.Web.Helpers;
using SchoolJournal.Web.ViewModels;

namespace SchoolJournal.Web.Controllers;

public class StudentsController(IStudentService service, ILookupService lookup) : Controller
{
    private const string FilterKey = "filter.students";

    public async Task<IActionResult> Index(StudentFilterDto filter, bool reset = false, CancellationToken ct = default)
    {
        filter = this.ResolveFilter(filter, FilterKey, reset);
        var model = new StudentIndexViewModel
        {
            Result = await service.GetPagedAsync(filter, ct),
            Filter = filter,
            Grades = new SelectList(Enumerable.Range(1, 11), filter.GradeLevel),
            Classes = new SelectList(await lookup.GetClassesAsync(null, ct), "Id", "Name", filter.ClassId)
        };
        return View(model);
    }

    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await PopulateAsync(ct);
        var today = DateOnly.FromDateTime(DateTime.Today);
        return View(new StudentEditDto { BirthDate = today.AddYears(-7), EnrollmentDate = today });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentEditDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAsync(ct);
            return View(dto);
        }

        try
        {
            await service.CreateAsync(dto, ct);
            TempData["Success"] = "Учащийся добавлен.";
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
    public async Task<IActionResult> Edit(int id, StudentEditDto dto, CancellationToken ct)
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
            TempData["Success"] = "Учащийся удалён.";
        }
        catch (BusinessRuleException exception)
        {
            TempData["Error"] = exception.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateAsync(CancellationToken ct)
    {
        ViewBag.Classes = new SelectList(await lookup.GetClassesAsync(null, ct), "Id", "Name");
    }
}
