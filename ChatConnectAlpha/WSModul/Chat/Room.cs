using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections.Generic;
using ChatConnect.Tcp.Protocol.WS;
using System.Diagnostics;
using System.Threading.Tasks;
using ChatConnect.Tcp.Protocol;

namespace ChatConnect.WebModul.Chat
{
    [JsonObject(MemberSerialization.OptIn)]
    class WsRoom
    {
		public event PHandlerEvent EventBroadcast
		{
			add
			{
				lock (SyncEvent)
					__EventBroadcast += value;
			}
			remove
			{
				lock (SyncEvent)
					__EventBroadcast -= value;
			}
		}

		[JsonProperty]
        public int Room { get; set; }
		[JsonProperty]
		public int Count { get; set; }
		[JsonProperty]
        public IUser Owner { get; set; }
        [JsonProperty]
        public string Capture { get; set; }
		[JsonProperty]
        public List<IUser> Register { get; set; }
        [JsonProperty]
        public List<byte[]> Messages { get; set; }
        [JsonProperty]
        public List<IWebModule> Unregister { get; set; }

		private event PHandlerEvent __EventBroadcast;
		private object SyncEvent	 =     new object();
		

		public WsRoom()
        {
            Register = new List<IUser>();
            Messages = new List<byte[]>();
            Unregister = new List<IWebModule>();
        }
        public WsRoom(int room)
        {
            Room = room;
            Owner = null;
            Capture = string.Empty;

            Register = new List<IUser>();
            Messages = new List<byte[]>();
            Unregister = new List<IWebModule>();
        }
        public WsRoom(int room, IUser owner)
        {
            Room = room;
            Owner = owner;
            Capture = string.Empty;

            Register = new List<IUser>();
            Messages = new List<byte[]>();
            Unregister = new List<IWebModule>();
        }
        public bool Update(IUser user)
        {
			
            bool result = false;
			IWebModule[] modules;
            lock (Register)
            {
                int index = -1;
                if ((index = Register.IndexOf(user)) > -1)
                {
					if (Register[index] != user)
					{
						Register[index].Bann = user.Bann;
						Register[index].Role = user.Role;
						user = Register[index];
					    Register[index] = 
								   Register[0];
						Register[  0  ] = user;
					}
                    modules = new WebModule[
						 user.Modules.Count];
					user.Modules.CopyTo(    modules, 0   );
					for (int i=0; i < modules.Length; i++)
					{
						HandlerModuleS.Handler(modules[i]);
					}
					result = true;
                }
                else
                    result = false;
            }
            return result;
        }
  async public void MsAsync(string message)
		{
			await Task.Run(() =>
			{
				Message(message);
			});
		}
		public void Message(string message)
        {
			Message(    Encoding.UTF8.GetBytes(message)    );
			
		}
		public void TestM(byte[] message)
		{
			int pcod = WSFrame7.BINNARY;
			WSFrame7 wsframe = new WSFrame7();
			wsframe.BitFind = 1;
			wsframe.BitPcod = pcod;
			wsframe.DataBody = message;
			wsframe.LengBody = message.Length;
			byte[] buffer = wsframe.GetDataFrame();

			PHandlerEvent h;
			lock (SyncEvent)
				h = __EventBroadcast;
			if (h != null)
				h(buffer, new PEventArgs("buffer", "message"));
		}
		public void Message(byte[] message)
		{
			int pcod = WSFrame7.TEXT;
			WSFrame7 wsframe = new WSFrame7();
					 wsframe.BitFind = 1;
					 wsframe.BitPcod = pcod;
					 wsframe.DataBody = message;
					 wsframe.LengBody = message.Length;
			byte[] buffer = wsframe.GetDataFrame();

			PHandlerEvent h;
			lock (SyncEvent)
				h = __EventBroadcast;
			if (h != null)
				h(buffer, new PEventArgs("buffer", "message"));
		}
        public bool Private(IUser user, string message)
        {

			bool result;
            /*    Список Модулей    */
            IList<IWebModule> modules;
        	lock ( Register )
            {
                int index = -1;
                if ((index = Register.IndexOf(user)) > -1)
                {
                	result = true;
            		modules = Register[index].Modules;
            
            		foreach (IWebModule module in modules)
					{
                        /* Отправка сообщения */
                        module.Message (message);
                    }
            	}
            	else
            		result = false;
            }
            return result;
        }
        
        public bool SetUserinRoom(IWebModule module)
        {
			   
			bool result;
			int index = -1;
        	lock (Register)
            {
                Count++;
                if ((index = Register.IndexOf(module.User)) == -1)
                {
					result = true;
                    /*  Добавить пользователя.  */
            		Register.Add(  module.User  );
                	
                }
            	else
            	{
            		result = false;
                    /* Скопировать пользователя */
                	module.User = Register[index];
            	}
				
				module.User.Modules.Add( module );
				EventBroadcast += module.EventBroadcast;
            }
            return result;
        }
        public bool UnsetUserinRoom(IWebModule module)
        {           
            bool result;
        	lock (Register)
            {
                int index = -1;
				if ((index = Register.IndexOf(module.User)) > -1)
				{
					Count--;
					Register[index].Modules.Remove(module);
					if (Register[index].Modules.Count == 0)
					{
						result = true;
						Register.RemoveAt(index);
					}
					else
						result = false;
				}
				else
					throw new ArgumentNullException( "Register" );
				EventBroadcast -= module.EventBroadcast;
			}
            return result;
        }
        public bool SetUnregisterUser(IWebModule module)
        {
        	bool result;
        	lock (Unregister)
        	{
				Count++;
                int index = -1;
                if ( (index = Unregister.IndexOf(module) ) > -1)
        			throw new ArgumentNullException("Unregister");
        		else
        		{
        			result = true;
                    Unregister.Add(   module   );
        		}
				EventBroadcast += module.EventBroadcast;
			}
        	return result;
        }
        public bool UnsetUnregisterUser(IWebModule module)
        {
        	bool result;
        	lock (Unregister)
        	{
				Count--;
                int index = -1;
                if ( (index = Unregister.IndexOf(module) ) == -1)
        		{
        			result = true;
        			Unregister.RemoveAt( index );
        		}
				else
					throw new ArgumentNullException("Unregister");
				EventBroadcast -= module.EventBroadcast;
			}
        	return result;
        }
    }
}
