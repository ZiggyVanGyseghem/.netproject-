using web_page_correct.school;
namespace web_page_correct.Models
{
    public class UserDto
    {
        public int? Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }

        public User ToDbo()
        {
            var userDbo = new User
            {
                FirstName = FirstName,
                LastName = LastName
            };
            if (Id != null)
            {
                userDbo.Id = (int)Id;
            }
            return userDbo;
        }

        public static UserDto FromDbo(User user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Id = user.Id
            };
        }
    }
}