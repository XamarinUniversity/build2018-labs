using System;

namespace Minutes
{
	public class NoteEntry
	{
        public string Id { get; set; }
        public DateTime CreatedDate { get; set; }
		public string Title { get; set; }
		public string Text { get; set; }

        public NoteEntry()
		{
            Id = Guid.NewGuid().ToString();
			CreatedDate = DateTime.Now;
            Title = Text = string.Empty;
		}

		public override string ToString()
		{
			return $"{Title} {CreatedDate}";
		}
	}
}