namespace LibraryApi.Models
{
    public class Member
    {
        public int MemberId { get; set; } //variable for storing member id

        public string FullName { get; set; } = string.Empty; //variable for storing member name

        public string Email { get; set; } = string.Empty; //variable for storing email 

        public string PhoneNumber { get; set; } = string.Empty; //variable for storing phone number

        public DateTime MembershipDate { get; set; } //variable for storing membership date
    }
}