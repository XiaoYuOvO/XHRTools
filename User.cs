using CsvHelper.Configuration.Attributes;

namespace XHRTools
{
    public class User
    {
        public User(string username, string id)
        {
            Username = username;
            Id = id;
        }
        [Index(0)]
        public string Username { get; }
        [Index(1)]
        public string Id { get; }

        public override string ToString()
        {
            return $"{nameof(Username)}: {Username}, {nameof(Id)}: {Id}";
        }
    }
}