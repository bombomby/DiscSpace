using Crosstales.BWF;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class Utils
{
	public static float Distance(Bounds bounds, Vector3 p)
	{
		return (p - bounds.ClosestPoint(p)).magnitude;

		//var dx = Mathf.Max(rect.min.x - p.x, 0, p.x - rect.max.x);
		//var dy = Mathf.Max(rect.min.y - p.y, 0, p.y - rect.max.y);
		//return Mathf.Sqrt(dx * dx + dy * dy);
	}

	public static string ReplaceBadWords(string text)
	{
		//return text;
		return BWFManager.Instance.ReplaceAll(text, Crosstales.BWF.Model.Enum.ManagerMask.BadWord | Crosstales.BWF.Model.Enum.ManagerMask.Domain);
	}

	const int KeyCodeRange = 256;
	public static int GenerateKeyCode(string text)
	{
		int hash = CalcDeterministicHashCode(text);
		return ((hash % KeyCodeRange) + KeyCodeRange) % KeyCodeRange;
	}

	private static System.Random random = new System.Random();
	public static string GenerateRandomString(int length)
	{
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
	}

	public static string ComputeSHA256(string input)
	{
		using (SHA256 sha256Hash = SHA256.Create())
		{
			byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
			var sBuilder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("x2"));
			}
			return sBuilder.ToString();
		}
	}

	static int CalcDeterministicHashCode(string str)
	{
		unchecked
		{
			int hash1 = (5381 << 16) + 5381;
			int hash2 = hash1;

			for (int i = 0; i < str.Length; i += 2)
			{
				hash1 = ((hash1 << 5) + hash1) ^ str[i];
				if (i == str.Length - 1)
					break;
				hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
			}

			return hash1 + (hash2 * 1566083941);
		}
	}

	static Regex SignatureRegex = new Regex(@"\[#([0-9A-Fa-f]+)\]$");

	public static string StripSignature(string nickname)
	{
		return SignatureRegex.Replace(nickname, "");
	}

	public static string ExtractSignature(string nickname)
	{
		Match match = SignatureRegex.Match(nickname);
		return (match.Success && match.Groups.Count > 1) ? match.Groups[1].Value : null;
	}

	public static string GenerateSignature(Player player, string room, string pin)
	{
		string signature = string.Format("{0}{1}{2}", StripSignature(player.NickName), room, pin);
		return ComputeSHA256(signature);
	}
}
