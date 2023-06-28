using System.Collections.Generic;

namespace EnesShahn.ObjectPool
{
    public abstract class GenericObjectPool<T>
    {
        #region Variables
        protected bool initialized;
        protected int initialPoolSize;
        protected int growth;
        protected T pooledObject;
        #endregion

        //#region Properties
        //public bool Initialized { get { return initialized; } private set { initialized = value; } }
        //public int InitialPoolSize { get { return initialPoolSize; } private set { initialPoolSize = value; } }
        //public int Growth { get { return growth; } private set { growth = value; } }
        //public T PooledObject { get { return pooledObject; } private set { pooledObject = value; } }
        //#endregion

        private Stack<T> objectPool;

        public GenericObjectPool(T pooledObject, int initialPoolSize, int growth)
        {
            this.pooledObject = pooledObject;
            this.initialPoolSize = initialPoolSize;
            this.growth = growth;
            Init();
        }

        private void Init()
        {
            initialized = true;
            OnPoolInit();
            objectPool = new Stack<T>(initialPoolSize);
            FillPool(initialPoolSize);
        }
        private void FillPool(int fillAmount)
        {
            for (int _ = 0; _ < fillAmount; _++)
            {
                T obj = CreateObject();
                objectPool.Push(obj);
            }
        }
        public T RequestObject()
        {
            if (!initialized)
                Init();
            if (objectPool.Count == 0)
                FillPool(growth);

            T obj = objectPool.Pop();
            OnRequest(obj);
            return obj;
        }
        public void ReturnObject(T obj)
        {
            OnReturn(obj);
            objectPool.Push(obj);
        }

        #region Abstract Method
        protected abstract void OnPoolInit();
        protected abstract T CreateObject();
        protected abstract void OnRequest(T obj);
        protected abstract void OnReturn(T obj);
        #endregion
    }
}