namespace SteamAuthCore.Models;

public class LoginAgainData
{
	public LoginAgainData()
	{
		
	}

	public LoginAgainData(LoginResult loginResult)
	{
		LoginResult = loginResult;
	}

	public LoginResult LoginResult { get; set; }
	public string? SessionCookie { get; set; }

	public UInt64 SteamId { get; set; }

	public string? EmailCode { get; set; }

	public string? CaptchaGid { get; set; }
	public string? CaptchaText { get; set; }
}