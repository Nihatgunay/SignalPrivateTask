using System.ComponentModel.DataAnnotations;

namespace SignalRtask.ViewModels
{
	public class LoginVM
	{
		[Required]
		public string Username { get; set; }
		[Required]
		[MinLength(8)]
		[DataType(DataType.Password)]
		public string Password { get; set; }
        public bool RememberMe { get; set; }
        public bool IsPersistent { get; set; }
	}
}
