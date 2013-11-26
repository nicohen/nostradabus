using System;
using System.Security.Cryptography;

namespace Nostradabus.BusinessEntities.Common
{
	/// <summary>
	/// Represents a password.
	/// Passwords can only be assigned and compared,
	/// never read. Upon assigment, the class
	/// computes the assigned string's hash and 
	/// saves it for future comparison.
	/// </summary>
	public class Password : IConvertible
	{
		#region Hash algorithm
		protected static HashAlgorithm _HashAlgorithm;

		/// <summary>
		/// An implementation of the Hash algorithm,
		/// in System.Security.Cryptography.
		/// </summary>
		public static HashAlgorithm HashAlgorithm 
		{
			get 
			{
				return _HashAlgorithm;
			}
			set 
			{
				_HashAlgorithm = value;
			}
		}
		#endregion

		#region Protected variables
		protected string _Hash;
		#endregion

		#region Properties
		/// <summary>
		/// Returns the computed hash string.
		/// </summary>
		public string Hash 
		{
			get 
			{
				return _Hash;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// The static constructor loads the appropiate
		/// hash algorithm specified by the configuration
		/// settings. If none is specified, the SHA1 
		/// algorithm is loaded by default.
		/// </summary>
		static Password() 
		{
			/*
			// Read what hash algorithm to use.
			System.Collections.Specialized.NameValueCollection collection = ConfigurationManager.Instance[ConfigurationSection.Password];

			if (collection != null) 
			{
				string algorithm = collection["HashAlgorithm"];

				if (algorithm != null) 
				{
					_HashAlgorithm = HashAlgorithm.Create(algorithm);
				}
				else 
				{
					_HashAlgorithm = HashAlgorithm.Create();
				}
			}
			else 
			{
				_HashAlgorithm = HashAlgorithm.Create();
			}
			*/

			_HashAlgorithm = HashAlgorithm.Create("md5");
		}

		public Password() 
		{
		}

		public Password(string value) : this(value, false)
		{
			
		}

		public Password(string value, bool hash) 
		{
			if (hash) 
			{
				_Hash = value;
			}
			else 
			{
				_Hash = null;

				Assign(value);
			}
		}
		#endregion

		#region Operators
		public static implicit operator Password(string s) 
		{
			return new Password(s);
		}

		public static bool operator ==(Password p, string s) 
		{
			return p.Equals(s);
		}

		public static bool operator !=(Password p, string s) 
		{
			return !(p.Equals(s));
		}
		#endregion

		#region Methods
		public void Assign(string value) 
		{
			_Hash = ComputeHash(value);
		}

		public override bool Equals(object obj)
		{
			try 
			{
				var value = obj as string;
				if (value != null)
				{
					return ComputeHash(value).Equals(_Hash);
				}
				
				return _Hash == ((Password)obj)._Hash;
			}
			catch 
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return _Hash.GetHashCode();
		}

		public override string ToString()
		{
			return new string('*', 8);
		}

		public static Password FromHash(string s) 
		{
			return new Password(s, true);
		}
		#endregion

		#region Internal stuff
		private string ComputeHash(string value)
		{
			byte[] bytes = System.Text.Encoding.ASCII.GetBytes(value);

			return ComputeHash(bytes);
		}

		private string ComputeHash(byte[] buffer) 
		{
			_HashAlgorithm.Initialize();

			byte[] hash = _HashAlgorithm.ComputeHash(buffer);

			System.Text.StringBuilder builder = new System.Text.StringBuilder(hash.Length * 2);

			foreach (byte b in hash) 
			{
				builder.AppendFormat("{0:X2}", b);
			}

			return builder.ToString();
		}
		#endregion

		#region Random password generation
		/// <summary>
		/// Generates a new random password.
		/// </summary>
		/// <param name="length">Length of password to be generated.</param>
		/// <returns>Password string.</returns>
		public static string New(int length) 
		{
			var sp = new RNGCryptoServiceProvider();

			// Create an array of the specified length
			var bytes = new byte[length];

			// Fill each byte
			for (int i = 0; i < length; i++) 
			{
				byte[] b = {0};

				int j = 0;

				// Alternate a letter and a number
				if (i % 2 == 0) 
				{
					// Character must be number
					while (!(j > 47 && j < 58)) 
					{
						sp.GetNonZeroBytes(b);

						j = Convert.ToInt32(b[0]);
					}
				}
				else 
				{
					// Character must be either an uppercase
					// or a lowercase letter
					while (!((j > 64 && j < 91) || (j > 96 && j < 123))) 
					{
						sp.GetNonZeroBytes(b);

						j = Convert.ToInt32(b[0]);
					}
				}

				// Assign the random byte to the original array
				bytes[i] = b[0];
			}

			// Convert the byte array to an UTF8 string and return it
			return System.Text.Encoding.UTF8.GetString(bytes);
		}

		public static string New() 
		{
			return New(8);
		}
		#endregion

		#region IConvertible Members

		public ulong ToUInt64(IFormatProvider provider)
		{
			return 0;
		}

		public sbyte ToSByte(IFormatProvider provider)
		{
			return 0;
		}

		public double ToDouble(IFormatProvider provider)
		{
			return 0;
		}

		public DateTime ToDateTime(IFormatProvider provider)
		{
			return new DateTime ();
		}

		public float ToSingle(IFormatProvider provider)
		{
			return 0;
		}

		public TypeCode GetTypeCode()
		{
			return Hash.GetTypeCode();
		}

		public bool ToBoolean(IFormatProvider provider)
		{
			return false;
		}

		public int ToInt32(IFormatProvider provider)
		{
			return 0;
		}

		public ushort ToUInt16(IFormatProvider provider)
		{
			return 0;
		}

		public short ToInt16(IFormatProvider provider)
		{
			return 0;
		}

		string IConvertible.ToString(IFormatProvider provider)
		{
			return ToString();
		}

		public byte ToByte(IFormatProvider provider)
		{
			return 0;
		}

		public char ToChar(IFormatProvider provider)
		{
			return '\0';
		}

		public long ToInt64(IFormatProvider provider)
		{
			return 0;
		}
		
		public decimal ToDecimal(IFormatProvider provider)
		{
			return 0;
		}

		public object ToType(Type conversionType, IFormatProvider provider)
		{
			return null;
		}

		public uint ToUInt32(IFormatProvider provider)
		{
			return 0;
		}

		#endregion
	}
}
