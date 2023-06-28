namespace SDK
{
    public class BaseModule
    {
        protected BaseModule() { }
        public virtual void OnLogin() { }
        public virtual void OnRegister() { }
        public virtual void OnLogout() { }
        public virtual void OnUpdate() { }
    }
}