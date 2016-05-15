using System;
using System.Collections.Generic;

namespace ChatConnect.WebModul.Chat
{
    public enum Role : int
    {
        User = 1,
        Admin = 2,
        Owner = 4,
        Moder = 8,
        Strimer = 16,
        Assistant = 32,
        Subscriber = 64
    }
    interface IUser : IEquatable<IUser>
    {
		int Id { get; set; }
		int Room { get; set; }
        int Bann { get; set; }
        Role Role { get; set; }
        string Name { get; set; }
		DateTime Date { get; set; }
		IList<IWebModule> Modules { get; set; }

        bool IsUser();
        bool IsAdmin();
        bool IsOwner();
        bool IsModer();
        bool IsStrimer();
        bool IsAssistant();
        bool IsSubscriber();
    }
}