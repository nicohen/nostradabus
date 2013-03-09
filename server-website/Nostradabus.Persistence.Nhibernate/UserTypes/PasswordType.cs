using System;
using Nostradabus.BusinessEntities.Common;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace Nostradabus.Persistence.Nhibernate.UserTypes
{
	public class PasswordType : IUserType
	{
		#region IUserType Members

		public object NullSafeGet(System.Data.IDataReader rs, string[] names, object owner)
		{
			var password = NHibernateUtil.String.NullSafeGet(rs,names[0]) as string;

			return new Password(password, true);
		}

		public void NullSafeSet(System.Data.IDbCommand cmd, object value, int index)
		{
			string password = value != null ? ((Password)value).Hash : null;

			NHibernateUtil.String.NullSafeSet(cmd, password, index);
		}
		
		public object Assemble(object cached, object owner)
		{
		    return cached;
		}

		public object DeepCopy(object value)
		{
			return new Password(((Password)value).Hash, true);
		}

		public object Disassemble(object value)
		{
		    return value;
		}

		public bool Equals(object x, object y)
		{
			return ((Password)x).Equals(y);
		}

		public int GetHashCode(object x)
		{
			return ((Password)x).GetHashCode();
		}

		public bool IsMutable
		{
			get { return false; }
		}
				
		public object Replace(object original, object target, object owner)
		{
		    return original;
		}

		public Type ReturnedType
		{
			get { return typeof(Password); }
		}

		public SqlType[] SqlTypes
		{
			get { return new SqlType[] { new StringSqlType() }; }
		}

		#endregion
	}
}
