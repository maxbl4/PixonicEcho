using System;
using System.Reactive.Subjects;

namespace PixonicEcho
{
    class Room : IDisposable
    {
        private readonly IDisposable subscription;

        public Room(string name, Subject<Message> subject)
        {
            if (subject == null) throw new ArgumentNullException(nameof(subject));
            Name = name;
            Subject = subject;
            subscription = subject.Subscribe(x => LastMessage = DateTime.Now);
        }

        public string Name { get; }

        public Subject<Message> Subject { get; }
        public DateTime LastMessage { get; private set; }

        public void Dispose()
        {
            subscription.Dispose();
            Subject.Dispose();
        }
    }
}