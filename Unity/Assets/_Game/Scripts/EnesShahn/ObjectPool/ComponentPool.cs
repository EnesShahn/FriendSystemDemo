using UnityEngine;

namespace EnesShahn.ObjectPool
{
    public class ComponentPool<T> : GenericObjectPool<T> where T : Component
    {
        private GameObject poolContainer;

        public ComponentPool(T pooledObject, int initialPoolSize, int growth) : base(pooledObject, initialPoolSize, growth) { }

        protected override void OnPoolInit()
        {
            poolContainer = new GameObject();
            poolContainer.name = pooledObject.name + " Pool";
        }
        protected override T CreateObject()
        {
            T obj = GameObject.Instantiate(pooledObject);
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(poolContainer.transform);
            return obj;
        }
        protected override void OnRequest(T obj)
        {
            obj.gameObject.SetActive(true);
        }
        protected override void OnReturn(T obj)
        {
            obj.transform.SetParent(poolContainer.transform);
            obj.gameObject.SetActive(false);
        }

    }
}