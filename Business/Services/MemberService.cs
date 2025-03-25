using Business.Factories;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Data.Interfaces;
using System.Diagnostics;

namespace Business.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;

    public MemberService(IMemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
    }

    public async Task AddMemberAsync(AddMemberForm form, string imageName)
    {
        var member = MemberFactory.Create(form);

        member.ImageName = imageName;

        await _memberRepository.BeginTransactionAsync();
        try
        {
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
            MemberFactory.Update(member, form);

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
        return await _memberRepository.GetOneAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<MemberEntity>> GetAllMembersAsync()
    {
        return await _memberRepository.GetAllAsync();
    }

    public async Task<IEnumerable<MemberEntity>> SearchMembersAsync(string searchTerm)
    {
        return await _memberRepository.SearchMembersAsync(searchTerm);
    }
}
