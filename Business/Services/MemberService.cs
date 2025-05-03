using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Business.Services;

/// <summary>
/// Handles member-related business operations, including retrieval, search,
/// creation, updating, deletion, and fetching of newly added members with related data.
/// </summary>
public class MemberService(
    IMemberRepository memberRepository,
    IRoleRepository roleRepository,
    IAddressService addressService,
    MemberFactory memberFactory,
    UserManager<ApplicationUser> userManager,
    INotificationService notificationService) : IMemberService
{
    private readonly IMemberRepository _memberRepository = memberRepository;
    private readonly IRoleRepository _roleRepository = roleRepository;
    private readonly IAddressService _addressService = addressService;
    private readonly MemberFactory _memberFactory = memberFactory;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly INotificationService _notificationService = notificationService;

    /// <summary>
    /// Retrieves all members along with their roles and addresses.
    /// </summary>
    public async Task<ServiceResult<IEnumerable<MemberModel>>> GetAllMembersAsync()
    {
        var members = await _memberRepository.GetAllWithDetailsAsync(q =>
            q.Include(m => m.Role)
             .Include(m => m.Address));
        var models = members.Select(m => _memberFactory.Create(m));
        return ServiceResult<IEnumerable<MemberModel>>.Success(models);
    }

    /// <summary>
    /// Retrieves a single member by its unique identifier.
    /// </summary>
    public async Task<ServiceResult<MemberModel>> GetMemberByIdAsync(int id)
    {
        var entity = await _memberRepository.GetOneWithIncludesAsync(id);
        if (entity == null)
            return ServiceResult<MemberModel>.Failure("Member not found", 404);

        var model = _memberFactory.Create(entity);
        return ServiceResult<MemberModel>.Success(model);
    }

    /// <summary>
    /// Searches for members matching the specified term.
    /// </summary>
    public async Task<List<MemberModel>> SearchMembersAsync(string searchTerm)
    {
        var results = await _memberRepository.SearchMembersAsync(searchTerm);
        return results.Select(m => _memberFactory.Create(m)).ToList();
    }

    /// <summary>
    /// Adds a new member with role and address resolution, within a transaction.
    /// </summary>
    public async Task<ServiceResult<MemberModel>> AddMemberAsync(AddMemberForm form, string imageName)
    {
        await _memberRepository.BeginTransactionAsync();

        try
        {
            var role = await GetOrCreateRoleAsync(form.RoleName!);
            var address = await _addressService.GetOrCreateAddressAsync(form.Street!, form.PostalCode!, form.City!);

            var member = _memberFactory.Create(form, role, imageName);
            member.ImageName = imageName;
            member.Address = address;

            await _memberRepository.CreateAsync(member);
            await _memberRepository.SaveToDatabaseAsync();
            await _memberRepository.CommitTransactionAsync();

            var model = _memberFactory.Create(member);
            return ServiceResult<MemberModel>.Success(model, 201);
        }
        catch (Exception ex)
        {
            await _memberRepository.RollbackTransactionAsync();
            return ServiceResult<MemberModel>.Failure($"Failed to add member: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Updates an existing member’s details, including role and address, within a transaction.
    /// </summary>
    public async Task<ServiceResult<MemberModel>> UpdateMemberAsync(int id, EditMemberForm form)
    {
        var member = await _memberRepository.GetOneAsync(m => m.Id == id);
        if (member == null)
            return ServiceResult<MemberModel>.Failure("Member not found", 404);

        await _memberRepository.BeginTransactionAsync();

        try
        {
            var role = await GetOrCreateRoleAsync(form.RoleName!);
            var address = await _addressService.GetOrCreateAddressAsync(form.Street!, form.PostalCode!, form.City!);

            _memberFactory.Update(member, form, role);
            member.Address = address;

            _memberRepository.Update(member);
            await _memberRepository.SaveToDatabaseAsync();
            await _memberRepository.CommitTransactionAsync();

            var model = _memberFactory.Create(member);
            return ServiceResult<MemberModel>.Success(model);
        }
        catch (Exception ex)
        {
            await _memberRepository.RollbackTransactionAsync();
            return ServiceResult<MemberModel>.Failure($"Failed to update member: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Deletes a member and its associated user account, within a transaction.
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteMemberAsync(int id)
    {
        var member = await _memberRepository.GetOneAsync(m => m.Id == id);
        if (member == null)
            return ServiceResult<bool>.Failure("Member not found", 404);

        await _memberRepository.BeginTransactionAsync();

        try
        {
            if (!string.IsNullOrEmpty(member.UserId))
            {
                var user = await _userManager.FindByIdAsync(member.UserId);
                if (user != null)
                {
                    var userDeletionResult = await _userManager.DeleteAsync(user);
                    if (!userDeletionResult.Succeeded)
                    {
                        await _memberRepository.RollbackTransactionAsync();
                        return ServiceResult<bool>.Failure("Failed to delete associated user", 500);
                    }
                }
            }

            _memberRepository.Delete(member);
            await _memberRepository.SaveToDatabaseAsync();
            await _memberRepository.CommitTransactionAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await _memberRepository.RollbackTransactionAsync();
            return ServiceResult<bool>.Failure($"Failed to delete member: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Retrieves an existing role by the specified name, or creates and persists a new one if none is found.
    /// </summary>
    private async Task<RoleEntity> GetOrCreateRoleAsync(string roleName)
    {
        roleName = roleName.Trim();
        var existingRole = await _roleRepository.GetOneAsync(r => r.Name.ToLower() == roleName.ToLower());
        if (existingRole != null)
            return existingRole;

        var newRole = new RoleEntity { Name = roleName };
        await _roleRepository.CreateAsync(newRole);
        await _roleRepository.SaveToDatabaseAsync();
        return newRole;
    }

    /// <summary>
    /// Retrieves members created after the specified date.
    /// </summary>
    public async Task<IEnumerable<MemberModel>> GetNewMembersAsync(DateTime since)
    {
        var members = await _memberRepository.GetCreatedAfterAsync(since);

        var models = members.Select(m => new MemberModel
        {
            Id = m.Id,
            FirstName = m.FirstName,
            LastName = m.LastName,
            Email = m.Email,
            ImageName = m.ImageName,
            Created = m.Created
        });

        return models;
    }
}
