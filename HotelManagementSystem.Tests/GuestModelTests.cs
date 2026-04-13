using HotelManagementSystem.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace HotelManagementSystem.Tests
{
    public class GuestModelTests
    {
        [Fact]
        public void GuestModel_RequiresFirstAndLastName()
        {
            var guest = new Guest { Email = "a@b.com" };
            var context = new ValidationContext(guest);
            var results = new List<ValidationResult>();

            var valid = Validator.TryValidateObject(guest, context, results, true);

            Assert.False(valid);
            Assert.Contains(results, r => r.MemberNames.Contains("FirstName"));
            Assert.Contains(results, r => r.MemberNames.Contains("LastName"));
        }
    }
}
