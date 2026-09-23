namespace KeePassNatMsg.Options
{
    class DatabaseItem
    {
        public string Id { get; set; }
        public string DbHash { get; set; }

        public override string ToString()
        {
            return Id ?? string.Empty;
        }
    }
}
