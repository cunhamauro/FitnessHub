namespace FitnessHub.Data.Entities.GymClasses
{
    public class ClassWaitlist : IEntity
    {
        public int Id { get; set; } // GymClassId

        public List<string> ClientEmailsOrderedList { get; set; } = new List<string>();

        public List<string> GetClientEmails()
        {
            List<string> emails = new();

            foreach (var email in ClientEmailsOrderedList)
            {
                // Split the string at the first "@" only (2 parts: before and after the first @)
                string[] emailParts = email.Split('@', 2);

                // If the email contains an '@', take everything after the '@'
                if (emailParts.Length > 1)
                {
                    emails.Add(emailParts[1]); // Add the part after the '@'
                }
            }

            return emails;
        }
    }
}
