using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Minutes
{
	public class MemoryEntryStore : INoteEntryStore
    {
        private readonly Dictionary<string, NoteEntry> entries = new Dictionary<string, NoteEntry>();

		public Task<IEnumerable<NoteEntry>> GetAll()
		{
            return Task.FromResult<IEnumerable<NoteEntry>>(entries.Values.ToList());
		}

		public Task AddAsync(NoteEntry entry)
		{
            entries.Add(entry.Id, entry);

            return Task.CompletedTask;
		}

        public Task UpdateAsync(NoteEntry entry)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(NoteEntry entry)
        {
            entries.Remove(entry.Id);
            return Task.CompletedTask;
        }

        public Task<NoteEntry> GetByIdAsync(string id)
        {
            if (entries.TryGetValue(id, out NoteEntry entry))
            {
                return Task.FromResult(entry);
            }

            return Task.FromResult<NoteEntry>(null);
        }
    }
}