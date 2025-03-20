using Business.Dtos;
using Business.Factories;
using Business.Interfaces;
using Data.Entities;
using Data.Interfaces;
using System.Diagnostics;

namespace Business.Services
{
    public class MemberService(IMemberRepository memberRepository) : IMemberService
    {
        private readonly IMemberRepository _memberRepository = memberRepository;

        public async Task AddMemberAsync(AddMemberForm form)
        {
            var member = MemberFactory.Create(form);

            await _memberRepository.BeginTransactionAsync();
            try
            {
                await _memberRepository.CreateAsync(member);
                await _memberRepository.SaveToDatabaseAsync();
                await _memberRepository.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _memberRepository.RollbackTransactionAsync();
                Debug.WriteLine($"Error adding member: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateMemberAsync(int id, AddMemberForm form)
        {
            var member = await _memberRepository.GetOneAsync(m => m.Id == id)
                         ?? throw new Exception("Member not found");

            member.FirstName = form.FirstName;
            member.LastName = form.LastName;
            member.Email = form.MemberEmail;
            member.Address = form.Address;
            member.Phone = form.Phone;
            member.JobTitle = form.JobTitle;
            member.DateOfBirth = form.DateOfBirth;

            await _memberRepository.BeginTransactionAsync();
            try
            {
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
}
