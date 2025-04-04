using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Data.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Business.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IAddressService _addressService;
    private readonly MemberFactory _memberFactory;

    public MemberService(IMemberRepository memberRepository, IRoleRepository roleRepository, IAddressService addressService, MemberFactory memberFactory)
    {
        _memberRepository = memberRepository;
        _roleRepository = roleRepository;
        _addressService = addressService;
        _memberFactory = memberFactory;
    }

    public async Task<ServiceResult<IEnumerable<MemberModel>>> GetAllMembersAsync()
    {
        var members = await _memberRepository.GetAllWithDetailsAsync(q =>
            q.Include(m => m.Role).Include(m => m.Address));

        var models = members.Select(m => _memberFactory.Create(m));
        return ServiceResult<IEnumerable<MemberModel>>.Success(models);
    }

    public async Task<ServiceResult<MemberModel>> GetMemberByIdAsync(int id)
    {
        var entity = await _memberRepository.GetOneWithIncludesAsync(id);
        if (entity == null)
            return ServiceResult<MemberModel>.Failure("Member not found", 404);

        var model = _memberFactory.Create(entity);
        return ServiceResult<MemberModel>.Success(model);
    }

    public async Task<ServiceResult<IEnumerable<MemberModel>>> SearchMembersAsync(string searchTerm)
    {
        var results = await _memberRepository.SearchMembersAsync(searchTerm);
        var models = results.Select(m => _memberFactory.Create(m));
        return ServiceResult<IEnumerable<MemberModel>>.Success(models);
    }

    public async Task<ServiceResult<MemberModel>> AddMemberAsync(AddMemberForm form, string imageName)
    {
        await _memberRepository.BeginTransactionAsync();

        try
        {
            var role = await GetOrCreateRoleAsync(form.RoleName!);
            var address = await _addressService.GetOrCreateAddressAsync(form.Street!, form.PostalCode!, form.City!);

            var member = _memberFactory.Create(form, role);
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

    public async Task<ServiceResult<bool>> DeleteMemberAsync(int id)
    {
        var member = await _memberRepository.GetOneAsync(m => m.Id == id);
        if (member == null)
            return ServiceResult<bool>.Failure("Member not found", 404);

        await _memberRepository.BeginTransactionAsync();

        try
        {
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
}
