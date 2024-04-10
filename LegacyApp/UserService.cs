using System;

namespace LegacyApp
{
    public class UserService
    {
        private readonly ClientRepository _clientRepository;

        public UserService()
        {
            _clientRepository = new ClientRepository();
        }

        public bool VerifyUserData(string firstName, string lastName, string email, DateTime dateOfBirth)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return false;
            }

            if (!email.Contains("@") && !email.Contains("."))
            {
                return false;
            }

            int age = CalculateUserAge(dateOfBirth);
            return age >= 21;
        }

        public int CalculateUserAge(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;
            return age;
        }

        public User CreateUser(Client client, DateTime dateOfBirth, string email, string firstName, string lastName)
        {
            return new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
        }

        public void CalculateCreditLimit(Client client, User user)
        {

            if (client.Type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else
            {
                using (var userCreditService = new UserCreditService())
                {
                    int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    user.CreditLimit = client.Type == "ImportantClient" ? creditLimit*2 : creditLimit;
                    user.HasCreditLimit = client.Type == "ImportantClient" ? false : true;
                }
            }

        }

        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            
            if(!VerifyUserData(firstName, lastName, email, dateOfBirth))
            {
                return false;
            }
            
            var client = _clientRepository.GetById(clientId);
            var user = CreateUser(client, dateOfBirth, email, firstName, lastName);

            CalculateCreditLimit(client, user);
            

            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }

            UserDataAccess.AddUser(user);
            return true;
        }
    }
}
