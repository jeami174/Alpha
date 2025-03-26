using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Business.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IAddressService _addressService;

    public MemberService(IMemberRepository memberRepository, IRoleRepository roleRepository, IAddressService addressService)
    {
        _memberRepository = memberRepository;
        _roleRepository = roleRepository;
        _addressService = addressService;
    }

    public async Task AddMemberAsync(AddMemberForm form, string imageName)
    {
        await _memberRepository.BeginTransactionAsync();

        try
        {
            var role = await GetOrCreateRoleAsync(form.RoleName!);
            var address = await _addressService.GetOrCreateAddressAsync(form.Street!, form.PostalCode!, form.City!);

            var member = MemberFactory.Create(form, role);
            member.ImageName = imageName;
            member.Address = address;

            await _memberRepository.CreateAsync(member);
            await _memberRepository.SaveToDatabaseAsync();
            await _memberRepository.CommitTransactionAsync();
        }
        catch
        {
            await _memberRepository.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task UpdateMemberAsync(int id, EditMemberForm form)
    {
        var member = await _memberRepository.GetOneAsync(m => m.Id == id)
                     ?? throw new Exception("Member not found");

        await _memberRepository.BeginTransactionAsync();

        try
        {
            var role = await GetOrCreateRoleAsync(form.RoleName!);
            var address = await _addressService.GetOrCreateAddressAsync(form.Street!, form.PostalCode!, form.City!);

            MemberFactory.Update(member, form, role);
            member.Address = address;

            _memberRepository.Update(member);
            await _memberRepository.SaveToDatabaseAsync();
            await _memberRepository.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _memberRepository.RollbackTransactionAsync();
            Debug.WriteLine($"Error updating member: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteMemberAsync(int id)
    {
        var member = await _memberRepository.GetOneAsync(m => m.Id == id)
                     ?? throw new Exception("Member not found");

        await _memberRepository.BeginTransactionAsync();

        try
        {
            _memberRepository.Delete(member);
            await _memberRepository.SaveToDatabaseAsync();
            await _memberRepository.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _memberRepository.RollbackTransactionAsync();
            Debug.WriteLine($"Error deleting member: {ex.Message}");
            throw;
        }
    }

    public async Task<MemberEntity?> GetMemberByIdAsync(int id)
    {
        return await _memberRepository.GetOneWithIncludesAsync(id);
    }

    public async Task<IEnumerable<MemberEntity>> GetAllMembersAsync()
    {
        return await _memberRepository.GetAllWithDetailsAsync(q => q.Include(m => m.Role));
    }

    public async Task<IEnumerable<MemberEntity>> SearchMembersAsync(string searchTerm)
    {
        return await _memberRepository.SearchMembersAsync(searchTerm);
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
