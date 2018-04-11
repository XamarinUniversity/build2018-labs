using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Minutes
{
    public class FileEntryStore : INoteEntryStore
    {
        List<NoteEntry> loadedNotes;
        string filename;

        public FileEntryStore()
        {
            this.filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "minutes.xml");
        }

        public async Task AddAsync(NoteEntry entry)
        {
            await InitializeAsync();
            
            if (!loadedNotes.Contains(entry))
            {
                loadedNotes.Add(entry);
                await SaveData(filename, loadedNotes);
            }
        }

        public async Task DeleteAsync(NoteEntry entry)
        {
            Debug.Assert(loadedNotes != null);
            loadedNotes.Remove(entry);
            await SaveData(filename, loadedNotes);
        }

        public async Task<IEnumerable<NoteEntry>> GetAll()
        {
            await InitializeAsync();
            return loadedNotes.OrderByDescending(n => n.CreatedDate);
        }

        public async Task<NoteEntry> GetByIdAsync(string id)
        {
            await InitializeAsync();
            return loadedNotes.SingleOrDefault(n => n.Id == id);
        }

        public Task UpdateAsync(NoteEntry entry)
        {
            Debug.Assert(loadedNotes != null);
            return SaveData(filename, loadedNotes);
        }

        private async Task InitializeAsync()
        {
            if (loadedNotes == null)
            {
                loadedNotes = (await ReadData(filename)).ToList();
            }
        }

        private static async Task<IEnumerable<NoteEntry>> ReadData(string filename)
        {
            if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
            {
                return Enumerable.Empty<NoteEntry>();
            }

            string text;
            using (var reader = new StreamReader(filename))
            {
                text = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return Enumerable.Empty<NoteEntry>();
            }

            return XDocument.Parse(text)
                    .Root
                    .Elements("entry")
                    .Select(e =>
                        new NoteEntry
                        {
                            Title = e.Attribute("title").Value,
                            Text = e.Attribute("text").Value,
                            CreatedDate = (DateTime) e.Attribute("createdDate")
                        });
        }

        private async Task SaveData(string filename, IEnumerable<NoteEntry> notes)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return;
            }

            var root = new XDocument(
                new XElement("minutes",
                    notes.Select(n =>
                        new XElement("entry",
                            new XAttribute("title", n.Title),
                            new XAttribute("text", n.Text),
                            new XAttribute("createdDate", n.CreatedDate)))));

            using (StreamWriter writer = new StreamWriter(filename))
            {
                await writer.WriteAsync(root.ToString());
            }
        }
    }
}
