using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChatConnect.WebModul.Chat
{
	[JsonObject(MemberSerialization.OptIn)]
    class User : IUser
    {
		[JsonProperty]
		public int Id
		{
			get
			{
				lock (this)
					return _id;
			}
			set
			{
				lock (this)
					_id = value;
			}
		}
		[JsonProperty]
        public int Room
        {
            get
            {
                lock (this)
                    return _room;
            }
            set
            {
                lock (this)
                    _room = value;
            }
        }
        [JsonProperty]
        public int Bann
        {
            get
            {
                lock (this)
                    return _bann;
            }
            set
            {
                lock (this)
                    _bann = value;
            }
        }
		[JsonProperty]
        public Role Role
        {
            get
            {
                lock (this)
                    return _role;
            }
            set
            {
                lock (this)
                    _role = value;
            }
        }
        [JsonProperty]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
		[ JsonIgnore ]
		public DateTime Date
		{
			get
			{
				return _date;
			}
			set
			{
				_date = value;
			}
		}
		[ JsonIgnore ]
        public IList<IWebModule> Modules
        {
            get
            {
                return _modles;
            }
            set
            {
                _modles = value;
            }
        }
		protected int _id;
        protected int _room;
        protected int _bann;
        protected Role _role;
        protected string _name;
		protected DateTime _date;
        protected IList<IWebModule> _modles;

        public User()
        {
            _modles = new List<IWebModule>();
        }

        public bool IsUser()
        {
            if ((Role & Role.User) > 0)
                return true;
            else
                return false;
        }
        public bool IsAdmin()
        {
            if ((Role & Role.Admin) > 0)
                return true;
            else
                return false;
        }
        public bool IsOwner()
        {
            if ((Role & Role.Owner) > 0)
                return true;
            else
                return false;
        }
        public bool IsModer()
        {
            if ((Role & Role.Moder) > 0)
                return true;
            else
                return false;
        }
        public bool IsStrimer()
        {
            if ((Role & Role.Strimer) > 0)
                return true;
            else
                return false;
        }
        public bool IsAssistant()
        {
            if ((Role & Role.Assistant) > 0)
                return true;
            else
                return false;
        }
        public bool IsSubscriber()
        {
            if ((Role & Role.Subscriber) > 0)
                return true;
            else
                return false;
        }
        public bool Equals(IUser User)
        {
            if (User == null)
                return false;
            if (string.IsNullOrEmpty(Name))
                return base.Equals(User);
            if (string.IsNullOrEmpty(User.Name))
                return false;
            else
                return Name.Equals ( User.Name );
        }
        public override int GetHashCode()
        {
            if (Name == null)
                return base.GetHashCode();
            else
                return Name.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else
                return Equals( (obj as IUser) );
        }
    }
}
