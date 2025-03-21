using Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class ProjectCrudController : Controller
{
    // private readonly IProjectService _projectService;

    [HttpPost]
    public IActionResult AddProject(AddProjectForm form)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(x => x.ErrorMessage).ToArray()
                );

            return BadRequest(new { success = false, errors });
        }

        //var result = await _memberService.AddMemberAsync(form);
        //if (result)
        //{
        //    return Ok(new { success = true });
        //}
        //else
        //{
        // return Problem("unable to submit data")
        //}

        return Ok(new { success = true }); //radera senare
    }
}
